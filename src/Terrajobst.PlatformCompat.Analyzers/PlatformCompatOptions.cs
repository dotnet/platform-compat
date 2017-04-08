using System;
using System.Collections.Immutable;
using Terrajobst.PlatformCompat.Analyzers.Store;

namespace Terrajobst.PlatformCompat.Analyzers
{
    public sealed class PlatformCompatOptions
    {
        public const string SettingsName = "Terrajobst.PlatformCompat.Analyzers.settings";

        public PlatformCompatOptions(ImmutableDictionary<string, string> options)
        {
            IgnoredPlatforms = ParseIgnoredPlatforms(options);
            TargetFramework = ParseTargetFramework(options);
        }

        public Platform IgnoredPlatforms { get; }
        public string TargetFramework { get; }

        public static Platform ParseIgnoredPlatforms(ImmutableDictionary<string, string> options)
        {
            var result = Platform.None;

            if (options.TryGetValue("PlatformCompatIgnore", out var value))
            {
                var names = value.Split(';');
                foreach (var name in names)
                {
                    var trimmedNamed = name.Trim();
                    if (Enum.TryParse<Platform>(trimmedNamed, out var platform))
                        result |= platform;
                }
            }

            return result;
        }

        public static string ParseTargetFramework(ImmutableDictionary<string, string> options)
        {
            options.TryGetValue("TargetFramework", out var value);
            return value ?? string.Empty;
        }
    }
}
