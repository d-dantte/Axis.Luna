using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Common.Utils
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public struct Page<TData>
    {
        private TData[] _data;
        private readonly int _dataHash;

        public IEnumerable<TData> Data => _data;

        public int Index { get; }

        public int PageNumber => Index + 1;

        public int MaxCount { get; }


        public Page(int index, int maxCount, params TData[] data)
        {
            _data = new TData[data.Length];
            Array.Copy(data, _data, data.Length);

            Index = Math.Abs(index);
            MaxCount = Math.Abs(maxCount);
            _dataHash = Luna.Extensions.Common.ValueHash(_data);
        }

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
    /// 
    /// </summary>
    public struct PageAdjacencySet
    {
        private readonly int[] _adjacencySet;
        private readonly int _adjacencyHash;

        public int SequenceLength { get; }

        public int PageLength { get; }

        public int PageIndex { get; }

        public IEnumerable<int> PageRefs => _adjacencySet;


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
