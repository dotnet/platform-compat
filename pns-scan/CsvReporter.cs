using Microsoft.Cci;
using Microsoft.Cci.Extensions;
using Terrajobst.Csv;
using Terrajobst.PlatformNotSupported.Analysis;

namespace NotImplementedScanner
{
    internal sealed class CsvReporter : IPlatformNotSupportedReporter
    {
        private readonly CsvWriter _writer;

        public CsvReporter(CsvWriter writer)
        {
            _writer = writer;
            WriteHeader();
        }

        public void Report(ExceptionResult result, ITypeDefinitionMember member)
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

        private void WriteMember(ExceptionResult result, ITypeDefinitionMember item)
        {
            if (!result.Throws)
                return;

            _writer.Write(item.DocId());
            _writer.Write(item.ContainingTypeDefinition.GetNamespaceName());
            _writer.Write(item.ContainingTypeDefinition.GetTypeName(false));
            _writer.Write(GetMemberSignature(item));
            _writer.Write(result.Level.ToString());
            _writer.WriteLine();
        }

        private static string GetMemberSignature(ITypeDefinitionMember member)
        {
            if (member is IFieldDefinition)
                return member.Name.Value;

            var memberSignature = MemberHelper.GetMemberSignature(member, NameFormattingOptions.Signature |
                                                                          NameFormattingOptions.TypeParameters |
                                                                          NameFormattingOptions.ContractNullable |
                                                                          NameFormattingOptions.OmitContainingType |
                                                                          NameFormattingOptions.OmitContainingNamespace |
                                                                          NameFormattingOptions.PreserveSpecialNames);
            return memberSignature;
        }
    }
}
