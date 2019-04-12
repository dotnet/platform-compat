using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.DotNet.Analyzers.Compatibility.BeginInvokeEndInvoke
{
    // Looks for use of BeginInvoke or EndInvoke methods on types derived from System.Delegate
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BeginInvokeEndInvokeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PC004";
        private const string Category = "Usage";
        private const string HelpLink = "https://github.com/dotnet/platform-compat/blob/master/docs/" + DiagnosticId + ".md";

        private const string TargetTypeFullName = "System.Delegate";
        private readonly string[] MethodNames = { "BeginInvoke", "EndInvoke" };

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.BeginInvokeEndInvokeAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.BeginInvokeEndInvokeAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, helpLinkUri: HelpLink);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            // Bail out if the syntax doesn't include a member access expression for some reason
            if (!(context.Node is InvocationExpressionSyntax invocationExpression &&
                invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression))
                return;

            // Bail out if the member access expression doesn't correspond to a method symbol
            var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccessExpression);
            if (!(symbolInfo.Symbol is IMethodSymbol methodSymbol))
            {
                // In some cases, the member access expression's symbol won't be definitively known
                // (if, for example, the argument passed to EndInvoke isn't definitely known yet), so
                // check candidate symbols and use that if there's only one candidate method there.
                methodSymbol = symbolInfo.CandidateSymbols.SingleOrDefault(s => s is IMethodSymbol) as IMethodSymbol;
                if (methodSymbol == null)
                    return;
            }                

            // Only analyze methods with an appropriate name
            if (!MethodNames.Contains(methodSymbol.Name))
                return;

            // Only analyze methods on Delegate types
            if (!TypeIsDescendentOf(methodSymbol.ContainingType, typeof(Delegate)))
                return;

            context.ReportDiagnostic(Diagnostic.Create(Rule, invocationExpression.GetLocation(), methodSymbol.Name));
        }

        private bool TypeIsDescendentOf(INamedTypeSymbol containingType, Type targetType)
        {
            // If the type has no base, return false
            if (containingType.BaseType == null)
            {
                return false;
            }

            // If the type's parent is the targetType, return true
            if (TargetTypeFullName.Equals(GetFullName(containingType.BaseType)))
            {
                return true;
            }

            // Otherwise, recurse and check the parent's parent
            return TypeIsDescendentOf(containingType.BaseType, targetType);
        }

        private string GetFullName(INamedTypeSymbol type) =>
            $"{type.ContainingNamespace.Name}.{type.Name}";
    }
}