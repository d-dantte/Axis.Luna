using System.Collections;

namespace Axis.Luna.BitSequence.Test
{
    internal static class ArrayUtil
    {
        internal static T[] Of<T>(params T[] array) => array;

        internal static IEnumerable<T> SelectAs<T>(this IEnumerable items)
        {
            foreach(var item in items)
            {
                yield return (T)item;
            }
        }
    }
}
