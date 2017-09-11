using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Terrajobst.Csv
{
    public class CsvReader
    {
        private readonly TextReader _reader;

        public CsvReader(TextReader reader)
        {
            _reader = reader;
        }

        public string[] ReadLine()
        {
            var line = _reader.ReadLine();
            if (line == null)
                return null;

            return ReadLine(line);
        }

        private static string[] ReadLine(string text)
        {
            var result = new List<string>();
            var index = 0;
            while (index < text.Length)
            {
                var value = ReadValue(text, ref index);
                index++;
                result.Add(value);
            }
            return result.ToArray();
        }

        private static string ReadValue(string text, ref int index)
        {
            if (text[index] != '"')
            {
                var start = index;

                while (index < text.Length && text[index] != ',')
                    index++;

                var length = index - start;

                return text.Substring(start, length);
            }
            else
            {
                // Skip leading quote
                index++;

                var sb = new StringBuilder(text.Length - index);

                while (index < text.Length)
                {
                    var c = text[index];
                    var l = index < text.Length - 1 ? text[index + 1] : '\0';

                    if (c == '"')
                    {
                        if (l == '"')
                            index++;
                        else
                            break;
                    }

                    sb.Append(c);
                    index++;
                }

                // Skip trailing quote
                index++;

                return sb.ToString();
            }
        }
    }
}
