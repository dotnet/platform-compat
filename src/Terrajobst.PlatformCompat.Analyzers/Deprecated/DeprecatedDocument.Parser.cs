using System;
using System.Collections.Immutable;
using Terrajobst.PlatformCompat.Analyzers.Exceptions;

namespace Terrajobst.PlatformCompat.Analyzers.Deprecated
{
    internal static partial class DeprecatedDocument
    {
        private sealed class Parser : ApiStoreParser<ImmutableArray<string>>
        {
            protected override ImmutableArray<string> ParseData(ArraySegment<string> values)
            {
                if (values.Count != 1)
                    throw InvalidDocument();

                var value = values.Array[values.Offset];
                var ids = value.Split(';');
                return ids.ToImmutableArray();
            }
        }
    }
}
