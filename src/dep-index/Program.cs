using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;
using Microsoft.DotNet.Cci;
using Microsoft.DotNet.Csv;

namespace dep_index
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
                var docsPath = Path.GetFullPath(Path.Combine(appPath, "..", "..", "..", "..", "..", "docs"));
                inputFiles = Directory.EnumerateFiles(docsPath, "DE*.md").ToArray();
                outputPath = Path.GetFullPath(Path.Combine(appPath, "..", "..", "..", "..", "..", "etc", "deprecated.csv"));
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
                var strackTrace = new StackTrace(ex, true);
                var frame = strackTrace.GetFrame(0);
                var fileName = $"{frame.GetFileName()}({frame.GetFileLineNumber()},{frame.GetFileColumnNumber()})";
                WriteError(ex.Message, fileName);
                return 1;
            }
        }

        private static void WriteError(string text, string fileName = null)
        {
            if (fileName == null)
                Console.Error.WriteLine($"error dep-index: {text}");
            else
                Console.Error.WriteLine($"{fileName} : error dep-index: {text}");
        }

        private static void Run(string outputPath, string[] inputFiles)
        {
            var docIdIndex = CreateDocIdIndex();
            var result = ProcessFiles(inputFiles, docIdIndex);
            WriteResults(outputPath, result);
        }

        private static Dictionary<string, IDefinition> CreateDocIdIndex()
        {
            var referenceAssemblyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                                                     "Reference Assemblies",
                                                     "Microsoft",
                                                     "Framework",
                                                     ".NETFramework",
                                                     "v4.6.1");

            var assemblies = HostEnvironment.LoadAssemblySet(referenceAssemblyPath).ToArray();
            var publicNamespaces = assemblies.SelectMany(a => a.GetAllNamespaces().Where(n => n.GetTypes().Any(t => t.IsVisibleOutsideAssembly())));
            var publicTypes = assemblies.SelectMany(a => a.GetAllTypes().Where(t => t.IsVisibleOutsideAssembly()));
            var publicMembers = publicTypes.SelectMany(t => t.Members.Where(m => m.IsVisibleOutsideAssembly()));
            return publicNamespaces.Concat<IDefinition>(publicTypes)
                                   .Concat(publicMembers)
                                   .GroupBy(d => d.UniqueId())
                                   .ToDictionary(g => g.Key, g => g.First());
        }

        private static Dictionary<(string docid, string namespaceName, string typeName, string signature), SortedSet<string>> ProcessFiles(string[] inputFiles, Dictionary<string, IDefinition> docIdIndex)
        {
            var result = new Dictionary<(string docid, string namespaceName, string typeName, string signature), SortedSet<string>>();

            foreach (var inputFile in inputFiles)
            {
                var diagnosticId = Path.GetFileNameWithoutExtension(inputFile);
                var docIds = GetDocIds(inputFile);
                if (!docIds.Any())
                {
                    WriteError("Input file doesn't list any APIs.", inputFile);
                }
                else
                {
                    foreach (var (docId, lineNumber) in docIds)
                    {
                        if (!docIdIndex.TryGetValue(docId, out var api))
                        {
                            WriteError($"The API '{docId}' cannot be resolved.", $"{inputFile}({lineNumber})");
                        }
                        else
                        {
                            var namespaceName = api.GetNamespaceName();
                            var typeName = api.GetTypeName();
                            var signature = api.GetMemberSignature();

                            var key = (docId, namespaceName, typeName, signature);

                            if (!result.TryGetValue(key, out var diagnosticIds))
                            {
                                diagnosticIds = new SortedSet<string>();
                                result.Add(key, diagnosticIds);
                            }

                            diagnosticIds.Add(diagnosticId);
                        }
                    }
                }
            }

            return result;
        }

        private static void WriteResults(string outputPath, Dictionary<(string docid, string namespaceName, string typeName, string signature), SortedSet<string>> result)
        {
            using (var textWriter = File.CreateText(outputPath))
            {
                var writer = new CsvWriter(textWriter);
                writer.Write("DocId");
                writer.Write("Namespace");
                writer.Write("Type");
                writer.Write("Member");
                writer.Write("DiagnosticIds");
                writer.WriteLine();

                foreach (var item in result.OrderBy(kv => kv.Key.namespaceName)
                                           .ThenBy(kv => kv.Key.typeName)
                                           .ThenBy(kv => kv.Key.signature))
                {
                    var list = string.Join(";", item.Value);

                    writer.Write(item.Key.docid);
                    writer.Write(item.Key.namespaceName);
                    writer.Write(item.Key.typeName);
                    writer.Write(item.Key.signature);
                    writer.Write(list);
                    writer.WriteLine();
                }
            }
        }

        private static (string docId, int line)[] GetDocIds(string fileName)
        {
            // We expect the input to be in the following form:
            //
            // <!--
            // DocId
            // DocId
            // ...
            // -->
            // <Rest of file>

            var result = new List<(string docId, int line)>();

            using (var reader = File.OpenText(fileName))
            {
                var lineNumber = 1;
                var hasSeenHeader = false;

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (!hasSeenHeader)
                    {
                        if (line == "<!--")
                            hasSeenHeader = true;
                    }
                    else if (line == "-->")
                    {
                        // Found end of header block, so we're done.
                        break;
                    }
                    else
                    {
                        result.Add((line, lineNumber));
                    }

                    lineNumber++;
                }
            }

            return result.ToArray();
        }
    }
}
