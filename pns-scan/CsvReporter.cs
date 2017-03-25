using System.IO;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;
using Terrajobst.PlatformNotSupported.Analysis;

namespace NotImplementedScanner
{
    internal sealed class CsvReporter : IPlatformNotSupportedReporter
    {
        private TextWriter _textWriter;

        public CsvReporter(TextWriter textWriter)
        {
            _textWriter = textWriter;
            WriteHeader();
        }

        public void Report(ExceptionResult result, ITypeDefinitionMember member)
        {
            WriteMember(result, member);
        }

        private void WriteHeader()
        {
            _textWriter.Write("DocId");
            _textWriter.Write(",");
            _textWriter.Write("Namespace");
            _textWriter.Write(",");
            _textWriter.Write("Type");
            _textWriter.Write(",");
            _textWriter.Write("Member");
            _textWriter.Write(",");
            _textWriter.Write("Nesting");
            _textWriter.WriteLine();
        }

        private void WriteMember(ExceptionResult result, ITypeDefinitionMember item)
        {
            if (!result.Throws)
                return;

            _textWriter.WriteEscaped(item.DocId());
            _textWriter.Write(",");
            _textWriter.WriteEscaped(item.ContainingTypeDefinition.GetNamespaceName());
            _textWriter.Write(",");
            _textWriter.WriteEscaped(item.ContainingTypeDefinition.GetTypeName(false));
            _textWriter.Write(",");
            _textWriter.WriteEscaped(GetMemberSignature(item));
            _textWriter.Write(",");
            _textWriter.Write(result);
            _textWriter.WriteLine();
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
