using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ApiCompat.Csv;

namespace modernsdk_index
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            string outputPath;
            string[] inputFiles;

            if (args.Length == 0)
            {
                var appPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                var wackPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Windows Kits\10\App Certification Kit");
                inputFiles = Directory.EnumerateFiles(wackPath, @"SupportedAPIs*.xml").ToArray();
                outputPath = Path.GetFullPath(Path.Combine(appPath, "..", "..", "..", "..", "..", "etc", "modernsdk.csv"));
            }
            else
            {
                if (args.Length < 2)
                {
                    var toolName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
                    Console.Error.WriteLine($"Usage: {toolName} <out-path> <input-file>...");
                    return 1;
                }

                outputPath = args[0];
                inputFiles = args.Skip(1).ToArray();
            }

            try
            {
                Run(outputPath, inputFiles);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static void Run(string outputPath, string[] inputFiles)
        {
            var comparer = Comparer<(string name, string moduleName)>.Create(
                (x, y) =>
                {
                    var nameResult = StringComparer.OrdinalIgnoreCase.Compare(x.name, y.name);
                    if (nameResult != 0)
                        return nameResult;

                    return StringComparer.OrdinalIgnoreCase.Compare(x.moduleName, y.moduleName);
                }
            );
            var allowedApis = new SortedSet<(string name, string moduleName)>(comparer);

            foreach (var inputFile in inputFiles)
            {
                var doc = XDocument.Load(inputFile);
                var apiElements = doc.Descendants("API");

                foreach (var apiElement in apiElements)
                {
                    var name = apiElement.Attribute("Name").Value;
                    var moduleName = apiElement.Attribute("ModuleName").Value;
                    allowedApis.Add((name, moduleName));
                }
            }

            using (var textWriter = File.CreateText(outputPath))
            {
                var writer = new CsvWriter(textWriter);

                writer.Write("Name");
                writer.Write("ModuleName");
                writer.WriteLine();

                foreach (var allowedApi in allowedApis)
                {
                    writer.Write(allowedApi.name);
                    writer.Write(allowedApi.moduleName);
                    writer.WriteLine();
                }
            }
        }
    }
}
