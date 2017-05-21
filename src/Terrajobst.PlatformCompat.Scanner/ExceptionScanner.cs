using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;

namespace Terrajobst.PlatformCompat.Scanner
{
    public sealed class ExceptionScanner
    {
        private readonly IExceptionReporter _reporter;

        public ExceptionScanner(IExceptionReporter reporter)
        {
            _reporter = reporter;
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
            if (!item.IsVisibleOutsideAssembly())
                return;

            var info = ScanPlatformNotSupported(item);
            _reporter.Report(info, item);
        }

        private static ExceptionInfo ScanPlatformNotSupported(ITypeDefinitionMember item)
        {
            if (item is IMethodDefinition m)
            {
                if (m.IsPropertyOrEventAccessor())
                    return ExceptionInfo.DoesNotThrow;

                return ScanPlatformNotSupported(m);
            }
            else if (item is IPropertyDefinition p)
            {
                return ScanPlatformNotSupported(p.Accessors);
            }
            else if (item is IEventDefinition e)
            {
                return ScanPlatformNotSupported(e.Accessors);
            }
            else if (item is IFieldDefinition || item is ITypeDefinition)
            {
                // Ignore
                return ExceptionInfo.DoesNotThrow;
            }
            else
            {
                throw new NotImplementedException($"Unexpected type member: {item.FullName()} ({item.GetApiKind()})");
            }
        }

        private static ExceptionInfo ScanPlatformNotSupported(IEnumerable<IMethodReference> accessors)
        {
            return accessors.Select(a => ScanPlatformNotSupported(a.ResolvedMethod))
                            .Aggregate(ExceptionInfo.DoesNotThrow, (c, o) => c.Combine(o));
        }

        private static ExceptionInfo ScanPlatformNotSupported(IMethodDefinition method, int nestingLevel = 0)
        {
            const int maxNestingLevel = 3;

            if (method is Dummy || method.IsAbstract)
                return ExceptionInfo.DoesNotThrow;

            foreach (var op in GetOperationsPreceedingThrow(method))
            {
                // throw new PlatformNotSupportedExeption(...)
                if (op.OperationCode == OperationCode.Newobj &&
                    op.Value is IMethodReference m &&
                    IsPlatformNotSupported(m))
                {
                    return ExceptionInfo.ThrowsAt(nestingLevel, method.ToString());
                }

                // throw SomeFactoryForPlatformNotSupportedExeption(...);
                if (op.Value is IMethodReference r &&
                    IsFactoryForPlatformNotSupported(r))
                {
                    return ExceptionInfo.ThrowsAt(nestingLevel, method.ToString());
                }
            }

            var result = ExceptionInfo.DoesNotThrow;

            if (nestingLevel < maxNestingLevel)
            {
                foreach (var calledMethod in GetCalls(method))
                {
                    var nestedResult = ScanPlatformNotSupported(calledMethod.ResolvedMethod, nestingLevel + 1);
                    result = result.Combine(nestedResult);
                }
            }

            return result;
        }

        private static IEnumerable<IOperation> GetOperationsPreceedingThrow(IMethodDefinition method)
        {
            IOperation previous = null;

            foreach (var op in method.Body.Operations)
            {
                if (op.OperationCode == OperationCode.Nop)
                    continue;

                if (op.OperationCode == OperationCode.Throw && previous != null)
                    yield return previous;

                previous = op;
            }
        }

        private static IEnumerable<IMethodReference> GetCalls(IMethodDefinition method)
        {
            return method.Body.Operations.Where(o => o.OperationCode == OperationCode.Call ||
                                                     o.OperationCode == OperationCode.Callvirt)
                                         .Select(o => o.Value as IMethodReference)
                                         .Where(m => m != null);
        }

        private static bool IsPlatformNotSupported(IMethodReference constructorReference)
        {
            return constructorReference.ContainingType.FullName() == "System.PlatformNotSupportedException";
        }

        private static bool IsFactoryForPlatformNotSupported(IMethodReference reference)
        {
            if (reference.ResolvedMethod is Dummy || reference.ResolvedMethod.IsAbstract)
                return false;

            IMethodReference constructorReference = null;

            foreach (var op in reference.ResolvedMethod.Body.Operations)
            {
                switch (op.OperationCode)
                {
                    case OperationCode.Newobj:
                        constructorReference = op.Value as IMethodReference;
                        break;
                    case OperationCode.Ret:
                        if (constructorReference != null && IsPlatformNotSupported(constructorReference))
                            return true;
                        break;
                }
            }

            return false;
        }
    }
}
