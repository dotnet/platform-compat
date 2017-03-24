using System.IO;

namespace NotImplementedScanner
{
    internal static class TextWriterExtensions
    {
        private static char[] _specialChars = new[] { ',', '"' };

        public static void WriteEscaped(this TextWriter writer, string text)
        {
            var needsEscaping = text.IndexOfAny(_specialChars) >= 0;
            if (!needsEscaping)
            {
                writer.Write(text);
            }
            else
            {
                writer.Write('"');
                foreach (var c in text)
                {
                    if (c == '"')
                        writer.Write('"');

                    writer.Write(c);
                }
                writer.Write('"');
            }
        }
    }
}
