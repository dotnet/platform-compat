using Microsoft.CodeAnalysis;

namespace Terrajobst.Pns.Analyzer.Test.Helpers
{
    public abstract partial class CSharpDiagnosticTest : DiagnosticTest
    {
        protected override string GetLanguage()
        {
            return LanguageNames.CSharp;
        }
    }
}
