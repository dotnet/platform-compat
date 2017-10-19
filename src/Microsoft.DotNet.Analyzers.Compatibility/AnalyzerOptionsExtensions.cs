using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.DotNet.Analyzers.Compatibility
{
    public static class AnalyzerOptionsExtensions
    {
        public static ImmutableDictionary<string, string> GetFileSettings(this AnalyzerOptions options, string fileName)
        {
            var additionalFile = options.AdditionalFiles.SingleOrDefault(a => string.Equals(Path.GetFileName(a.Path), fileName, StringComparison.OrdinalIgnoreCase));
            if (additionalFile == null)
                return ImmutableDictionary<string, string>.Empty;

            var lines = additionalFile.GetText().Lines;
            var result = new Dictionary<string, string>();

            var previousKey = (string)null;

            foreach (var line in lines)
            {
                var lineText = line.ToString().Trim();
                if (lineText.Length == 0)
                    continue;

                var kv = ParseKeyValue(lineText);

                // Don't throw if the key already exists. It's following the MBuild/CLI conventions where
                // later values simply overwrite earlier options.
                //
                // Also, if the value is NULL, it means we need to associate it with the previous key.
                // That's because the task writing the lines will put a new line for every semi-colon.
                //
                // For example, this configuration:
                //
                //      <PropertyGroup>
                //          <Prop1>Value1;Value2</Prop1>
                //          <Prop2>Value3</Prop2>
                //      </PropertyGroup>
                //
                //      <ItemGroup>
                //          <AdditionalFileContents Include="Prop1=$(Prop1);Prop2=$(Prop2)">
                //              <AdditionalFileName>Foo.settings</AdditionalFile>
                //          </AdditionalFileContents>
                //      </ItemGroup>
                //
                // will create the file Foo.settings like this:
                //
                //      Prop1=Value1
                //      Value2
                //      Prop2=Value3

                if (kv.Key != null)
                {
                    result[kv.Key] = kv.Value;
                    previousKey = kv.Key;
                }
                else if (previousKey != null)
                {
                    if (result.ContainsKey(previousKey))
                        result[previousKey] += ";" + kv.Value;
                    else
                        result[previousKey] = kv.Value;
                }
            }

            return result.ToImmutableDictionary();
        }

        private static KeyValuePair<string, string> ParseKeyValue(string line)
        {
            var equalIndex = line.IndexOf('=');
            if (equalIndex < 0)
            {
                var key = (string)null;
                var value = line.Trim();
                return new KeyValuePair<string, string>(key, value);
            }
            else
            {
                var key = line.Substring(0, equalIndex).Trim();
                var value = line.Substring(equalIndex + 1).Trim(); ;
                return new KeyValuePair<string, string>(key, value);
            }
        }
    }
}
