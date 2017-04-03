using System;
using System.Collections.Immutable;
using Terrajobst.Pns.Analyzer.Store;

namespace Terrajobst.Pns.Analyzer
{
    internal sealed class PnsOptions
    {
        public PnsOptions(ImmutableDictionary<string, string> options)
        {
            IgnoredPlatforms = ParseIgnoredPlatforms(options);
            TargetFramework = ParseTargetFramework(options);
        }

        public Platform IgnoredPlatforms { get; }
        public string TargetFramework { get; }

        public static Platform ParseIgnoredPlatforms(ImmutableDictionary<string, string> options)
        {
            var result = Platform.None;

            if (options.TryGetValue("PnsIgnore", out var value))
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
