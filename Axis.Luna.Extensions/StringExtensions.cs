using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axis.Luna.Extensions
{
    public static class StringExtensions
    {

        /// <summary>
        /// Essentially divides an string into to at the given <paramref name="splitIndex"/>. The division works as follows:
        /// <code>
        /// return (@string[..splitIndex], @string[splitIndex..]);
        /// </code>
        /// This means the split-index is exclusive for the left array, and inclusive for the right array.
        /// </summary>
        /// <param name="string"></param>
        /// <param name="splitIndex"></param>
        /// <returns></returns>
        public static (string Left, string Right) SplitAt(this string @string, int splitIndex)
        {
            return (@string[..splitIndex], @string[splitIndex..]);
        }


        public static string AsString(this IEnumerable<char> chars)
        {
            ArgumentNullException.ThrowIfNull(chars);

            return new string(chars.ToArray());
        }

        #region String extensions

        public static string Trim(this
            string @string,
            string trimChars)
            => @string.TrimStart(trimChars).TrimEnd(trimChars);

        public static string TrimStart(this
            string original,
            string searchString)
            => original.StartsWith(searchString)
               ? original[searchString.Length..]
               : original;

        public static string TrimEnd(this
            string original,
            string searchString)
            => original.EndsWith(searchString)
               ? original[..^searchString.Length]
               : original;

        public static string JoinUsing(this
            IEnumerable<string> strings,
            string separator)
            => string.Join(separator, strings);

        public static string JoinUsing(this
            IEnumerable<char> subStrings,
            string separator)
            => string.Join(separator, subStrings.ToArray());

        public static string WrapIn(this
            string @string,
            string left,
            string right = null)
            => $"{left}{@string}{right ?? left}";

        public static string WrapIf(this
            string @string, Func<string, bool> predicate,
            string left,
            string right = null)
        {
            if (predicate.Invoke(@string))
                return @string.WrapIn(left, right);

            else return @string;
        }

        public static string UnwrapFrom(this
            string @string,
            string left,
            string right = null)
        {
            if (@string.IsWrappedIn(left, right))
                return @string
                    [left.Length..^(right ?? left).Length]; //remove the first left.length, and the last (left|right).length, characters

            else return @string;
        }

        public static string UnwrapIf(this
            string @string,
            Func<string, bool> predicate,
            string left,
            string right = null)
        {
            if (predicate.Invoke(@string))
                return @string.UnwrapFrom(left, right);

            else return @string;
        }

        public static bool IsWrappedIn(this
            string @string,
            string left,
            string right = null)
        {
            return @string.StartsWith(left) && @string.EndsWith(right ?? left);
        }

        public static int SubstringCount(this string source, string subString)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(subString))
                return 0;

            else if (subString.Length > source.Length)
                return 0;

            int count = 0;
            int i = 0;
            while ((i = source.IndexOf(subString, i)) != -1)
            {
                i += subString.Length;
                count++;
            }
            return count;
        }

        public static bool ContainsAny(this
            string source,
            params string[] substrings)
            => substrings.Any(source.Contains);

        public static bool ContainsAll(this
            string source,
            params string[] substrings)
            => substrings.All(source.Contains);

        public static string SplitCamelCase(this
            string source,
            string separator = " ")
            => source
            .Aggregate(new StringBuilder(), (acc, ch) => acc.Append(char.IsUpper(ch) ? separator : "").Append(ch))
            .ToString()
            .Trim();
        #endregion
    }
}
