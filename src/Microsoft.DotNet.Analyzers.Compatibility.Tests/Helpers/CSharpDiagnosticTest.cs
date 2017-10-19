using Microsoft.CodeAnalysis;

namespace Microsoft.DotNet.Analyzers.Compatibility.Tests.Helpers
{
    public abstract partial class CSharpDiagnosticTest : DiagnosticTest
    {
        protected override string GetLanguage()
        {
            return LanguageNames.CSharp;
        }
    }
}
