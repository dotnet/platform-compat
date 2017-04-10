using System;
using Terrajobst.PlatformCompat.Analyzers.Exceptions;

namespace Terrajobst.PlatformCompat.Analyzers.Net461
{
    internal static partial class Net461Document
    {
        private sealed class Parser : ApiStoreParser<string>
        {
            protected override void Initialize(ArraySegment<string> headers)
            {
            }

            protected override string ParseData(ArraySegment<string> values)
            {
                if (values.Count > 0)
                    throw InvalidDocument();

                return string.Empty;
            }
        }
    }
}
