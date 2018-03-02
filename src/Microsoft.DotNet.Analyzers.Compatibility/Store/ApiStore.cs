using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Microsoft.DotNet.Analyzers.Compatibility.Store
{
    internal sealed class ApiStore<T>
    {
        private readonly Dictionary<(string namespaceName, string typeName, string memberName), Dictionary<string, ApiEntry<T>>> _entries;

        private ApiStore(Dictionary<(string namespaceName, string typeName, string memberName), Dictionary<string, ApiEntry<T>>> entries)
        {
            _entries = entries;
        }

        public static ApiStore<T> Create(IEnumerable<(string docId, string namespaceName, string typeName, string signature, T data)> apis)
        {
            var globalEntries = new Dictionary<(string memberName, string typeName, string namespaceName), Dictionary<string, ApiEntry<T>>>();
                
            foreach (var api in apis)
            {
                var namespaceName = GetLastName(api.namespaceName);
                var typeName = api.typeName;
                var memberName = GetName(api.signature);
                var key = (namespaceName, typeName, memberName);

                if (!globalEntries.TryGetValue(key, out var localEntries))
                {
                    localEntries = new Dictionary<string, ApiEntry<T>>();
                    globalEntries.Add(key, localEntries);
                }

                var docId = api.docId;
                var data = api.data;
                var entry = new ApiEntry<T>(docId, data);
                localEntries.Add(entry.DocId, entry);
            }

            return new ApiStore<T>(globalEntries);
        }

        public bool TryLookup(ISymbol symbol, out ApiEntry<T> entry)
        {
            var key = GetKey(symbol);

            if (!_entries.TryGetValue(key, out var entries))
            {
                entry = default(ApiEntry<T>);
                return false;
            }

            var docId = symbol.GetDocumentationCommentId();
            return entries.TryGetValue(docId, out entry);
        }

        private static (string namespaceName, string typeName, string memberName) GetKey(ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Namespace:
                    return (symbol.Name, string.Empty, string.Empty);

                case SymbolKind.NamedType:
                    return (symbol.ContainingNamespace.Name, symbol.Name, string.Empty);

                case SymbolKind.Event:
                case SymbolKind.Field:
                case SymbolKind.Method:
                case SymbolKind.Property:
                    var memberName = IsConstructor(symbol) ? ".ctor" : symbol.Name;
                    // Certain symbols, e.g.: pointer op_Increment, do not have containing type or namespace, send those as null. 
                    return (symbol.ContainingNamespace?.Name, symbol.ContainingType?.Name, memberName);

                default:
                    return (null, null, null);
            }
        }

        private static bool IsConstructor(ISymbol symbol)
        {
            if (symbol.Kind != SymbolKind.Method)
                return false;

            var method = (IMethodSymbol)symbol;
            return method.MethodKind == MethodKind.Constructor;
        }

        private static string GetName(string signature)
        {
            var bracket = signature.IndexOf('<');
            if (bracket >= 0)
                signature = signature.Substring(0, bracket);

            var paren = signature.IndexOf('(');
            if (paren >= 0)
                signature = signature.Substring(0, paren);

            return signature;
        }

        private static string GetLastName(string dottedName)
        {
            var lastDot = dottedName.LastIndexOf('.');
            if (lastDot < 0)
                return dottedName;

            return dottedName.Substring(lastDot + 1);
        }
    }
}
