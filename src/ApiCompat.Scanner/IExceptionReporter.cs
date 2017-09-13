using Microsoft.Cci;

namespace ApiCompat.Scanner
{
    public interface IExceptionReporter
    {
        void Report(ExceptionInfo info, ITypeDefinitionMember member);
    }
}
