using System.Collections;
using System.Security.Cryptography;

namespace GridLuck.Common.Extensions
{
    public static class Enumerables
    {
        #region Array
        public static ReadOnlySpan<TValue> AsReadOnlySpan<TValue>(this TValue[] array) => new(array);
        #endregion

        #region Iteration
        public static void ForAll<TItem>(this IEnumerable<TItem> items, Action<TItem> action)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(action);

            foreach (var item in items)
            {
                action.Invoke(item);
            }
        }

        public static async Task ForAllAsync<TItem>(this IEnumerable<TItem> items, Func<TItem, Task> action)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(action);

            foreach (var item in items)
            {
                await action.Invoke(item);
            }
        }

        public static IEnumerable<TItem> WithAll<TItem>(this IEnumerable<TItem> items, Action<TItem> action)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(action);

            foreach (var item in items)
            {
                action.Invoke(item);
                yield return item;
            }
        }

        public static async IAsyncEnumerable<TItem> WithAllAsync<TItem>(this IEnumerable<TItem> items, Func<TItem, Task> action)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(action);

            foreach (var item in items)
            {
                await action.Invoke(item);
                yield return item;
            }
        }
        #endregion

        #region Misc
        /// <summary>
        /// https://stackoverflow.com/a/33336576
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <param name="choices"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Combinations<T>(this
            IEnumerable<T> sequence,
            int choices)
            => choices == 0 ? [[]] : sequence.SelectMany((e, i) =>
            {
                return sequence
                    .Skip(i + 1)
                    .Combinations(choices - 1)
                    .Select(c => (new[] { e }).Concat(c));
            });

        /// <summary>
        /// Resolve the permutations of the given enumerable
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Permutations<T>(this IEnumerable<T> values)
        {
            if ((values is T[] tarr && tarr.Length == 1)
                || (values is System.Collections.ICollection col && col.Count == 1)
                || (values is ICollection<T> tcol && tcol.Count == 1)
                || (values.Count() == 1))
                return [[values.First()]];

            else
            {
                return values
                    .SelectMany((value, index) =>
                    {
                        var primary = new[] { value };
                        return Enumerables
                            .Permutations(Splice(values, index))
                            .Select(perm => primary.Concat(perm).ToList() as IEnumerable<T>);
                    });
            }
        }

        private static T[] Splice<T>(IEnumerable<T> list, int index)
        {
            return list
                .Take(index)
                .Concat(list.Skip(index + 1))
                .ToArray();
        }

        /// <summary>
        ///  Fisher-Yates-Durstenfeld shuffle http://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle#The_modern_algorithm
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="source"></param>
        /// <param name="rng"></param>
        /// <param name="cycle">Number of times the shuffle algorithm should cycle through the sequence</param>
        /// <returns></returns>
        public static IEnumerable<V> Shuffle<V>(this IEnumerable<V> source, uint cycle = 1)
        {
            var buffer = source.ToArray();
            while ((cycle--) > 0)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    int j = RandomNumberGenerator.GetInt32(i, buffer.Length);
                    yield return buffer[j];

                    buffer[j] = buffer[i];
                }
            }
        }

        /// <summary>
        /// Verifies that the given sequence is empty
        /// </summary>
        /// <typeparam name="T">The type of the sequence</typeparam>
        /// <param name="items">The sequence instance</param>
        /// <returns></returns>
        public static bool IsEmpty<T>(this IEnumerable<T> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            return items switch
            {
                T[] arr => arr.Length == 0,
                ICollection<T> v => v.Count == 0,
                _ => !items.Any()
            };
        }
        #endregion

        #region Batch
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="batchSize"></param>
        /// <param name="skipBatches"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this
            IEnumerable<T> source,
            int batchSize,
            int skipBatches = 0)
            => BatchGroup(source, batchSize, skipBatches).Select(g => g.Batch);

        /// <summary>
        /// TODO: unit test this.
        /// Although I believe it may perform slower than the other implementation
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="source"></param>
        /// <param name="batchSize"></param>
        /// <param name="skipBatches"></param>
        /// <returns></returns>
        public static IEnumerable<(long Index, IEnumerable<TItem> Batch)> BatchGroup<TItem>(this
            IEnumerable<TItem> source,
            int batchSize,
            int skipBatches = 0)
        {
            ArgumentNullException.ThrowIfNull(source);

            return source
                .Select((item, index) => (item, index))
                .GroupBy(tuple => tuple.index / batchSize)
                .Select(group => group.Select(tuple => tuple.item))
                .Select((batch, index) => ((long)index, batch))
                .Skip(skipBatches);
        }
        #endregion

        #region Dictionary
        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TValue> valueProvider)
        {
            ArgumentNullException.ThrowIfNull(dictionary);
            ArgumentNullException.ThrowIfNull(valueProvider);

            if (!dictionary.TryGetValue(key, out var value))
                dictionary[key] = value = valueProvider.Invoke(key);

            return value;
        }
        #endregion
    }
}