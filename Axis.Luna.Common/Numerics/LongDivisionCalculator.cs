using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Axis.Luna.Common.Numerics
{
    internal class LongDivisionMechine
    {
        private readonly List<bool?> quotientBits = new List<bool>();
        private BigInteger? dividend = null;

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
                throw new DivideByZeroException();
        }

        public BigDecimal Divide(ushort bitPrecision = 128)
        {
            var (remainder, quotient) = BigInteger.DivRem(Numerator, Denominator);

            quotient
                .ToByteArray()
                .ToBitStream(true)
                .Select(bit => (bool?)bit)
                .Consume(quotientBits.AddRange);

            // numerator < denominator. Include a null to indicate the fractional point.
            if (quotientBits.Count == 0)
                quotientBits.Add(null);

            do
            {
                Load(remainder);
                remainder = SubtractAndPush();
            }
            while (remainder != 0 && quotientBits.Count < bitPrecision);

            // If fractional points exist, multiply by 10 for as many precisions are needed (to shift the point to the right),
            // then create and return the decimal.
        }

        private void Load(BigInteger remainder)
        {
            // first time
            var isFirstLoading = dividend is null;

            dividend = remainder;
            var shifts = 0;
            while (dividend < Denominator)
            {
                dividend <<= 1;
                shifts++;
            }

            Enumerable
                .Range(0, shifts - (isFirstLoading ? 0 : 1))
                .Select(index => (bool?)false)
                .Consume(quotientBits.AddRange);
        }

        private BigInteger SubtractAndPush()
        {
            var result = dividend - Denominator;

            if (result < 0)
                throw new ArithmeticException($"Fatal error: dividend ({dividend}) is less than the denominator ({Denominator}).");

            if (result > 0)
                quotientBits.Add(true);

            return result ?? throw new InvalidOperationException($"Null dividend encountered");
        }
    }
}
