using System;

namespace Axis.Luna.Common.Segments
{
    public readonly struct Segment :
        ICountable,
        IOffsetable,
        IEquatable<Segment>,
        IDefaultValueProvider<Segment>
    {
        #region Props
        /// <summary>
        /// The offset at which this segment begins.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// The number of items this segment represents.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// The offset at which the last item of this segment holds within the source.
        /// </summary>
        public int EndOffset => Count switch
        {
            0 => Offset,
            > 0 => Offset + Count - 1,
            _ => throw new InvalidOperationException($"Invalid {nameof(Count)}: {Count}")
        };
        #endregion

        #region DefaultValueProvider

        public bool IsDefault => Offset == 0 && Count == 0;

        public static Segment Default => default;

        #endregion

        #region Construction
        public Segment(int offset, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            Offset = offset;
            Count = length;
        }

        public static Segment Of(
            int offset,
            int length)
            => new(offset, length);

        public static Segment Of(
            int offset)
            => new(offset, 1);

        public static implicit operator Segment((int Offset, int Count) segment) => new(segment.Offset, segment.Count);

        public static implicit operator Range(Segment segment) => segment.ToRange();
        #endregion

        #region Index & Range

        public Segment this[Range range]
        {
            get
            {
                var tuple = range.GetOffsetAndLength(Count);
                return Slice(tuple.Offset, tuple.Length);
            }
        }

        public Segment Slice(
            int offset,
            int length)
            => Of(offset + Offset, length);

        public Segment Slice(int offset) => Of(offset + Offset, Count - offset);

        #endregion

        #region Helpers

        public bool Contains(Segment other)
        {
            return Offset <= other.Offset
                && EndOffset >= other.EndOffset;
        }

        public bool Intersects(Segment other)
        {
            return Offset <= other.Offset && other.Offset <= EndOffset
                || other.Offset <= Offset && Offset <= other.EndOffset;
        }

        public bool Succeeds(Segment other)
        {
            if (IsDefault || other.IsDefault)
                return false;

            return Offset == other.Offset + other.Count;
        }

        public bool Preceeds(
            Segment other)
            => other.Succeeds(this);

        public Segment Merge(Segment other)
        {
            var newOffset = Math.Min(Offset, other.Offset);
            var newEndOffset = Math.Max(EndOffset, other.EndOffset);
            return Of(newOffset, newEndOffset - newOffset + 1);
        }

        public static Segment operator +(Segment first, Segment second) => first.Merge(second);

        public Range ToRange() => new(Offset, EndOffset + 1);

        #endregion

        #region Overrides

        public override int GetHashCode() => HashCode.Combine(Offset, Count);

        public bool Equals(Segment other)
        {
            return Offset == other.Offset
                && Count == other.Count;
        }

        public override bool Equals(object? obj)
        {
            return obj is Segment other
                && Equals(other);
        }

        public override string ToString() => $"[offset: {Offset}, count: {Count}]";

        public static bool operator ==(Segment first, Segment second) => first.Equals(second);

        public static bool operator !=(Segment first, Segment second) => !first.Equals(second);

        #endregion
    }

    public static class SegmentExtensions
    {
        /// <summary>
        /// Get a slice/chunk of an array
        /// </summary>
        /// <typeparam name="T">The type of the array</typeparam>
        /// <param name="array">The array</param>
        /// <param name="offset">The offset at which the slice is made, i.e, how many elements to skip</param>
        /// <param name="length"> The length of the slice/chunk. <c>null</c> indicates using whatever length remains after the offset </param>
        /// <returns></returns>
        public static Segment SegmentSlice<T>(this
            T[] array,
            int offset,
            int? length = null)
            => new(offset, length ?? array.Length - offset);

        /// <summary>
        /// Splits an array into 2 <see cref="Segment"/> instances, using the given index as a pivot. Note that the element at the pivot index will always
        /// become the first element in the "right" <see cref="Segment"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="pivotIndex"></param>
        /// <returns></returns>
        public static (Segment, Segment) SegmentSplit<T>(this
            T[] array,
            int pivotIndex)
        {
            // validate pivot index
            if (pivotIndex < 0
                || pivotIndex >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(pivotIndex));


            return (
                Segment.Of(0, pivotIndex),
                Segment.Of(pivotIndex, array.Length - pivotIndex));
        }
    }
}
