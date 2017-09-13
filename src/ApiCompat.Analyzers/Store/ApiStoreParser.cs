using System;
using System.Collections.Generic;
using System.IO;
using ApiCompat.Csv;
using ApiCompat.Analyzers.Store;

namespace ApiCompat.Analyzers.Exceptions
{
    internal abstract class ApiStoreParser<T>
    {
        public ApiStore<T> Parse(string data)
        {
            var rows = EnumerateRows(data);
            var apis = ParseApis(rows);
            return ApiStore<T>.Create(apis);
        }

        private IEnumerable<string[]> EnumerateRows(string data)
        {
            using (var stringReader = new StringReader(data))
            {
                var csvReader = new CsvReader(stringReader);
                string[] line;
                while ((line = csvReader.ReadLine()) != null)
                    yield return line;
            }
        }

        protected virtual void Initialize(ArraySegment<string> headers)
        {
        }

        protected abstract T ParseData(ArraySegment<string> values);

        private IEnumerable<(string docid, string namespaceName, string typeName, string signature, T data)> ParseApis(IEnumerable<string[]> rows)
        {
            const int DataColumnStart = 4;

            var isHeader = true;

            foreach (var row in rows)
            {
                if (isHeader)
                {
                    var isValid = row.Length >= DataColumnStart &&
                                  row[0] == "DocId" &&
                                  row[1] == "Namespace" &&
                                  row[2] == "Type" &&
                                  row[3] == "Member";

                    if (!isValid)
                        throw InvalidDocument();

                    var headerNames = new ArraySegment<string>(row, DataColumnStart, row.Length - DataColumnStart);
                    Initialize(headerNames);

                    isHeader = false;
                }
                else
                {
                    var docId = row[0];
                    var namespaceName = row[1];
                    var typeName = row.Length > 2 ? row[2] : string.Empty;
                    var signature = row.Length > 3 ? row[3] : string.Empty;

                    var valueStart = Math.Min(row.Length, DataColumnStart);
                    var valueCount = Math.Max(0, row.Length - DataColumnStart);

                    var values = new ArraySegment<string>(row, valueStart, valueCount);
                    var data = ParseData(values);

                    yield return (docId, namespaceName, typeName, signature, data);
                }
            }
        }

        protected static InvalidDataException InvalidDocument()
        {
            return new InvalidDataException("The API document is malformed");
        }
    }
}
