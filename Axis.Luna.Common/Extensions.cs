using System;
using Axis.Luna.Common.Segments;

namespace Axis.Luna.Common
{
    public static class Extensions
    {
        public static bool IsEmpty(this ICountable countable)
        {
            ArgumentNullException.ThrowIfNull(countable);
            return countable.Count == 0;
        }

        public static bool IsEmpty(this ILongCountable countable)
        {
            ArgumentNullException.ThrowIfNull(countable);
            return countable.LongCount == 0;
        }

        internal static TOut ApplyTo<TIn, TOut>(this TIn @in, Func<TIn, TOut> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            return mapper.Invoke(@in);
        }
    }
}