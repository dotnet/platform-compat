using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Terrajobst.PlatformCompat.Analyzers.Tests.Helpers
{
    public abstract partial class DiagnosticTest
    {
        protected abstract DiagnosticAnalyzer CreateAnalyzer();

        protected virtual (string name, string defaultSettings) GetAdditionalFileSettings() => (null, null);

        protected abstract string GetLanguage();

        protected void AssertNoMatch(string annotatedSource, string settings = null)
        {
            AssertMatch(annotatedSource, "", settings);
        }

        protected void AssertMatch(string annotatedSource, string expectedDiagnosticsText, string settings = null)
        {
            var annotatedText = AnnotatedText.Parse(annotatedSource);
            var source = annotatedText.Text;
            var expectedSpans = annotatedText.Spans;
            var expectedDiagnostics = ParseDiagnostics(expectedDiagnosticsText).ToImmutableArray();

            if (expectedDiagnostics.Length != expectedSpans.Length)
                throw new ArgumentException($"{nameof(expectedDiagnosticsText)} must match the number of marked spans.", nameof(expectedDiagnosticsText));

            var analyzer = CreateAnalyzer();
            var actualDiagnostics = ComputeDiagnostics(analyzer, source, settings);

            Assert.Equal(expectedDiagnostics.Length, actualDiagnostics.Length);

            for (int i = 0; i < expectedSpans.Length; i++)
            {
                var expected = expectedDiagnostics[i];
                var expectedSpan = expectedSpans[i];
                var actual = actualDiagnostics[i];

                Assert.Equal(expected.id, actual.Id);
                Assert.Equal(expected.text, actual.GetMessage());
                Assert.Equal(expectedSpan, actual.Location.SourceSpan);
            }
        }

        private static IEnumerable<(string id, string text)> ParseDiagnostics(string text)
        {
            using (var stringReader = new StringReader(text))
            {
                string line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0)
                        continue;

                    var colon = line.IndexOf(':');
                    if (colon < 0)
                        throw new ArgumentException($"Missing diangostic ID in line: {line}", nameof(text));

                    var id = line.Substring(0, colon).Trim();
                    var message = line.Substring(colon + 1).Trim();
                    yield return (id, message);
                }
            }
        }

        private ImmutableArray<Diagnostic> ComputeDiagnostics(DiagnosticAnalyzer analyzer, string source, string settings)
        {
            var language = GetLanguage();
            var project = AnalyzedProject.Create(analyzer, language, source)
                                         .WithMetadataReferences(MetadataReferenceSet.NetFramework);

            var (settingsName, defaultSettings) = GetAdditionalFileSettings();
            if (settingsName != null)
            {
                var effectiveSettings = defaultSettings + Environment.NewLine + settings;
                project = project.AddAdditionalDocument(settingsName, effectiveSettings);

                var referenceSet = InferReferenceSet(project.Project, settingsName);
                project = project.WithMetadataReferences(referenceSet);
            }

            var document = project.Documents.Single();
            return document.Diagnostics;
        }

        private static IEnumerable<MetadataReference> InferReferenceSet(Project project, string settingsName)
        {
            var settings = project.AnalyzerOptions.GetFileSettings(settingsName);
            var options = new PlatformCompatOptions(settings);
            return options.TargetFrameworkIsNetStandard()
                        ? MetadataReferenceSet.NetStandard
                        : MetadataReferenceSet.NetFramework;
        }
    }
}
