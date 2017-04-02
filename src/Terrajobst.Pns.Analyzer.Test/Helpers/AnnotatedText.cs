using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Terrajobst.Pns.Analyzer.Test.Helpers
{
    public sealed partial class AnnotatedText
    {
        private AnnotatedText(string text, ImmutableArray<TextSpan> spans)
        {
            Text = text;
            Spans = spans;
        }

        public static AnnotatedText Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var parser = new Parser(text);
            return parser.Parse();
        }

        private static string NormalizeCode(string text)
        {
            return Unindent(text).Trim();
        }

        private static string Unindent(string text)
        {
            var minIndent = int.MaxValue;

            using (var stringReader = new StringReader(text))
            {
                string line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var indent = line.Length - line.TrimStart().Length;
                    minIndent = Math.Min(minIndent, indent);
                }
            }

            var sb = new StringBuilder();
            using (var stringReader = new StringReader(text))
            {
                string line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    var unindentedLine = line.Length < minIndent
                        ? line
                        : line.Substring(minIndent);
                    sb.AppendLine(unindentedLine);
                }
            }

            return sb.ToString();
        }

        public string Text { get; }

        public ImmutableArray<TextSpan> Spans { get; }
    }
}