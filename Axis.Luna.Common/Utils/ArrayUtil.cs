using Axis.Luna.Extensions;
using System;

namespace Axis.Luna.Common.Utils
{
    public static class ArrayUtil
    {
        public static TItem[] Of<TItem>(params TItem[] items) => items;

        public static TItem[] Of<TItem>(TItem value, int count)
        {
            var arr = new TItem[count];
            if (!default(TItem).Equals(value))
            {
                for (int cnt = 0; cnt < count; cnt++)
                    arr[cnt] = value;
            }

            return arr;
        }

        public static bool IsEmpty<TItem>(this TItem[] items)
        {
            ArgumentNullException.ThrowIfNull(items);
            return items.Length == 0;
        }

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array is null || array.IsEmpty();
        }
    }
}
