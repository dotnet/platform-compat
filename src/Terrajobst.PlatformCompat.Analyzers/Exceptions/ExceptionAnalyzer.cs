using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Terrajobst.PlatformCompat.Analyzers.Store;

namespace Terrajobst.PlatformCompat.Analyzers.Exceptions
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExceptionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PC001";
        private const string Category = "Usage";
        private const string HelpLink = "https://github.com/terrajobst/platform-compat/blob/master/docs/" + DiagnosticId + ".md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ExceptionAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ExceptionAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: HelpLink);

        private readonly Lazy<ApiStore<Platform>> _store = new Lazy<ApiStore<Platform>>(LoadStore);

        private static ApiStore<Platform> LoadStore()
        {
            return ExceptionDocument.Parse(Resources.Exceptions);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(startContext =>
            {
                var settings = startContext.Options.GetFileSettings(PlatformCompatOptions.SettingsName);
                var options = new PlatformCompatOptions(settings);

                // We only want to run if the project is targeting .NET Core or .NET Standard.
                var shouldRun = options.TargetFrameworkIsNetCore() || options.TargetFrameworkIsNetStandard();
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
            var isApplicable = symbol.Kind == SymbolKind.Method ||
                               symbol.Kind == SymbolKind.Property ||
                               symbol.Kind == SymbolKind.Event;
            if (!isApplicable)
                return;

            if (!_store.Value.TryLookup(symbol, out var entry))
                return;

            // Check that the affected platforms aren't suppressed
            var maskedPlatforms = entry.Data & ~options.IgnoredPlatforms;
            if (maskedPlatforms == Platform.None)
                return;

            var api = symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            var location = context.GetLocation();
            var list = maskedPlatforms.ToString();
            var diagnostic = Diagnostic.Create(Rule, location, api, list);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
