using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;

namespace dep_helper
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                var toolName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
                Console.Error.WriteLine($"Usage: {toolName} <directory-or-binary> <outputfile-path> <deprecated-type-regex-pattern>");
                Console.Error.WriteLine();
                Console.Error.WriteLine("This tool generates the list of constructors and methods for a type that is being");
                Console.Error.WriteLine("deprecated via a DE* rule. The output file contains the DocIds that go on the");
                Console.Error.WriteLine("header of a DE*.md file.");
                Console.Error.WriteLine();
                
                return 1;
            }

            var inputPath = args[0];
            var outputPath = args[1];
            var deprecatedTypeRegexPattern = args[2];

            var isFile = File.Exists(inputPath);
            var isDirectory = Directory.Exists(inputPath);
            if (!isFile && !isDirectory)
            {
                Console.Error.WriteLine($"ERROR: '{inputPath}' must be a file or directory.");
                return 1;
            }

            try
            {
                Run(inputPath, outputPath, deprecatedTypeRegexPattern);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR: {ex.Message}");
                return 1;
            }
        }

        private static void Run(string inputPath, string outputPath, string deprecatedTypeRegexPattern)
        {
            var assemblies = LoadAssemblies(inputPath);

            using (var textWriter = new StreamWriter(outputPath))
            {
                var reporter = new DeprecationReporter(textWriter);
                var deprecatedTypeRegex = new Regex(deprecatedTypeRegexPattern, RegexOptions.Compiled);
                var scanner = new DeprecationHelperScanner(reporter, deprecatedTypeRegex);

                foreach (var assembly in assemblies)
                    scanner.ScanAssembly(assembly);
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
