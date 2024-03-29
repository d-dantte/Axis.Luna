﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

            sequence = System.Linq.Enumerable.Range(0, 20).ToArray();

            var skipped = sequence.SkipEvery(1, (_cnt, _v) => _v >= 10).ToArray();
            Assert.IsTrue(new[] { 1, 3, 5, 7, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }.SequenceEqual(skipped));

            var taken = sequence.TakeEvery(1, (_cnt, _v) => _v >= 10).ToArray();
            Assert.IsTrue(new[] { 0, 2, 4, 6, 8, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }.SequenceEqual(taken));
        }

        [TestMethod]
        public void CombinationTest()
        {
            "aacde"
                .Permutations()
                .Select(arr => new string(arr.ToArray()))
                .Distinct()
                .WithEvery(Console.WriteLine)
                .Count()
                .Consume(count => Console.WriteLine("Total combination count: " + count));

        }

        [TestMethod]
        public void BatchTest()
        {
            var numbers = Enumerable
                .Range(0, 100)
                .Batch(20)
                .ToArray();
        }

        [TestMethod]
        public void InsertAt_Tests()
        {
            var enm = new[] { 1, 2, 3, 4, 5 };

            var result = enm.InsertAt(0, 50).ToArray();
            Assert.AreEqual(6, result.Length);
            Assert.AreEqual(50, result[0]);

            result = enm.InsertAt(5, 50).ToArray();
            Assert.AreEqual(6, result.Length);
            Assert.AreEqual(50, result[5]);

            enm = new int[0];
            result = enm.InsertAt(0, 2).ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(2, result[0]);
        }

        [TestMethod]
        public void SKipEvery_Tests()
        {
            var x = new[] { 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0 };
            var skipped = x.SkipEvery(4).ToArray();

            skipped.ForEvery(t => Console.Write(t + " "));
        }

        [TestMethod]
        public void TakeEvery_Tests()
        {
            var x = new[] { 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0 };
            var skipped = x.TakeEvery(3).ToArray();

            skipped.ForEvery(t => Console.Write(t + " "));
        }
    }
}