using System;

namespace Axis.Luna.Common.StringEscape
{
    /// <summary>
    /// A string escaper represents a set of rules that:
    /// <list type="number">
    /// <item>Identifies a valid escape sequence</item>
    /// <item>Converts between escaped and unescaped sequences</item>
    /// <item>Replaces (once) all occurences of escape sequences in a string, with the unescaped sequences</item>
    /// </list>
    /// </summary>
    public interface IStringEscaper
    {
        /// <summary>
        /// Verifies if the given sequence is an escape sequence
        /// </summary>
        /// <param name="escapeSequence">The escape sequence</param>
        /// <returns>True if the sequence is a valid escape sequence, false otherwise</returns>
        bool IsValidEscapeSequence(CharSequence escapeSequence);

        /// <summary>
        /// Converts the entire unescaped sequence into an escaped sequence. If the unescaped sequence cannot be escaped, return it as-is.
        /// </summary>
        /// <param name="chars">The unescaped sequence</param>
        /// <returns>The escaped sequence</returns>
        CharSequence Escape(CharSequence unescapedSequence);

        /// <summary>
        /// Escapes only characters in the unescaped sequence that match the predicate; otherwise returning other characters as-is.
        /// </summary>
        /// <param name="unescapedSequence">The unescaped sequence</param>
        /// <param name="predicate">A predicate that decides if a character is to be escaped</param>
        /// <returns>The escaped sequence</returns>
        CharSequence Escape(
            CharSequence unescapedSequence,
            Func<char, bool> predicate);

        /// <summary>
        /// Escapes only characters in the unescaped sequence that match the predicate; otherwise returning other characters as-is.
        /// </summary>
        /// <param name="unescapedSequence">The unescaped sequence</param>
        /// <param name="predicate">A predicate that decides if a character is to be escaped</param>
        /// <param name="charEscaper">A function that converts the given character to an escape sequence</param>
        /// <returns>The escaped sequence</returns>
        CharSequence Escape(
            CharSequence unescapedSequence,
            Func<char, bool> predicate,
            Func<char, CharSequence> charEscaper);

        /// <summary>
        /// Converts the escaped sequence into an unescaped sequence. If the escaped sequence is invalid, return it as-is.
        /// <para/>
        /// </summary>
        /// <param name="sequence">The escaped sequence, representing a sequence of characters that can be unescaped into a single character</param>
        /// <returns>The unescaped sequence</returns>
        CharSequence Unescape(CharSequence escapedSequence);

        /// <summary>
        /// Replaces (once) all occurences of escape sequences in a string, with the unescaped sequences
        /// </summary>
        /// <param name="string">The string containing escape sequences</param>
        /// <returns>the unescaped string</returns>
        string UnescapeString(string @string);
    }
}
