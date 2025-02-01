
using System;
using System.Collections;
using System.Linq;
using Axis.Luna.Common.Segments;
using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Axis.Luna.Common.Test.Segments
{
    [TestClass]
    public class PageTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullData_ThrowsArgumentNullException()
        {
            new Page<int>(0, 10, 100, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NegativePageIndex_ThrowsArgumentOutOfRangeException()
        {
            new Page<int>(-1, 10, 100, 1, 2, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NegativeMaxPageLength_ThrowsArgumentOutOfRangeException()
        {
            new Page<int>(0, -10, 100, 1, 2, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NegativeSequenceLength_ThrowsArgumentOutOfRangeException()
        {
            new Page<int>(0, 10, -100, 1, 2, 3);
        }

        [TestMethod]
        public void Constructor_ValidData_CreatesPage()
        {
            var page = new Page<int>(0, 10, 100, 1, 2, 3);

            Assert.AreEqual(3, page.Count);
            Assert.AreEqual(0, page.PageIndex);
            Assert.AreEqual(1, page.PageNumber);
            Assert.AreEqual(10, page.MaxPageLength);
            Assert.AreEqual(100, page.SequenceLength);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, page.Data.ToArray());
        }

        [TestMethod]
        public void DefaultValue_IsCorrect()
        {
            var defaultPage = Page<int>.Default;

            Assert.IsTrue(defaultPage.IsDefault);
            Assert.AreEqual(0, defaultPage.Count);
        }

        [TestMethod]
        public void Equality_TwoIdenticalPages_AreEqual()
        {
            var page1 = new Page<int>(0, 10, 100, 1, 2, 3);
            var page2 = new Page<int>(0, 10, 100, 1, 2, 3);

            Assert.AreEqual<Page<int>>(default, default);
            Assert.AreEqual(page1, page1);
            Assert.AreEqual(page1, page2);
            Assert.IsTrue(page1.Equals((object)page2));
            Assert.IsTrue(page1 == page2);
            Assert.IsFalse(page1 != page2);
        }

        [TestMethod]
        public void Equality_TwoDifferentPages_AreNotEqual()
        {
            var page1 = new Page<int>(0, 10, 100, 1, 2, 3);
            var page2 = new Page<int>(1, 10, 100, 4, 5, 6);
            var page3 = new Page<int>(0, 10, 100, 4, 5, 6);
            var page4 = new Page<int>(0, 10, 100, 4, 5, 6, 7);
            var page5 = new Page<int>(0, 10, 100, 4, 5, 6, 5);

            Assert.AreNotEqual(page1, page2);
            Assert.AreNotEqual(page1, page3);
            Assert.AreNotEqual(page1, page4);
            Assert.AreNotEqual(page5, page4);
            Assert.IsFalse(page1.Equals("bleh"));
            Assert.AreNotEqual(page1, default);
            Assert.AreNotEqual(default, page1);
            Assert.IsFalse(page1 == page2);
            Assert.IsTrue(page1 != page2);
        }

        [TestMethod]
        public void GetHashCode_IdenticalPages_SameHashCode()
        {
            var page1 = new Page<int>(0, 10, 100, 1, 2, 3);
            var page2 = new Page<int>(0, 10, 100, 1, 2, 3);

            Assert.AreEqual(page1.GetHashCode(), page2.GetHashCode());
            Assert.AreEqual(0, Page<int>.Default.GetHashCode());
        }

        [TestMethod]
        public void Indexer_ReturnsCorrectElement()
        {
            var page = new Page<int>(0, 10, 100, 1, 2, 3);

            Assert.AreEqual(2, page[1]);
            Assert.AreEqual(3, page[new Index(2)]);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Indexer_OutOfBounds_ThrowsException()
        {
            var page = new Page<int>(0, 10, 100, 1, 2, 3);

            var value = page[5];
        }

        [TestMethod]
        public void Enumeration_WorksCorrectly()
        {
            var page = new Page<int>(0, 10, 100, 1, 2, 3);

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, page.ToArray());
            CollectionAssert.AreEqual(Array.Empty<int>(), Page<int>.Default.ToArray());
            Assert.IsNotNull(page.As<IEnumerable>().GetEnumerator());
        }

        [TestMethod]
        public void CreateAdjacencySet_CreatesCorrectInstance()
        {
            var page = new Page<int>(0, 10, 100, 1, 2, 3);
            var adjacencySet = page.CreateAdjacencySet(5);

            Assert.AreEqual(100, adjacencySet.SequenceLength);
            Assert.AreEqual(10, adjacencySet.MaxPageLength);
            Assert.AreEqual(0, adjacencySet.PageIndex);
        }

        [TestMethod]
        public void PageStaticFactory_CreatesCorrectPage()
        {
            var page = Page.Of(1, 10, 100, 1, 2, 3);

            Assert.AreEqual(1, page.PageIndex);
            Assert.AreEqual(10, page.MaxPageLength);
            Assert.AreEqual(100, page.SequenceLength);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, page.Data.ToArray());
        }

        [TestMethod]
        public void PageStaticFactory_FromArray_CreatesCorrectPage()
        {
            var page = Page.Of(new int[] { 1, 2, 3 });

            Assert.AreEqual(0, page.PageIndex);
            Assert.AreEqual(3, page.MaxPageLength);
            Assert.AreEqual(3, page.SequenceLength);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, page.Data.ToArray());
        }
    }

}
