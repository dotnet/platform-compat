using Microsoft.Cci;

namespace Terrajobst.Pns.Scanner.Tests.Helpers
{
    partial class PnsTests
    {
        private sealed class DelegatedPnsReporter : IPnsReporter
        {
            private readonly System.Action<PnsResult, ITypeDefinitionMember> _handler;

            public DelegatedPnsReporter(System.Action<PnsResult, ITypeDefinitionMember> handler)
            {
                _handler = handler;
            }

            public void Report(PnsResult result, ITypeDefinitionMember member)
            {
                _handler(result, member);
            }
        }
    }
}
