using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ApiCompat.Analyzers.Store;

namespace ApiCompat.Analyzers.Net461
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Net461Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PC002";
        private const string Category = "Usage";
        private const string HelpLink = "https://github.com/ApiCompat/platform-compat/blob/master/docs/" + DiagnosticId + ".md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.Net461AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.Net461AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: HelpLink);

        private readonly Lazy<ApiStore<string>> _store = new Lazy<ApiStore<string>>(LoadStore);

        private static ApiStore<string> LoadStore()
        {
            return Net461Document.Parse(Resources.Net461);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(startContext =>
            {
                var settings = startContext.Options.GetFileSettings(PlatformCompatOptions.SettingsName);
                var options = new PlatformCompatOptions(settings);

                // We only want to run if the project is targeting .NET Standard.
                var shouldRun = options.TargetFrameworkIsNetStandard();
                if (!shouldRun)
                    return;

                startContext.RegisterSymbolUsageAction(
                    usageContext => AnalyzeSymbol(usageContext, options)
                );
            });
        }

        private void AnalyzeSymbol(SymbolUsageAnalysisContext context, PlatformCompatOptions options)
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

            if (!_store.Value.TryLookup(symbol, out var entry))
                return;

            var api = symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            var location = context.GetLocation();
            var diagnostic = Diagnostic.Create(Rule, location, api);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
