using System;
using ApiCompat.Analyzers.Exceptions;

namespace ApiCompat.Analyzers.Net461
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
