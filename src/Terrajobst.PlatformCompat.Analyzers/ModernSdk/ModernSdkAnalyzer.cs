using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;

namespace Terrajobst.PlatformCompat.Analyzers.ModernSdk
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ModernSdkAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PC003";
        private const string Category = "Usage";
        private const string HelpLink = "https://github.com/terrajobst/platform-compat/blob/master/docs/" + DiagnosticId + ".md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ModernSdkAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ModernSdkAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: HelpLink);

        private readonly Lazy<ModernSdkDocument> _modernSdk = new Lazy<ModernSdkDocument>(LoadDocument);

        private static ModernSdkDocument LoadDocument()
        {
            return ModernSdkDocument.Parse(Resources.ModernSdk);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(startContext =>
            {
                var settings = startContext.Options.GetFileSettings(PlatformCompatOptions.SettingsName);
                var options = new PlatformCompatOptions(settings);

                // We only want to run if the project is targeting .NET Standard or UWP.
                //var shouldRun = options.TargetFrameworkIsNetStandard() ||
                //                options.TargetFrameworkIsUwp();
                //if (!shouldRun)
                //    return;

                startContext.RegisterSymbolAction(
                    symbolContext => AnalyzeMethod(symbolContext, options),
                    SymbolKind.Method
                );
            });
        }

        private void AnalyzeMethod(SymbolAnalysisContext context, PlatformCompatOptions options)
        {
            var symbol = (IMethodSymbol) context.Symbol;
            var dllImportData = symbol.GetDllImportData();
            if (dllImportData == null)
                return;

            var entryPoint = dllImportData.EntryPointName ?? symbol.Name;
            var moduleName = dllImportData.ModuleName;
            var key = (entryPoint, moduleName);

            if (_modernSdk.Value.Contains(key))
                return;

            var location = symbol.Locations.First();
            var diagnostic = Diagnostic.Create(Rule, location, moduleName, entryPoint);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
