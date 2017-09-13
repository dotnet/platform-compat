using Microsoft.Cci;

namespace ApiCompat.Scanner.Tests.Helpers
{
    partial class PlatformCompatTests
    {
        private sealed class DelegatedExceptionReporter : IExceptionReporter
        {
            private readonly System.Action<ExceptionInfo, ITypeDefinitionMember> _handler;

            public DelegatedExceptionReporter(System.Action<ExceptionInfo, ITypeDefinitionMember> handler)
            {
                _handler = handler;
            }

            public void Report(ExceptionInfo result, ITypeDefinitionMember member)
            {
                _handler(result, member);
            }
        }
    }
}
