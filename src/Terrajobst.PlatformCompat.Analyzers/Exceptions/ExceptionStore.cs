using System.Collections.Generic;
using System.IO;
using Terrajobst.Csv;
using Terrajobst.PlatformCompat.Analyzers.Store;

namespace Terrajobst.PlatformCompat.Analyzers.Exceptions
{
    internal static class ExceptionStore
    {
        public static ApiStore<Platform> Parse(string data)
        {
            var rows = EnumerateRows(data);
            var apis = ParseApis(rows);
            return ApiStore<Platform>.Create(apis);
        }

        private static IEnumerable<string[]> EnumerateRows(string data)
        {
            using (var stringReader = new StringReader(data))
            {
                var csvReader = new CsvReader(stringReader);
                string[] line;
                while ((line = csvReader.ReadLine()) != null)
                    yield return line;
            }
        }

        private static IEnumerable<(string docid, string namespaceName, string typeName, string signature, Platform platform)> ParseApis(IEnumerable<string[]> rows)
        {        
            Platform[] platforms = null;

            const int PlatformColumnStart = 4;

            foreach (var row in rows)
            {
                if (platforms == null)
                {
                    var isValid = row.Length > PlatformColumnStart &&
                                  row[0] == "DocId" &&
                                  row[1] == "Namespace" &&
                                  row[2] == "Type" &&
                                  row[3] == "Member";

                    if (!isValid)
                        throw InvalidDocument();

                    platforms = new Platform[row.Length - PlatformColumnStart];

                    for (var i = PlatformColumnStart; i < row.Length; i++)
                    {
                        if (!TryParsePlatformName(row[i], out platforms[i - PlatformColumnStart]))
                            throw InvalidDocument();
                    }
                }
                else
                {
                    var docId = row[0];
                    var namespaceName = row[1];
                    var typeName = row[2];
                    var signature = row[3];
                    var data = Platform.None;

                    for (var i = PlatformColumnStart; i < row.Length; i++)
                    {
                        const string ThrowIndicator = "X";

                        var isValid = row[i].Length == 0 || row[i] == ThrowIndicator;
                        if (!isValid)
                            throw InvalidDocument();

                        var throws = row[i] == ThrowIndicator;
                        var platform = platforms[i - PlatformColumnStart];

                        if (throws)
                            data |= platform;
                    }

                    yield return (docId, namespaceName, typeName, signature, data);
                }
            }
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

        private static InvalidDataException InvalidDocument()
        {
            return new InvalidDataException($"The file '{0}' is not a valid CSV file with PlatformNotSupported data.");
        }
    }
}
