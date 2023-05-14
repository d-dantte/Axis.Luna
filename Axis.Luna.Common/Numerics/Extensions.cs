using Axis.Luna.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Axis.Luna.Common.Numerics
{
    internal static class Extensions
    {
        private static readonly byte[] ByteMasks = new byte[]
        {
            1,
            2,
            4,
            8,
            16,
            32,
            64,
            128
        };

        internal static bool IsSet(this byte @byte, int bitIndex) => (@byte & ByteMasks[bitIndex]) == ByteMasks[bitIndex];

        internal static (BigInteger mantissa, int scale) NormalizeBigDecimal(this (BigInteger mantissa, int scale) values)
        {
            if (values.scale < 0)
                throw new ArgumentOutOfRangeException($"{nameof(values.scale)} is < 0. '{values.scale}'");

            if (values.scale == 0)
                return values;

            var trailingZeros = values.mantissa.TrailingDecimalZeroCount();
            var truncationCount = trailingZeros >= values.scale
                ? values.scale : trailingZeros;
            var newScale = values.scale - truncationCount;

            return (values.mantissa / (BigInteger.Pow(10, (int)truncationCount)), (int)newScale);
        }

        internal static (BigInteger mantissa, byte scale) Deconstruct(this Half half) => Deconstruct((double)half);

        internal static (BigInteger mantissa, byte scale) Deconstruct(this float @float) => Deconstruct((double)@float);

        internal static (BigInteger mantissa, byte scale) Deconstruct(this double @double)
        {
            return @double
                .NonScientificNotation()
                .DeconstructFromNotation();
        }

        internal static (BigInteger mantissa, byte scale) Deconstruct(this decimal @decimal)
        {
            return @decimal
                .NonScientificNotation()
                .DeconstructFromNotation();
        }

        internal static (BigInteger mantissa, byte scale) DeconstructFromNotation(this string notation)
        {
            var negative = notation.StartsWith('-');
            notation = notation.TrimStart("-");

            var pointIndex = notation.IndexOf('.');
            notation = notation.Replace(".", "");

            var mantissa = BigInteger.Parse(notation.TrimStart('0'));
            if (negative)
                mantissa = BigInteger.Negate(mantissa);

            var scale = pointIndex > 0
                ? notation.Length - pointIndex
                : 0;

            return (mantissa, (byte)scale);
        }

        private static (BigInteger mantissa, byte scale) DeconstructFromUnderlyingRepresentation(this decimal @decimal)
        {
            var ints = decimal.GetBits(@decimal);
            var scaleComponent = BitConverter.GetBytes(ints[3]);

            var scale = scaleComponent[2];
            var sign = scaleComponent[3] == 0;
            var mantissa = ints
                .Take(3)
                .SelectMany(@int => BitConverter.GetBytes(@int))
                .ApplyTo(_bytes => new BigInteger(_bytes.ToArray(), true));

            if (!sign)
                mantissa = BigInteger.Negate(mantissa);

            return (mantissa, scale);
        }

        internal static byte[] ToBytes(this BitArray bitArray)
        {
            var quotient = Math.DivRem(bitArray.Length, 8, out var rem);
            var count = rem > 0 ? quotient + 1 : quotient;

            return Enumerable
                .Range(0, count)
                .Select(index => bitArray.ToByte(index))
                .ToArray();
        }

        internal static byte ToByte(this BitArray bitArray, int byteIndex)
        {
            var bitOffset = byteIndex * 8;
            byte @byte = 0;

            for (int index = 0; index < 8; index++)
            {
                var currentOffset = bitOffset + index;
                if (currentOffset < bitArray.Length)
                {
                    if (bitArray[currentOffset])
                        @byte |= ByteMasks[index];
                }
                else break;
            }
            return @byte;
        }

        /// <summary>
        /// Converts the bits from the given byte array into a stream of bits, starting from the lower-end bit.
        /// <para>
        /// If the <paramref name="includeOnlySignificantBits"/> is set, this will only consider bits up on till the last '1' bit in the last byte.
        /// <code>
        /// E.g, given the array with 3 bytes,
        ///     input = (high end)0001-0111 0000-0000 1011-1000(low end)
        ///     in array form = [
        ///         1011-1000,
        ///         0000-0000,
        ///         0001-0111
        ///     ]
        ///     
        ///     output with only significant bits  = [
        ///         0,0,0,1,1,1,0,1,0,0,0,0,0,0,0,0,1,1,1,0,1
        ///     ]
        ///     
        ///     output with all bits = [
        ///         0,0,0,1,1,1,0,1,0,0,0,0,0,0,0,0,1,1,1,0,1,0,0,0
        ///     ]
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="bytes">The array</param>
        /// <param name="includeOnlySignificantBits">indicates if only significant bits should be included</param>
        /// <returns></returns>
        internal static IEnumerable<bool> ToBitStream(this byte[] bytes, bool includeOnlySignificantBits = false)
        {
            var last = bytes.Length - 1;
            for (int index = 0; index < bytes.Length; index++)
            {
                if (!includeOnlySignificantBits || index != last)
                {
                    yield return bytes[index].IsSet(0);
                    yield return bytes[index].IsSet(1);
                    yield return bytes[index].IsSet(2);
                    yield return bytes[index].IsSet(3);
                    yield return bytes[index].IsSet(4);
                    yield return bytes[index].IsSet(5);
                    yield return bytes[index].IsSet(6);
                    yield return bytes[index].IsSet(7);
                }
                else
                {
                    var significantBitIndex = FindSignificantBitIndex(bytes[index]);
                    for (int bitIndex = 0; bitIndex <= significantBitIndex; bitIndex++)
                    {
                        yield return bytes[index].IsSet(bitIndex);
                    }
                }
            }
        }

        internal static int FindSignificantBitIndex(this byte @byte)
        {
            if (@byte.IsSet(7))
                return 7;

            if (@byte.IsSet(6))
                return 6;

            if (@byte.IsSet(5))
                return 5;

            if (@byte.IsSet(7))
                return 4;

            if (@byte.IsSet(7))
                return 3;

            if (@byte.IsSet(7))
                return 2;

            if (@byte.IsSet(7))
                return 1;

            if (@byte.IsSet(7))
                return 0;

            return -1;
        }

        internal static string NonScientificNotation(this double d) => d.ToString("0." + new string('#', 339));

        internal static string NonScientificNotation(this decimal d) => d.ToString("0." + new string('#', 39));

        internal static IEnumerable<TItem> TakeExactly<TItem>(this IEnumerable<TItem> items, int value)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            var taken = items.Take(value);

            using var enumerator = taken.GetEnumerator();
            for (var index = 0; index < value; index++)
            {
                if (enumerator.MoveNext())
                    yield return enumerator.Current;

                else yield return default;
            }
        }

        internal static int TrailingDecimalZeroCount(this BigInteger value)
        {
            var count = 0;
            foreach(var @char in value.ToString().Reverse())
            {
                if (@char == '0')
                    count++;

                else break;
            }

            return count;
        }

        internal static string AsString(this IEnumerable<char> chars) => new string(chars.ToArray());

    }
}
