using Microsoft.Cci;

namespace Terrajobst.PlatformNotSupported.Analysis
{
    public interface IPlatformNotSupportedReporter
    {
        void Report(ExceptionResult result, ITypeDefinitionMember member);
    }
}
