
using System;
using System.Collections.Generic;
using System.Linq;
using Axis.Luna.Operation;
using System.Security.Cryptography;
using System.Diagnostics;

namespace Axis.Luna.Extensions
{
    [DebuggerStepThrough]
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Splices the enumerable at the specified POSITIVE index, making it the head of the enumerable, joining the old head at the tail
        /// e.g
        /// <para>
        ///  {1,2,3,4,5,6,7,8,9,0}, spliced at index 4, becomes {5,6,7,8,9,0,1,2,3,4}
        /// </para>
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="spliceIndex"></param>
        /// <returns></returns>
        public static IEnumerable<V> Splice<V>(this IEnumerable<V> enumerable, int spliceIndex)
        => enumerable.Skip(Math.Abs(spliceIndex)).Concat(enumerable.Take(Math.Abs(spliceIndex)));

        public static IEnumerable<V> AppendAt<V>(this IEnumerable<V> enumerable, int position, V value)
        {
            position.ThrowIf(p => p < 0, "invalid position");

            int pos = 0;
            foreach(var v in enumerable)
            {
                if (pos++ == position) yield return value;
                yield return v;
            }
        }

        public static IEnumerable<V> Append<V>(this IEnumerable<V> enumerable, V value) => enumerable.Concat(value.Enumerate());

        public static IEnumerable<V> UsingEach<V>(this IEnumerable<V> enumerable, Action<V> action)
        {
            foreach(var v in enumerable)
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
            if (enumerable == null) return false;
            else
            {
                int totalCount = 0,
                    trueCount = 0;

                foreach (var v in enumerable)
                {
                    totalCount++;
                    if (predicate(v)) trueCount++;
                }

                return totalCount > 0 && totalCount == trueCount;
            }
        }

        public static IEnumerable<KeyValuePair<K,V>> PairWith<K,V>(this IEnumerable<K> keys, IEnumerable<V> values)
        {
            using (var ktor = keys.GetEnumerator())
            using (var vtor = values.GetEnumerator())
            while (ktor.MoveNext() && vtor.MoveNext())
                yield return ktor.Current.ValuePair(vtor.Current);
        }

        public static IEnumerable<KeyValuePair<K,V>> PairWith<K,V>(this IEnumerable<K> keys, IEnumerable<V> values, bool padWithDefault)
        {
            if (!padWithDefault) return keys.PairWith(values);
            else
            {
                var list = new List<KeyValuePair<K, V>>();
                using (var ktor = keys.GetEnumerator())
                using (var vtor = values.GetEnumerator())
                {
                    while (ktor.MoveNext())
                        list.Add(ktor.Current.ValuePair(vtor.MoveNext() ? vtor.Current : default(V)));
                }
                return list;
            }
        }

        public static void ForAll<T>(this IEnumerable<T> enumerable, Action<long, T> loopAction)
        {
            var cnt = 0L;
            foreach (var t in enumerable) loopAction(cnt++, t);
        }
        public static void ForAll<T>(this IEnumerable<T> enumerable, Action<T> loopAction)
        {
            foreach (var t in enumerable) loopAction(t);
        }

        public static void Repeat(this long repetitions, Action<long> repeatAction)
        {
            for (long cnt = 0, limit = Math.Abs(repetitions); cnt < limit; cnt++)
                repeatAction(cnt);
        }

        public static IEnumerable<V> GenerateSequence<V>(this long repetitions, Func<long, V> generator)
        {
            for (long cnt = 0; cnt < repetitions; cnt++)
                yield return generator.Invoke(cnt);
        }
        public static IEnumerable<V> GenerateSequence<V>(this uint repetitions, Func<uint, V> generator)
        {
            for (uint cnt = 0; cnt < repetitions; cnt++)
                yield return generator.Invoke(cnt);
        }
        public static IEnumerable<V> GenerateSequence<V>(this int repetitions, Func<int, V> generator)
        {
            for (int cnt = 0; cnt < repetitions; cnt++)
                yield return generator.Invoke(cnt);
        }
        public static IEnumerable<V> GenerateSequence<V>(this ushort repetitions, Func<ushort, V> generator)
        {
            for (ushort cnt = 0; cnt < repetitions; cnt++)
                yield return generator.Invoke(cnt);
        }
        public static IEnumerable<V> GenerateSequence<V>(this short repetitions, Func<short, V> generator)
        {
            for (short cnt = 0; cnt < repetitions; cnt++)
                yield return generator.Invoke(cnt);
        }

        public static T GetOrAdd<T>(this ICollection<T> collection, Func<T, bool> predicate, Func<T> generator)
        {
            var value = collection.FirstOrDefault(predicate);
            if (EqualityComparer<T>.Default.Equals(value, default(T))) collection.Add(value = generator.Invoke());
            return value;
        }

        //convert the args to an enumerable
        public static IEnumerable<T> Enumerate<T>(this T value, params T[] args) => new T[] { value }.Concat(args);

        public static IEnumerable<T> Enumerate<T>(this T value, Func<T, ResolvedOperation<T>> generator)
        {
            T prev = value;
            List<T> enm = new List<T>();
            enm.Add(prev);
            ResolvedOperation<T> opt = null;
            while ((opt = generator(prev)).Succeeded == true) enm.Add(prev = opt.Result);

            return enm;
        }

        public static IEnumerable<T> Enumerate<T>(params T[] values) => new List<T>(values ?? new T[0]);

        public static int PositionOf<T>(this IEnumerable<T> @enum, T item, IEqualityComparer<T> equalityComparer = null)
        {
            var eqc = equalityComparer ?? EqualityComparer<T>.Default;

            int pos = 0;
            foreach(var x in @enum)
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
            TValue value;
            if (!@this.TryGetValue(key, out value))
            {
                @this.Add(key, value = valueFactory(key));
            }
            return value;
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
                    yield return (indx++).ValuePair(l.Cast<IEnumerable<T>>());
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

        public static IEnumerable<IQueryable<T>> Batch<T>(this IQueryable<T> source, int batchSize, int skipBatches = 0)
        => BatchGroup(source, batchSize, skipBatches).Select(g => g.Value);
        public static IEnumerable<KeyValuePair<int, IQueryable<T>>> BatchGroup<T>(this IQueryable<T> source, int batchSize, int skipBatches = 0)
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

        #region Sequence Page
        public static SequencePage<Data> Paginate<Data>(this IEnumerable<Data> sequence, int pageIndex, int pageSize)
            => new SequencePage<Data>(sequence.Skip(pageSize * pageIndex).Take(pageSize).ToArray(),
                                      sequence.Count(),
                                      pageSize,
                                      pageIndex);

        public static SequencePage<Data> Paginate<Data, OrderKey>(this IOrderedQueryable<Data> sequence, int pageIndex, int pageSize)
            => new SequencePage<Data>(sequence.Skip(pageSize * pageIndex).Take(pageSize).ToArray(),
                                      sequence.Count(),
                                      pageSize,
                                      pageIndex);
        #endregion
    }
}
