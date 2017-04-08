using System.Collections.Generic;

namespace ex_gen
{
    internal sealed class DatabaseEntry
    {
        public DatabaseEntry(string docId, string namespaceName, string typeName, string memberName)
        {
            DocId = docId;
            NamespaceName = namespaceName;
            TypeName = typeName;
            MemberName = memberName;
            Platforms = new HashSet<string>();
        }

        public string DocId { get; }
        public string NamespaceName { get; }
        public string TypeName { get; }
        public string MemberName { get; }
        public ISet<string> Platforms { get; }
    }
}
