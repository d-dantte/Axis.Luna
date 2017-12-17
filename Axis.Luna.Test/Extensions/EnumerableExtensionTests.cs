using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Axis.Luna.Test.Extensions
{
    [TestClass]
    public class EnumerableExtensionTests
    {
        [TestMethod]
        public void SkipEveryTest()
        {
            var seq1 = new[]
            {
                0,0,0,1,0,0,0,2,0,0,0,3,0,0,0,4,0,0,0,5,0,0,0,6,0,0,0,7
            };
            var seq2 = new[]
            {
                0,0,0,1,0,0,0,2,0,0,0,3,0,0,0,4,0,0,0,5,0,0,0,6,0,0,0,7,0
            };
            var seq3 = new[]
            {
                0,0,0,1,0,0,0,2,0,0,0,3,0,0,0,4,0,0,0,5,0,0,0,6,0,0,0,7,0,0
            };
            var seq4 = new[]
            {
                0,0,0,1,0,0,0,2,0,0,0,3,0,0,0,4,0,0,0,5,0,0,0,6,0,0,0,7,0,0,0
            };

            var r = seq1.SkipEvery(3).ToArray();
            Assert.IsTrue(r.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6, 7 }));

            r = seq2.SkipEvery(3).ToArray();
            Assert.IsTrue(r.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6, 7 }));

            r = seq3.SkipEvery(3).ToArray();
            Assert.IsTrue(r.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6, 7 }));

            r = seq4.SkipEvery(3).ToArray();
            Assert.IsTrue(r.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6, 7 }));
        }

        [TestMethod]
        public void TakeEveryTest()
        {
            var seq1 = new[]
            {
                1,2,3,0,4,5,6,0,7,8,9,0,10,11,12
            };
            var seq2 = new[]
            {
                1,2,3,0,4,5,6,0,7,8,9,0,10,11,12,0
            };
            var seq3 = new[]
            {
                1,2,3,0,4,5,6,0,7,8,9,0,10,11,12,0,13
            };
            var seq4 = new[]
            {
                1,2,3,0,4,5,6,0,7,8,9,0,10,11,12,0,13,14
            };
            var seq5 = new[]
            {
                1,2,3,0,4,5,6,0,7,8,9,0,10,11,12,0,13,14,15
            };
            var seq6 = Enumerable.Range(0, 157);

            var r = seq1.TakeEvery(3).ToArray();
            Assert.IsTrue(r.SequenceEqual(new[] { 1,2,3,4,5,6,7,8,9,10,11,12 }));

            r = seq2.TakeEvery(3).ToArray();
            Assert.IsTrue(r.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }));

            r = seq3.TakeEvery(3).ToArray();
            Assert.IsTrue(r.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }));

            r = seq4.TakeEvery(3).ToArray();
            Assert.IsTrue(r.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }));

            r = seq5.TakeEvery(3).ToArray();
            Assert.IsTrue(r.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));

            r = seq6.TakeEvery(4).ToArray();
            Assert.IsTrue(r.Length == 126);
        }
    }
}
