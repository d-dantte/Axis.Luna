using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Extensions.Test
{
    [TestClass]
    public class EnumerableExtensionsTests
    {
        [TestMethod]
        public void MiscTests()
        {
            var sequence = new[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };
            var empty = new int[0];

            var result = sequence.ExactlyAll(_v => _v % 2 == 0);
            Assert.IsTrue(result);

            result = empty.ExactlyAll(_v => _v % 2 == 0);
            Assert.IsFalse(result);

            sequence = Enumerable.Range(0, 20).ToArray();

            var skipped = sequence.SkipEvery(1, (_cnt, _v) => _v >= 10).ToArray();
            Assert.IsTrue(new[] { 1, 3, 5, 7, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }.SequenceEqual(skipped));

            var taken = sequence.TakeEvery(1, (_cnt, _v) => _v >= 10).ToArray();
            Assert.IsTrue(new[] { 0, 2, 4, 6, 8, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }.SequenceEqual(taken));
        }
    }
}