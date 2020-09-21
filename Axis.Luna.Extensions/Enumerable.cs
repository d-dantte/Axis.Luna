using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using static Axis.Luna.Extensions.Async;

namespace Axis.Luna.Extensions
{

    //[DebuggerStepThrough]
    public static class EnumerableExtensions
    {
        public static bool ContainsAll<V>(this IEnumerable<V> enumerable, IEnumerable<V> items)
        {
            var itemArray = items.ToArray();
            return enumerable.Intersect(itemArray).Count() == itemArray.Length;
        }

        public static bool IsSubsetOf<V>(this IEnumerable<V> subset, IEnumerable<V> enumerable)
        {
            var subsetArr = subset.ToArray(); //inevitable
            if (subsetArr.Length == 0) return true;
            var searchIndex = 0;
            var comparer = EqualityComparer<V>.Default;
            foreach (var t in enumerable)
            {
                if (searchIndex == subsetArr.Length) break;
                else if (comparer.Equals(t, subsetArr[searchIndex])) searchIndex++;
                else searchIndex = 0;
            }

            return searchIndex == subsetArr.Length;
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

        public static IEnumerable<V> AppendAt<V>(this IEnumerable<V> enumerable, int position, V value)
        {
            position.ThrowIf(p => p < 0, "invalid position");

            int pos = 0;
            foreach (var v in enumerable)
            {
                if (pos++ == position) yield return value;
                yield return v;
            }
        }
        
        public static IEnumerable<V> WithEach<V>(this IEnumerable<V> enumerable, Action<V> action)
        {
            foreach (var v in enumerable)
            {
                action(v);
                yield return v;
            }
        }

        public static IEnumerable<Task<V>> WithEach<V>(this IEnumerable<V> enumerable, Func<V, Task> action)
        {
            foreach (var v in enumerable)
            {
                var task = action(v);
                yield return task.ContinueWith(_t =>
                {
                    _t.GetAwaiter().GetResult(); //throw exception if faulted
                    return v;
                });
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

        public static IEnumerable<KeyValuePair<K, V>> PairWith<K, V>(this IEnumerable<K> keys, IEnumerable<V> values)
        {
            using (var ktor = keys.GetEnumerator())
            using (var vtor = values.GetEnumerator())
            {
                while (ktor.MoveNext() && vtor.MoveNext())
                    yield return ktor.Current.ValuePair(vtor.Current);
            }
        }

        public static IEnumerable<KeyValuePair<K, V>> PairWith<K, V>(this IEnumerable<K> keys, IEnumerable<V> values, bool padWithDefault)
        {
            return !padWithDefault ? keys.PairWith(values) : keys.PairWithPad(values);
        }

        private static IEnumerable<KeyValuePair<K, V>> PairWithPad<K, V>(this IEnumerable<K> keys, IEnumerable<V> values)
        {
            using (var ktor = keys.GetEnumerator())
            using (var vtor = values.GetEnumerator())
            {
                while (ktor.MoveNext())
                    yield return ktor.Current.ValuePair(vtor.MoveNext() ? vtor.Current : default(V));
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

        public static IEnumerable<V> GenerateSequence<V>(this long repetitions, Func<long, V> generator)
        {
            for (long cnt = 0; cnt < repetitions; cnt++)
                yield return generator.Invoke(cnt);
        }

        public static IEnumerable<Task<V>> GenerateSequenceAsync<V>(this long repetitions, Func<long, Task<V>> generator)
        {
            for (long cnt = 0; cnt < repetitions; cnt++)
                yield return generator.Invoke(cnt);
        }

        public static T GetOrAdd<T>(this ICollection<T> collection, Func<T, bool> predicate, Func<T> generator)
        {
            var value = collection.FirstOrDefault(predicate);
            if (EqualityComparer<T>.Default.Equals(value, default(T))) collection.Add(value = generator.Invoke());
            return value;
        }

        public static async Task<T> GetOrAddAsync<T>(this ICollection<T> collection, Func<T, bool> predicate, Func<Task<T>> generator)
        {
            T value;
            if (collection.Any(predicate)) return collection.First(predicate);
            else
            {
                collection.Add(value = await generator.Invoke());
                return value;
            }
        }

        //convert the args to an enumerable
        public static IEnumerable<T> Enumerate<T>(this T value, params T[] args) => new T[] { value }.Concat(args);

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
            else return new List<T>(enumerable)[index];
        }

        public static ICollection<Value> AddRange<Value>(this ICollection<Value> collection, IEnumerable<Value> values)
        {
            values.ForAll(collection.Add);
            return collection;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (!@this.TryGetValue(key, out TValue value))
                @this.Add(key, value = valueFactory(key));

            return value;
        }

        public static async Task<TValue> GetOrAddAsync<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, Task<TValue>> valueFactory)
        {
            return await AsyncLock(async () =>
            {
                if (!@this.TryGetValue(key, out TValue value))
                    @this.Add(key, value = await valueFactory(key));

                return value;
            });
        }

        public static void RemoveAll<V>(this ICollection<V> collection, params V[] values)
        => values.ForAll(v => collection.Remove(v));

        public static void RemoveAll<V>(this ICollection<V> collection, Func<V, bool> predicate)
        => collection.RemoveAll(collection.Where(predicate).ToArray());

        public static Dictionary<K, V> AddAll<K, V>(this Dictionary<K, V> dict, IEnumerable<KeyValuePair<K, V>> values)
        {
            foreach (var v in values) dict.Add(v.Key, v.Value);
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
        /// 
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
                        return Permutations(Splice(values, index))
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
        /// <returns></returns>
        public static IEnumerable<V> Shuffle<V>(this IEnumerable<V> source)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var buffer = source.ToArray();
                for (int i = 0; i < buffer.Length; i++)
                {
                    int j = rng.RandomInt(i, buffer.Length);
                    yield return buffer[j];

                    buffer[j] = buffer[i];
                }
            }
        }

        public static IEnumerable<V> SelectWhen<V>(this IEnumerable<V> sequence, Func<V, bool> predicate, Func<V, V> projection)
        {
            foreach (var v in sequence)
            {
                yield return predicate?.Invoke(v) ?? false ?
                             projection(v) :
                             v;
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
            bool hasRemainingItems = false;
            var cache = new Queue<T>(count + 1);

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
        public static IEnumerable<T> TakeEvery<T>(this IEnumerable<T> sequence, int takeCount, Func<long, T, bool> whilenot = null)
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

        public static IEnumerable<KeyValuePair<int, IEnumerable<T>>> BatchGroup<T>(this IEnumerable<T> source, int batchSize, int skipBatches = 0)
        {
            batchSize = Math.Abs(batchSize);
            int indx = Math.Abs(skipBatches);
            IEnumerable<T> result = source ?? new T[0];

            using (var enumerator = result.Skip(indx * batchSize).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    //cache the items before loading the kvp
                    var l = enumerator.enumerateSome(batchSize).ToList();
                    yield return (indx++).ValuePair(l.As<IEnumerable<T>>());
                }
            }
        }

        private static IEnumerable<T> enumerateSome<T>(this IEnumerator<T> enumerator, int count)
        {
            yield return enumerator.Current;

            for (int i = 1; i < count; i++)
            {
                if (!enumerator.MoveNext()) yield break;
                else yield return enumerator.Current;
            }
        }

        public static int BatchCount<T>(this IEnumerable<T> source, int batchSize)
        => (int)Math.Round(((double)source.Count()) / batchSize, MidpointRounding.AwayFromZero);

        public static IEnumerable<IQueryable<T>> Batch<T>(this IOrderedQueryable<T> source, int batchSize, int skipBatches = 0)
        => BatchGroup(source, batchSize, skipBatches).Select(g => g.Value);

        public static IEnumerable<KeyValuePair<int, IQueryable<T>>> BatchGroup<T>(this IOrderedQueryable<T> source, int batchSize, int skipBatches = 0)
        {
            batchSize = Math.Abs(batchSize);
            int indx = Math.Abs(skipBatches);
            IQueryable<T> result = source ?? new T[0].AsQueryable();
            do
            {
                result = source.Skip(indx * batchSize).Take(batchSize);
                if (result.Count() > 0) yield return new KeyValuePair<int, IQueryable<T>>(indx++, result);
                else break;
            }
            while (true);
        }

        public static int BatchCount<T>(this IQueryable<T> source, int batchSize)
        => (int)Math.Round(((double)source.Count()) / batchSize, MidpointRounding.AwayFromZero);
        #endregion
    }
}