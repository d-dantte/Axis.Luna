using Axis.Luna.Common.Numerics;
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
    /// Despite being a struct, a <see cref="BitSequence"/> is seen to externally be a sequence of zero or more bits;
    /// i.e, it's internal list of bits is either empty, or contains elements, it is (externally) never null;
    /// </para>
    /// </summary>
    public struct BitSequence :
        IEnumerable<bool>,
        IDefaultValueProvider<BitSequence>
    {
        #region Fields
        private readonly bool[] bits;
        #endregion

        #region Members
        public int Length => bits?.Length ?? 0;

        public BitSequence SignificantBits
        {
            get
            {
                if (IsDefault)
                    return this;

                int index = bits.Length - 1;
                for(; index >= 0; index--)
                {
                    if (bits[index])
                        break;
                }

                if (index < 0)
                    return this;

                return this[..(index + 1)];
            }
        }

        public bool this[int index]
        {
            get
            {
                if (IsDefault || index < 0 || index >= bits.Length)
                    throw new IndexOutOfRangeException();

                return bits[index];
            }
        }

        public BitSequence Slice(int start, int length)
        {
            if (IsDefault && start == 0 && length == 0)
                return this;

            return new BitSequence(bits.Slice(start, length));
        }
        #endregion

        #region Construction
        public BitSequence(IEnumerable<bool> bits)
        {
            ArgumentNullException.ThrowIfNull(bits);

            var barr = bits.ToArray();
            this.bits = barr.IsEmpty() ? null : barr;
        }

        public static BitSequence Of(IEnumerable<bool> bits) => new BitSequence(bits);

        public static BitSequence Of(params byte[] bytes) => Of(ToBits(bytes));

        /// <summary>
        /// converts only the significant bits of the bytes given.
        /// <para>
        /// Significant bits are what are left after trimming "0" bits from the right-end of the list of bits.
        /// 0000-0000 => empty bit sequence
        /// 0000-0001 => 1 bit sequence
        /// 0000-0100 => 100 bit sequence
        /// </para>
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static BitSequence OfSignificantBits(params byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);

            if (bytes.IsEmpty())
                return new BitSequence(Enumerable.Empty<bool>());

            var bits = ToBitList(bytes);
            var lastIndexOf1 = bits.LastIndexOf(true);
            return bits.ToArray()[..(lastIndexOf1 + 1)];
        }

        public static BitSequence Of(byte @byte) => Of(new[] { @byte });

        public static BitSequence Of(sbyte @byte) => Of(new[] { (byte)@byte });

        public static BitSequence Of(short value) => Of(BitConverter.GetBytes(value));

        public static BitSequence Of(ushort value) => Of(BitConverter.GetBytes(value));

        public static BitSequence Of(int value) => Of(BitConverter.GetBytes(value));

        public static BitSequence Of(uint value) => Of(BitConverter.GetBytes(value));

        public static BitSequence Of(long value) => Of(BitConverter.GetBytes(value));

        public static BitSequence Of(ulong value) => Of(BitConverter.GetBytes(value));

        public static BitSequence Of(Half value) => Of(BitConverter.GetBytes(value));

        public static BitSequence Of(float value) => Of(BitConverter.GetBytes(value));

        public static BitSequence Of(double value) => Of(BitConverter.GetBytes(value));

        public static BitSequence Of(
            decimal value)
            => decimal
                .GetBits(value)
                .Select(BitConverter.GetBytes)
                .SelectMany()
                .ToArray()
                .ApplyTo(Of);

        public static BitSequence Of(BigInteger value) => Of(value.ToByteArray());

        public static BitSequence Of(Guid guid) => Of(guid.ToByteArray());

        public static BitSequence Of(BitArray bits) => Of(bits.SelectAs<bool>());

        public static BitSequence Of(params bool[] bits) => new BitSequence(bits);

        public static BitSequence Of(Span<bool> bits) => Of(bits.ToArray());

        public static BitSequence Of(ArraySegment<bool> bits) => Of(bits.ToArray());
        #endregion

        #region Implicits
        public static implicit operator BitSequence(byte[] value) => Of(value);
        public static implicit operator BitSequence(byte value) => Of(value);
        public static implicit operator BitSequence(sbyte value) => Of(value);
        public static implicit operator BitSequence(short value) => Of(value);
        public static implicit operator BitSequence(ushort value) => Of(value);
        public static implicit operator BitSequence(int value) => Of(value);
        public static implicit operator BitSequence(uint value) => Of(value);
        public static implicit operator BitSequence(long value) => Of(value);
        public static implicit operator BitSequence(ulong value) => Of(value);
        public static implicit operator BitSequence(Half value) => Of(value);
        public static implicit operator BitSequence(float value) => Of(value);
        public static implicit operator BitSequence(double value) => Of(value);
        public static implicit operator BitSequence(decimal value) => Of(value);
        public static implicit operator BitSequence(BigInteger value) => Of(value);
        public static implicit operator BitSequence(Guid value) => Of(value);
        public static implicit operator BitSequence(BitArray value) => Of(value);
        public static implicit operator BitSequence(bool[] value) => Of(value);
        public static implicit operator BitSequence(Span<bool> value) => Of(value);
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
            return bits?
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
            return bits?
                .Select(bit => bit ? 1 : 0)
                .Aggregate(0, HashCode.Combine)
                ?? 0;
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return obj is BitSequence other
                && bits.NullOrTrue(other.bits, Enumerable.SequenceEqual);
        }

        public static bool operator ==(BitSequence first, BitSequence second) => first.Equals(second);
        public static bool operator !=(BitSequence first, BitSequence second) => !first.Equals(second);
        #endregion

        #region IEnumerable
        public IEnumerator<bool> GetEnumerator()
        {
            return IsDefault
                ? Enumerable.Empty<bool>().GetEnumerator()
                : ((IEnumerable<bool>)bits).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region DefaultProvider
        public bool IsDefault => bits is null;

        public static BitSequence Default => default;
        #endregion

        #region API

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

        public bool IsEmpty => IsDefault;

        public static BitSequence Empty => Default;

        public byte ByteAt(Index index)
        {
            if (IsDefault)
                throw new IndexOutOfRangeException();

            var actualIndex = index.GetOffset(bits.Length);
            if (actualIndex >= bits.Length)
                throw new IndexOutOfRangeException(nameof(index));

            var totalCount = bits.Length - actualIndex;

            return this
                .ToByteArray(
                    actualIndex,
                    totalCount > 8 ? 8 : totalCount)
                [0];
        }

        public byte[] ToByteArray() => ToByteArray(Range.All);

        public byte[] ToByteArray(Range bitRange)
        {
            var rangeInfo = bitRange.GetOffsetAndLength(Length);
            return ToByteArray(rangeInfo.Offset, rangeInfo.Length);
        }

        public byte[] ToByteArray(int startIndex, int bitLength)
        {
            if (startIndex == 0 && bitLength == 0)
                return Array.Empty<byte>();

            if (IsDefault)
                throw new IndexOutOfRangeException();

            return bits
                .Slice(startIndex, bitLength)
                .Batch(8)
                .Select(ToByte)
                .ToArray();
        }

        public BitArray ToBitArray() => IsDefault 
            ? new BitArray(Array.Empty<bool>())
            : new BitArray(bits);

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

        internal static bool[] ToBits(byte @byte)
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

        internal static bool[] ToBits(byte[] bytes) => ToBitList(bytes).ToArray();

        internal static List<bool> ToBitList(byte[] bytes)
        {
            var list = new List<bool>(8 * bytes.Length);
            foreach (var @byte in bytes)
            {
                var bits = ToBits(@byte);
                list.AddRange(bits);
            }

            return list;
        }

        #endregion
    }

    public struct BitSequence2 :
        IEnumerable<bool>,
        IDefaultValueProvider<BitSequence2>
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
        private readonly long length;
        #endregion

        #region Members
        public int Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public BitSequence2 SignificantBits
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public BitSequence2 Slice(int start, int length)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Construction
        public BitSequence2(IEnumerable<byte> bits, int length)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(IEnumerable<bool> bits)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(params byte[] bytes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// converts only the significant bits of the bytes given.
        /// <para>
        /// Significant bits are what are left after trimming "0" bits from the right-end of the list of bits.
        /// 0000-0000 => empty bit sequence
        /// 0000-0001 => 1 bit sequence
        /// 0000-0100 => 100 bit sequence
        /// </para>
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static BitSequence2 OfSignificantBits(params byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(byte @byte, bool useSignificantBits = true)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(sbyte @byte, bool useSignificantBits = true)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(short value, bool useSignificantBits = true)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(ushort value, bool useSignificantBits = true)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(int value, bool useSignificantBits = true)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(uint value, bool useSignificantBits = true)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(long value, bool useSignificantBits = true)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(ulong value, bool useSignificantBits = true)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(Half value, bool useSignificantBits = true)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(float value, bool useSignificantBits = true)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(double value, bool useSignificantBits = true)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(decimal value, bool useSignificantBits = true)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(BigInteger value, bool useSignificantBits = true)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(Guid guid)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(BitArray bits)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(params bool[] bits)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(Span<bool> bits)
        {
            throw new NotImplementedException();
        }

        public static BitSequence2 Of(ArraySegment<bool> bits)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Implicits
        public static implicit operator BitSequence2(byte[] value) => Of(value);
        public static implicit operator BitSequence2(byte value) => Of(value);
        public static implicit operator BitSequence2(sbyte value) => Of(value);
        public static implicit operator BitSequence2(short value) => Of(value);
        public static implicit operator BitSequence2(ushort value) => Of(value);
        public static implicit operator BitSequence2(int value) => Of(value);
        public static implicit operator BitSequence2(uint value) => Of(value);
        public static implicit operator BitSequence2(long value) => Of(value);
        public static implicit operator BitSequence2(ulong value) => Of(value);
        public static implicit operator BitSequence2(Half value) => Of(value);
        public static implicit operator BitSequence2(float value) => Of(value);
        public static implicit operator BitSequence2(double value) => Of(value);
        public static implicit operator BitSequence2(decimal value) => Of(value);
        public static implicit operator BitSequence2(BigInteger value) => Of(value);
        public static implicit operator BitSequence2(Guid value) => Of(value);
        public static implicit operator BitSequence2(BitArray value) => Of(value);
        public static implicit operator BitSequence2(bool[] value) => Of(value);
        public static implicit operator BitSequence2(Span<bool> value) => Of(value);
        public static implicit operator BitSequence2(ArraySegment<bool> value) => Of(value);
        #endregion

        #region Object overrides
        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(BitSequence2 first, BitSequence2 second) => first.Equals(second);
        public static bool operator !=(BitSequence2 first, BitSequence2 second) => !first.Equals(second);
        #endregion

        #region IEnumerable
        public IEnumerator<bool> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region DefaultProvider
        public bool IsDefault => bits is null;

        public static BitSequence2 Default => default;
        #endregion

        #region API

        public bool IsEmpty => IsDefault;

        public static BitSequence2 Empty => Default;

        public byte ByteAt(Index index)
        {
            throw new NotImplementedException();
        }

        public byte[] ToByteArray()
        {
            throw new NotImplementedException();
        }

        public byte[] ToByteArray(Range bitRange)
        {
            throw new NotImplementedException();
        }

        public byte[] ToByteArray(int startIndex, int bitLength)
        {
            throw new NotImplementedException();
        }

        public BitArray ToBitArray() => IsDefault
            ? new BitArray(Array.Empty<bool>())
            : new BitArray(bits);

        /// <summary>
        /// Creates a new bit sequence that is the concatenation of one bit sequence to the other
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public BitSequence2 Concat(BitSequence2 sequence)
        {
            return Of(((IEnumerable<bool>)this).Concat(sequence));
        }

        /// <summary>
        /// Creates a new bit sequence that is the result of shifting the bits of this sequence
        /// <paramref name="count"/> number of times to the left, padding the shifted side with "0"
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public BitSequence2 LeftShift(int count)
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
        public BitSequence2 RightShift(int count)
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
        public BitSequence2 CycleLeft(int count)
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
        public BitSequence2 CycleRight(int count)
        {
            if (count == Length)
                return this;

            count %= Length;
            var (left, right) = Split(count);
            return right.Concat(left);
        }

        public (BitSequence2 Left, BitSequence2 Right) Split(int index)
        {
            if (index < 0 || index > Length)
                throw new IndexOutOfRangeException();

            return (
                Left: this[..index],
                Right: this[index..]);
        }
        #endregion

        #region Operators
        public static BitSequence2 operator +(BitSequence2 left, BitSequence2 right) => left.Concat(right);

        public static BitSequence2 operator <<(BitSequence2 sequence, int shiftCount) => sequence.LeftShift(shiftCount);

        public static BitSequence2 operator >>(BitSequence2 sequence, int shiftCount) => sequence.RightShift(shiftCount);
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

        internal static bool[] ToBits(byte @byte)
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

        internal static bool[] ToBits(byte[] bytes) => ToBitList(bytes).ToArray();

        internal static List<bool> ToBitList(byte[] bytes)
        {
            var list = new List<bool>(8 * bytes.Length);
            foreach (var @byte in bytes)
            {
                var bits = ToBits(@byte);
                list.AddRange(bits);
            }

            return list;
        }

        #endregion
    }
}
