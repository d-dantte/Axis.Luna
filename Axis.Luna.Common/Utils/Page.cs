using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Common.Utils
{
    /// <summary>
    /// Represents an indexed chunk of continguous data from a stream of data
    /// </summary>
    /// <typeparam name="TData">The type of data</typeparam>
    public struct Page<TData>
    {
        private readonly TData[] _data;
        private readonly int _dataHash;

        public IEnumerable<TData> Data => _data;

        /// <summary>
        /// Index of the current page. Indices start from 0.
        /// <para>
        /// If the original stream is divided by the value of <c>MaxCount</c> into chunks, this index represents the <c>n-1th</c> chunk
        /// </para>
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Ordinal page number. This is effectively <see cref="Page{TData}.Index"/> <c>+ 1</c>
        /// </summary>
        public int PageNumber => Index + 1;

        /// <summary>
        /// The maximum number of items that can be contained by a page
        /// </summary>
        public int MaxCount { get; }

        /// <summary>
        /// Creates a new page given a chunk of data
        /// </summary>
        /// <param name="index">The page index</param>
        /// <param name="maxCount">The max count value</param>
        /// <param name="data">The data chunk</param>
        public Page(int index, int maxCount, params TData[] data)
        {
            _data = new TData[data.Length];
            Array.Copy(data, _data, data.Length);

            Index = Math.Abs(index);
            MaxCount = Math.Abs(maxCount);
            _dataHash = Luna.Extensions.Common.ValueHash(_data);
        }


        /// <summary>
        /// Creates a new page given a chunk of data
        /// </summary>
        /// <param name="index">The page index</param>
        /// <param name="data">The data chunk</param>
        public Page(int index, params TData[] data)
            :this(index, data.Length, data)
        {
        }

        public override bool Equals(object obj)
            => obj is Page<TData> other
            && Index == other.Index
            && MaxCount == other.MaxCount
            && _data.SequenceEqual(other._data);                

        public override int GetHashCode() => HashCode.Combine(_dataHash, Index, MaxCount);

        public static bool operator ==(Page<TData> first, Page<TData> second) => first.Equals(second);

        public static bool operator !=(Page<TData> first, Page<TData> second) => !(first == second);
    }

    /// <summary>
    /// This struct is used to generate pagination references - i.e, given some parameters, it creates an array of consecutive page indexes.
    /// </summary>
    public struct PageAdjacencySet
    {
        private readonly int[] _adjacencySet;
        private readonly int _adjacencyHash;

        /// <summary>
        /// The total number of values in the orignal sequence of items
        /// </summary>
        public int SequenceLength { get; }

        /// <summary>
        /// The maximum number of times that can reside in a page
        /// </summary>
        public int PageLength { get; }

        /// <summary>
        /// The index of the page that sits in the relative center of the Adjacency set
        /// </summary>
        public int PageIndex { get; }

        /// <summary>
        /// The list of page indices.
        /// </summary>
        public IEnumerable<int> PageRefs => _adjacencySet;

        /// <summary>
        /// Create an adjacency set
        /// </summary>
        public PageAdjacencySet(int sequenceLength, int pageLength, int pageIndex, int setLength = 1)
        {
            if (sequenceLength < 0)
                throw new ArgumentException($"Invalid {nameof(sequenceLength)}: {sequenceLength}");

            if (pageLength < 0)
                throw new ArgumentException($"Invalid {nameof(pageLength)}: {pageLength}");

            if (pageIndex < 0)
                throw new ArgumentException($"Invalid {nameof(pageIndex)}: {pageIndex}");

            if (setLength <= 0)
                throw new ArgumentException($"Invalid {nameof(setLength)}: {setLength}");


            _adjacencySet = EvaluateRefs(sequenceLength, pageLength, setLength, ref pageIndex);
            _adjacencyHash = Luna.Extensions.Common.ValueHash(_adjacencySet);

            PageLength = pageLength;
            SequenceLength = sequenceLength;
            PageIndex = pageIndex;
        }

        private static int[] EvaluateRefs(int sequenceLength, int pageLength, int setLength, ref int pageIndex)
        {
            var pageCount = Math.DivRem(
                sequenceLength,
                pageLength,
                out var remainder);

            if (remainder > 0)
                pageCount++;

            if (pageCount <= pageIndex)
                pageIndex = pageCount - 1;

            var split = setLength / 2;

            var startIndex = pageIndex - split;
            startIndex = startIndex < 0 ? 0 : startIndex;

            var count = startIndex + setLength > pageCount
                ? pageCount - startIndex
                : setLength;

            return Enumerable
                .Range(startIndex, count)
                .ToArray();
        }


        public override bool Equals(object obj)
            => obj is PageAdjacencySet other
            && PageIndex == other.PageIndex
            && PageLength == other.PageLength
            && SequenceLength == other.SequenceLength
            && _adjacencySet.SequenceEqual(other._adjacencySet);

        public override int GetHashCode() 
            => HashCode.Combine(
                _adjacencyHash,
                PageIndex,
                PageLength,
                SequenceLength);

        public static bool operator ==(PageAdjacencySet first, PageAdjacencySet second) => first.Equals(second);

        public static bool operator !=(PageAdjacencySet first, PageAdjacencySet second) => !(first == second);

    }
}
