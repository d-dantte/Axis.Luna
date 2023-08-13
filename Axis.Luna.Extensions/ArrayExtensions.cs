using System;

namespace Axis.Luna.Extensions
{
    public static class ArrayExtensions
    {
        public static TItem[] ConcatWith<TItem>(this TItem[] first, TItem[] second)
        {
            ArgumentNullException.ThrowIfNull(first);
            ArgumentNullException.ThrowIfNull(second);

            var combinedArray = new TItem[first.Length + second.Length];

            Buffer.BlockCopy(first, 0, combinedArray, 0, first.Length);
            Buffer.BlockCopy(second, 0, combinedArray, first.Length, second.Length);

            return combinedArray;
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

        /// <summary>
        /// Essentially divides an array into to at the given <paramref name="splitIndex"/>. The division works as follows:
        /// <code>
        /// return (array[..splitIndex], array[splitIndex..]);
        /// </code>
        /// This means the split-index is exclusive for the left array, and inclusive for the right array.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="array"></param>
        /// <param name="index"></param>
        public static (TItem[] Left, TItem[] Right) SplitAt<TItem>(this TItem[] array, int splitIndex)
        {
            return (array[..splitIndex], array[splitIndex..]);
        }
    }
}
