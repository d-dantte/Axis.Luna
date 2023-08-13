using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
