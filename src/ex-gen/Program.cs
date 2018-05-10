using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Cci.Extensions;
using Microsoft.DotNet.Csv;
using Microsoft.DotNet.Scanner;

namespace ex_gen
{
    internal static class Program
    {
        const string SourcePathSwitch = "-src";
        const string InclusionFileSwitch = "-inc";
        const string ExclusionFileSwitch = "-exc";
        const string OutputFileSwitch = "-out";

        private static int Main(string[] args)
        {
            if (!TryParseArguments(args, out string sourcePath, out string inclusionFile, out string exclusionFile, out string outputPath))
            {
                var toolName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
                Console.Error.WriteLine($"Usage: {toolName} [{SourcePathSwitch}:<src-path>] [{InclusionFileSwitch}:<inclusion-file>] [{ExclusionFileSwitch}:<exclusion-file>] {OutputFileSwitch}:<out-path>");
                Console.Error.WriteLine($"\nOptional:\n");
                Console.Error.WriteLine($"\t{SourcePathSwitch}:<source-path>");
                Console.Error.WriteLine($"\t{InclusionFileSwitch}:<inclusion-file>");
                Console.Error.WriteLine($"\t{ExclusionFileSwitch}:<exclusion-file>");
                Console.Error.WriteLine($"\nMandatory:\n");
                Console.Error.WriteLine($"\t{OutputFileSwitch}:<out-path>");
                Console.Error.WriteLine();
                return 1;
            }

            try
            {
                Run(sourcePath, inclusionFile, exclusionFile, outputPath);
                return 0;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine($"ERROR: {ex.Message}");
                return 1;
            }
        }

        private static bool TryParseArguments(string[] args, out string sourcePath, out string inclusionFile, out string exclusionFile, out string outputPath)
        {
            const int minArgsLength = 1;
            const int maxArgsLength = 5;

            sourcePath = inclusionFile = exclusionFile = outputPath = null;
            if (args.Length < minArgsLength || args.Length > maxArgsLength)
                return false;

            for (var i = 0; i < args.Length; ++i)
            {
                var tokens = args[i].Split(new[] { ':' }, 2);
                if (tokens.Length != 2)
                    return false;

                var fullPath = Path.GetFullPath(tokens[1]);
                switch (tokens[0])
                {
                    case SourcePathSwitch:
                        sourcePath = fullPath;
                        break;
                    case InclusionFileSwitch:
                        inclusionFile = fullPath;
                        break;
                    case ExclusionFileSwitch:
                        exclusionFile = fullPath;
                        break;
                    case OutputFileSwitch:
                        outputPath = fullPath;
                        break;
                    default:
                        return false;
                }
            }

            return !string.IsNullOrWhiteSpace(outputPath);
        }

        private static void Run(string sourcePath, string inclusionFile, string exclusionFile, string outputPath, string sdkVersion = "2.1.4")
        {
            string tempFolder = null;
            try
            {
                if (sourcePath == null)
                {
                    tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    sourcePath = tempFolder;
                }

                if (!Directory.Exists(sourcePath))
                {
                    var tfm = @"netcoreapp2.1";

                    var packages = new[]
                    {
                        ("Microsoft.Windows.Compatibility", "2.0.0-rc1"),
                    };

                    var rids = new[]
                    {
                        "win-x64",
                        "osx-x64",
                        "linux-x64",
                    };

                    var projectDirectoryPath = Path.Combine(sourcePath, "project");
                    var projectFilePath = Path.Combine(projectDirectoryPath, "project.csproj");

                    GenerateProject(projectFilePath, tfm, packages);

                    var allSucceeded = true;
                    
                    foreach (var rid in rids)
                    {
                        var ridOutputFolder = Path.Combine(sourcePath, rid);
                        if (!PublishProject(projectFilePath, ridOutputFolder, rid))
                            allSucceeded = false;
                    }

                    if (!allSucceeded)
                    {
                        Console.Error.WriteLine("FATAL: Errors occurred during restore.");
                        return;
                    }
                }

                var databases = Scan(sourcePath, inclusionFile, exclusionFile);
                ExportCsv(databases.filtered, outputPath, writeSite:false);

                var filteredWithSiteOutputPath = Path.ChangeExtension(outputPath, ".filtered.site.csv");
                ExportCsv(databases.filtered, filteredWithSiteOutputPath, writeSite: true);

                var unfilteredWithSiteOutputPath = Path.ChangeExtension(outputPath, ".unfiltered.site.csv");
                ExportCsv(databases.raw, unfilteredWithSiteOutputPath, writeSite: true);
            }
            finally
            {
                if (tempFolder != null)
                    Directory.Delete(tempFolder, true);
            }
        }

        private static void GenerateProject(string projectFilePath, string tfm, IEnumerable<(string id, string version)> packageReferences)
        {
            string contents = $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>{tfm}</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    {
    string.Join(Environment.NewLine, packageReferences.Select(pr => $@"<PackageReference Include=""{pr.id}"" Version=""{pr.version}"" />"))
    }
  </ItemGroup>

</Project>";

            var emptyMain = @"internal static class Program
{
    public static void Main()
    {
    }
}";

            var directoryPath = Path.GetDirectoryName(projectFilePath);
            Directory.CreateDirectory(directoryPath);

            File.WriteAllText(projectFilePath, contents);

            var programCs = Path.Combine(directoryPath, "Program.cs");
            File.WriteAllText(programCs, emptyMain);
        }

