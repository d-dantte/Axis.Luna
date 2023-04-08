using System;
using System.Collections;
using System.Collections.Generic;

namespace Axis.Luna.Common.Numerics
{
    internal class GenericBitArray : ICollection<bool>
    {
        private BitArray _bitArray;

        public GenericBitArray(BitArray bitArray)
        {
            _bitArray = bitArray ?? throw new ArgumentNullException(nameof(bitArray));
        }

        public int Count => _bitArray.Count;

        public bool this[int index]
        {
            get => _bitArray[index];
            set => _bitArray[index] = value;
        }

        #region Not Supported
        public bool IsReadOnly => throw new NotSupportedException();

        void ICollection<bool>.Add(bool item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Remove(bool item)
        {
            throw new NotSupportedException();
        }
        #endregion

        public bool Contains(bool item)
        {
            foreach(var b in _bitArray)
            {
                if (b.Equals(item)) return true;
            }
            return false;
        }

        public void CopyTo(bool[] array, int arrayIndex)
        {
            var index = arrayIndex;
            foreach(var b in _bitArray)
            {
                array[index++] = (bool)b;
            }
        }

        public IEnumerator<bool> GetEnumerator() => new Enumerator(_bitArray.GetEnumerator());

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal class Enumerator : IEnumerator<bool>
        {
            private IEnumerator _enumerator;

            internal Enumerator(IEnumerator enumerator)
            {
                _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
            }

            public bool Current => (bool)_enumerator.Current;

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset() => _enumerator.Reset();
        }
    }
}
