using System;
using System.Diagnostics.CodeAnalysis;

namespace Axis.Luna.Common
{

    /// <summary>
    /// Rolling polynomial hash implementation.
    /// <para/>
    /// 
    /// Instances of this type, upon creation, assume that the "current" offset is one step behind the provided
    /// offset value in the constructor. This means <see cref="RollingHash.WindowHash"/> will hold "null" until
    /// the first call to either of the <c>TryNext(...)</c> methods.
    /// </summary>
    abstract public class RollingHash<TValue>
    {
        protected readonly IIndexableSequence<TValue> _source;
        protected readonly int _windowLength;
        protected int _offset;

        public Hash? WindowHash { get; protected set; }

        public int WindowLength => _windowLength;

        public int Offset => _offset;

        public IIndexableSequence<TValue> Source => _source;

        protected RollingHash(IIndexableSequence<TValue> source, int offset, int windowLength)
        {
            Validate(source, offset, windowLength);

            _source = source;
            _offset = offset - 1;
            _windowLength = windowLength;
        }

        public static RollingHash<TValue> Of(IIndexableSequence<TValue> source, int offset, int windowLength)
        {
            if (windowLength == 1)
                return new RollingValueHash(source, offset, windowLength);

            else return new RollingWindowHash(source, offset, windowLength);
        }

        /// <summary>
        /// Moves the offset by 1, and calculates the new hash.
        /// <para/>
        /// If the new offset is beyond the end of the source string, then don't move the offset, and return false,
        /// and a default hash object.
        /// </summary>
        /// <param name="result">The has at the new offset</param>
        /// <returns>True if we were able to calculate the hash of the new offset, false otherwise</returns>
        abstract public bool TryNext(out Hash result);

        /// <summary>
        /// Slides the window by <paramref name="count"/> characters, calculating the hash along the way, and only
        /// returning the hash at the last offset calculated.
        /// <para/>
        /// If the final offset is beyond the end of the source string, stop at the end of the string, return false,
        /// and a default hash object.
        /// </summary>
        /// <param name="count">The number of characters to slide the window along</param>
        /// <param name="result">The hash at the final offset</param>
        /// <returns>True if we were able to calculate the hash of the final offset, false otherwise</returns>
        abstract public bool TryNext(int count, out Hash result);

        abstract public Hash ComputeHash(IIndexableSequence<TValue> source, int offset, int length);

        protected static void Validate(IIndexableSequence<TValue> source, int offset, int length)
        {
            if (source is null || source.IsEmpty())
                throw new ArgumentException($"Invalid tokens: null/empty");

            if (offset < 0 || offset >= source.Count)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset + length > source.Count)
                throw new ArgumentOutOfRangeException(nameof(length));
        }

        public static Hash ComputeHash(IIndexableSequence<TValue> source)
        {
            var impl = Of(source, 0, source.Count);
            if (impl.TryNext(out var hash))
                return hash;

            throw new InvalidOperationException(
                $"Invalid string: could not calculate hash of '{source}'");
        }

        #region Nested types
        public readonly struct Hash :
            IEquatable<Hash>,
            IDefaultValueProvider<Hash>
        {
            private readonly long _hash1;
            private readonly long _hash2;

            internal long Hash1 => _hash1;
            internal long Hash2 => _hash2;

            public bool IsDefault => _hash1 == 0 && _hash2 == 0;

            public static Hash Default => default;

            public Hash(long hash1, long hash2)
            {
                _hash1 = hash1;
                _hash2 = hash2;
            }

            public static Hash Of(long hash1, long hash2) => new Hash(hash1, hash2);

            public override string ToString() => $"[{_hash1:x}:{_hash2:x}]";

            public override int GetHashCode() => HashCode.Combine(_hash1, _hash2);

            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                return obj is Hash other && Equals(other);
            }

            public bool Equals(Hash other) => other._hash1 == _hash1 && other._hash2 == _hash2;

            public static bool operator ==(Hash left, Hash right) => left.Equals(right);

