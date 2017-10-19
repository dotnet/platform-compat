using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.DotNet.Csv;
using Microsoft.DotNet.Analyzers.Compatibility.Store;

namespace Microsoft.DotNet.Analyzers.Compatibility.Deprecated
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DeprecatedAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Usage";
        private const string HelpLinkFormat = "https://github.com/dotnet/platform-compat/blob/master/docs/{0}.md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.DeprecatedAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.DeprecatedAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));

        private readonly ApiStore<ImmutableArray<string>> _store = DeprecatedDocument.Parse(Resources.Deprecated);

        public static ImmutableArray<DiagnosticDescriptor> GetDescriptors()
        {
            var diagnosticIds = new SortedSet<string>();

            using (var stringReader = new StringReader(Resources.Deprecated))
            {
                var reader = new CsvReader(stringReader);
                string[] line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length == 5)
                    {
                        var ids = line[4].Split(';');
                        diagnosticIds.UnionWith(ids);
                    }
                }
            }

            return diagnosticIds.Select(id => new DiagnosticDescriptor(id, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: string.Format(HelpLinkFormat, id)))
                                .ToImmutableArray();
        }

        public DeprecatedAnalyzer()
        {
            SupportedDiagnostics = GetDescriptors();
            DescriptorById = SupportedDiagnostics.ToImmutableDictionary(d => d.Id);
        }
    
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        public ImmutableDictionary<string, DiagnosticDescriptor> DescriptorById { get; }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolUsageAction(AnalyzeSymbol);
        }

        private void AnalyzeSymbol(SymbolUsageAnalysisContext context)
        {
            var symbol = context.Symbol;

            // We only want to handle a specific set of symbols
            var isApplicable = symbol.Kind == SymbolKind.Event ||
                               symbol.Kind == SymbolKind.Field ||
                               symbol.Kind == SymbolKind.Method ||
                               symbol.Kind == SymbolKind.NamedType ||
                               symbol.Kind == SymbolKind.Namespace ||
                               symbol.Kind == SymbolKind.Property;
            if (!isApplicable)
                return;

            if (!_store.TryLookup(symbol, out var entry))
                return;

            var api = symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            var location = context.GetLocation();

            foreach (var diagnosticId in entry.Data)
            {
                var descriptor = DescriptorById[diagnosticId];
                var diagnostic = Diagnostic.Create(descriptor, location, api);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
