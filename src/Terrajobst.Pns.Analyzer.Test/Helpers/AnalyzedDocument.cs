using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Terrajobst.Pns.Analyzer.Test.Helpers
{
    public sealed class AnalyzedDocument
    {
        public AnalyzedDocument(Document document, ImmutableArray<Diagnostic> diagnostics)
        {
            Document = document;
            Diagnostics = diagnostics;
        }

        public Document Document { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
    }
}
