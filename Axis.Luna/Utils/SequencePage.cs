using Axis.Luna.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Luna
{
    public class SequencePage<Data>
    {
        public long PageIndex { get; private set; }
        public long SequenceLength { get; private set; }
        public Data[] Page { get; private set; }
        public long PageCount { get; private set; }
        public long PageSize { get; private set; }

        public SequencePage(Data[] page, long sequenceLength, long pageSize = -1, long pageIndex = 0)
        {
            if (page == null || pageIndex < 0 || sequenceLength < 0 || pageSize < -1) throw new Exception("invalid page");
            PageIndex = pageIndex;
            SequenceLength = sequenceLength;
            Page = page;
            PageSize = pageSize < 0 ? page.Length : pageSize;
            PageCount = SequenceLength / PageSize + (SequenceLength % PageSize > 0 ? 1 : 0);
        }

        /// <summary>
        /// Returns an array containing page indexes for pages immediately adjecent to the current page.
        /// The span indicates how many pages indexes to each side of the current page should be returned
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public long[] AdjacentIndexes(int span)
        {
            var fullspan = (span.ThrowIf(_s => _s < 0, "invalid span: " + span) * 2) + 1;
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
