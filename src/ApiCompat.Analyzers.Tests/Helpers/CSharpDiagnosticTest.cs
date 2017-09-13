using Microsoft.CodeAnalysis;

namespace ApiCompat.Analyzers.Tests.Helpers
{
    public abstract partial class CSharpDiagnosticTest : DiagnosticTest
    {
        protected override string GetLanguage()
        {
            return LanguageNames.CSharp;
        }
    }
}
