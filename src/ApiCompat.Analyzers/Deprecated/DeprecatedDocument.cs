using System.Collections.Immutable;
using ApiCompat.Analyzers.Store;

namespace ApiCompat.Analyzers.Deprecated
{
    internal static partial class DeprecatedDocument
    {
        public static ApiStore<ImmutableArray<string>> Parse(string data)
        {
            var parser = new Parser();
            return parser.Parse(data);
        }
    }
}
