using Axis.Luna.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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

        internal static (BigInteger mantissa, short scale) Deconstruct(this double @double) => Deconstruct((decimal)@double);

        internal static (BigInteger mantissa, byte scale) Deconstruct(this decimal @decimal)
        {
            var ints = decimal.GetBits(@decimal);
            var scaleComponent = BitConverter.GetBytes(ints[3]);

            var scale = scaleComponent.ToArray()[2];
            var sign = scaleComponent.ToArray()[3] == 0;
            var mantissa = ints
                .Take(3)
                .SelectMany(@int => BitConverter.GetBytes(@int))
                .ApplyTo(_bytes => new BigInteger(_bytes.ToArray(), true));

            if (!sign)
                mantissa = BigInteger.Negate(mantissa);

            return (mantissa, scale);
        }

        internal static BigInteger InsertBits(this BigInteger @this, byte[] bytes)
        {
            if (bytes is null)
                throw new ArgumentNullException(nameof(bytes));

            return bytes
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
    }
}
