using System.Numerics;

namespace Axis.Luna.Numerics
{
    internal static class Extensions
    {
        internal static IEnumerable<TItem> TakeExactly<TItem>(this IEnumerable<TItem> items, int value)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            var taken = items.Take(value);

            using var enumerator = taken.GetEnumerator();
            for (var index = 0; index < value; index++)
            {
                if (enumerator.MoveNext())
                    yield return enumerator.Current;

                else yield return default!;
            }
        }

        internal static int TrailingDecimalZeroCount(this BigInteger value)
        {
            var count = 0;
            var str = value.ToString();
            for (int index = str.Length - 1; index >= 0; index--)
            {
                if (str[index] == '0')
                    count++;
                else break;
            }

            return count;
        }

        internal static TOut ApplyTo<TIn, TOut>(this TIn @in, Func<TIn, TOut> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            return mapper.Invoke(@in);
        }
    }
}
