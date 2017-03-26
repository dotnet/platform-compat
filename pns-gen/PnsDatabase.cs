using System.Collections.Generic;
using Microsoft.Cci;
using Microsoft.Cci.Extensions;
using Terrajobst.Cci;

namespace pns_gen
{
    internal sealed class PnsDatabase
    {
        private readonly SortedSet<string> _platforms = new SortedSet<string>();
        private readonly Dictionary<string, PnsEntry> _entries = new Dictionary<string, PnsEntry>();

        public void Add(ITypeDefinitionMember member, string platform)
        {
            _platforms.Add(platform);
            
            var docId = member.DocId();

            if (!_entries.TryGetValue(docId, out var entry))
            {
                var namespaceName = member.GetNamespaceName();
                var typeName = member.GetTypeName();
                var memberName = member.GetMemberSignature();

                entry = new PnsEntry(docId, namespaceName, typeName, memberName);
                _entries.Add(docId, entry);
            }

            entry.Platforms.Add(platform);
        }

        public ISet<string> Platforms => _platforms;

        public IEnumerable<PnsEntry> Entries => _entries.Values;
    }
}
