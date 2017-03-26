using Microsoft.Cci;
using Microsoft.Cci.Extensions;
using Terrajobst.Cci;
using Terrajobst.Csv;
using Terrajobst.Pns.Scanner;

namespace NotImplementedScanner
{
    internal sealed class CsvReporter : IPnsReporter
    {
        private readonly CsvWriter _writer;

        public CsvReporter(CsvWriter writer)
        {
            _writer = writer;
            WriteHeader();
        }

        public void Report(PnsResult result, ITypeDefinitionMember member)
        {
            WriteMember(result, member);
        }

        private void WriteHeader()
        {
            _writer.Write("DocId");
            _writer.Write("Namespace");
            _writer.Write("Type");
            _writer.Write("Member");
            _writer.Write("Nesting");
            _writer.WriteLine();
        }

        private void WriteMember(PnsResult result, ITypeDefinitionMember member)
        {
            if (!result.Throws)
                return;

            _writer.Write(member.DocId());
            _writer.Write(member.GetNamespaceName());
            _writer.Write(member.GetTypeName());
            _writer.Write(member.GetMemberSignature());
            _writer.Write(result.Level.ToString());
            _writer.WriteLine();
        }
    }
}