            public static bool operator !=(Hash left, Hash right) => !(left == right);
        }

        internal class RollingWindowHash : RollingHash<TValue>
        {
            private static readonly long _Base1 = 65537;
            private static readonly long _Base2 = 65539;
            private static readonly long _Mod1 = 1000000007;
            private static readonly long _Mod2 = 1000000009;

            private readonly long _factor1;
            private readonly long _factor2;

            public RollingWindowHash(IIndexableSequence<TValue> source, int offset, int length)
            : base(source, offset, length)
            {
                _factor1 = ComputeFactor(_Base1, _Mod1, _windowLength);
                _factor2 = ComputeFactor(_Base2, _Mod2, _windowLength);
            }

            override public bool TryNext(out Hash result)
            {
                var newOffset = _offset + 1;
                if (newOffset + _windowLength > _source.Count)
                {
                    result = default;
                    return false;
                }

                WindowHash = result = WindowHash is null
                    ? ComputeHash(_source, newOffset, _windowLength)
                    : NextHash(WindowHash.Value, _source, _offset, _windowLength, (_factor1, _factor2));
                _offset = newOffset;
                return true;
            }

            override public bool TryNext(int count, out Hash result)
            {
                result = default;
                for (int cnt = 1; cnt <= count; cnt++)
                {
                    var moved = cnt == count
                        ? TryNext(out result)
                        : TryNext(out _);

                    if (!moved)
                        return false;
                }

                return true;
            }

            override public Hash ComputeHash(IIndexableSequence<TValue> source, int offset, int length)
            {
                return Hash.Of(
                    ComputeHash(source, offset, length, _Mod1, _Base1),
                    ComputeHash(source, offset, length, _Mod2, _Base2));
            }

            #region Static helpers

            public static Hash NextHash(
                Hash previous,
                IIndexableSequence<TValue> source,
                int oldOffset,
                int length,
                (long factor1, long factor2) factors)
            {
                return Hash.Of(
                    NextHash(previous.Hash1, factors.factor1, source, oldOffset, length, _Mod1, _Base1),
                    NextHash(previous.Hash2, factors.factor2, source, oldOffset, length, _Mod2, _Base2));
            }

            private static long NextHash(
                long previousHash,
                long factor,
                IIndexableSequence<TValue> source,
                int oldOffset,
                int length,
                long mod,
                long @base)
            {
                Validate(source, oldOffset + 1, length);

                // Remove hash of left-most character, and refactor hash
                var hash = (previousHash + mod - factor * source[oldOffset].GetHashCode() % mod) % mod;

                // Add hash of new right-most character
                hash = (hash * @base + source[oldOffset + length].GetHashCode()) % mod;

                return hash;
            }

            private static long ComputeHash(
                IIndexableSequence<TValue> source,
                int offset,
                int length,
                long mod,
                long @base)
            {
                Validate(source, offset, length);

                long hash = 0;
                var limit = offset + length;
                for (int index = offset; index < limit; index++)
                {
                    hash = (@base * hash + source[index].GetHashCode()) % mod;
                }
                return hash;
            }

            private static long ComputeFactor(long @base, long mod, long length)
            {
                var factor = 1L;
                for (int cnt = 1; cnt < length; cnt++)
                {
                    factor = (@base * factor) % mod;
                }

                return factor;
            }
            #endregion
        }

        internal class RollingValueHash : RollingHash<TValue>
        {
            internal RollingValueHash(IIndexableSequence<TValue> source, int offset, int length)
            : base(source, offset, length)
            {
            }

            override public bool TryNext(out Hash result)
            {
                var newOffset = _offset + 1;
                if (newOffset + _windowLength > _source.Count)
                {
                    result = default;
                    return false;
                }

                WindowHash = result = ComputeHash(_source, newOffset, _windowLength);
                _offset = newOffset;
                return true;
            }

            override public bool TryNext(int count, out Hash result)
            {
                var finalOffset = _offset + count;
                if (finalOffset + _windowLength > _source.Count)
                {
                    result = default;
                    return false;
                }

                WindowHash = result = ComputeHash(_source, finalOffset, _windowLength);
                _offset = finalOffset;
                return true;
            }

            override public Hash ComputeHash(IIndexableSequence<TValue> source, int offset, int length)
            {
                Validate(source, offset, length);

                return Hash.Of(
                    source[offset..(offset + 1)][0].GetHashCode(),
                    0);
            }
        }

        #endregion
    }
}