        private static bool PublishProject(string projectFilePath, string outputPath, string rid)
        {
            var command = "dotnet";
            var args = $@"publish --output ""{outputPath}"" --runtime ""{rid}""";
            var workingDirectoryPath = Path.GetDirectoryName(projectFilePath);

            var process = new Process();
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = args;
            process.StartInfo.WorkingDirectory = workingDirectoryPath;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.OutputDataReceived += (s, d) => Console.Out.WriteLine(d.Data);
            process.ErrorDataReceived += (s, d) => Console.Error.WriteLine(d.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        private static (Database raw, Database filtered) Scan(string sourcePath, string inclusionFile, string exclusionFile)
        {
            Console.WriteLine("Analyzing...");

            var platforms = EnumeratePlatformDirectories(sourcePath);
            var platformNames = platforms.Select(e => e.platform).ToList();

            Database exclusionDatabase = null;
            if (exclusionFile != null)
            {
                exclusionDatabase = new Database();
                ImportCsv(exclusionFile, platformNames, exclusionDatabase);
            }

            var filtered = new Database(exclusionDatabase);
            if (inclusionFile != null)
                ImportCsv(inclusionFile, platformNames, filtered);

            var raw = new Database();

            foreach (var entry in platforms)
            {
                Console.WriteLine("\t" + entry.platform);

                var platform = entry.platform;
                var directory = entry.directory;
                var reporter = new DatabaseReporter(new[] { raw, filtered }, platform);

                var analyzer = new ExceptionScanner(reporter);
                var assemblies = HostEnvironment.LoadAssemblySet(directory);

                foreach (var assembly in assemblies)
                    analyzer.ScanAssembly(assembly);
            }

            return (raw, filtered);
        }

        private static IEnumerable<(string platform, string directory)> EnumeratePlatformDirectories(string tempFolder)
        {
            var roots = Directory.EnumerateDirectories(tempFolder);

            foreach (var root in roots)
            {
                // Skip the directory that contains the project
                if (Path.GetFileName(root) == "project")
                    continue;

                var match = Regex.Match(root, @"\\([^-]+)-x64");
                if (!match.Success)
                {
                    throw new InvalidDataException($"Published directory {root} name is not in the expected format.");
                }

                var platform = match.Groups[1].Value;
                yield return (platform, root);
            }
        }

        private static void ExportCsv(Database database, string path, bool writeSite)
        {
            using (var streamWriter = new StreamWriter(path))
            {
                var writer = new CsvWriter(streamWriter);

                writer.Write("DocId");
                writer.Write("Namespace");
                writer.Write("Type");
                writer.Write("Member");

                if (writeSite)
                    writer.Write("Site");

                foreach (var platform in database.Platforms)
                    writer.Write(platform);

                writer.WriteLine();

                foreach (var entry in database.Entries.OrderBy(e => e.NamespaceName)
                                                      .ThenBy(e => e.TypeName)
                                                      .ThenBy(e => e.MemberName)
                                                      .ThenBy(e => e.DocId))
                {
                    writer.Write(entry.DocId);
                    writer.Write(entry.NamespaceName);
                    writer.Write(entry.TypeName);
                    writer.Write(entry.MemberName);

                    if (writeSite)
                        writer.Write(entry.Site);

                    foreach (var platform in database.Platforms)
                    {
                        var value = entry.Platforms.Contains(platform) ? "X" : "";
                        writer.Write(value);
                    }

                    writer.WriteLine();
                }
            }
        }

        private static void ImportCsv(string path, IList<string> expectedPlatforms, Database database)
        {
            using (var streamReader = new StreamReader(path))
            {
                var csvReader = new CsvReader(streamReader);
                var headerFields = csvReader.ReadLine();
                var indexOfFirstPlatformField = headerFields
                    .Select((h, i) => new { p = h, idx = i})
                    .First(c => expectedPlatforms.Contains(c.p))
                    .idx;

                if (expectedPlatforms.Count != (headerFields.Length - indexOfFirstPlatformField))
                    throw new InvalidDataException($"Number of platforms in {path} did not match the expected value of {expectedPlatforms.Count}");

                if (!headerFields.Skip(indexOfFirstPlatformField).All(p => expectedPlatforms.Contains(p)))
                    throw new InvalidDataException($"Some platforms in {path} are not in the expected platforms: {string.Join(", ", expectedPlatforms)}");

                while (!streamReader.EndOfStream)
                {
                    var row = csvReader.ReadLine();
                    // Add the entry for each platform
                    for (var i = indexOfFirstPlatformField; i < row.Length; ++i)
                    {
                        if (!string.IsNullOrWhiteSpace(row[i]))
                            database.Add(row[0], row[1], row[2], row[3], string.Empty, headerFields[i]);
                    }
                }
            }
        }
    }
}
