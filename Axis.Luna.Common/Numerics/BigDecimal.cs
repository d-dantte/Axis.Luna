using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;

namespace Axis.Luna.Common.Numerics
{
    /// <summary>
    /// Big Decimal - unlimited mantissa, with <c>int.MaxValue</c> max scale
    /// <para>
    /// Note:
    /// <list type="number">
    /// <item>For all <c>Parse(...)</c> and <c>TryParse(...)</c> methods that take <see cref="IFormatProvider"/> argument, the provider is ignored (for now).</item>
    /// </list>
    /// </para>
    /// </summary>
    public readonly struct BigDecimal :
        IComparable,
        IComparable<BigDecimal>,
        IEquatable<BigDecimal>,
        IParsable<BigDecimal>,
        IResultParsable<BigDecimal>,
        IUnaryPlusOperators<BigDecimal, BigDecimal>,
        IUnaryNegationOperators<BigDecimal, BigDecimal>,
        IAdditionOperators<BigDecimal, BigDecimal, BigDecimal>,
        IAdditiveIdentity<BigDecimal, BigDecimal>,
        ISubtractionOperators<BigDecimal, BigDecimal, BigDecimal>,
        IMultiplyOperators<BigDecimal, BigDecimal, BigDecimal>,
        IMultiplicativeIdentity<BigDecimal, BigDecimal>,
        IDivisionOperators<BigDecimal, BigDecimal, BigDecimal>,
        IIncrementOperators<BigDecimal>,
        IDecrementOperators<BigDecimal>,
        ISpanFormattable,
        ISpanParsable<BigDecimal>,
        IModulusOperators<BigDecimal, BigDecimal, BigDecimal>,
        IEqualityOperators<BigDecimal, BigDecimal, bool>,
        IComparisonOperators<BigDecimal, BigDecimal, bool>,
        INumber<BigDecimal>,
        INumberBase<BigDecimal>
    {
        private static readonly RegexOptions PatternOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
        internal static readonly Regex DecimalPattern = new Regex("^(?'integral'[\\+\\-]?\\d+)(\\.(?'fraction'\\d+))?$", PatternOptions);
        internal static readonly Regex ScientificPattern = new Regex("^(?'integral'[\\+\\-]?\\d+)\\.(?'fraction'\\d+)E(?'exponent'\\-?\\d+)$", PatternOptions);
        internal static readonly Regex DeconstructedPattern = new Regex("^\\[Mantissa\\:\\s*(?'mantissa'[\\+\\-]?\\d+),\\s*Scale\\:\\s*(?'scale'\\d+)\\]$", PatternOptions);

        private readonly BigInteger _value;
        private readonly int _scale;


        public BigDecimal(int value)
        {
            _value = value;
            _scale = 0;
        }

        public BigDecimal(byte value)
        {
            _value = value;
            _scale = 0;
        }

        public BigDecimal(sbyte value)
        {
            _value = value;
            _scale = 0;
        }

        public BigDecimal(uint value)
        {
            _value = value;
            _scale = 0;
        }

        public BigDecimal(long value)
        {
            _value = value;
            _scale = 0;
        }

        public BigDecimal(ulong value)
        {
            _value = value;
            _scale = 0;
        }

        public BigDecimal(Half value)
        {
            var (Mantissa, Scale) = value.Deconstruct();
            _value = Mantissa;
            _scale = Scale;
        }

        public BigDecimal(float value)
        {
            var (Mantissa, Scale) = value.Deconstruct();
            _value = Mantissa;
            _scale = Scale;
        }

        public BigDecimal(double value)
        {
            var (Mantissa, Scale) = value.Deconstruct();
            _value = Mantissa;
            _scale = Scale;
        }

        public BigDecimal(decimal value)
        {
            var (Mantissa, Scale) = value.Deconstruct();
            _value = Mantissa;
            _scale = Scale;
        }

        public BigDecimal(BigInteger value)
        : this(value, 0)
        {
        }

        public BigDecimal(BigInteger value, int scale)
        {
            var (Mantissa, Scale) = (value, scale).NormalizeBigDecimal();
            _value = Mantissa;
            _scale = Scale;
        }


        public BigDecimal Floor() => new BigDecimal(Truncate(), 0);

        public BigDecimal Ceiling()
        {
            if (_scale == 0)
                return this;

            var digits = _value.ToString();
            byte roundingDigit = (byte)(digits[^_scale] - 48);
            return Truncate() + roundingDigit switch
            {
                < 5 => 0,
                _ => 1
            };
        }

        public BigDecimal Fraction()
        {
            var truncated = Truncate();
            return this - truncated;
        }

        public BigDecimal Round(int decimals = 0)
        {
            if (decimals < 0)
                throw new ArgumentOutOfRangeException($"{nameof(decimals)} is < 0. '{decimals}'");

            if (_scale == 0)
                return this;

            var newScale = decimals < _scale ? decimals : _scale;
            var anchor = DigitAtDecimalPlace(decimals + 1);
            var truncateCount = _scale > decimals ? _scale - decimals : 0;
            var rounded = _value / BigInteger.Pow(10, truncateCount);
            rounded += anchor >= 5 ? 1 : 0;

            return new BigDecimal(rounded, newScale);
        }


        #region Object
        public override string ToString()
        {
            return $"[Mantissa: {_value}, Scale: {_scale}]";
        }

        public override int GetHashCode() => HashCode.Combine(_value, _scale);

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return CompareTo(obj) == 0;
        }

