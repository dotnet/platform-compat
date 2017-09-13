using System.Collections.Generic;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;
using ApiCompat.Cci;

namespace ex_gen
{
    internal sealed class Database
    {
        private readonly SortedSet<string> _platforms = new SortedSet<string>();
        private readonly Dictionary<string, DatabaseEntry> _entries = new Dictionary<string, DatabaseEntry>();

        public void Add(ITypeDefinitionMember member, string platform)
        {
            _platforms.Add(platform);
            
            var docId = member.DocId();

            if (!_entries.TryGetValue(docId, out var entry))
            {
                var namespaceName = member.GetNamespaceName();
                var typeName = member.GetTypeName();
                var memberName = member.GetMemberSignature();

                entry = new DatabaseEntry(docId, namespaceName, typeName, memberName);
                _entries.Add(docId, entry);
            }

            entry.Platforms.Add(platform);
        }

        public ISet<string> Platforms => _platforms;

        public IEnumerable<DatabaseEntry> Entries => _entries.Values;
    }
}
