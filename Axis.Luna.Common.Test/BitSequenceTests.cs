using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Axis.Luna.Common.Test
{
    [TestClass]
    public class BitSequenceTests
    {
        // LE: 0000 1111, 1101 0010, 0011 1010, 0001 0011
        private static readonly int SampleValue = 265435667;


        [TestMethod]
        public void Length_Tests()
        {
            BitSequence bs = default;
            Assert.AreEqual(-1, bs.Length);

            bs = (byte)3;
            Assert.AreEqual(8, bs.Length);

            bs = 3;
            Assert.AreEqual(32, bs.Length);
        }

        [TestMethod]
        public void Indexer_Tests()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => default(BitSequence)[0]);

            Assert.ThrowsException<IndexOutOfRangeException>(
                () => BitSequence.Of((byte)3)[8]);

            BitSequence bs = 3;
            var bit = bs[1];
            Assert.IsTrue(bit);
            bit = bs[^31];
            Assert.IsTrue(bit);
        }

        [TestMethod]
        public void Slice_Tests()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => default(BitSequence).Slice(0, 2));

            // LE: 1011 1111
            BitSequence bs = (byte)191;

            // LE: 1111
            var slice = bs.Slice(0, 4);
            Assert.IsTrue(slice.All(b => b));

            // LE: 1011
            slice = bs[4..];
            Assert.IsTrue(slice[1]);
            Assert.IsFalse(slice[^2]);
            Assert.IsTrue(slice[^1]);
        }

        [TestMethod]
        public void ByteAt_Tests()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => default(BitSequence).ByteAt(0));

            BitSequence bs = SampleValue;

            // 0001 0011
            var @byte = bs.ByteAt(0);
            Assert.AreEqual(19, @byte);

            // 0000 01001
            @byte = bs.ByteAt(1);
            Assert.AreEqual(9, @byte);

            // 0100 0111
            @byte = bs.ByteAt(11);
            Assert.AreEqual(71, @byte);

            // 0000 0011
            @byte = bs.ByteAt(26);
            Assert.AreEqual(3, @byte);
        }

        [TestMethod]
        public void ToByteArray_Tests()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => default(BitSequence).ToByteArray(0, 3));

            BitSequence bs = SampleValue;

            // 0000 1111, 1101 0010, 0011 1010, 0001 0011
            var bytes = bs.ToByteArray();
            Assert.AreEqual(SampleValue, BitConverter.ToInt32(bytes));

            // 0000 1111, 1101 0010, 0011 1010, 0001 0011
            bytes = bs.ToByteArray(0, 32);
            Assert.AreEqual(SampleValue, BitConverter.ToInt32(bytes));

            // 0000 01001
            bytes = bs.ToByteArray(1, 8);
            Assert.AreEqual(1, bytes.Length);
            Assert.AreEqual(9, bytes[0]);

            // 0001 1101, 0000 1001
            bytes = bs.ToByteArray(1, 16);
            Assert.AreEqual(2, bytes.Length);
            Assert.AreEqual(7433, BitConverter.ToInt16(bytes));

            // 0000 0001, 0001 1101, 0000 1001
            bytes = bs.ToByteArray(1, 19);
            Assert.AreEqual(3, bytes.Length);
            Assert.AreEqual(72969, (int)new BigInteger(bytes));
        }

        [TestMethod]
        public void ToByteArray_WithRange_Tests()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => default(BitSequence).ToByteArray(0..3));

            BitSequence bs = SampleValue;

            // 0000 1111, 1101 0010, 0011 1010, 0001 0011
            var bytes = bs.ToByteArray(..);
            Assert.AreEqual(SampleValue, BitConverter.ToInt32(bytes));

            // 0000 01001
            bytes = bs.ToByteArray(1..8);
            Assert.AreEqual(1, bytes.Length);
            Assert.AreEqual(9, bytes[0]);

            // 0001 1101, 0000 1001
            bytes = bs.ToByteArray(1..16);
            Assert.AreEqual(2, bytes.Length);
            Assert.AreEqual(7433, BitConverter.ToInt16(bytes));

            // 0000 0001, 0001 1101, 0000 1001
            bytes = bs.ToByteArray(1..^13);
            Assert.AreEqual(3, bytes.Length);
            Assert.AreEqual(72969, (int)new BigInteger(bytes));
        }



        [TestMethod]
        public void Misc_Tests()
        {
            Console.WriteLine(Range.All);
            Console.WriteLine((^1).GetOffset(5));
            Console.WriteLine((^1).GetOffset(6));
            Console.WriteLine((^2).GetOffset(5));
            Console.WriteLine((1..5).GetOffsetAndLength(12));
            Console.WriteLine((1..^5).GetOffsetAndLength(12));
        }

        [TestMethod]
        public void Misc_Tests2()
        {
            var index = ^2;
            Console.WriteLine(index.Value);
            Console.WriteLine(index.IsFromEnd);
        }

        private void PrintSequence<T>(IEnumerable<T> enm)
        {
            var text = string.Join(
                ", ",
                enm.Select(item => item.ToString()));
            Console.WriteLine(text);
        }

    }
}