        public static bool operator ==(BigDecimal a, BigDecimal b) => a.Equals(b);

        public static bool operator !=(BigDecimal a, BigDecimal b) => !(a == b);

        #endregion

        #region IUnaryPlusOperators<,>

        public static BigDecimal operator +(BigDecimal value) => value;

        #endregion

        #region IUnaryNegationOperator<,>

        public static BigDecimal operator -(BigDecimal value) => new(BigInteger.Negate(value._value), value._scale);

        #endregion

        #region IAdditionOperators<,>

        public static BigDecimal operator +(BigDecimal left, BigDecimal right)
        {
            if (left == 0)
                return right;

            if (right == 0)
                return left;

            var (l, r) = Balance(left, right);
            return new BigDecimal(l + r, Math.Max(left._scale, right._scale));
        }

        #endregion

        #region ISubtractionOperators<,>

        public static BigDecimal operator -(BigDecimal left, BigDecimal right)
        {
            if (left == 0)
                return right;

            if (right == 0)
                return left;

            var (l, r) = Balance(left, right);
            return new BigDecimal(l - r, Math.Max(left._scale, right._scale));
        }

        #endregion

        #region IMultiplicationOperators<,>

        public static BigDecimal operator *(BigDecimal left, BigDecimal right)
        {
            if (left == 1)
                return right;

            if (right == 1)
                return left;

            var (l, r) = Balance(left, right);
            return new BigDecimal(l - r, left._scale + right._scale);
        }

        #endregion

        #region IDivisionOperators<,>

        public static BigDecimal operator /(BigDecimal left, BigDecimal right)
        {
            if (left == 1)
                return right;

            if (right == 1)
                return left;

            if (left < decimal.MaxValue && right < decimal.MaxValue)
            {
                var dleft = left.DemoteToDecimal();
                var dright = right.DemoteToDecimal();
                return new BigDecimal(dleft / dright);
            }

            var (l, r) = Balance(left, right);
            var (quotient, remainder) = BigInteger.DivRem(l, r);

            if (remainder == 0)
                return new BigDecimal(quotient);

            var raised = Raise(remainder, r);

            var maxFractionalDigits = FormatContext.AsyncLocal.MaxSignificantFractionalDigits;

            #region Special optimization cases should come here
            if (raised.raisedValue == r)
            {
                return new BigDecimal((quotient * 10) + 1, 1);
            }
            #endregion

            var fnumerator = raised.raisedValue * BigInteger.Pow(10, maxFractionalDigits - 1);
            var fquotientString = (fnumerator / r).ToString().TrimEnd('0');
            var fquotient = BigInteger.Parse(fquotientString);

            quotient *= BigInteger.Pow(10, fquotientString.Length);

            return new BigDecimal(quotient + fquotient, fquotientString.Length);
        }

        #endregion

        #region IComparisonOperators<,>

        public static bool operator >(BigDecimal left, BigDecimal right)
        {
            var (l, r) = Balance(left, right);
            return l > r;
        }

        public static bool operator >=(BigDecimal left, BigDecimal right)
        {
            var (l, r) = Balance(left, right);
            return l >= r;
        }

