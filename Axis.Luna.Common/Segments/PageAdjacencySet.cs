using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Axis.Luna.Common.Segments
{
    /// <summary>
    /// This struct is used to generate pagination references - i.e, given some parameters, it creates an array of consecutive page indexes.
    /// </summary>
    public readonly struct PageAdjacencySet :
        IDefaultValueProvider<PageAdjacencySet>,
        IEquatable<PageAdjacencySet>
    {
        private readonly ImmutableArray<long> _adjacencySet;
        private readonly int _adjacencyHash;

        /// <summary>
        /// The total number of values in the orignal sequence of items
        /// </summary>
        public long? SequenceLength { get; }

        /// <summary>
        /// The maximum number of times that can reside in a page
        /// </summary>
        public int MaxPageLength { get; }

        /// <summary>
        /// The index of the page that sits in the relative center of the Adjacency set
        /// </summary>
        public long PageIndex { get; }

        /// <summary>
        /// The list of page indices.
        /// </summary>
        public ImmutableArray<long> PageRefs => _adjacencySet;

        /// <summary>
        /// Create an adjacency set
        /// </summary>
        public PageAdjacencySet(long? sequenceLength, int pageLength, long pageIndex, int setLength = 1)
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
            _adjacencyHash = _adjacencySet.Aggregate(0, HashCode.Combine);

            MaxPageLength = pageLength;
            SequenceLength = sequenceLength;
            PageIndex = pageIndex;
        }


        public bool IsDefault => _adjacencySet.IsDefault;

        public static PageAdjacencySet Default => default;

        private static ImmutableArray<long> EvaluateRefs(long? sequenceLength, int pageLength, int setLength, ref long pageIndex)
        {
            var pageCount = Math.DivRem(
                sequenceLength ?? long.MaxValue,
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
                .Range(0, setLength)
                .Select(i => i + startIndex)
                .ToImmutableArray();
        }


        public override bool Equals(
            object? obj)
            => obj is PageAdjacencySet other && Equals(other);

        public bool Equals(PageAdjacencySet other)
        {
            if (IsDefault && other.IsDefault)
                return true;

            if (IsDefault ^ other.IsDefault)
                return false;

            if (SequenceLength != other.SequenceLength
                || MaxPageLength != other.MaxPageLength
                || PageIndex != other.PageIndex)
                return false;

            if (_adjacencyHash != other._adjacencyHash)
                return false;

            if (_adjacencySet.Equals(other._adjacencySet))
                return true;

            if (_adjacencySet.Length != other._adjacencySet.Length)
                return false;

            var comparer = EqualityComparer<long>.Default;
            for (int cnt = 0; cnt < _adjacencySet.Length; cnt++)
            {
                if (!comparer.Equals(_adjacencySet[cnt], other._adjacencySet[cnt]))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
            => HashCode.Combine(
                _adjacencyHash,
                PageIndex,
                MaxPageLength,
                SequenceLength);

        public static bool operator ==(PageAdjacencySet first, PageAdjacencySet second) => first.Equals(second);

        public static bool operator !=(PageAdjacencySet first, PageAdjacencySet second) => !(first == second);

    }
}
