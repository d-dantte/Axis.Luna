using System;
using System.Collections.Generic;
using System.Linq;
using Axis.Luna.Common.Indexers;
using Axis.Luna.Common.Segments;

namespace Axis.Luna.Common
{
    public interface IIndexableSequence<TValue>:
        IReadonlyIndexer<Index, TValue>,
        IReadonlyIndexer<Range, IIndexableSequence<TValue>>,
        ICountable
    {
        internal readonly struct ArrayIndexableSequenceWrapper :
            IIndexableSequence<TValue>,
            IDefaultValueProvider<ArrayIndexableSequenceWrapper>
        {
            private readonly TValue[] _array;

            internal ArrayIndexableSequenceWrapper(TValue[] array)
            {
                ArgumentNullException.ThrowIfNull(array);
                _array = array;
            }

            public static implicit operator ArrayIndexableSequenceWrapper(TValue[] array) => new(array);

            public TValue this[Index key]
                => IsDefault
                ? throw new InvalidOperationException($"Indexing a default instance is forbidden")
                : _array[key];

            public IIndexableSequence<TValue> this[Range key]
                => IsDefault
                ? throw new InvalidOperationException($"Indexing a default instance is forbidden")
                : new ArrayIndexableSequenceWrapper(_array[key]);

            public int Count => IsDefault ? 0 : _array.Length;

            public bool IsDefault => _array is null;

            public static IIndexableSequence<TValue>.ArrayIndexableSequenceWrapper Default => default;
        }

        internal readonly struct ListIndexableSequenceWrapper :
            IIndexableSequence<TValue>,
            IDefaultValueProvider<ListIndexableSequenceWrapper>
        {
            private readonly IList<TValue> _list;

            internal ListIndexableSequenceWrapper(IList<TValue> list)
            {
                ArgumentNullException.ThrowIfNull(list);
                _list = list;
            }

            public TValue this[Index key]
                => IsDefault
                ? throw new InvalidOperationException($"Indexing a default instance is forbidden")
                : _list[key];

            public IIndexableSequence<TValue> this[Range key]
            {
                get
                {
                    if (IsDefault)
                        throw new InvalidOperationException($"Indexing a default instance is forbidden");

                    var (start, length) = key.GetOffsetAndLength(_list.Count);
                    var items = _list
                        .Skip(start)
                        .Take(length)
                        .ToList()!;

                    return new ListIndexableSequenceWrapper(items);
                }
            }

            public static IIndexableSequence<TValue>.ListIndexableSequenceWrapper Default => default;

            public bool IsDefault => _list is null;

            public int Count => IsDefault ? 0 : _list.Count;
        }
    }

    public static class IndexableSequence
    {
        public static IIndexableSequence<TValue> Of<TValue>(
            TValue[] array)
            => new IIndexableSequence<TValue>.ArrayIndexableSequenceWrapper(array);

        public static IIndexableSequence<TValue> Of<TValue>(
            TValue first,
            params TValue[] array)
            => Of(array.Prepend(first).ToArray());

        public static IIndexableSequence<TValue> Of<TValue>(
            IList<TValue> list)
            => new IIndexableSequence<TValue>.ListIndexableSequenceWrapper(list);
    }
}