        public static bool operator <(BigDecimal left, BigDecimal right)
        {
            var (l, r) = Balance(left, right);
            return l < r;
        }

        public static bool operator <=(BigDecimal left, BigDecimal right)
        {
            var (l, r) = Balance(left, right);
            return l <= r;
        }

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
            var (first, second) = Balance(this, other);
            return first.CompareTo(second);
        }
        #endregion

        #region IEquatable

        public bool Equals(BigDecimal other)
        {
            return CompareTo(other) == 0;
        }
        #endregion

        #region IParsable
        public static BigDecimal Parse(string s, IFormatProvider provider)
        {
            return Parse(s).Resolve();
        }

        public static bool TryParse(
            [NotNullWhen(true)] string s,
            IFormatProvider provider,
            [MaybeNullWhen(false)] out BigDecimal result)
        {
            result = default;
            try
            {
                result = Parse(s, provider);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region IParsableResult
        public static bool TryParse(string text, out IResult<BigDecimal> result)
        {
            Match match = DecimalPattern.Match(text);
            if (match.Success)
            {
                result = Results.Result.Of(ParseFromDecimal(match));
                return true;
            }

            match = ScientificPattern.Match(text);
            if (match.Success)
            {
                result = Results.Result.Of(ParseFromScientific(match));
                return true;
            }

            match = DeconstructedPattern.Match(text);
            if (match.Success)
            {
                result = Results.Result.Of(ParseFromDeconstructed(match));
                return true;
            }

            // Note: Result.Of(..) captures the stack trace.
            result = Results.Result.Of<BigDecimal>(new FormatException($"Invalid {nameof(BigDecimal)} format: '{text}'"));
            return false;
        }

        public static IResult<BigDecimal> Parse(string text)
        {
            _ = TryParse(text, out var result);
            return result;
        }

        private static BigDecimal ParseFromDecimal(Match match)
        {
            var integer = match.Groups["integral"].Value;
            var fraction = match.Groups["fraction"].Success
                ? match.Groups["fraction"].Value.TrimEnd('0') // <-- cannonicalizes the fraction.
                : "";

            var notation = $"{integer}.{fraction}";
            var (mantissa, scale) = notation.DeconstructFromNotation();

            return new BigDecimal(mantissa, scale);
        }

        private static BigDecimal ParseFromScientific(Match match)
        {
            var integer = match.Groups["integral"].Value;
            var fraction = match.Groups["fraction"].Value;
            var exponent = match.Groups["exponent"].Value;

            var notation = exponent[0] switch
            {
                '-' => $"{PadLeft(integer, exponent[1..])}{fraction}",
                '+' => $"{integer}{PadRight(fraction, exponent[1..])}",
                _ => $"{integer}{PadRight(fraction, exponent)}"
            };

            var (mantissa, scale) = notation.DeconstructFromNotation();
            return new BigDecimal(mantissa, scale);
        }

        private static BigDecimal ParseFromDeconstructed(Match match)
        {
            var mantissa = match.Groups["mantissa"].Value;
            var scale = match.Groups["scale"].Value;

            return new BigDecimal(
                BigInteger.Parse(mantissa),
                int.Parse(scale));
        }

        private static string PadLeft(string integer, string padCount)
        {
            if ("0".Equals(padCount))
                return integer;

            var ipadCount = int.Parse(padCount);
            var padded = integer.PadLeft(ipadCount, '0');
            return padded
                .Insert(padded.Length - ipadCount, ".")
                .ApplyTo(v => v.StartsWith(".") ? $"0{v}" : v);
        }

        private static string PadRight(string fraction, string padCount)
        {
            if ("0".Equals(padCount)
                || fraction.Length.ToString().Equals(padCount))
                return fraction;

            var ipadCount = int.Parse(padCount);
            var padded = fraction.PadRight(ipadCount, '0');
            return ipadCount < fraction.Length ? padded.Insert(ipadCount, ".") : padded;
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
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
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

        #region IIncrementOperators<>
        public static BigDecimal operator ++(BigDecimal value) => value + 1;
        #endregion

        #region IDecrementOperators<>
        public static BigDecimal operator --(BigDecimal value) => value - 1;
        #endregion

        #region IAdditiveIdentity<,>
        public static BigDecimal AdditiveIdentity => Zero;
        #endregion

        #region IMultiplicativeIdentity<,>
        public static BigDecimal MultiplicativeIdentity => One;
        #endregion

        #region INumberBase<>
        private static BigDecimal _one = new BigDecimal(1);
        private static BigDecimal _zero = new BigDecimal(0);

        public static BigDecimal One => _one;

        public static BigDecimal Zero => _zero;

        public static int Radix => 10;

        public static BigDecimal Abs(BigDecimal value) => value < 0 ? -value : value;

        public static bool IsCanonical(BigDecimal value) => true;

        public static bool IsComplexNumber(BigDecimal value) => false;

        public static bool IsEvenInteger(BigDecimal value) => IsInteger(value) && (value._value & 1) == 0;

        public static bool IsFinite(BigDecimal value) => true;

        public static bool IsImaginaryNumber(BigDecimal value) => false;

        public static bool IsInfinity(BigDecimal value) => false;

        public static bool IsInteger(BigDecimal value) => value._scale == 0;

        public static bool IsNaN(BigDecimal value) => false;

        public static bool IsNegative(BigDecimal value) => BigInteger.IsNegative(value._value);

        public static bool IsNegativeInfinity(BigDecimal value) => false;

        public static bool IsNormal(BigDecimal value) => value != 0;

        public static bool IsOddInteger(BigDecimal value) => !IsEvenInteger(value);

        public static bool IsPositive(BigDecimal value) => BigInteger.IsPositive(value._value);

        public static bool IsPositiveInfinity(BigDecimal value) => false;

        public static bool IsRealNumber(BigDecimal value) => true;

        public static bool IsSubnormal(BigDecimal value) => false;

        public static bool IsZero(BigDecimal value) => value == 0;

        public static  BigDecimal MaxMagnitude(BigDecimal x, BigDecimal y)
        {
            var ax = Abs(x);
            var ay = Abs(y);

            if (ax > ay)
                return x;

            if (ax == ay)
                return IsNegative(x) ? y : x;

            return y;
        }

        public static  BigDecimal MaxMagnitudeNumber(BigDecimal x, BigDecimal y) => MaxMagnitude(x, y);

        public static  BigDecimal MinMagnitude(BigDecimal x, BigDecimal y)
        {
            var ax = Abs(x);
            var ay = Abs(y);

            if (ax < ay)
                return x;

            if (ax == ay)
                return IsNegative(x) ? x : y;

            return y;
        }

        public static  BigDecimal MinMagnitudeNumber(BigDecimal x, BigDecimal y) => MinMagnitude(x, y);

        public static BigDecimal Parse(string s, NumberStyles style, IFormatProvider provider) => Parse(s, provider);

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

        private static bool TryConvertFrom<TOther>(TOther value,  out BigDecimal result)
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
                Half v => new BigDecimal(v),
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

        #region ISignedNumber<>
        public static BigDecimal NegativeOne => -One;

        static BigDecimal INumberBase<BigDecimal>.One => One;

        static int INumberBase<BigDecimal>.Radix => Radix;

        static BigDecimal INumberBase<BigDecimal>.Zero => Zero;

        static BigDecimal IAdditiveIdentity<BigDecimal, BigDecimal>.AdditiveIdentity => AdditiveIdentity;

        static BigDecimal IMultiplicativeIdentity<BigDecimal, BigDecimal>.MultiplicativeIdentity => MultiplicativeIdentity;
        #endregion

        #region IModulusOperator<,,>
        public static BigDecimal operator %(BigDecimal left, BigDecimal right)
        {
            var (l, r) = Balance(left, right);
            return new BigDecimal(l % r, Math.Max(left._scale, right._scale));
        }
        #endregion

        #region Implicits
        public static implicit operator BigDecimal(byte value) => new BigDecimal(value);
        public static implicit operator BigDecimal(sbyte value) => new BigDecimal(value);
        public static implicit operator BigDecimal(char value) => new BigDecimal(value);
        public static implicit operator BigDecimal(short value) => new BigDecimal(value);
        public static implicit operator BigDecimal(ushort value) => new BigDecimal(value);
        public static implicit operator BigDecimal(int value) => new BigDecimal(value);
        public static implicit operator BigDecimal(uint value) => new BigDecimal(value);
        public static implicit operator BigDecimal(long value) => new BigDecimal(value);
        public static implicit operator BigDecimal(ulong value) => new BigDecimal(value);
        public static implicit operator BigDecimal(Half value) => new BigDecimal(value);
        public static implicit operator BigDecimal(float value) => new BigDecimal(value);
        public static implicit operator BigDecimal(double value) => new BigDecimal(value);
        public static implicit operator BigDecimal(decimal value) => new BigDecimal(value);
        public static implicit operator BigDecimal(BigInteger value) => new BigDecimal(value);
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

        #region Helpers

        internal static (BigInteger first, BigInteger second) Balance(BigDecimal first, BigDecimal second)
        {
            return first._scale.CompareTo(second._scale) switch
            {
                0 => (first._value, second._value),
                < 0 => (first._value * BigInteger.Pow(10, second._scale - first._scale), second._value),
                > 0 => (first._value, second._value * BigInteger.Pow(10, first._scale - second._scale))
            };
        }

        internal static (BigInteger raisedValue, int decimalShifts) Raise(BigInteger dividend, BigInteger  divisor)
        {
            var decimalShifts = 0;
            var raisedValue = dividend;
            while (raisedValue < divisor)
            {
                raisedValue *= 10;
                decimalShifts++;
            }

            return (raisedValue, decimalShifts);
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

        internal decimal DemoteToDecimal()
        {
            var sign = _value.Sign switch
            {
                < 0 => false,
                >= 0 => true
            };
            var absoluteValue = !sign ? BigInteger.Negate(_value) : _value;
            var ints = absoluteValue
                .ToByteArray()
                .TakeExactly(12)
                .Select((@byte, index) => (@byte, index))
                .GroupBy(tuple => tuple.index / 4, tuple => tuple.@byte)
                .Select(bytes => BitConverter.ToInt32(bytes.ToArray()))
                .ToArray();

            var scale = BitConverter.ToInt32(new byte[] { 0, 0, (byte)_scale, (byte)(sign ? 0 : 1) });

            return new decimal(ints.Append(scale).ToArray());
        }

        internal double DemoteToDouble()
        {
            var text = _value.ToString();
            var sign = text[0] == '-';

            var allDigits = text[1..];
            var totalLength = allDigits.Length;
            var significantDigits = allDigits.Take(16).JoinUsing("");
            var doubleValue =
                IsPointSignificant(totalLength, _scale) ? significantDigits.InsertAt(totalLength - _scale, '.').JoinUsing("") :
                _scale >= allDigits.Length ? $"0.{significantDigits.PadLeft(_scale - totalLength, '0')}" :
                significantDigits;

            return double.Parse(doubleValue);
        }

        internal double ToDouble()
        {
            if (this > double.MaxValue)
                return double.MaxValue;

            return DemoteToDouble();
        }

        internal decimal ToDecimal()
        {
            if (this > decimal.MaxValue)
                return decimal.MaxValue;

            return DemoteToDecimal();
        }

        internal BigInteger Truncate()
        {
            if (_scale == 0)
                return _value;

            return _value / BigInteger.Pow(10, _scale);
        }

        internal static bool IsPointSignificant(int totalValueLength, int scale)
        {
            return totalValueLength > scale && scale > totalValueLength - 16;
        }

        internal byte DigitAtDecimalPlace(int decimalPlace)
        {
            if (decimalPlace < 1)
                throw new ArgumentOutOfRangeException($"{nameof(decimalPlace)} is < 1. '{decimalPlace}'");

            var decimalIndex = decimalPlace - 1;
            var significantDigits = _value.ToString();
            var significantDigitCount = significantDigits.Length;

            if (_scale > significantDigitCount)
            {
                if (decimalIndex < (_scale - significantDigitCount))
                    return 0;

                else
                    return (byte)(significantDigits[decimalIndex - (_scale - significantDigitCount)] - 48);
            }

            else if (decimalIndex >= significantDigitCount)
                return 0;

            else
            {
                var resolvedIndex = decimalIndex + (significantDigitCount - _scale);

                if (resolvedIndex < significantDigitCount)
                    return (byte)(significantDigits[resolvedIndex] - 48);

                else return 0;
            }
        }

        internal int SignificantDigitCount() => _value.ToString().Length;

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
