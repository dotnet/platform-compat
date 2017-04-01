using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Terrajobst.Pns.Scanner.Tests.Helpers
{
    public abstract partial class PnsTests
    {
        private static readonly CSharpCompilation CompilationTemplate = CSharpCompilation.Create(
            "dummy.dll",
            references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, specificDiagnosticOptions: new[]{
                new KeyValuePair<string, ReportDiagnostic>("CS0162", ReportDiagnostic.Suppress) // unreachable code
            })
        );

        protected static void AssertMatch(string source, string matches)
        {
            using (var host = new HostEnvironment())
            {
                var expectedDocIds = ParseLines(matches);
                var assembly = CreateAssembly(host, source);
                var actualDocIds = GetResults(assembly).Select(r => r.docId);

                Assert.Equal(expectedDocIds, actualDocIds);
            }
        }

        protected static void AssertNoMatch(string source)
        {
            AssertMatch(source, string.Empty);
        }

        private static IAssembly CreateAssembly(HostEnvironment host, string text)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(text);
            var compilation = CompilationTemplate.AddSyntaxTrees(syntaxTree);

            using (var memoryStream = new MemoryStream())
            {
                var result = compilation.Emit(memoryStream);
                Assert.Empty(result.Diagnostics);

                memoryStream.Position = 0;

                return host.LoadAssemblyFrom(compilation.AssemblyName, memoryStream);
            }
        }

        private static IEnumerable<(string docId, PnsResult result)> GetResults(IAssembly assembly)
        {
            var results = new List<(string docId, PnsResult result)>();
            var handler = new DelegatedPnsReporter((r, m) =>
            {
                if (r.Throws)
                    results.Add((m.DocId(), r));
            });
            var scanner = new PnsScanner(handler);
            scanner.AnalyzeAssembly(assembly);
            return results;
        }

        private static IEnumerable<string> ParseLines(string text)
        {
            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length > 0)
                        yield return line.Trim();
                }
            }
        }

    }
}
