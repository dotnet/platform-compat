using System.IO;

namespace PlatformCompat.Csv
{
    public class CsvWriter
    {
        private static char[] _specialChars = new[] { ',', '"', '\n', '\r' };

        private readonly TextWriter _textWriter;
        private bool _needsComma;

        public CsvWriter(TextWriter textWriter)
        {
            _textWriter = textWriter;
        }

        public void Write(string text)
        {
            if (_needsComma)
                _textWriter.Write(",");

            var needsEscaping = text.IndexOfAny(_specialChars) >= 0;
            if (!needsEscaping)
            {
                _textWriter.Write(text);
            }
            else
            {
                _textWriter.Write('"');
                foreach (var c in text)
                {
                    if (c == '"')
                        _textWriter.Write('"');

                    _textWriter.Write(c);
                }
                _textWriter.Write('"');
            }

            _needsComma = true;
        }

        public void WriteLine()
        {
            _textWriter.WriteLine();
            _needsComma = false;
        }
    }
}
