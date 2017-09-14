using System;
using PlatformCompat.Analyzers.Exceptions;

namespace PlatformCompat.Analyzers.Net461
{
    internal static partial class Net461Document
    {
        private sealed class Parser : ApiStoreParser<string>
        {
            protected override string ParseData(ArraySegment<string> values)
            {
                if (values.Count > 0)
                    throw InvalidDocument();

                return string.Empty;
            }
        }
    }
}
