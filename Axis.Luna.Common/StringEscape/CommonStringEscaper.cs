using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Axis.Luna.Common.StringEscape
{
    /// <summary>
    /// Escaper implementation that works on individual characters.
    /// </summary>
    public class CommonStringEscaper : IStringEscaper
    {
        private static readonly ImmutableHashSet<char> SimpleEscapes = EnumerableUtil
            .Of('0', 'a', 'b', 'f', 'n', 'r', 't', 'v', '\'', '"', '\\')
            .ToImmutableHashSet();

        private static readonly ImmutableHashSet<CharSequence> AsciiEscapes = Enumerable
            .Range(0, byte.MaxValue + 1)
            .Select(value => CharSequence.Of($"\\x{value:x2}"))
            .ToImmutableHashSet();

        private static readonly Regex UnicodeEscapes = new(
            "^\\\\[uU][0-9a-fA-F]{4}$",
            RegexOptions.Compiled
            | RegexOptions.IgnoreCase);

        #region Escape Sequences
        private static readonly string SimpleEscapeSequences = "\\0\\a\\b\\f\\n\\r\\t\\v\\'\\\"\\\\";

        private static readonly string AsciiEscapeSequences = Enumerable
            .Range(0, byte.MaxValue + 1)
            .Select(value => $"\\x{value:x2}")
            .ApplyTo(value => string.Join("", value));
        #endregion

        public bool IsValidEscapeSequence(CharSequence escapeSequence)
        {
            if (escapeSequence.IsDefault)
                return false;

            if (!'\\'.Equals(escapeSequence[0]))
                return false;

            return escapeSequence.Length switch
            {
                2 => SimpleEscapes.Contains(escapeSequence[1]),
                4 => AsciiEscapes.Contains(escapeSequence),
                6 => UnicodeEscapes.IsMatch(escapeSequence.AsSpan()),
                _ => false
            };
        }

        public CharSequence Escape(CharSequence unescapedSequence)
        {
            if (unescapedSequence.IsDefault)
                throw new ArgumentException($"Invalid {nameof(unescapedSequence)}: default");

            return unescapedSequence.Aggregate(
                CharSequence.Empty,
                (seq, @char) => seq + EscapeChar(@char));
        }

        public CharSequence Escape(
            CharSequence unescapedSequence,
            Func<char, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            if (unescapedSequence.IsDefault)
                throw new ArgumentException($"Invalid {nameof(unescapedSequence)}: default");

            return unescapedSequence.Aggregate(
                CharSequence.Empty,
                (seq, @char) =>
                {
                    if (predicate.Invoke(@char))
                        return seq + EscapeChar(@char);
                    else return seq + @char;
                });
        }

        public CharSequence Unescape(CharSequence escapedSequence)
        {
            if (escapedSequence.IsDefault)
                throw new ArgumentException($"Invalid {nameof(escapedSequence)}: default");

            if (!IsValidEscapeSequence(escapedSequence))
                return escapedSequence;

            return escapedSequence.Length switch
            {
                2 => escapedSequence[1] switch
                {
                    '0' => "\0",
                    'a' => "\a",
                    'b' => "\b",
                    'f' => "\f",
                    'n' => "\n",
                    'r' => "\r",
                    't' => "\t",
                    'v' => "\v",
                    '\'' => "\'",
                    '\"' => "\"",
                    '\\' or _ => "\\"
                },

                4 or 6 or _ => int
                    .Parse(escapedSequence.AsSpan(2..), NumberStyles.HexNumber)
                    .ApplyTo(Convert.ToChar)
                    .ApplyTo(v => v.ToString())
            };
        }

        public string UnescapeString(string @string)
        {
            if (@string is null)
                return @string!;

            var stringBuilder = new StringBuilder();

            for (int index = 0; index < @string.Length; index++)
            {
                if (!'\\'.Equals(@string[index]))
                    stringBuilder.Append(@string[index]);

                else
                {
                    CharSequence es;

                    // try simple escape
                    if (index + 2 <= @string.Length)
                    {
                        es = CharSequence.Of(@string, index, 2);
                        if (IsValidEscapeSequence(es))
                        {
                            stringBuilder.Append(Unescape(es));
                            index += 1;
                            continue;
                        }
                    }

                    // try unicode escape
                    if (index + 6 <= @string.Length)
                    {
                        es = CharSequence.Of(@string, index, 6);
                        if (IsValidEscapeSequence(es))
                        {
                            stringBuilder.Append(Unescape(es));
                            index += 5;
                            continue;
                        }
                    }

                    // try ascii escape
                    if (index + 4 <= @string.Length)
                    {
                        es = CharSequence.Of(@string, index, 4);
                        if (IsValidEscapeSequence(es))
                        {
                            stringBuilder.Append(Unescape(es));
                            index += 3;
                            continue;
                        }
                    }

                    es = index + 6 <= @string.Length
                        ? CharSequence.Of(@string, index, 6)
                        : CharSequence.Of(@string, index..);
                    throw new InvalidEscapeSequence(es);
                }
            }

            if (@string.Length == stringBuilder.Length)
                return @string;

            else return stringBuilder.ToString();
        }

        private static CharSequence EscapeChar(char @char)
        {
            return @char switch
            {
                #region Simple
                '\0' => CharSequence.Of(SimpleEscapeSequences, 0 * 2, 2),
                '\a' => CharSequence.Of(SimpleEscapeSequences, 1 * 2, 2),
                '\b' => CharSequence.Of(SimpleEscapeSequences, 2 * 2, 2),
                '\f' => CharSequence.Of(SimpleEscapeSequences, 3 * 2, 2),
                '\n' => CharSequence.Of(SimpleEscapeSequences, 4 * 2, 2),
                '\r' => CharSequence.Of(SimpleEscapeSequences, 5 * 2, 2),
                '\t' => CharSequence.Of(SimpleEscapeSequences, 6 * 2, 2),
                '\v' => CharSequence.Of(SimpleEscapeSequences, 7 * 2, 2),
                '\'' => CharSequence.Of(SimpleEscapeSequences, 8 * 2, 2),
                '\"' => CharSequence.Of(SimpleEscapeSequences, 9 * 2, 2),
                '\\' => CharSequence.Of(SimpleEscapeSequences, 10 * 2, 2),
                #endregion

                #region Ascii
                <= '\xff' => CharSequence.Of(AsciiEscapeSequences, @char * 4, 4),
                #endregion

                #region Unicode
                <= '\uffff' => CharSequence.Of($"\\u{(int)@char:x4}")
                #endregion
            };
        }
    }
}
