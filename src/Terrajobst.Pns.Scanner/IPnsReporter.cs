using Microsoft.Cci;

namespace Terrajobst.Pns.Scanner
{
    public interface IPnsReporter
    {
        void Report(PnsResult result, ITypeDefinitionMember member);
    }
}
