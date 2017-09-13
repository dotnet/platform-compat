using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace ApiCompat.Scanner.Tests.Helpers
{
    public abstract partial class PlatformCompatTests
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
                var assembly = CreateAssembly(host, source);

                var expectedResults = ParseDocAndLevelLines(matches).OrderBy(t => t.docId).ToArray();
                var actualResults = GetResults(assembly).OrderBy(r => r.docId).ToArray();
                Assert.Equal(expectedResults.Length, actualResults.Length);

                for (int i = 0; i < expectedResults.Length; i++)
                {
                    var expectedResult = expectedResults[i];
                    var actualResult = actualResults[i];

                    Assert.Equal(expectedResult.docId, actualResult.docId);

                    if (expectedResult.level != null)
                        Assert.Equal(expectedResult.level.Value, actualResult.result.Level);
                }
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
                Assert.Equal(Enumerable.Empty<Diagnostic>(), result.Diagnostics);

                memoryStream.Position = 0;

                return host.LoadAssemblyFrom(compilation.AssemblyName, memoryStream);
            }
        }

        private static IEnumerable<(string docId, ExceptionInfo result)> GetResults(IAssembly assembly)
        {
            var results = new List<(string docId, ExceptionInfo result)>();
            var reporter = new DelegatedExceptionReporter((r, m) =>
            {
                if (r.Throws)
                    results.Add((m.DocId(), r));
            });
            var scanner = new ExceptionScanner(reporter);
            scanner.ScanAssembly(assembly);
            return results;
        }

        private static IEnumerable<(string docId, int? level)> ParseDocAndLevelLines(string text)
        {
            return ParseLines(text).Select(ParseDocIdAndLevel);
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

        private static (string docId, int? level) ParseDocIdAndLevel(string text)
        {
            var indexOfAt = text.IndexOf('@');
            if (indexOfAt < 0)
                return (text, null);

            var docId = text.Substring(0, indexOfAt - 1).Trim();
            var levelText = text.Substring(indexOfAt + 1).Trim();
            var level = int.Parse(levelText);
            return (docId, level);
        }
    }
}
