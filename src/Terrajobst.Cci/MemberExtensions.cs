using Microsoft.Cci;
using Microsoft.Cci.Extensions;

namespace Terrajobst.Cci
{
    public static class MemberExtensions
    {
        public static string GetNamespaceName(this ITypeDefinitionMember member)
        {
            return member.ContainingTypeDefinition.GetNamespaceName();
        }

        public static string GetTypeName(this ITypeDefinitionMember member)
        {
            return member.ContainingTypeDefinition.GetTypeName(false);
        }

        public static string GetMemberSignature(this ITypeDefinitionMember member)
        {
            if (member is IFieldDefinition)
                return member.Name.Value;

            return MemberHelper.GetMemberSignature(member, NameFormattingOptions.Signature |
                                                           NameFormattingOptions.TypeParameters |
                                                           NameFormattingOptions.ContractNullable |
                                                           NameFormattingOptions.OmitContainingType |
                                                           NameFormattingOptions.OmitContainingNamespace |
                                                           NameFormattingOptions.PreserveSpecialNames);
        }
    }
}
