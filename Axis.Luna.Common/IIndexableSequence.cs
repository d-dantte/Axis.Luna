using Axis.Luna.Common.Utils;
using System;

namespace Axis.Luna.Common
{
    public interface IIndexableSequence<TValue>:
        IReadonlyIndexer<Index, TValue>,
        IReadonlyIndexer<Range, IIndexableSequence<TValue>>,
        ICountable
    {

        internal readonly struct ArrayIndexableSequenceWrapper : IIndexableSequence<TValue>
        {
            private readonly TValue[] _array;

            internal ArrayIndexableSequenceWrapper(TValue[] array)
            {
                ArgumentNullException.ThrowIfNull(array);
                _array = array ;
            }

            public static implicit operator ArrayIndexableSequenceWrapper(TValue[] array) => new(array);

            public TValue this[Index key] => _array[key];

            public IIndexableSequence<TValue> this[Range key] => new ArrayIndexableSequenceWrapper(_array[key]);

            public int Count => _array.Length;
        }
    }

    public static class IndexableSequence
    {
        public static IIndexableSequence<TValue> Of<TValue>(
            TValue[] array)
            => new IIndexableSequence<TValue>.ArrayIndexableSequenceWrapper(array);
    }
}
