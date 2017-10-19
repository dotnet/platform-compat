using System;
using Microsoft.DotNet.Analyzers.Compatibility.Exceptions;

namespace Microsoft.DotNet.Analyzers.Compatibility.Net461
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
