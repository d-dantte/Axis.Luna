using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Axis.Luna.Extensions
{
    public static class EnumerableExtensions
    {
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

        public static bool IsEmpty<T>(this T[] array)
            => array
                .ThrowIfNull(new ArgumentNullException(nameof(array)))
                .Length == 0;

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null
                || array.IsEmpty();
        }

        /// <summary>
        /// Uses hard-casting on the individual values of the enumerable. This means it may throw <see cref="InvalidCastException"/>.
        /// Note that with boxed values, this cast may fail.
        /// </summary>
        /// <typeparam name="TOut">The type to be casted to</typeparam>
        /// <param name="enumerable"></param>
        /// <param name="enumerable">The enumerable</param>
        /// <returns>The casted enumerable</returns>
        /// <exception cref="ArgumentNullException">If the enumerable is null</exception>
        public static IEnumerable<TOut> HardCast<TOut>(this System.Collections.IEnumerable enumerable)
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
        /// Get a slice/chunk of an array
        /// </summary>
        /// <typeparam name="T">The type of the array</typeparam>
        /// <param name="array">The array</param>
        /// <param name="offset">The offset at which the slice is made, i.e, how many elements to skip</param>
        /// <param name="length"> The length of the slice/chunk. <c>null</c> indicates using whatever length remains after the offset </param>
        /// <returns></returns>
        public static ArraySegment<T> Slice<T>(this T[] array, int offset, int? length = null)
        {
            return new ArraySegment<T>(array, offset, length ?? array.Length - offset);
        }

        /// <summary>
        /// Splits an array into 2 <see cref="ArraySegment{T}"/> instances, using the given index as a pivot. Note that the element at the pivot index will always
        /// become the first element in the "right" <see cref="ArraySegment{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="pivotIndex"></param>
        /// <returns></returns>
        public static (ArraySegment<T>, ArraySegment<T>) Split<T>(this T[] array, int pivotIndex)
        {
            ArraySegment<T> segment = array;
            return segment.Split(pivotIndex);
        }

        /// <summary>
        /// Splits an input into 2 <see cref="ArraySegment{T}"/> instances, using the given index as a pivot. Note that the element at the pivot index will always
        /// become the first element in the "right" <see cref="ArraySegment{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="pivotIndex"></param>
        /// <returns></returns>
        public static (ArraySegment<T>, ArraySegment<T>) Split<T>(this ArraySegment<T> segment, int pivotIndex)
        {
            return (segment.Slice(0, pivotIndex), segment.Slice(pivotIndex));
        }
        public static T[] ArrayOf<T>(params T[] values) => values;

        public static bool ContainsAll<V>(this IEnumerable<V> superSet, IEnumerable<V> subSet)
        {
            return subSet.IsSubsetOf(superSet);
        }

        public static bool IsSubsetOf<V>(this IEnumerable<V> subset, IEnumerable<V> superSet)
        {
            return !subset.Except(superSet).Any();
        }

        public static IEnumerable<TOut> SelectMany<TOut>(this IEnumerable<IEnumerable<TOut>> enumerable) 
        => enumerable.SelectMany(enm => enm);


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
            position.ThrowIf(p => p < 0, new ArgumentException($"Invalid position: {position}"));

            int pos = 0;
            foreach (var v in items)
            {
                if (pos++ == position) yield return value;
                yield return v;
            }

            // adding at the end
            if (pos == position)
                yield return value;

            // items was empty
            if (pos == 0)
                yield return value;
        }
        
        public static IEnumerable<V> WithEach<V>(this IEnumerable<V> enumerable, Action<V> action)
        {
            foreach (var v in enumerable)
            {
                action(v);
                yield return v;
            }
        }

        /// <summary>
        /// Does the same thing as <c>Enumerable.All(...)</c>, with the exception that if the sequence is empty, it returns false.
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool ExactlyAll<V>(this IEnumerable<V> enumerable, Func<V, bool> predicate)
        {
            long count = 0;
            Func<V, bool> xpredicate = _v =>
            {
                count++;
                return predicate(_v);
            };

            if (enumerable == null) return false;
            else return enumerable.All(xpredicate) && count > 0;
        }

        public static IEnumerable<(K, V)> PairWith<K, V>(this IEnumerable<K> keys, IEnumerable<V> values)
        {
            using var ktor = keys.GetEnumerator();
            using var vtor = values.GetEnumerator();
            while (ktor.MoveNext() && vtor.MoveNext())
                yield return (ktor.Current, vtor.Current);
        }

        public static IEnumerable<(K, V)> PairWith<K, V>(this IEnumerable<K> keys, IEnumerable<V> values, bool padWithDefault)
        {
            return !padWithDefault ? keys.PairWith(values) : keys.PairWithPad(values);
        }

        private static IEnumerable<(K, V)> PairWithPad<K, V>(this IEnumerable<K> keys, IEnumerable<V> values)
        {
            using var ktor = keys.GetEnumerator();
            using var vtor = values.GetEnumerator();
            while(ktor.TryGetNext(out K kvalue) | vtor.TryGetNext(out V vvalue))
            {
                yield return (kvalue, vvalue);
            }
        }

        public static void ForAll<T>(this IEnumerable<T> enumerable, Action<long, T> loopAction)
        {
            var cnt = 0L;
            foreach (var t in enumerable) loopAction(cnt++, t);
        }

        public static void ForAll<T>(this IEnumerable<T> enumerable, Action<T> loopAction) => enumerable.ForAll((_cnt, _v) => loopAction(_v));

        public static async Task ForAllAsync<T>(this IEnumerable<T> enumerable, Func<long, T, Task> loopAction)
        {
            var cnt = 0L;
            foreach (var t in enumerable) await loopAction(cnt++, t);
        }

        public static async Task ForAllAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> loopAction) => await enumerable.ForAllAsync(async (_cnt, _v) => await loopAction(_v));

        public static void Repeat(this long repetitions, Action<long> repeatAction)
        {
            for (long cnt = 0, limit = Math.Abs(repetitions); cnt < limit; cnt++)
                repeatAction(cnt);
        }

        public static async Task RepeatAsync(this long repetitions, Func<long, Task> repeatAction)
        {
            for (long cnt = 0, limit = Math.Abs(repetitions); cnt < limit; cnt++)
                await repeatAction(cnt);
        }

        public static T GetOrAdd<T>(this ICollection<T> collection, Func<T, bool> predicate, Func<T> generator)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            foreach(var t in collection)
            {
                if (predicate.Invoke(t))
                    return t;
            }

            var newValue = generator.Invoke();
            collection.Add(newValue);

            return newValue;
        }

        public static async Task<T> GetOrAddAsync<T>(this ICollection<T> collection, Func<T, bool> predicate, Func<Task<T>> generator)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            foreach (var t in collection)
            {
                if (predicate.Invoke(t))
                    return t;
            }

            var newValue = await generator.Invoke();
            collection.Add(newValue);

            return newValue;
        }

        #region Concatenations
        public static IEnumerable<TItem> Enumerate<TItem>(this TItem item) => new[] { item };

        public static IEnumerable<T> EnumerateWith<T>(this T value, T otherValue)
        {
            yield return value;
            yield return otherValue;
        }

        public static IEnumerable<T> PrependTo<T>(this T item, IEnumerable<T> items)
        {
            yield return item;
            foreach (var t in items)
            {
                yield return t;
            }
        }

        public static IEnumerable<T> EnumerateWith<T>(this T item, params T[] items)
        {
            yield return item;
            foreach (var t in items)
            {
                yield return t;
            }
        }

        public static IEnumerable<T> Concat<T>(
            this IEnumerable<T> initialValues,
            params T[] otherValue)
            => Enumerable.Concat(initialValues, otherValue);
        #endregion

        public static int PositionOf<T>(this IEnumerable<T> enumerable, T item, IEqualityComparer<T> equalityComparer = null)
        {
            var eqc = equalityComparer ?? EqualityComparer<T>.Default;

            int pos = 0;
            foreach (var x in enumerable)
            {
                if (eqc.Equals(x, item)) return pos;
                else ++pos;
            }
            return -1;
        }

        public static T ItemAt<T>(this IEnumerable<T> enumerable, int index)
        {
            if (index < 0) throw new IndexOutOfRangeException();
            else if (enumerable is IList<T>) return (enumerable as IList<T>)[index];
            else if (enumerable is Array) return (T)((enumerable as Array).GetValue(index));
            else
            {
                int _index = 0;
                foreach(var item in enumerable)
                {
                    if (_index == index)
                        return item;
                }

                throw new IndexOutOfRangeException();
            }
        }

        public static ICollection<Value> AddRange<Value>(this ICollection<Value> collection, IEnumerable<Value> values)
        {
            values.ForAll(collection.Add);
            return collection;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            if (!@this.TryGetValue(key, out TValue value))
                @this.Add(key, value = valueFactory.Invoke(key));

            return value;
        }

        public static async Task<TValue> GetOrAddAsync<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, Task<TValue>> valueFactory)
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
            System.Collections.IDictionary dictionary,
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

        public static void RemoveAll<V>(this ICollection<V> collection, params V[] values)
        => values.ForAll(v => collection.Remove(v));

        public static void RemoveAll<V>(this ICollection<V> collection, Func<V, bool> predicate)
        => collection.RemoveAll(collection.Where(predicate).ToArray());

        public static Dictionary<K, V> AddAll<K, V>(this Dictionary<K, V> dict, IEnumerable<KeyValuePair<K, V>> values)
        {
            foreach (var v in values) 
                dict.Add(v.Key, v.Value);

            return dict;
        }

        public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> values) => new Dictionary<K, V>().AddAll(values);

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

        static private T[] Splice<T>(IEnumerable<T> list, int index)
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
        /// Skips every <c>skipCount</c> number of elements and takes one element after skipping. The process starts by skipping
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <param name="skipCount"></param>
        /// <param name="whilenot">a condition that must be false for the skip process to happen</param>
        /// <returns></returns>
        public static IEnumerable<T> SkipEvery<T>(this IEnumerable<T> sequence, int skipCount, Func<long, T, bool> whilenot = null)
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


        #region Batch
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize, int skipBatches = 0)
            => BatchGroup(source, batchSize, skipBatches).Select(g => g.Value);

        public static IEnumerable<KeyValuePair<long, IEnumerable<T>>> BatchGroup<T>(this IEnumerable<T> source, int batchSize, int skipBatches = 0)
        {
            long count = 0;
            long index = 0;
            var batch = new List<T>();

            foreach (var value in source.Skip(skipBatches * batchSize))
            {
                batch.Add(value);

                if (++count % batchSize == 0)
                {
                    yield return index++.ValuePair(batch.AsEnumerable());

                    batch = new List<T>();
                }
            }

            if (batch.Count > 0)
                yield return index++.ValuePair(batch.AsEnumerable());
        }

        /// <summary>
        /// TODO: unit test this
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="source"></param>
        /// <param name="batchSize"></param>
        /// <param name="skipBatches"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<long, IEnumerable<TItem>>> BatchGroup_<TItem>(this IEnumerable<TItem> source, int batchSize, int skipBatches = 0)
        {
            return source
                .Select((item, index) => (item, index))
                .GroupBy(tuple => tuple.index / batchSize)
                .Select(group => group.Select(tuple => tuple.item))
                .Select((batch, index) => KeyValuePair.Create((long)index, batch));
        }

        // This should be deprecated
        public static IEnumerable<IQueryable<T>> Batch<T>(this IOrderedQueryable<T> source, int batchSize, int skipBatches = 0)
        => BatchGroup(source, batchSize, skipBatches).Select(g => g.Value);

        // This should be deprecated
        public static IEnumerable<KeyValuePair<long, IQueryable<T>>> BatchGroup<T>(this IOrderedQueryable<T> source, int batchSize, int skipBatches = 0)
        {
            long count = 0;
            long index = 0;
            var batch = new List<T>();

            foreach (var value in source.Skip(skipBatches * batchSize))
            {
                batch.Add(value);

                if (++count % batchSize == 0)
                {
                    yield return index++.ValuePair(batch.AsQueryable());

                    batch = new List<T>();
                }
            }

            if (batch.Count > 0)
                yield return index++.ValuePair(batch.AsQueryable());
        }
        #endregion

        public static bool TryGetNext<T>(this IEnumerator<T> enumerator, out T value)
        {
            if(enumerator.MoveNext())
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