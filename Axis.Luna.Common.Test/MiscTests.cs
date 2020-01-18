using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axis.Luna.Common.Test
{
    public class SequencePage<Data>
    {
        /// <summary>
        /// A slice/page of the data returned by the query
        /// </summary>
        public Data[] Page { get; set; }

        /// <summary>
        /// The index of the current page. This index starts from 0
        /// </summary>
        public uint PageIndex { get; set; }

        /// <summary>
        /// The original size of a page requested. Note that the number of entries in the <c>Page</c> array is always <= this value.
        /// To illustrate this, imagine a sequence of 10 elements, requesting page with index 1 (the second page), where the page-size is 6...
        /// page 0 will contain the first 6 elements of the array, while page 1 will contain the last 4 elements of the array (4 <= 6).
        /// </summary>
        public uint PageSize { get; set; }

        /// <summary>
        /// The total length of the sequence from which the page is to be gotten
        /// </summary>
        public ulong? TotalLength { get; set; }

        /// <summary>
        /// Returns an array containing page indexes for pages immediately adjecent to the current page.
        /// The span indicates how many pages indexes to each side of the current page should be returned
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public ulong[] AdjacentIndexes(ulong span)
        {
            if (TotalLength == null)
                return new ulong[0];

            var fullspan = (span * 2) + 1;
            ulong start = 0,
                  count = 0;

            var pageCount = TotalLength.Value / PageSize + (ulong)(TotalLength.Value % PageSize > 0 ? 1 : 0);
            if (fullspan >= pageCount) count = pageCount;

            else
            {
                start = PageIndex - span;
                count = fullspan;

                if (start < 0) start = 0;
                if ((PageIndex + span) >= pageCount) start = pageCount - fullspan;
            }

            return this
                .GenerateSequence(count, _indx => _indx + start)
                .ToArray();
        }

        public bool IsValidPage()
        {
            if (Page == null) return false;

            else if (PageSize == 0) return false;

            else if (TotalLength.HasValue)
            {
                if ((ulong)Page.Length > TotalLength.Value)
                    return false;

                var pageCount = TotalLength.Value / PageSize + (ulong)(TotalLength.Value % PageSize > 0 ? 1 : 0);
                if (PageIndex >= pageCount)
                    return false;

                if (!IsLastPage() && (ulong)Page.Length < pageCount)
                    return false;
            }

            return true;
        }

        public bool IsLastPage()
        {
            var pageCount = TotalLength.Value / PageSize + (ulong)(TotalLength.Value % PageSize > 0 ? 1 : 0);
            return pageCount == PageIndex - 1;
        }

        public bool IsFirstPage() => PageIndex == 0;


        private IEnumerable<V> GenerateSequence<V>(ulong repetitions, Func<ulong, V> generator)
        {
            for (ulong cnt = 0; cnt < repetitions; cnt++)
                yield return generator.Invoke(cnt);
        }
    }

    [TestClass]
    public class MiscTests
    {
        [TestMethod]
        public void Test()
        {
            var page = new SequencePage<int>
            {
                Page = new int[10],
                PageIndex = 5,
                PageSize = 10,
                TotalLength = 200
            };
            var pages = page.AdjacentIndexes(2);
            Console.WriteLine(string.Join(',', pages));
        }
    }
}
