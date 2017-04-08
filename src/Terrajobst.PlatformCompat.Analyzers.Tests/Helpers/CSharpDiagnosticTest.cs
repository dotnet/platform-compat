using Microsoft.CodeAnalysis;

namespace Terrajobst.PlatformCompat.Analyzers.Tests.Helpers
{
    public abstract partial class CSharpDiagnosticTest : DiagnosticTest
    {
        protected override string GetLanguage()
        {
            return LanguageNames.CSharp;
        }
    }
}
