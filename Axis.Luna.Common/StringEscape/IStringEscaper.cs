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
        /// Converts the unescaped sequence into an escape sequence. If the unescaped sequence cannot be escaped, return it as-is.
        /// </summary>
        /// <param name="chars">The unescaped sequence</param>
        /// <returns>The escaped sequence</returns>
        CharSequence Escape(CharSequence unescapedSequence);

        /// <summary>
        /// Converts the escaped sequence into an unescaped sequence. If the escaped sequence is invalid, return it as-is.
        /// </summary>
        /// <param name="sequence">The escaped sequence</param>
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
