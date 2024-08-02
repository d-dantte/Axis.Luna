using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Axis.Luna.Extensions.Test
{
    [TestClass]
    public class RangeExtensionsTests
    {
        [TestMethod]
        public void StartIndex_Tests()
        {
            Assert.AreEqual(1, (1..).StartIndex(0));
            Assert.AreEqual(4, (^1..).StartIndex(5));
        }

        [TestMethod]
        public void EndIndex_Tests()
        {
            Assert.AreEqual(4, (1..).EndIndex(4));
            Assert.AreEqual(1, (1..1).EndIndex(5));
        }

        [TestMethod]
        public void Enumerate_Tests()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => (..).Enumerate().ToArray());
            Assert.ThrowsException<InvalidOperationException>(
                () => (^2..).Enumerate().ToArray());

            var enm = (0..4).Enumerate().ToArray();
            CollectionAssert.AreEquivalent(enm, new int[] { 0, 1, 2, 3 });

            enm = (4..0).Enumerate().ToArray();
            CollectionAssert.AreEquivalent(enm, new int[] { 4, 3, 2, 1 });

        }
    }
}
