using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Terrajobst.PlatformCompat.Analyzers.Store
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
            var memberName = IsConstructor(symbol) ? ".ctor" : symbol.Name;
            var typeName = symbol.ContainingType.Name;
            var namespaceName = symbol.ContainingNamespace.Name;
            var key = (namespaceName, typeName, memberName);


            if (!_entries.TryGetValue(key, out var entries))
            {
                entry = default(ApiEntry<T>);
                return false;
            }

            var docId = symbol.GetDocumentationCommentId();
            return entries.TryGetValue(docId, out entry);
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
            var paren = signature.IndexOf('(');
            if (paren < 0)
                return signature;

            return signature.Substring(0, paren);
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
