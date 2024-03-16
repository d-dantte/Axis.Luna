using Axis.Luna.Result;
using System.Collections;
using System.Numerics;

namespace Axis.Luna.BitSequence.Test
{
    [TestClass]
    public class BitSequenceTests
    {
        // LE: 0000 1111, 1101 0010, 0011 1010, 0001 0011
        private static readonly int SampleValue = 265435667;

        [TestMethod]
        public void Constructor_CreatesInstance()
        {
            var bitSeq = new BitSequence(Enumerable.Empty<bool>());
            Assert.IsNotNull(bitSeq);

            bitSeq = new BitSequence(ArrayUtil.Of(true));
            Assert.IsNotNull(bitSeq);

            bitSeq = new BitSequence(ArrayUtil.Of(false, true));
            Assert.IsNotNull(bitSeq);

            bitSeq = new BitSequence(Enumerable.Empty<byte>(), 0..);
            Assert.IsNotNull(bitSeq);

            bitSeq = new BitSequence(ArrayUtil.Of<byte>(0, 1, 2, 3), 1..22);
            Assert.IsNotNull(bitSeq);
        }

        [TestMethod]
        public void Constructor_WithInvalidArgs()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new BitSequence(null!));
            Assert.ThrowsException<ArgumentNullException>(
                () => new BitSequence(default(IEnumerable<byte>)!, 0..));
            Assert.ThrowsException<ArgumentNullException>(
                () => new BitSequence(default(byte[])!, 0..));
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new BitSequence(Enumerable.Empty<byte>(), 0..22));
        }


        [TestMethod]
        public void Length_Tests()
        {
            BitSequence bs = default;
            Assert.AreEqual(0, bs.Length);

            bs = (byte)3;
            Assert.AreEqual(8, bs.Length);

            bs = 3;
            Assert.AreEqual(8, bs.Length);

            bs = BitConverter.GetBytes(3);
            Assert.AreEqual(32, bs.Length);
        }

        [TestMethod]
        public void Indexer_Tests()
        {
            Assert.ThrowsException<IndexOutOfRangeException>(
                () => BitSequence.Of((byte)3)[8]);

            Assert.ThrowsException<IndexOutOfRangeException>(
                () => BitSequence.Of((byte)3)[-1]);
            Assert.ThrowsException<IndexOutOfRangeException>(
                () =>default(BitSequence)[0]);

            BitSequence bs = 3;
            var bit = bs[1];
            Assert.IsTrue(bit);

            bit = bs[^1];
            Assert.IsFalse(bit);

            bit = bs[^8];
            Assert.IsTrue(bit);
        }

        [TestMethod]
        public void Slice_Tests()
        {
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

            // Default
            bs = default;
            Assert.AreEqual(bs, bs.Slice(0, 0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => bs.Slice(0, 4));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => bs.Slice(5, 4));
        }

        [TestMethod]
        public void ByteAt_Tests()
        {

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

            @byte = bs.ByteAt(^2);
            Assert.AreEqual(0, @byte);

            Assert.ThrowsException<IndexOutOfRangeException>(() => bs.ByteAt(^44));
            Assert.ThrowsException<IndexOutOfRangeException>(() => bs.ByteAt(44));
        }

        [TestMethod]
        public void CycleLeft_Tests()
        {
            BitSequence bs = ArrayUtil.Of(true, false);
            var result = bs.CycleLeft(2);
            Assert.AreEqual(bs, result);

            result = bs.CycleLeft(1);
            Assert.AreEqual(BitSequence.Of(false, true), result);
        }

        [TestMethod]
        public void CycleRight_Tests()
        {
            BitSequence bs = ArrayUtil.Of(true, false);
            var result = bs.CycleRight(2);
            Assert.AreEqual(bs, result);

            result = bs.CycleRight(1);
            Assert.AreEqual(BitSequence.Of(false, true), result);
        }

        [TestMethod]
        public void ToByteArray_Tests()
        {
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

            bs = default;
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => bs.ToByteArray(1..2));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => bs.ToByteArray(0..2));
        }

        [TestMethod]
        public void GetSignificantBitIndex_Test()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => BitSequence.GetSignificantBitIndex(null!, 0));

            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => BitSequence.GetSignificantBitIndex(ArrayUtil.Of<byte>(1, 2), 33));

            var index = BitSequence.GetSignificantBitIndex(
                ArrayUtil.Of<byte>(0, 0, 0),
                10);
            Assert.IsNull(index);

            index = BitSequence.GetSignificantBitIndex(
                ArrayUtil.Of<byte>(2),
                3);
            Assert.AreEqual(index, 1);
        }

        [TestMethod]
        public void BitChunk_Test()
        {
            var result = BitSequence.BitChunk(Array.Empty<byte>(), 0..0);
            Assert.AreEqual(0, result.BitArray.Length);
            Assert.AreEqual(0, result.BitLength);

            var bytes = ArrayUtil.Of<byte>(1, 2, 3, 233, 234, 235, 0, 1, 2, 7);
            result = BitSequence.BitChunk(bytes, 0..1);
            Assert.AreEqual(1, result.BitLength);

            result = BitSequence.BitChunk(bytes, 1..);
            Assert.AreEqual(79, result.BitLength);

            result = BitSequence.BitChunk(bytes, 2..);
            Assert.AreEqual(78, result.BitLength);

            result = BitSequence.BitChunk(bytes, 3..);
            Assert.AreEqual(77, result.BitLength);

            result = BitSequence.BitChunk(bytes, 4..);
            Assert.AreEqual(76, result.BitLength);

            result = BitSequence.BitChunk(bytes, 5..);
            Assert.AreEqual(75, result.BitLength);

            result = BitSequence.BitChunk(bytes, 6..);
            Assert.AreEqual(74, result.BitLength);

            result = BitSequence.BitChunk(bytes, 7..);
            Assert.AreEqual(73, result.BitLength);
        }

        [TestMethod]
        public void OnBitTest()
        {
            var result = BitSequence.OnBits(0);
            Assert.AreEqual(0, result);

            result = BitSequence.OnBits(1);
            Assert.AreEqual(1, result);

            result = BitSequence.OnBits(2);
            Assert.AreEqual(3, result);

            result = BitSequence.OnBits(3);
            Assert.AreEqual(7, result);

            result = BitSequence.OnBits(4);
            Assert.AreEqual(15, result);

            result = BitSequence.OnBits(5);
            Assert.AreEqual(31, result);

            result = BitSequence.OnBits(6);
            Assert.AreEqual(63, result);

            result = BitSequence.OnBits(7);
            Assert.AreEqual(127, result);

            result = BitSequence.OnBits(8);
            Assert.AreEqual(255, result);
        }

        [TestMethod]
        public void ToByteArray_WithRange_Tests()
        {
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
        public void ToStringTests()
        {
            BitSequence bs = ArrayUtil.Of(true, false, true, false, true);
            var text = bs.ToString();
            Assert.AreEqual("[1010 1]", text);

            bs = ArrayUtil.Of(true, false, true, false, true, true, true, true);
            text = bs.ToString();
            Assert.AreEqual("[1010 1111]", text);

            bs = ArrayUtil.Of(
                true, false, true, false,  true, true, true, true,
                false, false, false, true, true, true, false, true);
            text = bs.ToString();
            Assert.AreEqual("[1010 1111, 0001 1101]", text);

            bs = default;
            text = bs.ToString();
            Assert.AreEqual("[]", text);
        }

        [TestMethod]
        public void GetHashCode_Tests()
        {
            BitSequence bs = default;
            var hash = bs.GetHashCode();
            Assert.AreEqual(0, hash);

            bs = ArrayUtil.Of(true, false, true, false);
            hash = bs.GetHashCode();
        }

        [TestMethod]
        public void Equals_Tests()
        {
            BitSequence bs = ArrayUtil.Of(true, false, true, false, true);
            BitSequence bs2 = ArrayUtil.Of(true, false, true, false, true);
            Assert.AreEqual(bs, bs);
            Assert.AreEqual(bs, bs2);
            Assert.IsTrue(bs.Equals(bs));
            Assert.IsTrue(bs.Equals(bs2));
            Assert.IsTrue(bs == bs2);
            Assert.IsFalse(bs != bs2);

            bs2 = ArrayUtil.Of(true, false, true, false, true, true, true, true);
            Assert.AreNotEqual(bs, bs2);
            Assert.IsFalse(bs.Equals(bs2));
            Assert.IsFalse(bs == bs2);
            Assert.IsTrue(bs != bs2);

            Assert.IsFalse(bs.Equals(""));
        }

        [TestMethod]
        public void Of_Tests()
        {
            var bs = BitSequence.Of((IEnumerable<bool>)ArrayUtil.Of(true, false));
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(true, false),
                bs));

            bs = BitSequence.Of(0..1, 0);
            Assert.IsTrue(
                Enumerable.SequenceEqual(ArrayUtil.Of(false), bs));

            bs = BitSequence.Of(0, 10);
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, false, false, false, false, false, false, false,
                    false, true, false, true, false, false, false, false),
                bs));

            bs = BitSequence.Of(10);
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = BitSequence.Of(new BigInteger((sbyte)10));
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = BitSequence.Of((short)10);
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = BitSequence.Of((ushort)10);
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = BitSequence.Of(new BigInteger(10));
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = BitSequence.Of(new BigInteger((uint)10));
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = BitSequence.Of(new BigInteger((long)10));
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = BitSequence.Of(new BigInteger((ulong)10));
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = BitSequence.Of(new BigInteger(211));
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    true, true, false, false, true, false, true, true,
                    false, false, false, false, false, false, false, false),
                bs));

            bs = BitSequence.Of(true, 211);
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    true, true, false, false, true, false, true, true),
                bs));

            bs = BitSequence.Of(new BitArray(ArrayUtil.Of(true, false, true)).SelectAs<bool>());
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    true, false, true),
                bs));

            bs = BitSequence.Of(true, false, true);
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    true, false, true),
                bs));

            bs = BitSequence.Of(new ArraySegment<bool>(ArrayUtil.Of(true, false, true)));
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    true, false, true),
                bs));
        }

        [TestMethod]
        public void Implicit_Tests()
        {
            BitSequence bs = ArrayUtil.Of<byte>(0, (byte)10);
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, false, false, false, false, false, false, false,
                    false, true, false, true, false, false, false, false),
                bs));

            bs = (byte)10;
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = (sbyte)10;
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = (short)10;
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = ((ushort)10);
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = 10;
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = ((uint)10);
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = ((long)10);
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = ((ulong)10);
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = BigInteger.Parse("10");
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    false, true, false, true, false, false, false, false),
                bs));

            bs = new BitArray(ArrayUtil.Of(true, false, true));
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    true, false, true),
                bs));

            bs = ArrayUtil.Of(true, false, true);
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    true, false, true),
                bs));

            bs = new Span<bool>(ArrayUtil.Of(true, false, true));
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    true, false, true),
                bs));

            bs = new ArraySegment<bool>(ArrayUtil.Of(true, false, true));
            Assert.IsTrue(Enumerable.SequenceEqual(
                ArrayUtil.Of(
                    true, false, true),
                bs));
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

            var bs = BitSequence.Of((byte)0);
            Console.WriteLine(bs);
            Console.WriteLine(bs.SignificantBits.Length);
            Console.WriteLine(bs.ToLittleEndianString());
        }

        [TestMethod]
        public void Empty_Tests()
        {
            BitSequence bs = new bool[0];
           
            Assert.ThrowsException<IndexOutOfRangeException>(() => bs.ByteAt(0));
            Assert.AreEqual(0, bs.ToByteArray().Length);
            Assert.AreEqual(0, bs.ToByteArray(0..0).Length);
            Assert.AreEqual(0, bs.ToByteArray(..).Length);
            Assert.AreEqual(0, bs.ToBitArray().Length);
            Assert.AreEqual("[]", bs.ToString());
            Assert.AreEqual(0, bs[..].Length);
        }

        [TestMethod]
        public void Parse()
        {
            Assert.ThrowsException<ArgumentNullException>(() => BitSequence.Parse(null!));
            Assert.ThrowsException<FormatException>(() => BitSequence.Parse("   "));

            var bs = BitSequence.Parse("");
            Assert.AreEqual(default(BitSequence), bs.Resolve());

            bs = BitSequence.Parse("0101");
            Assert.AreEqual(BitSequence.Of(false, true, false, true), bs.Resolve());
        }

        [TestMethod]
        public void SignificantBits_Tests()
        {
            var bytes = BitConverter.GetBytes(0);
            var bs = BitSequence.Of(bytes).SignificantBits;
            Assert.AreEqual(32, bs.Length);

            bytes = BitConverter.GetBytes(1);
            bs = BitSequence.Of(bytes).SignificantBits;
            Assert.AreEqual(1, bs.Length);
            Assert.AreEqual(true, bs[0]);

            bytes = BitConverter.GetBytes(3);
            bs = BitSequence.Of(bytes).SignificantBits;
            Assert.AreEqual(2, bs.Length);
            Assert.AreEqual(true, bs[0]);
            Assert.AreEqual(true, bs[1]);

            bytes = BitConverter.GetBytes(256);
            bs = BitSequence.Of(bytes).SignificantBits;
            Assert.AreEqual(9, bs.Length);
            Assert.AreEqual(true, bs[8]);
            Assert.AreEqual(false, bs[7]);
            Assert.AreEqual(false, bs[6]);
            Assert.AreEqual(false, bs[5]);
            Assert.AreEqual(false, bs[4]);
            Assert.AreEqual(false, bs[3]);
            Assert.AreEqual(false, bs[2]);
            Assert.AreEqual(false, bs[1]);
            Assert.AreEqual(false, bs[0]);
        }

        [TestMethod]
        public void Concat_Tests()
        {
            BitSequence bs1 = new[] { true, false, true, false };
            BitSequence bs2 = new[] { true, true, true };

            var bs3 = bs1.Concat(bs2);
            Assert.IsTrue(Enumerable.SequenceEqual(
                bs3,
                new[] { true, false, true, false, true, true, true }));

            bs3 = bs1.Concat(default);
            Assert.AreEqual(bs1, bs3);

            bs3 = default(BitSequence).Concat(bs1);
            Assert.AreEqual(bs1, bs3);

            bs3 = default(BitSequence).Concat(default);
            Assert.AreEqual(default, bs3);
        }


        [TestMethod]
        public void LeftShift_Tests()
        {
            BitSequence bs1 = new[] { true, false, true, false };

            var bs2 = bs1.LeftShift(1);
            Assert.IsTrue(Enumerable.SequenceEqual(
                bs2,
                new[] { false, true, false, true }));

            bs2 = bs1.LeftShift(2);
            Assert.IsTrue(Enumerable.SequenceEqual(
                bs2,
                new[] { false, false, true, false }));
        }


        [TestMethod]
        public void RightShift_Tests()
        {
            BitSequence bs1 = new[] { true, false, true, false };

            var bs2 = bs1.RightShift(1);
            Assert.IsTrue(Enumerable.SequenceEqual(
                bs2,
                new[] { false, true, false, false }));

            bs2 = bs1.RightShift(2);
            Assert.IsTrue(Enumerable.SequenceEqual(
                bs2,
                new[] { true, false, false, false }));
        }


        [TestMethod]
        public void LeftCycle_Test()
        {
            BitSequence bs1 = new[] { true, false, true, false };

            var bs2 = bs1.CycleLeft(1);
            Assert.IsTrue(Enumerable.SequenceEqual(
                bs2,
                new[] { false, true, false, true }));

            bs2 = bs1.CycleLeft(2);
            Assert.IsTrue(Enumerable.SequenceEqual(
                bs2,
                new[] { true, false, true, false }));
        }


        [TestMethod]
        public void RightCycle_Tests()
        {
            BitSequence bs1 = new[] { true, false, true, false };

            var bs2 = bs1.CycleRight(1);
            Assert.IsTrue(Enumerable.SequenceEqual(
                bs2,
                new[] { false, true, false, true }));

            bs2 = bs1.CycleRight(2);
            Assert.IsTrue(Enumerable.SequenceEqual(
                bs2,
                new[] { true, false, true, false }));
        }

        [TestMethod]
        public void Split_Tests()
        {
            BitSequence bs1 = new[] { true, false, true, false };

            Assert.ThrowsException<IndexOutOfRangeException>(() => bs1.Split(-1));
            Assert.ThrowsException<IndexOutOfRangeException>(() => bs1.Split(5));

            var split = bs1.Split(0);
            Assert.AreEqual(0, split.Left.Length);
            Assert.AreEqual(4, split.Right.Length);

            split = bs1.Split(1);
            Assert.AreEqual(1, split.Left.Length);
            Assert.AreEqual(true, split.Left[0]);
            Assert.AreEqual(3, split.Right.Length);

            split = bs1.Split(2);
            Assert.AreEqual(2, split.Left.Length);
            Assert.AreEqual(2, split.Right.Length);

            split = bs1.Split(3);
            Assert.AreEqual(3, split.Left.Length);
            Assert.AreEqual(1, split.Right.Length);

            split = bs1.Split(4);
            Assert.AreEqual(4, split.Left.Length);
            Assert.AreEqual(0, split.Right.Length);
        }


        [TestMethod]
        public void Miscc_Tests()
        {
            var dto = DateTimeOffset.Now;
            var bs = BitSequence.Of(new BigInteger(dto.Year));

            Console.WriteLine(bs);
            Console.WriteLine(bs.SignificantBits);

            bs = BitSequence.Of((byte)14);
            Console.WriteLine(bs);
            Console.WriteLine(bs.SignificantBits);

            Console.WriteLine(dto.Millisecond);
            Console.WriteLine(dto.Microsecond);
            Console.WriteLine(dto.Nanosecond);
            Console.WriteLine(dto.Nanosecond/100);

            bs = BitSequence.Of(new BigInteger(33));
            var bs2 = BitSequence.Of(new BigInteger(-33));
        }

        [TestMethod]
        public void toSTringTests()
        {
            var bint = BigInteger.Parse("1234567890987654321234567890");
            var bs = BitSequence.Of(bint.ToByteArray());
            Console.WriteLine($"Regular tostring:\n{bs}");
            Console.WriteLine($"\n\nLittle Endian tostring:\n{bs.ToLittleEndianString()}");
            Console.WriteLine($"\n\nBig Endian tostring:\n{bs.ToBigEndianString()}");
        }


        [TestMethod]
        public void ParseTest()
        {
            var bis = BitSequence.Of(false, false, false, true, false);
            var str = bis.ToString();
            var result = BitSequence.Parse(str);
            Assert.IsTrue(result.IsDataResult());
            var bis2 = result.Resolve();
            Assert.AreEqual(bis, bis2);
        }
    }
}
