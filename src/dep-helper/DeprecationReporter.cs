using System.IO;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;

namespace dep_helper
{
    internal sealed class DeprecationReporter
    {
        private readonly TextWriter _writer;

        public DeprecationReporter(TextWriter writer)
        {
            _writer = writer;
        }

        public void Report(ITypeDefinitionMember member)
        {
            _writer.WriteLine(member.DocId());
        }
    }
}
