using System;
using System.Linq;

namespace Axis.Luna.Extensions
{
    public static class EnumExtensions
    {
        public static T ParseEnum<T>(this string @string)
        where T : Enum
        {
            return Enum
                .GetValues(typeof(T))
                .As<T[]>()
                .First(t => t.ToString().Equals(@string));
        }
    }
}
