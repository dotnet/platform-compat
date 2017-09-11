using System;
using Terrajobst.PlatformCompat.Analyzers.Exceptions;

namespace Terrajobst.PlatformCompat.Analyzers.Store
{
    internal static partial class ExceptionDocument
    {
        private class Parser : ApiStoreParser<Platform>
        {
            private Platform[] _platforms;

            protected override void Initialize(ArraySegment<string> headers)
            {
                _platforms = new Platform[headers.Count];

                for (var i = 0; i < headers.Count; i++)
                {
                    if (!TryParsePlatformName(headers.Array[headers.Offset + i], out _platforms[i]))
                        throw InvalidDocument();
                }
            }

            protected override Platform ParseData(ArraySegment<string> values)
            {
                var data = Platform.None;

                for (var i = 0; i < values.Count; i++)
                {
                    const string ThrowIndicator = "X";

                    var value = values.Array[values.Offset + i];
                    var isValid = value.Length == 0 || value == ThrowIndicator;
                    if (!isValid)
                        throw InvalidDocument();

                    var throws = value == ThrowIndicator;
                    var platform = _platforms[i];

                    if (throws)
                        data |= platform;
                }

                return data;
            }

            private static bool TryParsePlatformName(string text, out Platform platform)
            {
                switch (text.ToLowerInvariant())
                {
                    case "linux":
                        platform = Platform.Linux;
                        return true;
                    case "osx":
                        platform = Platform.MacOSX;
                        return true;
                    case "win":
                        platform = Platform.Windows;
                        return true;
                    default:
                        platform = default(Platform);
                        return false;
                }
            }
        }
    }
}
