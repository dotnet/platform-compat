using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;

namespace dep_helper
{
    internal sealed class DeprecationHelperScanner
    {
        private readonly DeprecationReporter _reporter;
        private readonly Regex _deprecatedTypeRegex;

        public DeprecationHelperScanner(DeprecationReporter reporter, Regex deprecatedTypeRegex)
        {
            _reporter = reporter;
            _deprecatedTypeRegex = deprecatedTypeRegex;
        }

        public void ScanAssembly(IAssembly assembly)
        {
            foreach (var type in assembly.GetAllTypes())
                ScanType(type);
        }

        private void ScanType(INamedTypeDefinition type)
        {
            if (!type.IsVisibleOutsideAssembly())
                return;

            foreach (var item in type.Members)
                ScanMember(item);
        }

        private void ScanMember(ITypeDefinitionMember item)
        {
            if (!item.IsVisibleOutsideAssembly() ||
                item is IPropertyDefinition ||
                item is IEventDefinition ||
                !_deprecatedTypeRegex.IsMatch(item.ContainingType.FullName()))
                return;

            if (IsDeprecated(item))
                _reporter.Report(item);
        }

        private bool IsDeprecated(ITypeDefinitionMember item)
        {
            if (item is IMethodDefinition m)
            {
                return CheckMethodForDeprecation(m);
            }
            else if (item is IFieldDefinition || item is ITypeDefinition)
            {
                // Ignore
                return false;
            }
            else
            {
                throw new NotImplementedException($"Unexpected type member: {item.FullName()} ({item.GetApiKind()})");
            }
        }

        private bool CheckMethodForDeprecation(IMethodDefinition method)
        {
            if (method is Dummy)
                return false;

            var shouldDeprecate = method.IsConstructor || 
                (method.IsStatic && _deprecatedTypeRegex.IsMatch(method.Type.FullName()));

            return shouldDeprecate;
        }
    }
}
