using Microsoft.Cci;

namespace NotImplementedScanner
{
    internal interface IPlatformNotSupportedReporter
    {
        void Report(ExceptionResult result, ITypeDefinitionMember member);
    }
}
