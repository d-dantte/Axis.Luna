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
    /// A sequence of bits, represented as bools, that can be manipulated into
    /// bytes.
    /// </summary>
    public struct BitSequence :
        IEnumerable<bool>,
        IDefaultValueProvider<BitSequence>
    {
        #region Fields
        private bool[] bits;
        #endregion

        #region Members
        public int Length => bits?.Length ?? -1;

        public bool this[int index]
        {
            get
            {
                ValidateState();
                return bits[index];
            }
        }

        public BitSequence Slice(int start, int length)
        {
            ValidateState();
            return new BitSequence(bits.Slice(start, length));
        }
        #endregion

        #region Construction
        public BitSequence(IEnumerable<bool> bits)
        {
            ArgumentNullException.ThrowIfNull(bits);

            this.bits = bits.ToArray();
        }

        public static BitSequence Of(IEnumerable<bool> bits) => new BitSequence(bits);

        public static BitSequence Of(params byte[] bytes)
            => new BitArray(bytes).SelectAs<bool>().ApplyTo(Of);

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
            return bits?
                .Select(bit => bit ? "1" : "0")
                .GroupBy((bit, index) => index / 8)
                .Select(group => group
                    .GroupBy((bit, index) => index / 4)
                    .Select(group => group.JoinUsing(""))
                    .JoinUsing(" "))
                .JoinUsing(", ")
                .WrapIn("[", "]")
                ?? "[*]";
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
            return ((IEnumerable<bool>)bits).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return bits.GetEnumerator();
        }
        #endregion

        #region Byte Manipulation methods
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

        public byte ByteAt(Index index)
        {
            ValidateState();
            var actualIndex = index.GetOffset(bits.Length);
            if (actualIndex >= bits.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var totalCount = bits.Length - actualIndex;

            return this
                .ToByteArray(
                    actualIndex,
                    totalCount > 8 ? 8 : totalCount)
                [0];
        }

        public byte[] ToByteArray() => ToByteArray(Range.All);

        public byte[] ToByteArray(Range range)
        {
            ValidateState();
            var rangeInfo = range.GetOffsetAndLength(bits.Length);
            return ToByteArray(rangeInfo.Offset, rangeInfo.Length);
        }

        public byte[] ToByteArray(int startIndex, int bitLength)
        {
            ValidateState();
            return bits
                .Slice(startIndex, bitLength)
                .Batch(8)
                .Select(ToByte)
                .ToArray();
        }
        #endregion

        #region DefaultProvider
        public bool IsDefault => bits is null;

        public BitSequence Default => default;
        #endregion

        #region Misc
        public BitArray ToBitArray() => new BitArray(bits);
        #endregion

        #region Helpers

        private void ValidateState()
        {
            if (IsDefault)
                throw new InvalidOperationException("BitSequence is in an invalid state");
        }


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

        #endregion
    }
}
