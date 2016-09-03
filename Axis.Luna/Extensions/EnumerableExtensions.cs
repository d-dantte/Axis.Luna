using static Axis.Luna.Extensions.ObjectExtensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Axis.Luna.Extensions
{
    public static class EnumerableExtensions
    {
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

        public static IEnumerable<Out> Transform<In, Out>(this IQueryable<In> query, Func<In, Out> transformation)
        {
            foreach (var qin in query) yield return transformation(qin);
        }
        public static IEnumerable<V> UsingEach<V>(this IEnumerable<V> enumerable, Action<V> action)
        {
            foreach(var v in enumerable)
            {
                action(v);
                yield return v;
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
        public static void Repeat(this long repetitions, Action<long> repeatAction)
        {
            for (long cnt = 0, limit = Math.Abs(repetitions); cnt < limit; cnt++)
                repeatAction(cnt);
        }

        public static IEnumerable<V> GenerateSequence<V>(this long repetitions, Func<long, V> generator)
        {
            using (var entor = new CountdownEnumerator(repetitions))
                while (entor.MoveNext()) yield return generator.Invoke(entor.Current);
        }
        public static IEnumerable<V> GenerateSequence<V>(this uint repetitions, Func<uint, V> generator)
        {
            using (var entor = new CountdownEnumerator(repetitions))
                while (entor.MoveNext()) yield return generator.Invoke((uint)entor.Current);
        }
        public static IEnumerable<V> GenerateSequence<V>(this int repetitions, Func<int, V> generator)
        {
            using (var entor = new CountdownEnumerator(repetitions))
                while (entor.MoveNext()) yield return generator.Invoke((int)entor.Current);
        }
        public static IEnumerable<V> GenerateSequence<V>(this ushort repetitions, Func<ushort, V> generator)
        {
            using (var entor = new CountdownEnumerator(repetitions))
                while (entor.MoveNext()) yield return generator.Invoke((ushort)entor.Current);
        }
        public static IEnumerable<V> GenerateSequence<V>(this short repetitions, Func<short, V> generator)
        {
            using (var entor = new CountdownEnumerator(repetitions))
                while (entor.MoveNext()) yield return generator.Invoke((short)entor.Current);
        }

        public static T AddAndGet<T>(this ICollection<T> collection, T item)
        {
            collection.Add(item);
            return item;
        }
        public static T GetOrAdd<T>(this ICollection<T> collection, Func<T, bool> predicate, Func<T> generator)
        {
            var value = collection.FirstOrDefault(predicate);
            if (EqualityComparer<T>.Default.Equals(value, default(T))) collection.Add(value = generator.Invoke());
            return value;
        }

        //convert the args to an enumerable
        public static IEnumerable<T> Enumerate<T>(this T value, params T[] args) => new T[] { value }.Concat(args);

        public static IEnumerable<T> Enumerate<T>(this T value, Func<T, Operation<T>> generator)
        {
            T prev = value;
            List<T> enm = new List<T>();
            enm.Add(prev);
            Operation<T> opt = null;
            while (Eval(() => (opt = generator(prev)).Succeeded)) enm.Add(prev = opt.Result);

            return enm;
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
            values.ToList().ForEach(v => collection.Add(v));
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
            => values.ToList().ForEach(v => collection.Remove(v));

        public static void RemoveAll<V>(this ICollection<V> collection, Func<V, bool> predicate)
            => collection.RemoveAll(collection.Where(v => predicate(v)).ToArray());

        public static Dictionary<K, V> AddAll<K, V>(this Dictionary<K, V> dict, IEnumerable<KeyValuePair<K, V>> values)
        {
            foreach (var v in values) dict.Add(v.Key, v.Value);
            return dict;
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

        #region Match Expressions on an IEnumerable
        public static MatchExpressionEnumerable<In, Out> When<In, Out>(this IEnumerable<In> @this,
                                                                       Func<In, bool> predicate,
                                                                       Func<In, Out> projection)
            => new MatchExpressionEnumerable<In, Out>(@this, new MatchExpression<In, Out>
            {
                Predicate = predicate,
                Projection = projection
            });

        public static MatchExpressionEnumerable<In, Out> When<In, Out>(this MatchExpressionEnumerable<In, Out> @this,
                                                                       Func<In, bool> predicate,
                                                                       Func<In, Out> projection)
            => @this.AddExpression(new MatchExpression<In, Out> { Predicate = predicate, Projection = projection });

        public static IEnumerable<Out> ElseProject<In, Out>(this MatchExpressionEnumerable<In, Out> @this,
                                                            Func<In, Out> projection)
            => @this.AddExpression(new MatchExpression<In, Out> { Predicate = x => true, Projection = projection });
        #endregion

        #region Sequence Page
        public static SequencePage<Data> Paginate<Data>(this IEnumerable<Data> sequence, int pageIndex, int pageSize)
            => new SequencePage<Data>(sequence.Skip(pageSize * pageIndex).Take(pageSize).ToArray(),
                                      pageIndex,
                                      pageSize,
                                      sequence.Count());

        public static SequencePage<Data> Paginate<Data, OrderKey>(this IOrderedQueryable<Data> sequence, int pageIndex, int pageSize)
            => new SequencePage<Data>(sequence.Skip(pageSize * pageIndex).Take(pageSize).ToArray(),
                                      pageIndex,
                                      pageSize,
                                      sequence.Count());
        #endregion
    }

    /// <summary>
    /// I believe something like this should already exist...
    /// </summary>
    public class CountdownEnumerator : IEnumerator<long>
    {
        /// <summary>
        /// limit always resolves to a positive number
        /// </summary>
        /// <param name="count"></param>
        public CountdownEnumerator(long count)
        {
            Count = Math.Abs(count);
            _finalIndex = Count - 1;
            Reset();
        }

        private long _finalIndex;
        public long Count { get; private set; }

        private long _current;
        public long Current
        {
            get
            {
                if (_disposed) throw new Exception("Enumerator is disposed");
                else if (_current < 0) throw new Exception("Enumeration has not started");
                else return _current;
            }
        }
        object IEnumerator.Current => this.Current;

        private volatile bool _disposed = false;
        public void Dispose() => _disposed = true;

        public bool MoveNext()
        {
            if (_disposed) throw new Exception("Enumerator is disposed");
            else if (_current == _finalIndex) return false;
            //else

            ++_current;
            return true;
        }

        public void Reset()
        {
            if (_disposed) throw new Exception("Enumerator is disposed");
            this._current = -1;
        }
    }


    public class MatchExpressionEnumerable<In, Out> : IEnumerable<Out>
    {
        private IEnumerable<In> _originalEnumerable { get; set; }
        internal List<MatchExpression<In, Out>> _mexps = new List<MatchExpression<In, Out>>();

        internal MatchExpressionEnumerable(IEnumerable<In> enumerable, params MatchExpression<In, Out>[] exps)
        {
            _mexps.AddRange(exps);
            _originalEnumerable = enumerable;
        }

        internal MatchExpressionEnumerable<In, Out> AddExpression(MatchExpression<In, Out> expression)
        {
            if (expression == null) throw new ArgumentException(nameof(expression));

            _mexps.Add(expression);
            return this;
        }

        public IEnumerator<Out> GetEnumerator() => new Enumerator { _enumerator = _originalEnumerable.GetEnumerator(), _parent = this };

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal class Enumerator : IEnumerator<Out>
        {
            internal IEnumerator<In> _enumerator { get; set; }
            internal MatchExpressionEnumerable<In, Out> _parent { get; set; }

            public Out Current
            {
                get
                {
                    var _in = _enumerator.Current;
                    var exp = _parent._mexps
                                     .FirstOrDefault(mexp => mexp.Predicate(_in));
                    if (exp == null) return _in.As<Out>();
                    else return exp.Projection(_in);
                }
            }

            object IEnumerator.Current => Current;
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator.Reset();
            public void Dispose() => _enumerator.Dispose();
        }
    }

    public class MatchExpression<In, Out>
    {
        internal MatchExpression()
        { }

        public virtual Func<In, bool> Predicate { get; internal set; }
        public Func<In, Out> Projection { get; internal set; }
    }
}
