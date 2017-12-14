using System;

namespace Microsoft.DotNet.Analyzers.Compatibility
{
    [Flags]
    public enum Platform
    {
        None = 0,
        Linux = 1,
        MacOS = 2,
        Windows = 4
    }

    public static class PlatformEnumHelpers
    {
        private const string Linux = "Linux";
        private const string MacOS = "macOS";
        private const string Windows = "Windows";

        private static readonly string[] s_mapPlatformsToFriendlyString = new[]
            {
                String.Empty,
                Linux,
                MacOS,
                $"{Linux}, {MacOS}",
                $"{Windows}",
                $"{Linux}, {Windows}",
                $"{MacOS}, {Windows}",
                $"{Linux}, {MacOS}, {Windows}"
            };

        public static string ToFriendlyString(this Platform platform)
        {
            var friendlyStringIndex = (int)platform;
            if (friendlyStringIndex < 0 && friendlyStringIndex >= s_mapPlatformsToFriendlyString.Length)
                throw new ArgumentOutOfRangeException(nameof(platform));

            return s_mapPlatformsToFriendlyString[friendlyStringIndex];
        }

        public static bool TryParse(string trimmedPlatformName, out Platform platform)
        {
            platform = Platform.None;

            switch (trimmedPlatformName)
            {
                case Linux:
                    platform = Platform.Linux;
                    break;
                case MacOS:
                    platform = Platform.MacOS;
                    break;
                case Windows:
                    platform = Platform.Windows;
                    break;
            }

            return platform != Platform.None;
        }
    }
}
