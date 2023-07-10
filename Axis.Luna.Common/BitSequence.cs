using Axis.Luna.Common.Numerics;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;

namespace Axis.Luna.Common
{
    /// <summary>
    /// A sequence of zero or more bits, represented as bools, that can be manipulated into bytes, or have some
    /// bitwise operations performed on them.
    /// <para>
    /// Despite being a struct, a <see cref="BitSequence__"/> is seen to externally be a sequence of zero or more bits;
    /// i.e, it's internal list of bits is either empty, or contains elements, it is (externally) never null;
    /// </para>
    /// </summary>
    public struct BitSequence :
        IEnumerable<bool>,
        IDefaultValueProvider<BitSequence>,
        IResultParsable<BitSequence>
    {
        internal static byte[] BitMasks = new byte[]
        {
            1,  // index 0
            2,  // index 1
            4,  // index 2
            8,  // index 3
            16, // index 4
            32, // index 5
            64, // index 6
            128 // index 7
        };

        #region Fields
        private readonly byte[] bits;
        private readonly int bitLength;
        private readonly int? significantBitIndex;
        #endregion

        #region Properties
        public int Length => bitLength;

        /// <summary>
        /// Gets a <see cref="BitSequence"/> representing only the significant bits
        /// <para>
        /// Significant bits are what are left after trimming "0" bits from the right-end of the list of bits.
        /// 0000-0000 => empty bit sequence
        /// 0000-0001 => 1 bit sequence
        /// 0000-0100 => 100 bit sequence
        /// </para>
        /// </summary>
        public BitSequence SignificantBits
            => significantBitIndex is null ? this : this[..(significantBitIndex!.Value + 1)];

        public bool this[int index]
        {
            get
            {
                if (index >= Length || index < 0)
                    throw new IndexOutOfRangeException(nameof(index));

                var (ByteIndex, BitIndex) = ToIndex(index);
                return bits[ByteIndex].IsSet(BitIndex);
            }
        }
        #endregion

        #region Construction

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bits"></param>
        public BitSequence(IEnumerable<bool> bits)
        {
            ArgumentNullException.ThrowIfNull(bits);

            var bitsArray = bits.ToArray();
            this.bitLength = bitsArray.Length;

            this.bits = bitsArray
                .Batch(8)
                .Select(ToByte)
                .ToArray()
                .ApplyTo(arr => arr.IsEmpty() ? null : arr);

            this.significantBitIndex = this.bits is not null
                ? GetSignificantBitIndex(this.bits, this.bitLength)
                : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="bitRange"></param>
        public BitSequence(IEnumerable<byte> bits, Range bitRange)
            : this(bits.ToArray(), bitRange)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="bitRange"></param>
        public BitSequence(byte[] bits, Range bitRange)
        {
            ArgumentNullException.ThrowIfNull(bits);

            var chunkInfo = BitChunk(bits, bitRange);
            this.bits = chunkInfo.BitArray.IsEmpty() ? null : chunkInfo.BitArray;
            bitLength = chunkInfo.BitLength;
            this.significantBitIndex = this.bits is not null
                ? GetSignificantBitIndex(this.bits, this.bitLength)
                : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static BitSequence Of(IEnumerable<bool> bits) => new BitSequence(bits);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static BitSequence Of(params bool[] bits) => new BitSequence(bits);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static BitSequence Of(params byte[] bytes) => new BitSequence(bytes, Range.All);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitRange"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static BitSequence Of(Range bitRange, params byte[] bytes) => new BitSequence(bytes, bitRange);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="integer"></param>
        /// <returns></returns>
        public static BitSequence Of(BigInteger integer) => new BitSequence(integer.ToByteArray(), Range.All);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="integer"></param>
        /// <returns></returns>
        public static BitSequence Of(bool isUnsigned, BigInteger integer) => new BitSequence(integer.ToByteArray(isUnsigned), Range.All);

        #endregion

        #region Implicits
        public static implicit operator BitSequence(byte[] value) => Of(value);
        public static implicit operator BitSequence(byte value) => Of(value);
        public static implicit operator BitSequence(sbyte value) => Of(new BigInteger(value));
        public static implicit operator BitSequence(short value) => Of(new BigInteger(value));
        public static implicit operator BitSequence(ushort value) => Of(new BigInteger(value));
        public static implicit operator BitSequence(int value) => Of(new BigInteger(value));
        public static implicit operator BitSequence(uint value) => Of(new BigInteger(value));
        public static implicit operator BitSequence(long value) => Of(new BigInteger(value));
        public static implicit operator BitSequence(ulong value) => Of(new BigInteger(value));
        public static implicit operator BitSequence(BigInteger value) => Of(value);
        public static implicit operator BitSequence(BitArray value) => Of(value.SelectAs<bool>());
        public static implicit operator BitSequence(bool[] value) => Of(value);
        public static implicit operator BitSequence(Span<bool> value) => Of(value.ToArray());
        public static implicit operator BitSequence(ArraySegment<bool> value) => Of(value);
        #endregion

        #region Object overrides
        public override string ToString()
        {
            // 1. converts bools into 1 or 0
            // 2. group the list into octets (sets of 8)
            // 3. further group each octet into a quartet (sets of 4)
            // 4. join the quartets using a space " "
            // 5. join the octets using a comma ","
            // 6. surround the result in sqare brackets "[..]"
            // 7. if the original data was null, return "[*]"
            return this
                .Select(bit => bit ? "1" : "0")
                .GroupBy((bit, index) => index / 8)
                .Select(group => group
                    .GroupBy((bit, index) => index / 4)
                    .Select(group => group.JoinUsing(""))
                    .JoinUsing(" "))
                .JoinUsing(", ")
                .WrapIn("[", "]")
                ?? "[]";
        }

        public override int GetHashCode()
        {
            return this
                .Select(bit => bit ? 1 : 0)
                .Aggregate(0, HashCode.Combine);
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return obj is BitSequence other
                && Enumerable.SequenceEqual(this, other);
        }

        public static bool operator ==(BitSequence first, BitSequence second) => first.Equals(second);
        public static bool operator !=(BitSequence first, BitSequence second) => !first.Equals(second);
        #endregion

        #region IEnumerable
        public IEnumerator<bool> GetEnumerator()
        {
            if (IsDefault)
                yield break;

            for (int cnt = 0; cnt < Length; cnt++)
                yield return this[cnt];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region DefaultProvider
        public bool IsDefault => bits is null;

        public static BitSequence Default => default;
        #endregion

        #region Empty
        public bool IsEmpty => IsDefault;

        public static BitSequence Empty => Default;
        #endregion

        #region API

        /// <summary>
        /// Returns a representation of the bits where they are arranged like a series of bytes in memory,
        /// little-endian style.
        /// </summary>
        public string ToLittleEndianString()
        {
            // 1. converts bools into 1 or 0
            // 2. group the list into octets (sets of 8)
            // 3. further group each octet into a quartet (sets of 4)
            // 4. join the quartets using a space " "
            // 5. reverse the octet so they are in the format [7,6,5,4,3,2,1,0]
            // 6. surround the octet with square brackets "[..]"
            // 7. join the octets using a new-line
            // 8. if the original data was null, return "[]"
            return this
                .Select(bit => bit ? "1" : "0")
                .GroupBy((bit, index) => index / 8)
                .Select(group => group
                    .GroupBy((bit, index) => index / 4)
                    .Select(group => group.JoinUsing(""))
                    .JoinUsing(" ")
                    .ReverseString()
                    .WrapIn("[", "]"))
                .JoinUsing(Environment.NewLine)
                ?? "[]";
        }

        /// <summary>
        /// Returns a representation of the bits where they are arranged like a series of bytes in memory,
        /// big-endian style.
        /// </summary>
        public string ToBigEndianString()
        {
            return ToLittleEndianString()
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Reverse()
                .JoinUsing(Environment.NewLine);
        }

        public BitSequence Slice(int start, int length)
        {
            if (IsDefault)
            {
                if (start == 0 && length == 0)
                    return this;

                throw new ArgumentOutOfRangeException($"start: {start}, length: {length}");
            }

            return new BitSequence(bits, start..(start + length));
        }

        public byte ByteAt(Index index)
        {
            if (IsDefault)
                throw new IndexOutOfRangeException();

            var offset = index.IsFromEnd ? Length - index.Value : index.Value;

            if (offset < 0 || offset >= Length)
                throw new IndexOutOfRangeException();

            var newSequence = Length - offset > 8
                ? new BitSequence(bits, offset..(offset + 8))
                : new BitSequence(bits, offset..);

            return newSequence.ToByteArray()[0];
        }

        public byte[] ToByteArray()
        {
            if (IsDefault)
                return Array.Empty<byte>();

            //var (ByteIndex, BitIndex) = ToIndex(Length);
            //var array = new byte[ByteIndex + 1];
            //var lastByte = array.Length - 1;
            //for (int cnt = 0; cnt < array.Length; cnt++)
            //{
            //    array[cnt] = cnt == lastByte
            //        ? (byte)(bits[cnt] & OnBits(BitIndex + 1))
            //        : array[cnt] = bits[cnt];
            //}

            //return array;

            var array = new byte[bits.Length];
            Buffer.BlockCopy(bits, 0, array, 0, bits.Length);
            return array;
        }

        public byte[] ToByteArray(Range bitRange)
        {
            if (IsDefault)
            {
                if (Range.All.Equals(bitRange))
                    return Array.Empty<byte>();

                if (bitRange.Start.Value == 0 && bitRange.End.Value == 0)
                    return Array.Empty<byte>();

                throw new ArgumentOutOfRangeException(nameof(bitRange));
            }

            return new BitSequence(bits, bitRange).ToByteArray();
        }

        public byte[] ToByteArray(int startIndex, int bitLength) => ToByteArray(startIndex..(startIndex + bitLength));

        public BitArray ToBitArray() => IsDefault
            ? new BitArray(Array.Empty<bool>())
            : new BitArray(this.ToArray());

        /// <summary>
        /// Creates a new bit sequence that is the concatenation of one bit sequence to the other
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public BitSequence Concat(BitSequence sequence)
        {
            return Of(((IEnumerable<bool>)this).Concat(sequence));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public BitSequence Append(bool bit)
        {
            return this
                .As<IEnumerable<bool>>()
                .Append(bit)
                .ApplyTo(BitSequence.Of);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public BitSequence PrePend(BitSequence bits)
        {
            return bits.Concat(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bit"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public BitSequence Insert(bool bit, int index)
        {
            return this
                .As<IEnumerable<bool>>()
                .InsertAt(index, bit)
                .ApplyTo(BitSequence.Of);
        }

        /// <summary>
        /// Creates a new bit sequence that is the result of shifting the bits of this sequence
        /// <paramref name="count"/> number of times to the left, padding the shifted side with "0"
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public BitSequence LeftShift(int count)
        {
            return this
                .ToBitArray()
                .LeftShift(count);
        }

        /// <summary>
        /// Creates a new bit sequence that is the result of shifting the bits of this sequence
        /// <paramref name="count"/> number of times to the right, padding the shifted side with "0"
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public BitSequence RightShift(int count)
        {
            return this
                .ToBitArray()
                .RightShift(count);
        }

        /// <summary>
        /// Same as the left shift, except that the shifted end is padded with the values shifted
        /// off from the opposite end
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public BitSequence CycleLeft(int count)
        {
            if (count == Length)
                return this;

            count %= Length;
            var (left, right) = Split(Length - count);
            return right.Concat(left);
        }

        /// <summary>
        /// Same as the right shift, except that the shifted end is padded with the values shifted
        /// off from the opposite end
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public BitSequence CycleRight(int count)
        {
            if (count == Length)
                return this;

            count %= Length;
            var (left, right) = Split(count);
            return right.Concat(left);
        }

        public (BitSequence Left, BitSequence Right) Split(int index)
        {
            if (index < 0 || index > Length)
                throw new IndexOutOfRangeException();

            return (
                Left: this[..index],
                Right: this[index..]);
        }

        public static bool[] ToBits(byte @byte)
        {
            var array = new bool[8];
            array[0] = @byte.IsSet(0);
            array[1] = @byte.IsSet(1);
            array[2] = @byte.IsSet(2);
            array[3] = @byte.IsSet(3);
            array[4] = @byte.IsSet(4);
            array[5] = @byte.IsSet(5);
            array[6] = @byte.IsSet(6);
            array[7] = @byte.IsSet(7);
            return array;
        }
        #endregion

        #region Operators
        public static BitSequence operator +(BitSequence left, BitSequence right) => left.Concat(right);

        public static BitSequence operator <<(BitSequence sequence, int shiftCount) => sequence.LeftShift(shiftCount);

        public static BitSequence operator >>(BitSequence sequence, int shiftCount) => sequence.RightShift(shiftCount);
        #endregion

        #region Helpers

        private static byte ToByte(IEnumerable<bool> bitOctet)
        {
            return bitOctet
                .Select((bit, index) => (bit, index))
                .Aggregate((byte)0, (@byte, bitInfo) => @byte |= bitInfo.bit switch
                {
                    true => BitMasks[bitInfo.index],
                    false => 0
                });
        }

        internal static int? GetSignificantBitIndex(byte[] bytes, int bitCount)
        {
            ArgumentNullException.ThrowIfNull(bytes);

            if (bitCount > bytes.Length * 8)
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            for(int cnt = bitCount - 1; cnt >= 0; cnt--)
            {
                var (ByteIndex, BitIndex) = ToIndex(cnt);
                if (bytes[ByteIndex].IsSet(BitIndex))
                    return cnt;
            }

            return null;
        }

        internal static (int ByteIndex, int BitIndex) ToIndex(int flatBitIndex)
        {
            return (
                ByteIndex: Math.DivRem(flatBitIndex, 8, out var bitIndex),
                BitIndex: bitIndex);
        }


        /// <summary>
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="bitRange"></param>
        /// <returns></returns>
        internal static (byte[] BitArray, int BitLength) BitChunk(byte[] bytes, Range bitRange)
        {
            ArgumentNullException.ThrowIfNull(bytes);

            var chunkInfo = ToChunkInfo(bytes.Length, bitRange);
            var byteArray = new byte[chunkInfo.DestinationByteCount];
            var bitCount = chunkInfo.BitCount;
            var sourceIndex = chunkInfo.SourceByteOffset;
            for (int destinationIndex = 0; destinationIndex < byteArray.Length; destinationIndex++)
            {
                byteArray[destinationIndex] = (byte)(OnBits(bitCount) & (chunkInfo.BitPivot switch
                {
                    0 => bytes[sourceIndex],

                    1 => (bytes[sourceIndex] >> 1) | (bitCount > 7 ? (bytes[sourceIndex + 1] << 7) : 0),

                    2 => (bytes[sourceIndex] >> 2) | (bitCount > 6 ? (bytes[sourceIndex + 1] << 6) : 0),

                    3 => (bytes[sourceIndex] >> 3) | (bitCount > 5 ? (bytes[sourceIndex + 1] << 5) : 0),

                    4 => (bytes[sourceIndex] >> 4) | (bitCount > 4 ? (bytes[sourceIndex + 1] << 4) : 0),

                    5 => (bytes[sourceIndex] >> 5) | (bitCount > 3 ? (bytes[sourceIndex + 1] << 3) : 0),

                    6 => (bytes[sourceIndex] >> 6) | (bitCount > 2 ? (bytes[sourceIndex + 1] << 2) : 0),

                    7 => (bytes[sourceIndex] >> 7) | (bitCount > 1 ? (bytes[sourceIndex + 1] << 1) : 0),

                    _ => throw new InvalidOperationException($"Invalid Bit Pivot value: {chunkInfo.BitPivot}")
                }));

                sourceIndex++;
                bitCount -= 8;
            }

            return (byteArray, chunkInfo.BitCount);
        }

        internal static byte[] Chunk(byte[] bytes, Range bitRange) => BitChunk(bytes, bitRange).BitArray;

        private static (int SourceByteOffset, int DestinationByteCount, int BitPivot, int BitCount) ToChunkInfo(
            int byteCount,
            Range bitRange)
        {
            var offset = bitRange.GetOffsetAndLength(byteCount * 8);
            return (
                SourceByteOffset: offset.Offset / 8,
                DestinationByteCount: Math.DivRem(offset.Length, 8, out var rem) + (rem > 0 ? 1 : 0),
                BitPivot: offset.Offset % 8,
                BitCount: offset.Length);
        }

        private static byte OnBits(int bitCount)
        {
            if (bitCount < 0)
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            return bitCount switch
            {
                0 => 0,
                1 => 1,
                2 => 3,
                3 => 7,
                4 => 15,
                5 => 31,
                6 => 63,
                7 => 127,
                _ => 255
            };
        }
        #endregion

        #region IResultParsable
        public static bool TryParse(
            string text,
            out IResult<BitSequence> result)
            => (result = Parse(text)).IsDataResult();

        /// <summary>
        /// Parses a sequential arrangement of 1s and 0s into a bit sequence.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FormatException"></exception>
        public static IResult<BitSequence> Parse(string text)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            if (string.Empty.Equals(text))
                return default;

            if (string.IsNullOrWhiteSpace(text))
                throw new FormatException("Invalid text");

            return text
                .Where(c => '1'.Equals(c) || '0'.Equals(c))
                .Select('1'.Equals)
                .ApplyTo(BitSequence.Of)
                .ApplyTo(Result.Of);
        }

        #endregion
    }
}
