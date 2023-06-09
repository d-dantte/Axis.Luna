using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Axis.Luna.Common.Numerics
{
    public readonly struct BigDecimal :
        IComparable,
        IComparable<BigDecimal>,
        IEquatable<BigDecimal>,
        IParsable<BigDecimal>,
        IResultParsable<BigDecimal>,
        ISpanFormattable,
        ISpanParsable<BigDecimal>,
        ISignedNumber<BigDecimal>,
        IEqualityOperators<BigDecimal, BigDecimal, bool>,
        IUnaryPlusOperators<BigDecimal, BigDecimal>,
        IUnaryNegationOperators<BigDecimal, BigDecimal>,
        IIncrementOperators<BigDecimal>,
        IDecrementOperators<BigDecimal>,
        IAdditiveIdentity<BigDecimal, BigDecimal>,
        IMultiplicativeIdentity<BigDecimal, BigDecimal>,
        ISubtractionOperators<BigDecimal, BigDecimal, BigDecimal>,
        IMultiplyOperators<BigDecimal, BigDecimal, BigDecimal>,
        IDivisionOperators<BigDecimal, BigDecimal, BigDecimal>,
        IModulusOperators<BigDecimal, BigDecimal, BigDecimal>,
        INumber<BigDecimal>,
        INumberBase<BigDecimal>
    {
        #region Fields
        private readonly BigInteger _significand;
        private readonly int _scale;
        #endregion

        #region Constructors
        public BigDecimal((BigInteger value, int scale) components)
        {
            var (value, scale) = Normalize(components.value);
            _significand = value;
            _scale = BigInteger.Zero.Equals(value) ? 0 : components.scale + scale;
        }

        public BigDecimal(BigInteger value)
        : this((value, 0))
        {
        }

        public BigDecimal(ulong value)
        : this((value, 0))
        {
        }

        public BigDecimal(long value)
        : this((value, 0))
        {
        }

        public BigDecimal(double value)
        : this(Deconstruct(value))
        {
        }

        public BigDecimal(decimal value)
        : this(Deconstruct(value))
        {
        }

        public BigDecimal(string notation)
        : this(BigDecimal
            .ParseScientificNotation(notation)
            .BindError(ex => ParseDecimalNotation(notation))
            .Resolve())
        { 
        }
        #endregion

        #region Object
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return obj is BigDecimal other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_significand.GetHashCode(), _scale);
        }

        public override string ToString() => ToScientificString();
        #endregion

        #region Values
        public static readonly BigDecimal _zero = default;
        public static readonly BigDecimal _one = new BigDecimal(1);
        #endregion

        #region ICompareable
        public int CompareTo(object obj)
        {
            var other = obj switch
            {
                byte v => new BigDecimal(v),
                sbyte v => new BigDecimal(v),
                short v => new BigDecimal(v),
                ushort v => new BigDecimal(v),
                int v => new BigDecimal(v),
                uint v => new BigDecimal(v),
                long v => new BigDecimal(v),
                ulong v => new BigDecimal(v),
                Half v => new BigDecimal((double)v),
                float v => new BigDecimal(v),
                double v => new BigDecimal(v),
                decimal v => new BigDecimal(v),
                BigInteger v => new BigDecimal(v),
                BigDecimal v => v,
                null => throw new ArgumentNullException(nameof(obj)),
                _ => throw new ArgumentException($"Cannot compare the given type: {obj.GetType()}")
            };

            return this.CompareTo(other);
        }

        public int CompareTo(BigDecimal other)
        {
            var (first, second, _) = Balance(this, other);
            return first.CompareTo(second);
        }
        #endregion

        #region IEquatable

        public bool Equals(BigDecimal other)
        {
            return CompareTo(other) == 0;
        }
        #endregion

        #region IAdditiveIdentity<,>
        public static BigDecimal AdditiveIdentity => Zero;
        #endregion

        #region IMultiplicativeIdentity<,>
        public static BigDecimal MultiplicativeIdentity => One;
        #endregion

        #region NumberBase

        public static BigDecimal One => _one;

        public static BigDecimal Zero => _zero;

        public static int Radix => 10;

        public static bool IsCanonical(BigDecimal value) => true;

        public static bool IsComplexNumber(BigDecimal value) => false;

        public static bool IsEvenInteger(BigDecimal value)
            => IsInteger(value) 
            && (value._scale > 0
            || (value._significand & 1) == 0);

        public static bool IsOddInteger(BigDecimal value)
            => value._scale == 0 && (value._significand & 1) == 1;

        public static bool IsFinite(BigDecimal value) => true;

        public static bool IsImaginaryNumber(BigDecimal value) => false;

        public static bool IsInfinity(BigDecimal value) => false;

        public static bool IsInteger(BigDecimal value) => value._scale >= 0;

        public static bool IsNaN(BigDecimal value) => false;

        public static bool IsNegative(BigDecimal value) => BigInteger.IsNegative(value._significand);

        public static bool IsNegativeInfinity(BigDecimal value) => false;

        public static bool IsNormal(BigDecimal value) => value != _zero;

        public static bool IsPositive(BigDecimal value) => BigInteger.IsPositive(value._significand);

        public static bool IsPositiveInfinity(BigDecimal value) => false;

        public static bool IsRealNumber(BigDecimal value) => true;

        public static bool IsSubnormal(BigDecimal value) => false;

        public static bool IsZero(BigDecimal value) => value == _zero;

        public static BigDecimal MaxMagnitude(BigDecimal x, BigDecimal y)
        {
            var ax = Abs(x);
            var ay = Abs(y);

            if (ax > ay)
                return x;

            if (ax == ay)
                return IsNegative(x) ? y : x;

            return y;
        }

        public static BigDecimal MaxMagnitudeNumber(BigDecimal x, BigDecimal y) => MaxMagnitude(x, y);

        public static BigDecimal MinMagnitude(BigDecimal x, BigDecimal y)
        {
            var ax = Abs(x);
            var ay = Abs(y);

            if (ax < ay)
                return x;

            if (ax == ay)
                return IsNegative(x) ? x : y;

            return y;
        }

        public static BigDecimal MinMagnitudeNumber(BigDecimal x, BigDecimal y) => MinMagnitude(x, y);

        public static BigDecimal Parse(
            string s,
            NumberStyles style,
            IFormatProvider provider) => Parse(s, provider);


        public static BigDecimal Parse(
            ReadOnlySpan<char> s,
            NumberStyles style,
            IFormatProvider provider)
            => Parse(s, provider);

        static bool INumberBase<BigDecimal>.TryConvertFromChecked<TOther>(
            TOther value,
            [MaybeNullWhen(false)]
            out BigDecimal result)
            => TryConvertFrom(value, out result);

        static bool INumberBase<BigDecimal>.TryConvertFromSaturating<TOther>(
            TOther value,
            [MaybeNullWhen(false)] out BigDecimal result)
            => TryConvertFrom(value, out result);


        static bool INumberBase<BigDecimal>.TryConvertFromTruncating<TOther>(
            TOther value,
            [MaybeNullWhen(false)] out BigDecimal result)
            => TryConvertFrom(value, out result);


        static bool INumberBase<BigDecimal>.TryConvertToChecked<TOther>(
            BigDecimal value,
            [MaybeNullWhen(false)] out TOther result)
            => TryConvertTo(value, out result);


        static bool INumberBase<BigDecimal>.TryConvertToSaturating<TOther>(
            BigDecimal value,
            [MaybeNullWhen(false)] out TOther result)
            => TryConvertTo(value, out result);


        static bool INumberBase<BigDecimal>.TryConvertToTruncating<TOther>(
            BigDecimal value,
            [MaybeNullWhen(false)] out TOther result)
            => TryConvertTo(value, out result);

        private static bool TryConvertFrom<TOther>(TOther value, out BigDecimal result)
        {
            var bigDecimal = value switch
            {
                byte v => new BigDecimal(v),
                sbyte v => new BigDecimal(v),
                char v => new BigDecimal(v),
                short v => new BigDecimal(v),
                ushort v => new BigDecimal(v),
                int v => new BigDecimal(v),
                uint v => new BigDecimal(v),
                long v => new BigDecimal(v),
                ulong v => new BigDecimal(v),
                Half v => new BigDecimal((double)v),
                float v => new BigDecimal(v),
                double v => new BigDecimal(v),
                decimal v => new BigDecimal(v),
                _ => default(BigDecimal?)
            };

            if (bigDecimal is null)
            {
                result = default;
                return false;
            }

            result = bigDecimal.Value;
            return true;
        }

        private static bool TryConvertTo<TOther>(BigDecimal value, out TOther result)
        {
            var otherType = typeof(TOther);
            var _result =
                typeof(byte).Equals(otherType) ? checked((byte)value) :
                typeof(sbyte).Equals(otherType) ? checked((sbyte)value) :
                typeof(char).Equals(otherType) ? checked((char)value) :
                typeof(short).Equals(otherType) ? checked((short)value) :
                typeof(ushort).Equals(otherType) ? checked((ushort)value) :
                typeof(int).Equals(otherType) ? checked((int)value) :
                typeof(uint).Equals(otherType) ? checked((uint)value) :
                typeof(long).Equals(otherType) ? checked((long)value) :
                typeof(ulong).Equals(otherType) ? checked((ulong)value) :
                typeof(Half).Equals(otherType) ? checked((Half)value) :
                typeof(float).Equals(otherType) ? checked((float)value) :
                typeof(double).Equals(otherType) ? checked((double)value) :
                typeof(decimal).Equals(otherType) ? checked((decimal)value) :
                default(object);

            if (_result is not null)
            {
                result = (TOther)_result;
                return true;
            }

            result = default;
            return true;
        }

        public static bool TryParse(
            [NotNullWhen(true)] string s,
            NumberStyles style,
            IFormatProvider provider,
            [MaybeNullWhen(false)] out BigDecimal result)
            => TryParse(s, provider, out result);

        public static bool TryParse(
            ReadOnlySpan<char> s,
            NumberStyles style,
            IFormatProvider provider,
            [MaybeNullWhen(false)] out BigDecimal result)
            => TryParse(s, provider, out result);

        #endregion

        #region Arithmetic Operations
        internal static BigDecimal Add(BigDecimal left, BigDecimal right)
        {
            if (left == _zero)
                return right;

            if (right == _zero)
                return left;

            var (l, r, s) = Balance(left, right);

            return new BigDecimal((l + r, s));
        }

        internal static BigDecimal Subtract(BigDecimal left, BigDecimal right)
        {
            if (left == _zero)
                return right;

            if (right == _zero)
                return left;

            var (l, r, s) = Balance(left, right);

            return new BigDecimal((l - r, s));
        }

        internal static BigDecimal Multiply(BigDecimal left, BigDecimal right)
        {
            if (left == _zero || right == _zero)
                return _zero;

            if (left == 1)
                return right;

            if (right == 1)
                return left;

            return (
                left._significand * right._significand,
                (left._scale + right._scale));
        }

        internal static BigDecimal Divide(BigDecimal left, BigDecimal right, int precision = 64)
        {
            if (precision < 0)
                throw new ArgumentOutOfRangeException(nameof(precision), "Precision cannot be < 0");

            if (left == _zero)
                return _zero;
                
            if (right == _zero)
                throw new DivideByZeroException();

            if (right == _one)
                return left;

            var (first, second, _) = Balance(left, right);
            var (Quotient, Remainder) = BigInteger.DivRem(first, second);
            var fraction = DecimalShift(Remainder, precision) / second;
            return (
                DecimalShift(Quotient, precision) + fraction,
                -precision);
        }

        internal static BigDecimal Modulus(BigDecimal left, BigDecimal right)
        {
            if (left == _zero)
                return _zero;

            if (right == _zero)
                throw new DivideByZeroException();

            if (right == _one)
                return left;

            var (bfirst, bsecond, scale) = Balance(left, right);

            return (
                bfirst % bsecond,
                scale);
        }

        public static BigDecimal Power(BigDecimal value, int exponent)
        {
            return (
                BigInteger.Pow(value._significand, exponent),
                value._scale * exponent);
        }

        #endregion

        #region Implicits
        public static implicit operator BigDecimal(BigInteger value) => new BigDecimal(value);
        public static implicit operator BigDecimal(ulong value) => new BigDecimal(value);
        public static implicit operator BigDecimal(long value) => new BigDecimal(value);
        public static implicit operator BigDecimal(uint value) => new BigDecimal(value);
        public static implicit operator BigDecimal(int value) => new BigDecimal(value);
        public static implicit operator BigDecimal(ushort value) => new BigDecimal(value);
        public static implicit operator BigDecimal(short value) => new BigDecimal(value);
        public static implicit operator BigDecimal(sbyte value) => new BigDecimal(value);
        public static implicit operator BigDecimal(byte value) => new BigDecimal(value);
        public static implicit operator BigDecimal(Half value) => new BigDecimal((double)value);
        public static implicit operator BigDecimal(float value) => new BigDecimal(value);
        public static implicit operator BigDecimal(double value) => new BigDecimal(value);
        public static implicit operator BigDecimal(decimal value) => new BigDecimal(value);
        public static implicit operator BigDecimal(ValueTuple<BigInteger, int> value) => new BigDecimal(value);
        #endregion

        #region Explicits
        public static explicit operator byte(BigDecimal value) => (byte)value.Truncate();
        public static explicit operator sbyte(BigDecimal value) => (sbyte)value.Truncate();
        public static explicit operator char(BigDecimal value) => (char)value.Truncate();
        public static explicit operator short(BigDecimal value) => (short)value.Truncate();
        public static explicit operator ushort(BigDecimal value) => (ushort)value.Truncate();
        public static explicit operator int(BigDecimal value) => (int)value.Truncate();
        public static explicit operator uint(BigDecimal value) => (uint)value.Truncate();
        public static explicit operator long(BigDecimal value) => (long)value.Truncate();
        public static explicit operator ulong(BigDecimal value) => (ulong)value.Truncate();
        public static explicit operator Half(BigDecimal value) => (Half)value.ToDouble();
        public static explicit operator float(BigDecimal value) => (float)value.ToDouble();
        public static explicit operator double(BigDecimal value) => value.ToDouble();
        public static explicit operator decimal(BigDecimal value) => value.ToDecimal();
        public static explicit operator BigInteger(BigDecimal value) => (char)value.Truncate();
        #endregion

        #region IResultParsable
        public static bool TryParse(
            string text,
            out IResult<BigDecimal> result)
            => (result = Parse(text)) is IResult<BigDecimal>.DataResult;

        public static IResult<BigDecimal> Parse(string text)
        {
            if (ParseScientificNotation(text) is IResult<(BigInteger, int)>.DataResult data)
                return data.Map(components => new BigDecimal(components));

            return ParseDecimalNotation(text).Map(components => new BigDecimal(components));
        }
        #endregion

        #region IParsable
        public static BigDecimal Parse(string s, IFormatProvider provider) => Parse(s).Resolve();

        public static bool TryParse(
            [NotNullWhen(true)] string s,
            IFormatProvider provider,
            [MaybeNullWhen(false)] out BigDecimal result)
        {
            bool parsed;
            result = (parsed = TryParse(s, out var r))
                ? r.Resolve()
                : default;

            return parsed;
        }
        #endregion

        #region ISpanParsable
        public static BigDecimal Parse(ReadOnlySpan<char> s, IFormatProvider provider) => Parse(s.ToString()).Resolve();

        public static bool TryParse(
            ReadOnlySpan<char> s,
            IFormatProvider provider,
            [MaybeNullWhen(false)] out BigDecimal result)
            => TryParse(s.ToString(), provider, out result);
        #endregion

        #region ISpanFormattable
        public bool TryFormat(
            Span<char> destination,
            out int charsWritten,
            ReadOnlySpan<char> format,
            IFormatProvider provider)
        {
            destination = new Span<char>(ToString().ToArray());
            charsWritten = destination.Length;
            return true;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToString();
        }
        #endregion

        #region ISignedNumber<>
        public static BigDecimal NegativeOne => -One;
        #endregion

        #region Operators

        #region IEqualityOperators
        public static bool operator ==(BigDecimal left, BigDecimal right) => left.Equals(right);

        public static bool operator !=(BigDecimal left, BigDecimal right) => !left.Equals(right);
        #endregion

        #region IComparisonOperators<,>

        public static bool operator >(BigDecimal left, BigDecimal right)
        {
            var (l, r, _) = Balance(left, right);
            return l > r;
        }

        public static bool operator >=(BigDecimal left, BigDecimal right)
        {
            var (l, r, _) = Balance(left, right);
            return l >= r;
        }

        public static bool operator <(BigDecimal left, BigDecimal right)
        {
            var (l, r, _) = Balance(left, right);
            return l < r;
        }

        public static bool operator <=(BigDecimal left, BigDecimal right)
        {
            var (l, r, _) = Balance(left, right);
            return l <= r;
        }

        #endregion

        #region IUnaryPlusOperators<,>

        public static BigDecimal operator +(BigDecimal value) => value;

        #endregion

        #region IUnaryNegationOperator<,>

        public static BigDecimal operator -(BigDecimal value) => new((BigInteger.Negate(value._significand), value._scale));

        #endregion

        #region IAdditionOperators<,>

        public static BigDecimal operator +(BigDecimal left, BigDecimal right) => Add(left, right);

        #endregion

        #region ISubtractionOperators<,>

        public static BigDecimal operator -(BigDecimal left, BigDecimal right) => Subtract(left, right);

        #endregion

        #region IMultiplicationOperators<,>

        public static BigDecimal operator *(BigDecimal left, BigDecimal right) => Multiply(left, right);

        #endregion

        #region IDivisionOperators<,>

        public static BigDecimal operator /(BigDecimal left, BigDecimal right) => Divide(left, right);

        #endregion

        #region IModulusOperator<,,>
        public static BigDecimal operator %(BigDecimal left, BigDecimal right) => Modulus(left, right);
        #endregion

        #region IIncrementOperators<>
        public static BigDecimal operator ++(BigDecimal value) => value + 1;
        #endregion

        #region IDecrementOperators<>
        public static BigDecimal operator --(BigDecimal value) => value - 1;
        #endregion

        #endregion

        #region Misc
        public static BigDecimal Abs(BigDecimal value)
        {
            return (
                BigInteger.Abs(value._significand),
                value._scale);
        }

        public static BigDecimal WithContext(FormatContext context, Func<BigDecimal> func)
        {
            FormatContext.AsyncLocal = context;
            try
            {
                return func();
            }
            finally
            {
                FormatContext.AsyncLocal = default;
            }
        }

        public BigDecimal Floor() => new BigDecimal((Truncate(), 0));

        public BigDecimal Ceiling()
        {
            if (_scale >= 0)
                return this;

            var digits = _significand.ToString();
            if (-_scale > digits.Length)
                return Zero;

            byte roundingDigit = (byte)(digits[^-_scale] - 48);
            return Truncate() + roundingDigit switch
            {
                < 5 => 0,
                _ => 1
            };
        }

        public BigDecimal Fraction() =>  this - Truncate();

        public BigDecimal Round(int decimals = 0)
        {
            if (decimals < 0)
                throw new ArgumentOutOfRangeException($"{nameof(decimals)} is < 0. '{decimals}'");

            if (_scale >= 0)
                return this;

            if (decimals >= -_scale)
                return this;

            var digits = _significand
                .ToString()
                .PadLeft(-_scale, '0');

            var anchor = _scale + decimals;
            return (
                DecimalShift(_significand, anchor) + digits[^(-anchor)] switch
                {
                    >= '5' => 1,
                    _ => 0
                },
                -decimals);
        }
        #endregion

        #region Helpers

        private static readonly RegexOptions RegexOptions =
            RegexOptions.Compiled
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnoreCase;

        internal static readonly string SignPattern = "(?'sign'[\\+\\-])";
        internal static readonly string IntegralPattern = "(?'integral'\\d+)";
        internal static readonly string FractionPattern = "(?'fraction'\\.\\d+)";
        internal static readonly string ExponentPattern = "(?'exponent'E[\\+\\-]?\\d+)";

        internal static readonly Regex DecimalRegex = new Regex(
            $"^{SignPattern}?{IntegralPattern}{FractionPattern}?\\z",
            RegexOptions);

        internal static readonly Regex IntegerRegex = new Regex(
            "^(?'sign'\\-)?(?'integral'\\d+)\\z",
            RegexOptions);

        internal static readonly Regex ScientificRegex = new Regex(
            $"^{SignPattern}?{IntegralPattern}{FractionPattern}?{ExponentPattern}\\z",
            RegexOptions);

        /// <summary>
        /// Truncates and converts any trailing zeros to scale
        /// </summary>
        /// <param name="value">The value to normalize</param>
        /// <returns>The noralizing components</returns>
        internal static (BigInteger value, int scale) Normalize(BigInteger value)
        {
            if (value == BigInteger.Zero)
                return (value, 0);

            return (
                BigInteger.Parse(value.ToString().TrimEnd('0')),
                value.TrailingDecimalZeroCount());
        }

        /// <summary>
        /// </summary>
        internal static (BigInteger first, BigInteger second, int scale) Balance(BigDecimal first, BigDecimal second)
        {
            var dfirst = Denormalize(first);
            var dsecond = Denormalize(second);
            return dfirst.shifts.CompareTo(dsecond.shifts) switch
            {
                0 => (dfirst.value, dsecond.value, first._scale),

                > 0 => (
                    dfirst.value,
                    DecimalShift(dsecond.value, dfirst.shifts - dsecond.shifts),
                    first._scale),

                < 0 => (
                    DecimalShift(dfirst.value, dsecond.shifts - dfirst.shifts),
                    dsecond.value,
                    second._scale)
            };
        }


        /// <summary>
        /// </summary>
        internal static (BigInteger value, int scale) Deconstruct(double value)
        {
            return BigDecimal
                .ParseScientificNotation(value.ToString("E17"))
                .Resolve();
        }

        /// <summary>
        /// 
        /// </summary>
        internal static (BigInteger value, int scale) Deconstruct(decimal value)
        {
            return BigDecimal
                .ParseScientificNotation(value.ToString("E39"))
                .Resolve();
        }

        /// <summary>
        /// 
        /// </summary>
        internal static IResult<(BigInteger value, int scale)> ParseScientificNotation(string scientificNotation)
        {
            var match = ScientificRegex.Match(scientificNotation);

            if (!match.Success)
                return Result.Of<(BigInteger value, int scale)>(
                    new ArgumentException($"Invalid number format: {scientificNotation}"));

            try
            {
                var sign = match.Groups["sign"].Value switch
                {
                    "+" or "" => true,
                    "-" => false,
                    _ => throw new FormatException($"Invalid number format: '{scientificNotation}'")
                };

                var significantIntegral = match.Groups["integral"].Value
                    .TrimStart('0')
                    .PadLeft(1, '0'); // sets a zero in case the integral part only contained zeros.

                var significantFractional = match.Groups["fraction"].Success switch
                {
                    false => "",
                    true => match.Groups
                        ["fraction"].Value
                        [1..]
                        .TrimEnd('0')
                };

                var exponent = match.Groups["exponent"].Value[1..];
                var significantDigits = $"{significantIntegral}{significantFractional}";

                var intValue = BigInteger.Parse($"{(sign ? "" : "-")}{significantDigits}");
                var unadjustedScale = int.Parse(exponent);
                var pointPosition = significantIntegral.Length;

                return Result.Of((
                    intValue,
                    BigInteger.Zero.Equals(intValue) ? 0 : unadjustedScale - (significantDigits.Length - pointPosition)));
            }
            catch(Exception e)
            {
                return Result.Of<(BigInteger value, int scale)>(e);
            }
        }

        internal static IResult<(BigInteger value, int scale)> ParseDecimalNotation(string decimalNotation)
        {
            return ParseScientificNotation($"{decimalNotation}E0");
        }

        /// <summary>
        /// Returns the significant digits, and an integer representing the decimal point shifts made to transform the fraction
        /// to a whole number
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static (BigInteger value, int shifts) Denormalize(BigDecimal value)
        {
            return value._scale switch
            {
                0 => (value._significand, 0),
                > 0 => (PowerShift(value._significand, value._scale), 0),
                < 0 => (value._significand, -value._scale),
            };
        }

        /// <summary>
        /// Has faster benchmark for <paramref name="shiftCount"/> &lt;= -30.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="shiftCount"></param>
        /// <returns></returns>
        internal static BigInteger StringShift(BigInteger value, int shiftCount)
        {
            return shiftCount switch
            {
                0 => value,
                > 0 => $"{value}{new string('0', shiftCount)}".ApplyTo(BigInteger.Parse),
                < 0 => value
                    .ToString()
                    .ApplyTo(stringValue =>
                    {
                        var absShit = Math.Abs(shiftCount);
                        var signless = stringValue[0] == '-' ? stringValue[1..] : stringValue;
                        return absShit.CompareTo(signless.Length) switch
                        {
                            < 0 => stringValue[..(stringValue.Length - absShit)].ApplyTo(BigInteger.Parse),
                            _ => BigInteger.Zero
                        };
                    })
            };
        }

        /// <summary>
        /// Has faster benchmark for <paramref name="shiftCount"/> &gt; -30.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="shiftCount"></param>
        /// <returns></returns>
        internal static BigInteger PowerShift(BigInteger value, int shiftCount)
        {
            return shiftCount switch
            {
                0 => value,
                > 0 => value * BigInteger.Pow(10, shiftCount),
                < 0 => value / BigInteger.Pow(10, Math.Abs(shiftCount))
            };
        }

        internal static BigInteger DecimalShift(BigInteger value, int shiftCount)
        {
            return shiftCount switch
            {
                > 0 => PowerShift(value, shiftCount),
                0 => value,
                > -30 => PowerShift(value, shiftCount),
                <= -30 => StringShift(value, shiftCount)
            };
        }

        public BigInteger Truncate()
        {
            if (_scale == 0)
                return _significand;

            return DecimalShift(_significand, _scale);
        }

        internal decimal DemoteToDecimal()
        {
            var sign = _significand.Sign switch
            {
                < 0 => false,
                >= 0 => true
            };

            var (value, shifts) = Denormalize(this);
            var ints = BigInteger
                .Abs(value)
                .ToByteArray()
                .TakeExactly(12)
                .Select((@byte, index) => (@byte, index))
                .GroupBy(tuple => tuple.index / 4, tuple => tuple.@byte)
                .Select(bytes => BitConverter.ToInt32(bytes.ToArray()))
                .ToArray();

            var scale = BitConverter.ToInt32(new byte[] { 0, 0, (byte)shifts, (byte)(sign ? 0 : 1) });

            return new decimal(ints.Append(scale).ToArray());
        }

        internal double DemoteToDouble() => double.Parse(ToScientificString(14));

        public double ToDouble()
        {
            if (this > double.MaxValue)
                return double.MaxValue;

            return DemoteToDouble();
        }

        public decimal ToDecimal()
        {
            if (this > decimal.MaxValue)
                return decimal.MaxValue;

            return DemoteToDecimal();
        }

        /// <summary>
        /// precision of <c>-1</c> means use only the exact amount of significant digits available
        /// </summary>
        /// <param name="precision"></param>
        /// <returns></returns>
        public string ToScientificString(int precision = -1)
        {
            if (precision < -1)
                throw new ArgumentException($"Precision cannot be < -1");

            var intComponents = IntegerRegex.Match(_significand.ToString());
            var intString = intComponents.Groups["integral"].Value;
            var sign = intComponents.Groups["sign"].Value;

            var exponent = _scale + (intString.Length - 1);
            return intString.Length switch
            {
                1 => $"{sign}{intString}.0E{exponent}",
                > 1 => $"{sign}{intString[0]}.{AtMost(intString[1..], precision)}E{exponent}",
                _ => throw new FormatException($"Invalid number format: '{_significand}'")
            };
        }

        public string ToNonScientificString(int precision = -1)
        {
            if (precision < -1)
                throw new ArgumentOutOfRangeException(nameof(precision), "Precision cannot be < -1");

            if (Zero.Equals(this))
                return "0";

            var sign = IsNegative(this) ? "-" : "";

            var digits = _significand
                .ToString()
                .ApplyTo(text => "".Equals(sign) ? text: text[1..]);

            var text =
                (int.IsNegative(_scale) && -_scale >= digits.Length) ? $"{sign}0.{new string('0', -_scale - digits.Length)}{digits}" :
                int.IsNegative(_scale) ? $"{sign}{digits.Split(digits.Length + _scale).JoinUsing(".")}":
                $"{sign}{digits}{new string('0', _scale)}";

            var currentPrecision = text.Length -  (text.IndexOf('.') + 1);
            return
                precision == -1 ? text :
                precision > currentPrecision ? text :
                text[..(currentPrecision - precision)];
        }

        private static string AtMost(string s, int length)
        {
            if (length == -1 || length > s.Length)
                return s;

            if (length == 0)
                return "";

            return s[0..length];
        }
        #endregion

        #region Nested types
        /// <summary>
        /// Specifies optional data used in representing <see cref="BigDecimal"/> values during various operations
        /// </summary>
        public readonly struct FormatContext
        {
            #region AsyncLocal
            private static readonly AsyncLocal<FormatContext> AsyncContext = new();

            /// <summary>
            /// Gets of sets the AsyncLocal instance.
            /// </summary>
            public static FormatContext AsyncLocal
            {
                get => AsyncContext.Value;
                set => AsyncContext.Value = value;
            }
            #endregion

            private readonly int? _maxSignificantFractionalDigits;

            /// <summary>
            /// Maximum number of digits after the '.'. Default is 50.
            /// </summary>
            public int MaxSignificantFractionalDigits => _maxSignificantFractionalDigits ?? 50;

            public FormatContext(int maxSignificantFractionalDigits)
            {
                _maxSignificantFractionalDigits = maxSignificantFractionalDigits.ThrowIf(
                    i => i < 1,
                    new ArgumentException($"'{nameof(maxSignificantFractionalDigits)}' must be > 0"));
            }
        }
        #endregion
    }
}
