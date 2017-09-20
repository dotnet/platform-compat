using System.Collections.Generic;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;
using PlatformCompat.Cci;

namespace ex_gen
{
    internal sealed class Database
    {
        private readonly SortedSet<string> _platforms = new SortedSet<string>();
        private readonly Dictionary<string, DatabaseEntry> _entries = new Dictionary<string, DatabaseEntry>();
        private readonly Database _exclusionDatabase;

        public Database(Database exclusionDatabase = null)
        {
            _exclusionDatabase = exclusionDatabase;
        }

        public void Add(ITypeDefinitionMember member, string platform)
        {
            var docId = member.DocId();

            if (_exclusionDatabase != null &&
                _exclusionDatabase._entries.TryGetValue(docId, out var _exclusionEntry) &&
                _exclusionEntry.Platforms.Contains(platform))
            {
                return;
            }

            var namespaceName = member.GetNamespaceName();
            var typeName = member.GetTypeName();
            var memberName = member.GetMemberSignature();
            Add(docId, namespaceName, typeName, memberName, platform);
        }

        public void Add(string docId, string namespaceName, string typeName, string memberName, string platform)
        {
            _platforms.Add(platform);

            if (!_entries.TryGetValue(docId, out var entry))
            {
                entry = new DatabaseEntry(docId, namespaceName, typeName, memberName);
                _entries.Add(docId, entry);
            }

            entry.Platforms.Add(platform);
        }

        public ISet<string> Platforms => _platforms;

        public IEnumerable<DatabaseEntry> Entries => _entries.Values;
    }
}
