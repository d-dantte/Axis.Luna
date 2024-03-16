using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Axis.Luna.Extensions
{
    public static class EnumerableExtensions
    {
        #region Concatenations

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<TItem> Enumerate<TItem>(this TItem item) => new[] { item };

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="otherValue"></param>
        /// <returns></returns>
        public static IEnumerable<T> EnumerateWith<T>(this
            T value,
            T otherValue)
            => new[] { value, otherValue };

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IEnumerable<T> EnumerateWith<T>(this T item, params T[] items)
        {
            yield return item;
            foreach (var t in items)
            {
                yield return t;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IEnumerable<T> PrependTo<T>(this T item, IEnumerable<T> items)
        {
            yield return item;
            foreach (var t in items)
            {
                yield return t;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="initialValues"></param>
        /// <param name="otherValue"></param>
        /// <returns></returns>
        public static IEnumerable<T> Concat<T>(
            this IEnumerable<T> initialValues,
            params T[] otherValue)
            => Enumerable.Concat(initialValues, otherValue);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="items"></param>
        /// <param name="grouper"></param>
        /// <returns></returns>
        public static IEnumerable<IGrouping<TKey, TItem>> GroupBy<TKey, TItem>(
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="items"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static IEnumerable<TItem> JoinUsing<TItem>(
            this IEnumerable<IEnumerable<TItem>> items,
            params TItem[] delimiter)
        {
            return items
                .Join(delimiter)
                .SelectMany();
        }

        private static IEnumerable<IEnumerable<TItem>> Join<TItem>(
            this IEnumerable<IEnumerable<TItem>> items,
            params TItem[] delimiter)
        {
            using var enumerator = items.GetEnumerator();

            if (enumerator.MoveNext())
                yield return enumerator.Current;

            while (enumerator.MoveNext())
            {
                yield return delimiter;
                yield return enumerator.Current;
            }
        }

        /// <summary>
        /// Stuffs the given enumerable with the given value
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="items"></param>
        /// <param name="joiner"></param>
        /// <returns></returns>
        public static IEnumerable<TItem> JoinUsing<TItem>(this IEnumerable<TItem> items, TItem joiner)
        {
            var started = false;
            foreach (var item in items)
            {
                if (started)
                    yield return joiner;

                yield return item;
                started = true;
            }
        }
        #endregion

        #region First/Last
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T FirstOrThrow<T>(this IEnumerable<T> enumerable, Exception exception)
        {
            if (enumerable is null)
                throw new ArgumentNullException(nameof(enumerable));

            exception ??= new Exception();

            using var enumerator = enumerable.GetEnumerator();

            if (enumerator.MoveNext())
                return enumerator.Current;

            else return exception.Throw<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static TItem? FirstOrNull<TItem>(this IEnumerable<TItem> enumerable)
        where TItem : struct
        {
            if (enumerable is null)
                throw new ArgumentNullException(nameof(enumerable));

            var item = enumerable.FirstOrDefault();
            if (item.IsDefault())
                return null;

            return item;
        }

        /// <summary>
        /// Finds the first value that satisfies the provided predicate.
        /// if no values are found, return null.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static TValue? FirstOrNull<TValue>(this IEnumerable<TValue> enumerable, Func<TValue, bool> predicate)
        where TValue : struct
        {
            if (enumerable is null)
                throw new ArgumentNullException(nameof(enumerable));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return enumerable
                .Where(predicate)
                .FirstOrNull();
        }

        public static TItem? LastOrNull<TItem>(this IEnumerable<TItem> enumerable)
        where TItem : struct
        {
            if (enumerable is null)
                throw new ArgumentNullException(nameof(enumerable));

            var item = enumerable.LastOrDefault();
            if (item.IsDefault())
                return null;

            return item;
        }

        /// <summary>
        /// Finds the last value that satisfies the provided predicate.
        /// if no values are found, return null.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static TValue? LastOrNull<TValue>(this IEnumerable<TValue> enumerable, Func<TValue, bool> predicate)
        where TValue : struct
        {
            if (enumerable is null)
                throw new ArgumentNullException(nameof(enumerable));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return enumerable
                .Where(predicate)
                .LastOrNull();
        }
        #endregion

        #region Operate over each item

        /// <summary>
        /// Uses hard-casting on the individual values of the enumerable. This means it may throw <see cref="InvalidCastException"/>.
        /// Note that with boxed values, this cast may fail.
        /// </summary>
        /// <typeparam name="TOut">The type to be casted to</typeparam>
        /// <param name="enumerable"></param>
        /// <param name="enumerable">The enumerable</param>
        /// <returns>The casted enumerable</returns>
        /// <exception cref="ArgumentNullException">If the enumerable is null</exception>
        public static IEnumerable<TOut> HardCast<TOut>(this IEnumerable enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            foreach (var item in enumerable)
            {
                yield return (TOut)item;
            }
        }

        /// <summary>
        /// Uses hard-casting on the individual values of the enumerable. This means it may throw <see cref="InvalidCastException"/>.
        /// Note that with boxed values, this cast may fail.
        /// </summary>
        /// <typeparam name="TIn">The type of the enumerable</typeparam>
        /// <typeparam name="TOut">The type to be casted to</typeparam>
        /// <param name="enumerable">The enumerable</param>
        /// <returns>The casted enumerable</returns>
        /// <exception cref="ArgumentNullException">If the enumerable is null</exception>
        public static IEnumerable<TOut> HardCast<TIn, TOut>(this IEnumerable<TIn> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            return enumerable.Select(value => (TOut)(object)value);
        }

        /// <summary>
        /// Casts the given <see cref="IEnumerable"/> items into the supplied type
        /// </summary>
        /// <typeparam name="TOut">The type to be casted into</typeparam>
        /// <param name="items">the items</param>
        /// <returns>An enumerable with items casted to the supplied <typeparamref name="TOut"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<TOut> SelectAs<TOut>(this IEnumerable items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                yield return (TOut)item;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static IEnumerable<TOut> SelectMany<TOut>(this IEnumerable<IEnumerable<TOut>> enumerable) 
        => enumerable.SelectMany(enm => enm);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerable<V> WithEvery<V>(this IEnumerable<V> enumerable, Action<V> action)
        {
            foreach (var v in enumerable)
            {
                action(v);
                yield return v;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="loopAction"></param>
        public static void ForEvery<T>(this IEnumerable<T> enumerable, Action<long, T> loopAction)
        {
            var cnt = 0L;
            foreach (var t in enumerable)
                loopAction(cnt++, t);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="loopAction"></param>
        public static void ForEvery<T>(this
            IEnumerable<T> enumerable,
            Action<T> loopAction)
            => enumerable.ForEvery((_cnt, _v) => loopAction(_v));

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="loopAction"></param>
        /// <returns></returns>
        public static async Task ForEveryAsync<T>(this
            IEnumerable<T> enumerable,
            Func<long, T, Task> loopAction)
        {
            var cnt = 0L;
            foreach (var t in enumerable) await loopAction(cnt++, t);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="loopAction"></param>
        /// <returns></returns>
        public static async Task ForAllAsync<T>(this
            IEnumerable<T> enumerable,
            Func<T, Task> loopAction)
            => await enumerable.ForEveryAsync(async (_cnt, _v) => await loopAction(_v));


        /// <summary>
        /// "Zips" the two sequences into a single sequence of a tuple of corresponding items.
        /// The zipping operation stops when one of the sequences ends.
        /// </summary>
        /// <typeparam name="K">The item type for the first sequence</typeparam>
        /// <typeparam name="V">The item type for the second sequence</typeparam>
        /// <param name="first">The first sequence</param>
        /// <param name="second">The second sequence</param>
        /// <returns>A new sequence containing a tuple of corresponding items</returns>
        public static IEnumerable<(K, V)> Zip<K, V>(this IEnumerable<K> first, IEnumerable<V> second)
        {
            using var ktor = first.GetEnumerator();
            using var vtor = second.GetEnumerator();
            while (ktor.MoveNext() && vtor.MoveNext())
                yield return (ktor.Current, vtor.Current);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="keys"></param>
        /// <param name="values"></param>
        /// <param name="padWithDefault"></param>
        /// <returns></returns>
        public static IEnumerable<(K, V)> Zip<K, V>(this
            IEnumerable<K> keys,
            IEnumerable<V> values,
            bool padWithDefault)
        {
            return !padWithDefault ? keys.Zip(values) : keys.ZipWithDefaultPadding(values);
        }

        private static IEnumerable<(K, V)> ZipWithDefaultPadding<K, V>(this IEnumerable<K> keys, IEnumerable<V> values)
        {
            using var ktor = keys.GetEnumerator();
            using var vtor = values.GetEnumerator();
            while (ktor.TryGetNext(out K kvalue) | vtor.TryGetNext(out V vvalue))
            {
                yield return (kvalue, vvalue);
            }
        }

        #endregion

        #region Add/Remove items

        public static ICollection<Value> AddRange<Value>(this ICollection<Value> collection, IEnumerable<Value> values)
        {
            values.ForEvery(collection.Add);
            return collection;
        }

        public static TItems AddItem<TItems, TItem>(this TItems items, TItem item)
        where TItems : ICollection<TItem>
        {
            ArgumentNullException.ThrowIfNull(items);

            items.Add(item);
            return items;
        }

        public static TItems AddItems<TItems, TItem>(this TItems items, IEnumerable<TItem> addendum)
        where TItems : ICollection<TItem>
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(addendum);

            foreach (var item in addendum)
                items.Add(item);

            return items;
        }

        /// <summary>
        /// Append a value to a position within the enumerable.
        /// <para>Note that if the position is beyond the bounds of the enumerable, the value will never be added</para>
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="items"></param>
        /// <param name="position"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<V> InsertAt<V>(this IEnumerable<V> items, int position, V value)
        {
            position.ThrowIf(
                p => p < 0,
                _ => new ArgumentOutOfRangeException(nameof(position)));

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

        public static void RemoveAll<V>(this ICollection<V> collection, params V[] values)
        => values.ForEvery(v => collection.Remove(v));

        public static void RemoveAll<V>(this ICollection<V> collection, Func<V, bool> predicate)
        => collection.RemoveAll(collection.Where(predicate).ToArray());
        #endregion

        #region Get/Find Items
        public static T ItemAt<T>(this IEnumerable<T> enumerable, int index)
        {
            ArgumentNullException.ThrowIfNull(enumerable);

            if (index < 0)
                throw new IndexOutOfRangeException();

            else if (enumerable is IList<T> l)
                return l[index];

            else if (enumerable is T[] tarr)
                return tarr[index];

            else
            {
                int _index = 0;
                foreach (var item in enumerable)
                {
                    if (_index == index)
                        return item;
                }

                throw new IndexOutOfRangeException();
            }
        }

        public static TValue GetOrAdd<TKey, TValue>(this
            IDictionary<TKey, TValue> @this,
            TKey key,
            Func<TKey, TValue> valueFactory)
        {
            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            if (!@this.TryGetValue(key, out TValue value))
                @this.Add(key, value = valueFactory.Invoke(key));

            return value;
        }

        public static async Task<TValue> GetOrAddAsync<TKey, TValue>(this
            IDictionary<TKey, TValue> @this,
            TKey key,
            Func<TKey, Task<TValue>> valueFactory)
        {
            if (!@this.TryGetValue(key, out TValue value))
                @this.Add(key, value = await valueFactory(key));

            return value;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (!@this.TryGetValue(key, out var value))
                return default;

            return value;
        }

        public static bool TryGetValue(this
            IDictionary dictionary,
            object key,
            out object value)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            if (dictionary.Contains(key))
            {
                value = dictionary[key];
                return true;
            }

            value = null;
            return false;
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
        public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> sequence, int choices)
        => choices == 0
           ? new[] { new T[0] }
           : sequence.SelectMany((e, i) =>
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
                return new T[][] { new T[] { values.First() } };

            else
            {
                return values
                    .SelectMany((value, index) =>
                    {
                        var primary = new[] { value };
                        return EnumerableExtensions
                            .Permutations(Splice(values, index))
                            .Select(perm =>
                            {
                                return primary.Concat(perm).ToList() as IEnumerable<T>;
                            });
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
        /// Snips the enumerable at the specified POSITIVE index, making it the head of the enumerable, splicing the old head at the tail
        /// e.g
        /// <para>
        ///  {1,2,3,4,5,6,7,8,9,0}, spliced at index 4, becomes {5,6,7,8,9,0,1,2,3,4}
        /// </para>
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="spliceIndex"></param>
        /// <returns></returns>
        public static IEnumerable<V> SnipSplice<V>(this IEnumerable<V> enumerable, int spliceIndex)
        {
            var array = enumerable.ToArray();
            return array.Skip(Math.Abs(spliceIndex)).Concat(array.Take(Math.Abs(spliceIndex)));
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
        /// Skips every <c>skipCount</c> number of elements and takes one element after skipping. The process starts by skipping
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <param name="skipCount"></param>
        /// <param name="whilenot">a condition that must be false for the skip process to happen</param>
        /// <returns></returns>
        public static IEnumerable<T> SkipEvery<T>(this
            IEnumerable<T> sequence,
            int skipCount,
            Func<long, T, bool> whilenot = null)
        {
            var count = -1L;
            var mod = skipCount + 1;
            foreach (var t in sequence)
            {
                ++count;
                if (whilenot?.Invoke(count, t) ?? false) yield return t;
                else if ((count + 1) % mod == 0) yield return t;
            }
        }

        /// <summary>
        /// Skips the last <c>count</c> elements of a sequence
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int count = 1)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            //else
            var sourceEnumerator = source.GetEnumerator();
            var cache = new Queue<T>(count + 1);

            bool hasRemainingItems;
            do
            {
                if (hasRemainingItems = sourceEnumerator.MoveNext())
                {
                    cache.Enqueue(sourceEnumerator.Current);
                    if (cache.Count > count)
                        yield return cache.Dequeue();
                }
            } while (hasRemainingItems);
        }

        /// <summary>
        /// Takes every <c>takeCount</c> elements, and skips one element. The method starts by taking elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <param name="takeCount"></param>
        /// <param name="whilenot">a condition that must be false for the skip process to happen</param>
        /// <returns></returns>
        public static IEnumerable<T> TakeEvery<T>(
            this IEnumerable<T> sequence,
            int takeCount,
            Func<long, T, bool> whilenot = null)
        {
            var count = -1L;
            var mod = takeCount + 1;
            foreach (var t in sequence)
            {
                ++count;
                if (whilenot?.Invoke(count, t) ?? false) yield return t;
                else if ((count + 1) % mod != 0) yield return t;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerator"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetNext<T>(this IEnumerator<T> enumerator, out T value)
        {
            if (enumerator.MoveNext())
            {
                value = enumerator.Current;
                return true;
            }
            else
            {
                value = default;
                return false;
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
            => EnumerableExtensions
                .BatchGroup(source, batchSize, skipBatches)
                .Select(g => g.Batch);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="batchSize"></param>
        /// <param name="skipBatches"></param>
        /// <returns></returns>
        public static IEnumerable<(long Index, IEnumerable<T> Batch)> BatchGroup<T>(this
            IEnumerable<T> source,
            int batchSize,
            int skipBatches = 0)
        {
            long count = 0;
            long index = 0;
            var batch = new List<T>();

            foreach (var value in source.Skip(skipBatches * batchSize))
            {
                batch.Add(value);

                if (++count % batchSize == 0)
                {
                    yield return (index++, batch.AsEnumerable());

                    batch = new List<T>();
                }
            }

            if (batch.Count > 0)
                yield return (index++, batch.AsEnumerable());
        }

        /// <summary>
        /// TODO: unit test this.
        /// Although I believe it may perform slower than the other implementation
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="source"></param>
        /// <param name="batchSize"></param>
        /// <param name="skipBatches"></param>
        /// <returns></returns>
        public static IEnumerable<(long Index, IEnumerable<TItem> Batch)> BatchGroup_<TItem>(this
            IEnumerable<TItem> source,
            int batchSize,
            int skipBatches = 0)
        {
            return source
                .Select((item, index) => (item, index))
                .GroupBy(tuple => tuple.index / batchSize)
                .Select(group => group.Select(tuple => tuple.item))
                .Select((batch, index) => ((long)index, batch))
                .Skip(skipBatches);
        }
        #endregion

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
    }
}