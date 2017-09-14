using Microsoft.Cci;

namespace PlatformCompat.Scanner
{
    public interface IExceptionReporter
    {
        void Report(ExceptionInfo info, ITypeDefinitionMember member);
    }
}
