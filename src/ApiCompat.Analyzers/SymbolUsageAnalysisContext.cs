using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Terrajobst.PlatformCompat.Analyzers
{
    public struct SymbolUsageAnalysisContext
    {
        public SymbolUsageAnalysisContext(SyntaxNodeAnalysisContext originalContext, ISymbol symbol)
        {
            OriginalContext = originalContext;
            Symbol = symbol;
        }

        private SyntaxNodeAnalysisContext OriginalContext { get; }

        public CancellationToken CancellationToken => OriginalContext.CancellationToken;

        public ISymbol Symbol { get ; }

        public Location GetLocation()
        {
            return OriginalContext.Node.GetLocation();
        }

        public void ReportDiagnostic(Diagnostic diagnostic)
        {
            OriginalContext.ReportDiagnostic(diagnostic);
        }
    }
}
