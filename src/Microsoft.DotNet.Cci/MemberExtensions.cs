using Microsoft.Cci;
using Microsoft.Cci.Extensions;

namespace Microsoft.DotNet.Cci
{
    public static class MemberExtensions
    {
        public static string GetNamespaceName(this IDefinition definition)
        {
            if (definition is ITypeDefinitionMember member)
                return member.ContainingTypeDefinition.GetNamespaceName();

            if (definition is ITypeDefinition type)
                return type.GetNamespace().GetNamespaceName();

            if (definition is INamespaceDefinition nsp)
                return nsp.FullName();

            return string.Empty;
        }

        public static string GetTypeName(this IDefinition definition)
        {
            if (definition is ITypeDefinitionMember member)
                return member.ContainingTypeDefinition.GetTypeName(false);

            if (definition is ITypeDefinition type)
                return type.GetTypeName(includeNamespace: false);

            return string.Empty;
        }

        public static string GetMemberSignature(this IDefinition definition)
        {
            if (definition is IFieldDefinition field)
                return field.Name.Value;

            if (definition is ITypeDefinitionMember member)
                return MemberHelper.GetMemberSignature(member, NameFormattingOptions.Signature |
                                                               NameFormattingOptions.TypeParameters |
                                                               NameFormattingOptions.ContractNullable |
                                                               NameFormattingOptions.OmitContainingType |
                                                               NameFormattingOptions.OmitContainingNamespace |
                                                               NameFormattingOptions.PreserveSpecialNames);

            return string.Empty;
        }
    }
}
