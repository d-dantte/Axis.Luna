using Axis.Luna.Common.Segments;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;

namespace Axis.Luna.Common
{
    public readonly struct CharSequence :
        IDefaultValueProvider<CharSequence>,
        IEquatable<CharSequence>,
        IEnumerable<char>
    {
        private readonly Segment _segment;
        private readonly string _ref;
        private readonly Lazy<int> _hashCode;

        #region Construction
        /// <summary>
        /// Constructs a new instance of the sequence
        /// </summary>
        /// <param name="ref">The string</param>
        /// <param name="offset">The offset</param>
        /// <param name="length">The length of the sequence. A value of -1 represents "all characters from the offset onwards".</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public CharSequence(string @ref, int offset, int length)
        {
            ArgumentNullException.ThrowIfNull(@ref);

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (length < -1 || length > @ref.Length - offset)
                throw new ArgumentOutOfRangeException(nameof(length));

            _ref = @ref;
            _segment = (offset, length < 0 ? (length = @ref.Length) : length);
            _hashCode = new Lazy<int>(() =>
            {
                // exclude offset and length because we are only concerned with the content
                return @ref
                    .Substring(offset, length)
                    .Aggregate(0, HashCode.Combine);
            });
        }

        public CharSequence(string @ref, int offset)
        : this(@ref, offset, -1)
        { }

        public CharSequence(string @ref)
        : this(@ref, 0, -1)
        { }

        public CharSequence(string @ref, Range range)
        : this(@ref,
              range.GetOffsetAndLength(@ref.Length).Offset,
              range.GetOffsetAndLength(@ref.Length).Length)
        { }

        public CharSequence(char c)
        : this(c.ToString())
        { }
        #endregion

        #region Of
        public static CharSequence Of(
            string @ref,
            int offset,
            int length)
            => new(@ref, offset, length);

        public static CharSequence Of(
            string @ref,
            int offset)
            => new(@ref, offset);

        public static CharSequence Of(
            string @ref)
            => new(@ref);

        public static CharSequence Of(
            char c)
            => new(c);

        public static CharSequence Of(
            string @ref,
            Range range)
            => new(@ref, range);
        #endregion

        #region Implicits
        public static implicit operator CharSequence(string @ref) => new(@ref);

        public static implicit operator string(CharSequence chars) => chars.ToString();
        #endregion

        #region DefaultValueProvider
        public bool IsDefault
            => _segment.IsDefault
            && _ref is null;

        public static CharSequence Default => default;
        #endregion

        #region IEnumerable
        public IEnumerator<char> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region API

        public bool IsEmpty => _segment.IsEmpty();

        public static CharSequence Empty { get; } = new CharSequence(string.Empty, 0, 0);

        public string Ref => _ref;

        public Segment Segment => _segment;

        public int Length => _segment.Count;

        public char this[int index] => IsDefault
            ? throw new InvalidOperationException($"Invalid state: default")
            : _ref[index + _segment.Offset];

        public CharSequence this[Range range]
        {
            get
            {
                if (IsDefault)
                    throw new InvalidOperationException($"Invalid state: default");

                var newSegment = range.GetOffsetAndLength(_segment.Count);
                return new(_ref, newSegment.Offset + _segment.Offset, newSegment.Length);
            }
        }

        public ReadOnlySpan<char> AsSpan() => IsDefault
            ? throw new InvalidOperationException($"Invalid state: default")
            : _ref.AsSpan(_segment.Offset, _segment.Count);

        public ReadOnlySpan<char> AsSpan(int offset)
        {
            if (IsDefault)
                throw new InvalidOperationException($"Invalid state: default");

            var newOffset = _segment.Offset + offset;
            return _ref.AsSpan(newOffset, _segment.Count - newOffset);
        }

        public ReadOnlySpan<char> AsSpan(int offset, int length) => IsDefault
            ? throw new InvalidOperationException($"Invalid state: default")
            : _ref.AsSpan(_segment.Offset + offset, length);

        public ReadOnlySpan<char> AsSpan(Range range)
        {
            if (IsDefault)
                throw new InvalidOperationException($"Invalid state: default");

            var segment = range.GetOffsetAndLength(_segment.Count);
            return _ref.AsSpan(_segment.Offset + segment.Offset, segment.Length);
        }

        /// <summary>
        /// Concatenates two <see cref="CharSequence"/> instances, similar to <see cref="String.Concat(IEnumerable{string?})"/>.
        /// <para/>
        /// Note: concatenating a default CharSequence with a non-default CharSequence yields the non-default CharSequence.
        /// Note: concatenating any non-default CharSequence with an empty CharSequence yields the non-default CharSequence.
        /// </summary>
        /// <param name="other">The token instance to merge with.</param>
        /// <returns></returns>
        public CharSequence Concat(CharSequence other)
        {
            if (IsDefault && other.IsDefault)
                return this;

            if (IsDefault)
                return other;

            if (other.IsDefault)
                return this;

            if (IsEmpty)
                return other;

            if (other.IsEmpty)
                return this;

            if (EqualityComparer<string>.Default.Equals(_ref, other._ref)
                && _segment.Preceeds(other._segment))
                return Of(_ref!, _segment + other._segment);

            else return Of(
                _ref.Substring(_segment.Offset, _segment.Count) + other._ref.Substring(other._segment.Offset, other._segment.Count),
                0, _segment.Count + other._segment.Count);
        }

        public CharSequence Concat(string @string) => this.Concat(Of(@string));

        public CharSequence Concat(char @char) => this.Concat(Of(@char));

        public static CharSequence operator +(CharSequence chars, int charCount)
        {
            if (charCount < 0)
                return chars - Math.Abs(charCount);

            else if ((charCount + chars.Segment.Offset + chars.Segment.Count) <= chars.Ref.Length)
                return Of(chars.Ref, chars.Segment.Offset, chars.Segment.Count + charCount);

            else throw new ArgumentOutOfRangeException(
                nameof(charCount),
                "Expanding beyond the limit of the sequence is forbidden");
        }

        public static CharSequence operator -(CharSequence chars, int charCount)
        {
            if (charCount < 0)
                return chars + Math.Abs(charCount);

            else if (charCount < chars.Segment.Count)
                return Of(chars.Ref, chars.Segment.Offset, chars.Segment.Count - charCount);

            else throw new ArgumentOutOfRangeException(
                nameof(charCount),
                "Subtracting beyond the limit of this sequence is forbidden");
        }

        #endregion

        public bool Equals(CharSequence other)
        {
            if (IsDefault && other.IsDefault)
                return true;

            if (IsDefault ^ other.IsDefault)
                return false;

            if (_segment.Count != other._segment.Count)
                return false;

            // only check offset if the refs are equal
            if (_ref == other._ref
                && _segment.Offset == other._segment.Offset)
                return true;

            for (int index = 0; index < _segment.Count; index++)
            {
                if (_ref[index + _segment.Offset] != other._ref[index + other._segment.Offset])
                    return false;
            }
            return true;
        }

        public override bool Equals(
            object? obj)
            => obj is CharSequence other && Equals(other);

        public override string ToString()
        {
            if (IsDefault)
                return null!;

            return _ref.Substring(_segment.Offset, _segment.Count);
        }

        /// <summary>
        /// Gets the hashcode of ONLY the values of the string segment
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => IsDefault ? 0 : _hashCode.Value;

        public int FullHash() => HashCode.Combine(GetHashCode(), _segment);

        public static bool operator ==(
            CharSequence lhs,
            CharSequence rhs)
            => lhs.Equals(rhs);

        public static bool operator !=(
            CharSequence lhs,
            CharSequence rhs)
            => !lhs.Equals(rhs);

        public static CharSequence operator +(CharSequence lhs, CharSequence rhs) => lhs.Concat(rhs);

        public static CharSequence operator +(CharSequence lhs, string rhs) => lhs.Concat(rhs);

        public static CharSequence operator +(CharSequence lhs, char rhs) => lhs.Concat(rhs);


        internal class Enumerator: IEnumerator<char>
        {
            private readonly CharSequence _sequence;
            private int _index = -1;

            internal Enumerator(CharSequence sequence)
            {
                _sequence = sequence;
            }

            public char Current => _index < 0
                ? throw new InvalidOperationException($"Invalid state: Enumeration has not started.")
                : _sequence[_index];

            object IEnumerator.Current => Current;

            public void Dispose()
            { }

            public bool MoveNext()
            {
                if ((_index + 1) >= _sequence.Segment.Count)
                    return false;

                _index++;
                return true;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
    }
}
