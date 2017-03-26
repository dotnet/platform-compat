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
using Terrajobst.Csv;
using Terrajobst.PlatformNotSupported.Analysis;

namespace pns_gen
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                var toolName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
                Console.Error.WriteLine($"Usage: {toolName} <out-path>");
                return 1;
            }

            var outputPath = args[0];

            try
            {
                Run(outputPath);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR: {ex.Message}");
                return 1;
            }
        }

        private static void Run(string outputPath)
        {
            var rootUrl = "https://dotnetcli.blob.core.windows.net/dotnet/Sdk/master/";
            var files = new[]
            {
                "dotnet-dev-win-x64.latest.zip",
                "dotnet-dev-osx-x64.latest.tar.gz",
                "dotnet-dev-linux-x64.latest.tar.gz"
            };

            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);
            try
            {
                DownloadFiles(rootUrl, files, tempFolder);
                ExtractFiles(tempFolder);

                var database = Analyze(tempFolder);
                ExportCsv(database, outputPath);
            }
            finally
            {
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
            foreach (var fileName in Directory.GetFiles(tempFolder))
            {
                Console.WriteLine("\t" + Path.GetFileName(fileName));

                var extractedFolderName = Path.GetFileNameWithoutExtension(fileName).Replace(".tar", "");
                var exractedFolderPath = Path.Combine(tempFolder, extractedFolderName);

                using (var fileStream = File.OpenRead(fileName))
                using (var archive = OpenArchive(fileStream))
                {
                    archive.WriteToDirectory(exractedFolderPath, new ExtractionOptions
                    {
                        ExtractFullPath = true,
                        Overwrite = true,
                    });
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

        private static PnsDatabase Analyze(string tempFolder)
        {
            Console.WriteLine("Analyzing...");

            var result = new PnsDatabase();

            var platforms = EnumeratePlatformDirectories(tempFolder);

            foreach (var entry in platforms)
            {
                Console.WriteLine("\t" + entry.platform);

                var platform = entry.platform;
                var directory = entry.directory;
                var reporter = new PnsReporter(result, platform);

                var analyzer = new PlatformNotSupportedAnalyzer(reporter);
                var assemblies = HostEnvironment.LoadAssemblySet(directory);

                foreach (var assembly in assemblies)
                    analyzer.AnalyzeAssembly(assembly);
            }

            return result;
        }

        private static IEnumerable<(string platform, string directory)> EnumeratePlatformDirectories(string tempFolder)
        {
            var roots = Directory.EnumerateDirectories(tempFolder);

            foreach (var root in roots)
            {
                var match = Regex.Match(root, @"dotnet-dev-([^-]+)-[^-]+.latest");
                var platform = match.Success ? match.Groups[1].Value : root;

                var sharedFrameworkFolder = Path.Combine(root, "shared", "Microsoft.NETCore.App");
                var version200Folder = Directory.EnumerateDirectories(sharedFrameworkFolder, "2.0.0*", SearchOption.TopDirectoryOnly).Single();

                yield return (platform, version200Folder);
            }
        }

        private static void ExportCsv(PnsDatabase database, string path)
        {
            using (var streamWriter = new StreamWriter(path))
            {
                var writer = new CsvWriter(streamWriter);

                writer.Write("DocId");
                writer.Write("Namespace");
                writer.Write("Type");
                writer.Write("Member");

                foreach (var platform in database.Platforms)
                    writer.Write(platform);

                writer.WriteLine();

                foreach (var entry in database.Entries)
                {
                    writer.Write(entry.DocId);
                    writer.Write(entry.NamespaceName);
                    writer.Write(entry.TypeName);
                    writer.Write(entry.MemberName);

                    foreach (var platform in database.Platforms)
                    {
                        var value = entry.Platforms.Contains(platform) ? "X" : "";
                        writer.Write(value);
                    }

                    writer.WriteLine();
                }
            }
        }
    }
}
