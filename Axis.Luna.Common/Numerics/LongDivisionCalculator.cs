using Axis.Luna.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Axis.Luna.Common.Numerics
{
    internal class LongDivisionMechine
    {
        private const decimal DecimalDigitRatio = 0.30102999566398114m;
        private readonly List<bool> fractionBits = new List<bool>();
        private BigInteger dividend = default;

        /// <summary>
        /// 
        /// </summary>
        public BigInteger Numerator { get; }

        /// <summary>
        /// 
        /// </summary>
        public BigInteger Denominator { get; }

        public LongDivisionMechine(BigInteger numerator, BigInteger denominator)
        {
            Numerator = numerator;
            Denominator = denominator;

            if (Denominator == 0)
                throw new ArgumentException($"{nameof(denominator)} cannot be 0");
        }

        public BigDecimal Divide(ushort bitPrecision = 128)
        {
            dividend = default;
            var (quotient, remainder) = BigInteger.DivRem(Numerator, Denominator);

            if (remainder == 0)
                return new BigDecimal(quotient);

            while (remainder != 0 && fractionBits.Count < bitPrecision)
            {
                Load(remainder);
                remainder = SubtractAndPush();
            }

            // If fractional points exist, multiply by 10 for as many precisions are needed (to shift the point to the right),
            // then create and return the decimal.
            var base2Scale = fractionBits.Count;
            var normalizedFraction = fractionBits
                // remove the initial zeros after the fractional point
                .SkipWhile(bit => !bit)
                .Reverse()
                .ToArray()
                .ApplyTo(bits => new BitArray(bits))
                .ToBytes()
                .ApplyTo(bytes => new BigInteger(bytes));

            var decimalPrecision = (int)Math.Ceiling(bitPrecision * DecimalDigitRatio);
            var normalizedQuotient = quotient * BigInteger.Pow(10, decimalPrecision);
            normalizedFraction *= BigInteger.Pow(10, decimalPrecision);
            normalizedFraction >>= base2Scale;

            return (
                normalizedQuotient + normalizedFraction,
                decimalPrecision);
        }

        private void Load(BigInteger remainder)
        {
            dividend = remainder;
            var shifts = -1;
            while (dividend < Denominator)
            {
                dividend <<= 1;
                shifts++;
            }

            Enumerable
                .Range(0, shifts)
                .Select(index => false)
                .Consume(fractionBits.AddRange);
        }

        private BigInteger SubtractAndPush()
        {
            var result = dividend - Denominator;

            if (result < 0)
                throw new ArithmeticException($"Fatal error: dividend ({dividend}) is less than the denominator ({Denominator}).");

            if (result > 0)
                fractionBits.Add(true);

            return result;
        }
    }
}
