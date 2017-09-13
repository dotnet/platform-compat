using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace ApiCompat.Analyzers.Tests.Helpers
{
    public sealed partial class AnnotatedText
    {
        private sealed class Parser
        {
            private static readonly IComparer<TextSpan> SpanComparer = Comparer<TextSpan>.Create((x, y) => x.Start.CompareTo(y.Start));

            private readonly string _text;
            private int _position;
            private readonly StringBuilder _textBuilder = new StringBuilder();
            private readonly ImmutableArray<TextSpan>.Builder _spanBuilder = ImmutableArray.CreateBuilder<TextSpan>();

            public Parser(string text)
            {
                _text = text;
            }

            private char Char
            {
                get { return _position < _text.Length ? _text[_position] : '\0'; }
            }

            private char Lookahead
            {
                get { return _position + 1 < _text.Length ? _text[_position + 1] : '\0'; }
            }

            private bool IsSpanStart()
            {
                return Char == '{' && Lookahead == '{';
            }

            private bool IsSpanEnd()
            {
                return Char == '}' && Lookahead == '}';
            }

            public AnnotatedText Parse()
            {
                ParseRoot();

                var text = _textBuilder.ToString();

                _spanBuilder.Sort(SpanComparer);
                var spans = _spanBuilder.ToImmutable();

                return new AnnotatedText(text, spans);
            }

            private void ParseRoot()
            {
                while (_position < _text.Length)
                {
                    if (IsSpanStart())
                        ParseSpan();
                    else
                        ParseText();
                }
            }

            private void ParseSpan()
            {
                // Skip intitial braces
                _position += 2;

                var spanStart = _textBuilder.Length;

                while (true)
                {
                    if (IsSpanEnd())
                    {
                        // Skip closing braces
                        _position += 2;
                        var spanEnd = _textBuilder.Length;
                        var span = TextSpan.FromBounds(spanStart, spanEnd);
                        _spanBuilder.Add(span);
                        break;
                    }
                    else if (IsSpanStart())
                    {
                        ParseSpan();
                    }
                    else if (Char == '\0')
                    {
                        throw MissingClosingBrace();
                    }
                    else
                    {
                        ParseText();
                    }
                }
            }

            private FormatException MissingClosingBrace()
            {
                var message = $"Missing '}}}}' at position {_position}.";
                return new FormatException(message);
            }

            private void ParseText()
            {
                var start = _position;
                while (_position < _text.Length &&
                       !IsSpanStart() &&
                       !IsSpanEnd())
                {
                    _position++;
                }

                var end = _position;
                var length = end - start;
                var text = _text.Substring(start, length);
                _textBuilder.Append(text);
            }
        }
    }
}