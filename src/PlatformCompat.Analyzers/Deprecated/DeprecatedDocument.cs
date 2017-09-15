using System.Collections.Immutable;
using PlatformCompat.Analyzers.Store;

namespace PlatformCompat.Analyzers.Deprecated
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
