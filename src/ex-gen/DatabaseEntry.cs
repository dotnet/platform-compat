using System.Collections.Generic;

namespace ex_gen
{
    internal sealed class DatabaseEntry
    {
        public DatabaseEntry(string docId, string namespaceName, string typeName, string memberName, string site)
        {
            DocId = docId;
            NamespaceName = namespaceName;
            TypeName = typeName;
            MemberName = memberName;
            Site = site;
            Platforms = new HashSet<string>();
        }

        public string DocId { get; }
        public string NamespaceName { get; }
        public string TypeName { get; }
        public string MemberName { get; }
        public string Site { get; }
        public ISet<string> Platforms { get; }
    }
}
