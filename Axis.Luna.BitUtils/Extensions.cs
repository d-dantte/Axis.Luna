using System.Collections;
using System.Linq;
using System.Numerics;

namespace Axis.Luna.BitSequence
{
    public static class Extensions
    {
        private static readonly byte[] ByteMasks = new byte[]
        {
            1,
            2,
            4,
            8,
            16,
            32,
            64,
            128
        };

        internal static bool IsSet(this
            byte @byte,
            int bitIndex)
            => (@byte & ByteMasks[bitIndex]) == ByteMasks[bitIndex];

        internal static IEnumerable<TOut> SelectAs<TOut>(this
            IEnumerable items)
        {
            ArgumentNullException.ThrowIfNull(items);

            return Enumerable.Cast<TOut>(items);
        }

        internal static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            long count = 0;
            var batch = new List<T>();

            foreach (var value in source)
            {
                batch.Add(value);

                if (++count % batchSize == 0)
                {
                    yield return batch;

                    batch = new List<T>();
                }
            }

            if (batch.Count > 0)
                yield return batch;
        }

        internal static TOut ApplyTo<TIn, TOut>(this TIn @in, Func<TIn, TOut> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            return mapper.Invoke(@in);
        }

        internal static bool IsEmpty<T>(this T[] array)
        {
            ArgumentNullException.ThrowIfNull(array);

            return array.Length == 0;
        }
        
        internal static IEnumerable<IGrouping<TKey, TItem>> GroupBy<TKey, TItem>(
            this IEnumerable<TItem> items,
            Func<TItem, int, TKey> grouper)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(grouper);

            return items
                .Select((item, index) => (item, index))
                .GroupBy(tuple => grouper.Invoke(tuple.item, tuple.index))
                .Select(group => new Grouping<TKey, TItem>(
                    group.Key,
                    group.Select(t => t.item)) as IGrouping<TKey, TItem>);
        }

        internal static string ReverseString(this string s)
        {
            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static IEnumerable<V> InsertAt<V>(this IEnumerable<V> items, int position, V value)
        {
            if (position < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            int pos = 0;

            // insert at the start
            if (position == 0)
            {
                yield return value;
                pos++;
            }

            foreach (var v in items)
            {
                if (pos++ == position) yield return value;
                yield return v;
            }

            // adding at the end
            if (pos == position)
                yield return value;
        }

        internal static TOut Cast<TOut>(this object @in) => (TOut)@in;

        #region Nested types
        internal readonly struct Grouping<TKey, TItem> : IGrouping<TKey, TItem>
        {
            private readonly TItem[] items;

            public TKey Key { get; }

            internal Grouping(TKey key, IEnumerable<TItem> items)
            {
                Key = key;
                this.items = items?.ToArray() ?? throw new ArgumentNullException(nameof(items));
            }

            public IEnumerator<TItem> GetEnumerator() => ((IEnumerable<TItem>)items).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        #endregion

        #region External Extensions

        /// <summary>
        /// Converts the significant bits of the integer to a <see cref="BitSequence"/> instance
        /// </summary>
        /// <param name="integer">The integer to convert</param>
        /// <returns>The bit sequence</returns>
        public static BitSequence ToBitSequence(this BigInteger integer)
        {
            return BitSequence
                .Of(integer.ToByteArray())
                [..(int)(integer.GetBitLength())];
        }
        #endregion
    }
}
