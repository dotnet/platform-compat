using Microsoft.Cci;

namespace Terrajobst.PlatformCompat.Scanner
{
    public interface IExceptionReporter
    {
        void Report(ExceptionInfo info, ITypeDefinitionMember member);
    }
}
