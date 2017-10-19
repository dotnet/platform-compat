using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Cci.Extensions;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.Zip;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Readers;
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
            catch (Exception ex)
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

        private static void Run(string sourcePath, string inclusionFile, string exclusionFile, string outputPath)
        {
            string tempFolder = null;
            try
            {
                if (sourcePath == null)
                {
                    tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempFolder);

                    var rootUrl = "https://dotnetcli.azureedge.net/dotnet/Sdk/2.0.0/";
                    var files = new[]
                    {
                        "dotnet-sdk-2.0.0-win-x64.zip",
                        "dotnet-sdk-2.0.0-osx-x64.tar.gz",
                        "dotnet-sdk-2.0.0-linux-x64.tar.gz"
                    };

                    DownloadFiles(rootUrl, files, tempFolder);
                    ExtractFiles(tempFolder);
                    sourcePath = tempFolder;
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

        private static void DownloadFiles(string rootUrl, string[] files, string tempFolder)
        {
            Console.WriteLine("Downloading files...");

            foreach (var file in files)
            {
                Console.WriteLine("\t" + file);

                var downloadUrl = rootUrl + file;
                var localPath = Path.Combine(tempFolder, file);

                WebClient client = new WebClient();
                client.DownloadFile(downloadUrl, localPath);
            }
        }

        private static void ExtractFiles(string tempFolder)
        {
            Console.WriteLine("Extracting files...");

            ExpandGzipStreams(tempFolder);
            ExtractArchives(tempFolder);
        }

        private static void ExpandGzipStreams(string directory)
        {
            var gzFiles = Directory.EnumerateFiles(directory, "*.gz");

            foreach (var gzFile in gzFiles)
            {
                Console.WriteLine("\t" + Path.GetFileName(gzFile));

                var source = gzFile;
                var target = Path.Combine(directory, Path.GetFileNameWithoutExtension(source));

                using (var sourceStream = File.OpenRead(source))
                using (var gzStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                using (var targetSteam = File.Create(target))
                    gzStream.CopyTo(targetSteam);

                File.Delete(source);
            }
        }

        private static void ExtractArchives(string tempFolder)
        {
            var options = new ExtractionOptions { ExtractFullPath = true, Overwrite = true };

            foreach (var fileName in Directory.GetFiles(tempFolder))
            {
                Console.WriteLine("\t" + Path.GetFileName(fileName));

                var extractedFolderName = Path.GetFileNameWithoutExtension(fileName).Replace(".tar", "");
                var exractedFolderPath = Path.Combine(tempFolder, extractedFolderName);

                using (var fileStream = File.OpenRead(fileName))
                using (var archive = OpenArchive(fileStream))
                {
                    var selectedEntries = archive.Entries.Where(
                        e => e.Key.StartsWith(@"./shared/Microsoft.NETCore.App/") ||
                        e.Key.StartsWith(@"shared/Microsoft.NETCore.App/") ||
                        e.Key.StartsWith(@"shared\Microsoft.NETCore.App\"));
                    if (!selectedEntries.Any())
                        throw new InvalidDataException($"No archive selected to be extracted from {fileName} at {tempFolder}");

                    foreach (var entry in selectedEntries)
                        entry.WriteToDirectory(exractedFolderPath, options);
                }

                File.Delete(fileName);
            }
        }

        private static IArchive OpenArchive(FileStream fileStream)
        {
            var extension = Path.GetExtension(fileStream.Name);
            switch (extension)
            {
                case ".tar":
                    return TarArchive.Open(fileStream);
                case ".zip":
                    return ZipArchive.Open(fileStream);
                default:
                    throw new NotImplementedException($"Unknown file {extension}");
            }
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
                var match = Regex.Match(root, @"dotnet-sdk-2.0.0-([^-]+)-x64");
                var platform = match.Success ? match.Groups[1].Value : root;

                var sharedFrameworkFolder = Path.Combine(root, "shared", "Microsoft.NETCore.App");
                var version200Folder = Directory.EnumerateDirectories(sharedFrameworkFolder, "2.0.0*", SearchOption.TopDirectoryOnly).Single();

                yield return (platform, version200Folder);
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
