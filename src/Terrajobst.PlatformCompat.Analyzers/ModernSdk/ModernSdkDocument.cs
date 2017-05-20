using System;
using System.Collections.Generic;
using System.IO;
using Terrajobst.Csv;

namespace Terrajobst.PlatformCompat.Analyzers.ModernSdk
{
    internal sealed class ModernSdkDocument
    {
        private SortedSet<(string name, string moduleName)> _apis;

        public ModernSdkDocument(SortedSet<(string name, string moduleName)> apis)
        {
            _apis = apis;
        }

        public static ModernSdkDocument Parse(string data)
        {
            var comparer = Comparer<(string name, string moduleName)>.Create(
                (x, y) =>
                {
                    var nameResult = StringComparer.OrdinalIgnoreCase.Compare(x.name, y.name);
                    if (nameResult != 0)
                        return nameResult;

                    return StringComparer.OrdinalIgnoreCase.Compare(x.moduleName, y.moduleName);
                }
            );

            var apis = new SortedSet<(string name, string moduleName)>(comparer);

            using (var stringReader = new StringReader(data))
            {
                var reader = new CsvReader(stringReader);
                string[] line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length != 2)
                        continue;

                    var entry = (line[0], line[1]);
                    apis.Add(entry);
                }
            }

            return new ModernSdkDocument(apis);
        }

        public bool Contains((string name, string moduleName) entry)
        {
            return _apis.Contains(entry);
        }
    }
}
