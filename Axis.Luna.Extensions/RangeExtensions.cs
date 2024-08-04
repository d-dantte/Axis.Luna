using System;
using System.Collections.Generic;

namespace Axis.Luna.Extensions
{
    public static class RangeExtensions
    {
        public static int StartIndex(this Range range, int sequenceLength)
        {
            return range.Start.IsFromEnd switch
            {
                true => sequenceLength - range.Start.Value,
                false => range.Start.Value
            };
        }

        public static int EndIndex(this Range range, int sequenceLength)
        {
            return range.End.IsFromEnd switch
            {
                true => sequenceLength - range.End.Value,
                false => range.End.Value
            };
        }

        public static IEnumerable<int> Enumerate(this Range range)
        {
            if (range.Start.IsFromEnd || range.End.IsFromEnd)
                throw new InvalidOperationException("Invalid range: relative indexes are forbidden");

            if (range.Start.Value.Equals(range.End.Value))
                yield break;

            var increment = range.End.Value < range.Start.Value ? -1 : 1;
            var value = range.Start.Value;

            do
            {
                yield return value;
            }
            while ((value += increment) != range.End.Value);
        }
    }
}
