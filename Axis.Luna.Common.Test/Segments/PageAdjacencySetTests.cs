using Axis.Luna.Common.Segments;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Axis.Luna.Common.Test.Segments
{
    [TestClass]
    public class PageAdjacencySetTests
    {
        [TestMethod]
        public void Construction_Tests()
        {
            var page = default(Page<int>);
            Assert.IsNotNull(page);
            Assert.IsTrue(page.IsDefault);
            Assert.AreEqual(Page<int>.Default, page);
            Assert.AreEqual(0, page.Count);
            Assert.AreEqual(0, page.LongOffset);
            Assert.AreEqual(0, page.MaxPageLength);
            Assert.AreEqual(0, page.PageIndex);
            Assert.AreEqual(0, page.PageNumber);
            Assert.IsNull(page.SequenceLength);

            page = new Page<int>(1, 20, 1000, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20);
            Assert.AreEqual(20, page.Count);
            Assert.AreEqual(20, page.LongOffset);
            Assert.AreEqual(20, page.MaxPageLength);
            Assert.AreEqual(1, page.PageIndex);
            Assert.AreEqual(2, page.PageNumber);
            Assert.AreEqual(1000, page.SequenceLength);
        }

        [TestMethod]
        public void CreateAdjacencySet_Tests()
        {
            var page = new Page<int>(1, 20, 1000, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20);
            var set = page.CreateAdjacencySet(4);
            Assert.IsTrue(Enumerable.SequenceEqual(
                new[] {0, 1L, 2, 3 },
                set.PageRefs));
            Assert.AreEqual(1, set.PageIndex);
        }
    }
}
