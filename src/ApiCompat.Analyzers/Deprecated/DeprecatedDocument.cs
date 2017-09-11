using System.Collections.Immutable;
using Terrajobst.PlatformCompat.Analyzers.Store;

namespace Terrajobst.PlatformCompat.Analyzers.Deprecated
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
