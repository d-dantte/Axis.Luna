using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Axis.Luna.Common.Segments
{
    /// <summary>
    /// Represents an indexed chunk of continguous data from a stream of data
    /// </summary>
    /// <typeparam name="TData">The type of data</typeparam>
    public readonly struct Page<TData> :
        IDefaultValueProvider<Page<TData>>,
        IEnumerable<TData>,
        IEquatable<Page<TData>>,
        ILongOffsetable,
        ICountable
    {
        private readonly ImmutableArray<TData> _data;
        private readonly Lazy<int> _dataHash;
        private readonly long _pageIndex;
        private readonly long? _sequenceLength;
        private readonly int _pageMaxLength;

        #region Default
        public static Page<TData> Default => default;

        public bool IsDefault
            => _data.IsDefault
            && _dataHash is null
            && _pageIndex == 0
            && _sequenceLength is null
            && _pageMaxLength == 0;
        #endregion

        /// <summary>
        /// Number of items in the data page
        /// </summary>
        public int Count => IsDefault ? 0 : _data.Length;

        /// <summary>
        /// Offset into the source sequence of the first item in this page
        /// </summary>
        public long LongOffset => _pageIndex * _pageMaxLength;

        /// <summary>
        /// The length of the underlying sequence, if known.
        /// </summary>
        public long? SequenceLength => _sequenceLength;

        /// <summary>
        /// Maximum number of items permissible in a page. Useful for when the number of items in the source sequence isn't a multiple
        /// of the <see cref="MaxPageLength"/> - here the last page will always contain less than the max number of items permissible.
        /// </summary>
        public int MaxPageLength => _pageMaxLength;

        /// <summary>
        /// The index of the page
        /// </summary>
        public long PageIndex => _pageIndex;

        /// <summary>
        /// The page number - Index + 1;
        /// </summary>
        public long PageNumber => IsDefault ? 0 : _pageIndex + 1;

        public ImmutableArray<TData > Data => _data;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="maxPageLength"></param>
        /// <param name="sequenceLength"></param>
        /// <param name="data"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Page(long pageIndex, int maxPageLength, long? sequenceLength, params TData[] data)
        {
            ArgumentNullException.ThrowIfNull(data);

            ArgumentOutOfRangeException.ThrowIfNegative(pageIndex);

            ArgumentOutOfRangeException.ThrowIfNegative(maxPageLength);

            if (sequenceLength < 0)
                throw new ArgumentOutOfRangeException(nameof(sequenceLength));

            _pageIndex = pageIndex;
            _pageMaxLength = maxPageLength;
            _sequenceLength = sequenceLength;
            _data = [.. data];
            _dataHash = new Lazy<int>(() => data.Aggregate(0, HashCode.Combine));
        }

        public bool Equals(Page<TData> other)
        {
            if (IsDefault && other.IsDefault)
                return true;

            if (IsDefault ^ other.IsDefault)
                return false;

            if (_pageIndex != other._pageIndex
                || _pageMaxLength != other._pageMaxLength
                || _sequenceLength != other._sequenceLength
                || _data.Length != other._data.Length)
                return false;

            if (_data.Equals(other._data))
                return true;

            if (_dataHash.Value != other._dataHash.Value)
                return false;

            var comparer = EqualityComparer<TData>.Default;
            for (int cnt = 0; cnt < _data.Length; cnt++)
            {
                if (!comparer.Equals(_data[cnt], other._data[cnt]))
                    return false;
            }

            return true;
        }

        public override int GetHashCode() => _dataHash?.Value ?? 0;

        public override bool Equals(object? obj)
            => obj is Page<TData> other && Equals(other);

        /// <summary>
        /// Creates a new instance of the <see cref="PageAdjacencySet"/> type/
        /// </summary>
        /// <param name="length">The length of the set</param>
        public PageAdjacencySet CreateAdjacencySet(int length)
            => new(SequenceLength, MaxPageLength, PageIndex, length);

        public IEnumerator<TData> GetEnumerator() => IsDefault
            ? Enumerable.Empty<TData>().GetEnumerator()
            : (_data as IEnumerable<TData>).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public TData this[int index] => _data[index];

        public TData this[Index index] => _data[index];

        public static bool operator ==(Page<TData> first, Page<TData> second) => first.Equals(second);

        public static bool operator !=(Page<TData> first, Page<TData> second) => !(first == second);
    }

    public static class Page
    {
        public static Page<TData> Of<TData>(long pageIndex, int maxPageLength, long? sequenceLength, params TData[] data)
        {
            return new(pageIndex, maxPageLength, sequenceLength, data);
        }

        public static Page<TData> Of<TData>(TData[] data) => new(0, data.Length, data.Length, data);
    }
}
