using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Microsoft.DotNet.Analyzers.Compatibility.Tests.Helpers
{
    internal static class MetadataReferenceSet
    {
        private static ImmutableArray<MetadataReference> _netFramework;
        private static ImmutableArray<MetadataReference> _netStandard;

        public static ImmutableArray<MetadataReference> NetFramework
        {
            get
            {
                if (_netFramework.IsDefault)
                {
                    var netFramework = LoadNetFramworkSet();
                    ImmutableInterlocked.InterlockedInitialize(ref _netFramework, netFramework);
                }

                return _netFramework;
            }
        }

        public static ImmutableArray<MetadataReference> NetStandard
        {
            get
            {
                if (_netStandard.IsDefault)
                {
                    var netStandard = LoadNetStandardSet();
                    ImmutableInterlocked.InterlockedInitialize(ref _netStandard, netStandard);
                }

                return _netStandard;
            }
        }

        private static ImmutableArray<MetadataReference> LoadNetFramworkSet()
        {
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var path = Path.Combine(programFilesX86, "Reference Assemblies", "Microsoft", "Framework", ".NETFramework", "v4.6.1");
            var names = new[] { "mscorlib", "System", "System.Core" };
            return names.Select(n => Path.Combine(path, n + ".dll"))
                        .Select(p => MetadataReference.CreateFromFile(p))
                        .ToImmutableArray<MetadataReference>();
        }

        private static ImmutableArray<MetadataReference> LoadNetStandardSet()
        {
            return ImmutableArray.Create(GetNetStandard(), GetRegistry(), GetCng());
        }

        private static MetadataReference GetNetStandard()
        {
            var latestPath = GetPackagePath("NETStandard.Library", "2.0.0");
            var path = Path.Combine(latestPath, "build", "netstandard2.0", "ref", "netstandard.dll");
            return MetadataReference.CreateFromFile(path);
        }

        private static MetadataReference GetRegistry()
        {
            var latestPath = GetPackagePath("Microsoft.Win32.Registry", "4.4.0");
            var path = Path.Combine(latestPath, "ref", "netstandard2.0", "Microsoft.Win32.Registry.dll");
            return MetadataReference.CreateFromFile(path);
        }

        private static MetadataReference GetCng()
        {
            var latestPath = GetPackagePath("System.Security.Cryptography.Cng", "4.4.0");
            var path = Path.Combine(latestPath, "ref", "netstandard1.6", "System.Security.Cryptography.Cng.dll");
            return MetadataReference.CreateFromFile(path);
        }

        private static string GetPackagePath(string packageName, string version)
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var netStandard = Path.Combine(userProfile, ".nuget", "packages", packageName);
            return Directory.EnumerateDirectories(netStandard, version + "*").OrderByDescending(p => p).First();
        }
    }
}
