using Axis.Luna.Extensions;
using System;
using System.Linq;

namespace Axis.Luna
{
    public class SequencePage<Data>
    {
        public long PageIndex { get; private set; }
        public long SequenceLength { get; private set; }
        public Data[] Page { get; private set; }
        public long PageCount { get; private set; }
        public long PageSize { get; private set; }

        public SequencePage(Data[] page, long sequenceLength, int pageSize = 1, int pageIndex = 0)
        {
            if (page == null) throw new Exception("invalid page");

            PageIndex = pageIndex;
            SequenceLength = Math.Abs(sequenceLength);
            Page = page;
            PageSize = pageSize == 0 && Page.Length == 0 ? 1 :
                       Page.Length == 0 ? 1 :
                       pageSize == 0 ? Page.Length :
                       Math.Abs(pageSize);

            PageCount = SequenceLength / PageSize + (SequenceLength % PageSize > 0 ? 1 : 0);
        }

        public SequencePage()
        : this(new Data[0], 0)
        { }

        /// <summary>
        /// Returns an array containing page indexes for pages immediately adjecent to the current page.
        /// The span indicates how many pages indexes to each side of the current page should be returned
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public long[] AdjacentIndexes(int span)
        {
            if (span < 0) throw new Exception("invalid span: " + span);

            var fullspan = (span * 2) + 1;
            long start = 0,
                 count = 0;

            if (fullspan >= PageCount) count = PageCount;

            else
            {
                start = PageIndex - span;
                count = fullspan;

                if (start < 0) start = 0;
                if ((PageIndex + span) >= PageCount) start = PageCount - fullspan;
            }

            return count.GenerateSequence(_indx => _indx + start).ToArray();
        }
    }
}
