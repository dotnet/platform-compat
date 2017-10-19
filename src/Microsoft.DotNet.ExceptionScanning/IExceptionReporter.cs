using Microsoft.Cci;

namespace Microsoft.DotNet.Scanner
{
    public interface IExceptionReporter
    {
        void Report(ExceptionInfo info, ITypeDefinitionMember member);
    }
}
