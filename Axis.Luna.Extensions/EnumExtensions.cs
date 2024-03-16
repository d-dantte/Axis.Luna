using System;
using System.Linq;

namespace Axis.Luna.Extensions
{
    public static class EnumExtensions
    {
        public static T ParseEnum<T>(this string @string)
        where T : struct, Enum
        {
            return Enum
                .GetValues<T>()
                .First(t => t.ToString().Equals(@string));
        }
    }
}
