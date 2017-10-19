using System.Collections.Immutable;
using Microsoft.DotNet.Analyzers.Compatibility.Store;

namespace Microsoft.DotNet.Analyzers.Compatibility.Deprecated
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
