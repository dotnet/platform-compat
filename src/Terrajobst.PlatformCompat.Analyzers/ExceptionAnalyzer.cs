using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Terrajobst.PlatformCompat.Analyzers.Store;
using Platform = Terrajobst.PlatformCompat.Analyzers.Store.Platform;

namespace Terrajobst.PlatformCompat.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExceptionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PC001";
        private const string Category = "Usage";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        private readonly Lazy<ApiStore<Platform>> _exceptionStore = new Lazy<ApiStore<Platform>>(LoadStore);

        private static ApiStore<Platform> LoadStore()
        {
            return ExceptionStore.Parse(Resources.Exceptions);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(startContext =>
            {
                var settings = startContext.Options.GetFileSettings(PlatformCompatOptions.SettingsName);
                var options = new PlatformCompatOptions(settings);

                // We only want to run if the project is targeting .NET Core or .NET Standard.
                var targetingNetCore = options.TargetFramework.StartsWith("netcoreapp", StringComparison.OrdinalIgnoreCase);
                var targetingNetStandard = options.TargetFramework.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase);
                var shouldRun = targetingNetCore || targetingNetStandard;
                if (!shouldRun)
                    return;

                startContext.RegisterSyntaxNodeAction(
                    nodeContext => AnalyzeSyntaxNode(nodeContext, options),
                    SyntaxKind.IdentifierName,
                    SyntaxKind.ObjectCreationExpression,

                    // These are the list of operators that can result in
                    // custom operators:

                    SyntaxKind.AddExpression,
                    SyntaxKind.SubtractExpression,
                    SyntaxKind.MultiplyExpression,
                    SyntaxKind.DivideExpression,
                    SyntaxKind.ModuloExpression,
                    SyntaxKind.LeftShiftExpression,
                    SyntaxKind.RightShiftExpression,
                    SyntaxKind.LogicalOrExpression,
                    SyntaxKind.LogicalAndExpression,
                    SyntaxKind.BitwiseOrExpression,
                    SyntaxKind.BitwiseAndExpression,
                    SyntaxKind.ExclusiveOrExpression,
                    SyntaxKind.EqualsExpression,
                    SyntaxKind.NotEqualsExpression,
                    SyntaxKind.LessThanExpression,
                    SyntaxKind.LessThanOrEqualExpression,
                    SyntaxKind.GreaterThanExpression,
                    SyntaxKind.GreaterThanOrEqualExpression,

                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxKind.AddAssignmentExpression,
                    SyntaxKind.SubtractAssignmentExpression,
                    SyntaxKind.MultiplyAssignmentExpression,
                    SyntaxKind.DivideAssignmentExpression,
                    SyntaxKind.ModuloAssignmentExpression,
                    SyntaxKind.AndAssignmentExpression,
                    SyntaxKind.ExclusiveOrAssignmentExpression,
                    SyntaxKind.OrAssignmentExpression,
                    SyntaxKind.LeftShiftAssignmentExpression,
                    SyntaxKind.RightShiftAssignmentExpression,

                    SyntaxKind.UnaryPlusExpression,
                    SyntaxKind.UnaryMinusExpression,
                    SyntaxKind.BitwiseNotExpression,
                    SyntaxKind.LogicalNotExpression,
                    SyntaxKind.PreIncrementExpression,
                    SyntaxKind.PreDecrementExpression,
                    SyntaxKind.PostIncrementExpression,
                    SyntaxKind.PostDecrementExpression
                );
            });
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context, PlatformCompatOptions options)
        {
            switch (context.Node.Kind())
            {
                case SyntaxKind.IdentifierName:
                    AnalyzeExpression(context, options, (ExpressionSyntax)context.Node);
                    break;
                case SyntaxKind.ObjectCreationExpression:
                    AnalyzeExpression(context, options, (ExpressionSyntax)context.Node);
                    break;
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubtractExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.ModuloExpression:
                case SyntaxKind.LeftShiftExpression:
                case SyntaxKind.RightShiftExpression:
                case SyntaxKind.LogicalOrExpression:
                case SyntaxKind.LogicalAndExpression:
                case SyntaxKind.BitwiseOrExpression:
                case SyntaxKind.BitwiseAndExpression:
                case SyntaxKind.ExclusiveOrExpression:
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                case SyntaxKind.SimpleAssignmentExpression:
                case SyntaxKind.AddAssignmentExpression:
                case SyntaxKind.SubtractAssignmentExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                case SyntaxKind.DivideAssignmentExpression:
                case SyntaxKind.ModuloAssignmentExpression:
                case SyntaxKind.AndAssignmentExpression:
                case SyntaxKind.ExclusiveOrAssignmentExpression:
                case SyntaxKind.OrAssignmentExpression:
                case SyntaxKind.LeftShiftAssignmentExpression:
                case SyntaxKind.RightShiftAssignmentExpression:
                case SyntaxKind.UnaryPlusExpression:
                case SyntaxKind.UnaryMinusExpression:
                case SyntaxKind.BitwiseNotExpression:
                case SyntaxKind.LogicalNotExpression:
                case SyntaxKind.PreIncrementExpression:
                case SyntaxKind.PreDecrementExpression:
                case SyntaxKind.PostIncrementExpression:
                case SyntaxKind.PostDecrementExpression:
                    AnalyzeExpression(context, options, (ExpressionSyntax)context.Node);
                    break;
                default:
                    throw new NotImplementedException($"Unexpected node. Kind = {context.Node.Kind()}");
            }
        }

        private void AnalyzeExpression(SyntaxNodeAnalysisContext context, PlatformCompatOptions options, ExpressionSyntax node)
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(node);
            var symbol = symbolInfo.Symbol;

            // No point in checking unresolved symbols.
            if (symbol == null)
                return;

            // We only want to handle a specific set of symbols
            var isApplicable = symbol.Kind == SymbolKind.Method ||
                               symbol.Kind == SymbolKind.Property ||
                               symbol.Kind == SymbolKind.Event;
            if (!isApplicable)
                return;

            // We don't want to check symbols that aren't best matches.
            if (symbolInfo.CandidateReason != CandidateReason.None)
                return;

            // We don't want to check symbols defined in source.
            if (symbol.DeclaringSyntaxReferences.Any())
                return;

            if (!_exceptionStore.Value.TryLookup(symbol, out var entry))
                return;

            // Check that the affected platforms aren't suppressed
            var maskedPlatforms = entry.Data & ~options.IgnoredPlatforms;
            if (maskedPlatforms == Platform.None)
                return;

            var api = symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            var location = node.GetLocation();
            var list = maskedPlatforms.ToString();
            var diagnostic = Diagnostic.Create(Rule, location, api, list);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
