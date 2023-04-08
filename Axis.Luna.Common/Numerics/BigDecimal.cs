using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Luna.Common.Numerics
{
    public readonly struct BigDecimal :
        IComparable,
        IComparable<BigDecimal>,
        IEquatable<BigDecimal>,
        IParsable<BigDecimal>,
        IParsableResult<BigDecimal>,
        IUnaryPlusOperators<BigDecimal, BigDecimal>,
        IAdditionOperators<BigDecimal, BigDecimal, BigDecimal>,
        IUnaryNegationOperators<BigDecimal, BigDecimal>,
        ISubtractionOperators<BigDecimal, BigDecimal, BigDecimal>,
        //IBitwiseOperators<BigDecimal, BigDecimal, BigDecimal>,
        IIncrementOperators<BigDecimal>,
        IDecrementOperators<BigDecimal>,
        IMultiplyOperators<BigDecimal, BigDecimal, BigDecimal>,
        IDivisionOperators<BigDecimal, BigDecimal, BigDecimal>,
        IModulusOperators<BigDecimal, BigDecimal, BigDecimal>,
        IShiftOperators<BigDecimal, int, BigDecimal>,
        IEqualityOperators<BigDecimal, BigDecimal, bool>,
        IComparisonOperators<BigDecimal, BigDecimal, bool>
    {
        #region Fields

        private static readonly BigDecimal zero = new BigDecimal(0);

        private static readonly BigDecimal one = new BigDecimal(1);

        private static readonly BigDecimal negativeOne = new BigDecimal(-1);

        private readonly BigInteger _unscaledValue;
        private readonly int _scale;
        #endregion

        #region Properties
        public static BigDecimal NegativeOne => negativeOne;

        public static BigDecimal One => one;

        public static int Radix => 10;

        public static BigDecimal Zero => zero;

        public static BigDecimal AdditiveIdentity => zero;

        public static BigDecimal MultiplicativeIdentity => one;

        public int Sign => _unscaledValue.Sign;

        /// <summary>
        /// Indicates that this <see cref="BigDecimal"/> represents a value that can be represented as a 
        /// standard CLR <see cref="decimal"/>.
        /// </summary>
        public bool IsStandardDecimal => _unscaledValue.GetBitLength() <= 96;
        #endregion

        public BigDecimal(BigInteger unscaledValue, int scale)
        {
            _unscaledValue = unscaledValue;
            _scale = scale;
        }

        public BigDecimal(BigInteger unscaledValue)
            : this(unscaledValue, 0)
        {
        }

        public BigDecimal(long @long)
            : this(new BigInteger(@long))
        {
        }

        public BigDecimal(decimal @decimal)
        {
            var (value, scale) = @decimal.Deconstruct();
            _unscaledValue = value;
            _scale = scale;
        }

        public BigDecimal(double @double)
        {
            var (value, scale) = @double.Deconstruct();
            _unscaledValue = value;
            _scale = scale;
        }

        public static BigDecimal Abs(BigDecimal value)
        {
            return value.Sign switch
            {
                -1 => value.Negate(),
                0 or 1 => value,
                _ => throw new ArgumentException($"Invalid sign value: {value.Sign}")
            };
        }

        public BigDecimal Negate() => new BigDecimal(BigInteger.Negate(_unscaledValue), _scale);

        public static BigDecimal BigDivision(BigDecimal numerator, BigDecimal denominator)
        {
            var (nbaanced, dbalanced) = Balance(numerator, denominator);
            var result = BigInteger.DivRem(balancedLeft, balancedRight);
        }

        #region IComparible
        public int CompareTo(BigDecimal other)
        {
            if (Equals(other))
                return 0;

            var (first, second) = Balance(this, other);
            var compared = first - second;

            if (compared == BigInteger.Zero)
                return 0;

            if (compared == BigInteger.One)
                return 1;

            else //if (compared == BigInteger.NegativeOne)
                return -1;
        }

        public int CompareTo(object obj)
        {
            if (obj is BigDecimal bd)
                return CompareTo(bd);

            throw new ArgumentException($"Invalid comparison: {obj}");
        }
        #endregion

        #region IEquatable
        public bool Equals(BigDecimal other)
        {
            return other._unscaledValue == _unscaledValue
                && other._scale == _scale;
        }

        public override bool Equals(object obj) => obj is BigDecimal bd && Equals(bd);

        public override int GetHashCode() => HashCode.Combine(_unscaledValue, _scale);
        #endregion

        #region IParsable
        public static BigDecimal Parse(string text, IFormatProvider provider)
        {
            var result = Parse(text);
            return result switch
            {
                IResult<BigDecimal>.DataResult data => data.Data,
                IResult<BigDecimal>.ErrorResult error => error.ThrowError(),
                _ => throw new ArgumentException($"Invalid result: {result}")
            };
        }

        public static bool TryParse(
            [NotNullWhen(true)] string text,
            IFormatProvider provider,
            [MaybeNullWhen(false)] out BigDecimal result)
        {
            var parsed = TryParse(text, out var parseResult);
            result = parsed switch
            {
                true => ((IResult<BigDecimal>.DataResult)parseResult).Data,
                false => default
            };

            return parsed;
        }
        #endregion

        #region IParsableResult
        public static bool TryParse(string text, out IResult<BigDecimal> result)
        {
            throw new NotImplementedException();
        }

        public static IResult<BigDecimal> Parse(string text)
        {
            _ = TryParse(text, out var result);
            return result;
        }
        #endregion

        #region Operators

        public static BigDecimal operator +(BigDecimal value) => value;

        public static BigDecimal operator +(BigDecimal left, BigDecimal right)
        {
            var (balancedLeft, balancedRight) = Balance(left, right);
            return new BigDecimal(
                balancedLeft + balancedRight,
                Math.Max(left._scale, right._scale));
        }

        public static BigDecimal operator -(BigDecimal value)
        {
            return new BigDecimal(BigInteger.Negate(value._unscaledValue), value._scale);
        }

        public static BigDecimal operator -(BigDecimal left, BigDecimal right)
        {
            var (balancedLeft, balancedRight) = Balance(left, right);
            return new BigDecimal(
                balancedLeft - balancedRight,
                Math.Max(left._scale, right._scale));
        }

        public static BigDecimal operator ++(BigDecimal value)
        {
            return new BigDecimal(value._unscaledValue + 1, value._scale);
        }

        public static BigDecimal operator --(BigDecimal value)
        {
            return new BigDecimal(value._unscaledValue - 1, value._scale);
        }

        public static BigDecimal operator *(BigDecimal left, BigDecimal right)
        {
            var (balancedLeft, balancedRight) = Balance(left, right);
            return new BigDecimal(
                balancedLeft * balancedRight,
                Math.Max(left._scale, right._scale));
        }

        public static BigDecimal operator /(BigDecimal left, BigDecimal right)
        {
            var (balancedLeft, balancedRight) = Balance(left, right);
            var result = BigInteger.DivRem(balancedLeft, balancedRight);

            if (result.Remainder == 0)
                return new BigDecimal(result.Quotient);

            // Can the value can be represented as a standard decimal value?
            var fraction = balancedRight.GetBitLength() <= 96
                ? new BigDecimal(((decimal)result.Remainder) / ((decimal)balancedRight))
                : BigDivision(result.Remainder, balancedRight);

            return result.Quotient + fraction;
        }

        public static BigDecimal operator %(BigDecimal left, BigDecimal right)
        {
            var (balancedLeft, balancedRight) = Balance(left, right);
            var (_, remainder) = BigInteger.DivRem(balancedLeft, balancedRight);
            return new BigDecimal(remainder, Math.Max(left._scale, right._scale));
        }

        public static BigDecimal IBitwiseOperators<BigDecimal, BigDecimal, BigDecimal>.operator &(BigDecimal left, BigDecimal right)
        {
            throw new NotImplementedException();
        }

        public static BigDecimal IBitwiseOperators<BigDecimal, BigDecimal, BigDecimal>.operator |(BigDecimal left, BigDecimal right)
        {
            throw new NotImplementedException();
        }

        public static BigDecimal IBitwiseOperators<BigDecimal, BigDecimal, BigDecimal>.operator ^(BigDecimal left, BigDecimal right)
        {
            throw new NotImplementedException();
        }

        public static BigDecimal IShiftOperators<BigDecimal, int, BigDecimal>.operator <<(BigDecimal value, int shiftAmount)
        {
            throw new NotImplementedException();
        }

        public static BigDecimal IShiftOperators<BigDecimal, int, BigDecimal>.operator >>(BigDecimal value, int shiftAmount)
        {
            throw new NotImplementedException();
        }

        public static bool IEqualityOperators<BigDecimal, BigDecimal, bool>.operator ==(BigDecimal left, BigDecimal right)
        {
            throw new NotImplementedException();
        }

        public static bool IEqualityOperators<BigDecimal, BigDecimal, bool>.operator !=(BigDecimal left, BigDecimal right)
        {
            throw new NotImplementedException();
        }

        public static bool IComparisonOperators<BigDecimal, BigDecimal, bool>.operator <(BigDecimal left, BigDecimal right)
        {
            throw new NotImplementedException();
        }

        public static bool IComparisonOperators<BigDecimal, BigDecimal, bool>.operator >(BigDecimal left, BigDecimal right)
        {
            throw new NotImplementedException();
        }

        public static bool IComparisonOperators<BigDecimal, BigDecimal, bool>.operator <=(BigDecimal left, BigDecimal right)
        {
            throw new NotImplementedException();
        }

        public static bool IComparisonOperators<BigDecimal, BigDecimal, bool>.operator >=(BigDecimal left, BigDecimal right)
        {
            throw new NotImplementedException();
        }

        public static BigDecimal IShiftOperators<BigDecimal, int, BigDecimal>.operator >>>(BigDecimal value, int shiftAmount)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Conversion Operators
        public static implicit operator BigDecimal(BigInteger @int)
        {

        }
        #endregion

        #region Helpers

        internal static (BigInteger first, BigInteger second) Balance(BigDecimal first, BigDecimal second)
        {
            return first._scale.CompareTo(second._scale) switch
            {
                0 => (first._unscaledValue, second._unscaledValue),
                < 0 => (first._unscaledValue * BigInteger.Pow(10, second._scale - first._scale), second._unscaledValue),
                > 0 => (first._unscaledValue, second._unscaledValue * BigInteger.Pow(10, first._scale - second._scale))
            };
        }
        #endregion
    }
}
