using System.Collections.Generic;

namespace Axis.Luna.Common
{
    public static class ArrayUtil
    {
        public static TItem[] Of<TItem>(params TItem[] items) => items;

        public static TItem[] Of<TItem>(TItem value, int count)
        {
            var arr = new TItem[count];
            var comparer = EqualityComparer<TItem>.Default;
            if (!comparer.Equals(default, value))
            {
                for (int cnt = 0; cnt < count; cnt++)
                    arr[cnt] = value;
            }

            return arr;
        }
    }
}
