using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;
using Terrajobst.Csv;
using Terrajobst.Pns.Scanner;

namespace pns_scan
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                var toolName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
                Console.Error.WriteLine($"Usage: {toolName} <directory-or-binary> <out-path>");
                return 1;
            }

            var inputPath = args[0];
            var outputPath = args[1];
            var isFile = File.Exists(inputPath);
            var isDirectory = Directory.Exists(inputPath);
            if (!isFile && !isDirectory)
            {
                Console.Error.WriteLine($"ERROR: '{inputPath}' must be a file or directory.");
                return 1;
            }

            try
            {
                Run(inputPath, outputPath);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR: {ex.Message}");
                return 1;
            }
        }

        private static void Run(string inputPath, string outputPath)
        {
            var assemblies = LoadAssemblies(inputPath);

            using (var textWriter = new StreamWriter(outputPath))
            {
                var csvWriter = new CsvWriter(textWriter);
                var reporter = new CsvReporter(csvWriter);
                var analyzer = new PnsScanner(reporter);

                foreach (var assembly in assemblies)
                    analyzer.AnalyzeAssembly(assembly);
            }
        }

        private static IEnumerable<IAssembly> LoadAssemblies(string input)
        {
            var inputPaths = HostEnvironment.SplitPaths(input);
            var filePaths = HostEnvironment.GetFilePaths(inputPaths, SearchOption.AllDirectories).ToArray();
            return HostEnvironment.LoadAssemblySet(filePaths).Distinct();
        }
    }
}
