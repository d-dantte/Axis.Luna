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
    /// <summary>
    /// Big Decimal - unlimited mantissa, with <c>int.MaxValue</c> max scale
    /// <para>
    /// Note:
    /// <list type="number">
    /// <item>For all <c>Parse(...)</c> and <c>TryParse(...)</c> methods that take <see cref="IFormatProvider"/> argument, the provider is ignored (for now).</item>
    /// </list>
    /// </para>
    /// </summary>
    internal readonly struct BigDecimal___ :
        IComparable,
        IComparable<BigDecimal___>,
        IEquatable<BigDecimal___>,
        IParsable<BigDecimal___>,
        IResultParsable<BigDecimal___>,
        IUnaryPlusOperators<BigDecimal___, BigDecimal___>,
        IUnaryNegationOperators<BigDecimal___, BigDecimal___>,
        IAdditionOperators<BigDecimal___, BigDecimal___, BigDecimal___>,
        IAdditiveIdentity<BigDecimal___, BigDecimal___>,
        ISubtractionOperators<BigDecimal___, BigDecimal___, BigDecimal___>,
        IMultiplyOperators<BigDecimal___, BigDecimal___, BigDecimal___>,
        IMultiplicativeIdentity<BigDecimal___, BigDecimal___>,
        IDivisionOperators<BigDecimal___, BigDecimal___, BigDecimal___>,
        IIncrementOperators<BigDecimal___>,
        IDecrementOperators<BigDecimal___>,
        ISpanFormattable,
        ISpanParsable<BigDecimal___>,
        IModulusOperators<BigDecimal___, BigDecimal___, BigDecimal___>,
        IEqualityOperators<BigDecimal___, BigDecimal___, bool>,
        IComparisonOperators<BigDecimal___, BigDecimal___, bool>,
        INumber<BigDecimal___>,
        INumberBase<BigDecimal___>
    {
        private static readonly RegexOptions PatternOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
        internal static readonly Regex DecimalPattern = new Regex("^(?'integral'[\\+\\-]?\\d+)(\\.(?'fraction'\\d+))?$", PatternOptions);
        internal static readonly Regex ScientificPattern = new Regex("^(?'integral'[\\+\\-]?\\d+)\\.(?'fraction'\\d+)E(?'exponent'\\-?\\d+)$", PatternOptions);
        internal static readonly Regex DeconstructedPattern = new Regex("^\\[Mantissa\\:\\s*(?'mantissa'[\\+\\-]?\\d+),\\s*Scale\\:\\s*(?'scale'\\d+)\\]$", PatternOptions);

        private readonly BigInteger _value;
        private readonly int _scale;

        #region construction
        public BigDecimal___(int value)
        {
            _value = value;
            _scale = 0;
        }

        public BigDecimal___(byte value)
        {
            _value = value;
            _scale = 0;
        }

        public BigDecimal___(sbyte value)
        {
            _value = value;
            _scale = 0;
        }

        public BigDecimal___(uint value)
        {
            _value = value;
            _scale = 0;
        }

        public BigDecimal___(long value)
        {
            _value = value;
            _scale = 0;
        }

        public BigDecimal___(ulong value)
        {
            _value = value;
            _scale = 0;
        }

        public BigDecimal___(Half value)
        {
            var (Mantissa, Scale) = value.Deconstruct();
            _value = Mantissa;
            _scale = Scale;
        }

        public BigDecimal___(float value)
        {
            var (Mantissa, Scale) = value.Deconstruct();
            _value = Mantissa;
            _scale = Scale;
        }

        public BigDecimal___(double value)
        {
            var (Mantissa, Scale) = value.Deconstruct();
            _value = Mantissa;
            _scale = Scale;
        }

        public BigDecimal___(decimal value)
        {
            var (Mantissa, Scale) = value.Deconstruct();
            _value = Mantissa;
            _scale = Scale;
        }

        public BigDecimal___(BigInteger value)
        : this(value, 0)
        {
        }

        public BigDecimal___(BigInteger value, int scale)
        {
            var (Mantissa, Scale) = (value, scale).NormalizeBigDecimal();
            _value = Mantissa;
            _scale = Scale;
        }
        #endregion


        public BigDecimal___ Floor() => new BigDecimal___(Truncate(), 0);

        public BigDecimal___ Ceiling()
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

        public BigDecimal___ Fraction()
        {
            var truncated = Truncate();
            return this - truncated;
        }

        public BigDecimal___ Round(int decimals = 0)
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

            return new BigDecimal___(rounded, newScale);
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

        public static bool operator ==(BigDecimal___ a, BigDecimal___ b) => a.Equals(b);

        public static bool operator !=(BigDecimal___ a, BigDecimal___ b) => !(a == b);

        #endregion

        #region IUnaryPlusOperators<,>

        public static BigDecimal___ operator +(BigDecimal___ value) => value;

        #endregion

        #region IUnaryNegationOperator<,>

        public static BigDecimal___ operator -(BigDecimal___ value) => new(BigInteger.Negate(value._value), value._scale);

        #endregion

        #region IAdditionOperators<,>

        public static BigDecimal___ operator +(BigDecimal___ left, BigDecimal___ right)
        {
            if (left == 0)
                return right;

            if (right == 0)
                return left;

            var (l, r) = Balance(left, right);
            return new BigDecimal___(l + r, Math.Max(left._scale, right._scale));
        }

        #endregion

        #region ISubtractionOperators<,>

        public static BigDecimal___ operator -(BigDecimal___ left, BigDecimal___ right)
        {
            if (left == 0)
                return right;

            if (right == 0)
                return left;

            var (l, r) = Balance(left, right);
            return new BigDecimal___(l - r, Math.Max(left._scale, right._scale));
        }

        #endregion

        #region IMultiplicationOperators<,>

        public static BigDecimal___ operator *(BigDecimal___ left, BigDecimal___ right)
        {
            if (left == 1)
                return right;

            if (right == 1)
                return left;

            var (l, r) = Balance(left, right, out var scale);
            return new BigDecimal___(l * r, scale * 2);
        }

        #endregion

        #region IDivisionOperators<,>

        public static BigDecimal___ operator /(BigDecimal___ left, BigDecimal___ right)
        {
            if (left == 1)
                return right;

            if (right == 1)
                return left;

            if (left < decimal.MaxValue && right < decimal.MaxValue)
            {
                var dleft = left.DemoteToDecimal();
                var dright = right.DemoteToDecimal();
                return new BigDecimal___(dleft / dright);
            }

            var (l, r) = Balance(left, right);
            var (quotient, remainder) = BigInteger.DivRem(l, r);

            if (remainder == 0)
                return new BigDecimal___(quotient);

            var raised = Raise(remainder, r);

            var maxFractionalDigits = FormatContext.AsyncLocal.MaxSignificantFractionalDigits;

            #region Special optimization cases should come here
            if (raised.raisedValue == r)
            {
                return new BigDecimal___((quotient * 10) + 1, 1);
            }
            #endregion

            var fnumerator = raised.raisedValue * BigInteger.Pow(10, maxFractionalDigits - 1);
            var fquotientString = (fnumerator / r).ToString().TrimEnd('0');
            var fquotient = BigInteger.Parse(fquotientString);

            quotient *= BigInteger.Pow(10, fquotientString.Length);

            return new BigDecimal___(quotient + fquotient, fquotientString.Length);
        }

        #endregion

        #region IComparisonOperators<,>

        public static bool operator >(BigDecimal___ left, BigDecimal___ right)
        {
            var (l, r) = Balance(left, right);
            return l > r;
        }

        public static bool operator >=(BigDecimal___ left, BigDecimal___ right)
        {
            var (l, r) = Balance(left, right);
            return l >= r;
        }

        public static bool operator <(BigDecimal___ left, BigDecimal___ right)
        {
            var (l, r) = Balance(left, right);
            return l < r;
        }

        public static bool operator <=(BigDecimal___ left, BigDecimal___ right)
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
                byte v => new BigDecimal___(v),
                sbyte v => new BigDecimal___(v),
                short v => new BigDecimal___(v),
                ushort v => new BigDecimal___(v),
                int v => new BigDecimal___(v),
                uint v => new BigDecimal___(v),
                long v => new BigDecimal___(v),
                ulong v => new BigDecimal___(v),
                float v => new BigDecimal___(v),
                double v => new BigDecimal___(v),
                decimal v => new BigDecimal___(v),
                BigInteger v => new BigDecimal___(v),
                BigDecimal___ v => v,
                null => throw new ArgumentNullException(nameof(obj)),
                _ => throw new ArgumentException($"Cannot compare the given type: {obj.GetType()}")
            };

            return this.CompareTo(other);
        }

        public int CompareTo(BigDecimal___ other)
        {
            var (first, second) = Balance(this, other);
            return first.CompareTo(second);
        }
        #endregion

        #region IEquatable

        public bool Equals(BigDecimal___ other)
        {
            return CompareTo(other) == 0;
        }
        #endregion

        #region IParsable
        public static BigDecimal___ Parse(string s, IFormatProvider provider)
        {
            return Parse(s).Resolve();
        }

        public static bool TryParse(
            [NotNullWhen(true)] string s,
            IFormatProvider provider,
            [MaybeNullWhen(false)] out BigDecimal___ result)
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
        public static bool TryParse(string text, out IResult<BigDecimal___> result)
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
            result = Results.Result.Of<BigDecimal___>(new FormatException($"Invalid {nameof(BigDecimal___)} format: '{text}'"));
            return false;
        }

        public static IResult<BigDecimal___> Parse(string text)
        {
            _ = TryParse(text, out var result);
            return result;
        }

        private static BigDecimal___ ParseFromDecimal(Match match)
        {
            var integer = match.Groups["integral"].Value;
            var fraction = match.Groups["fraction"].Success
                ? match.Groups["fraction"].Value.TrimEnd('0') // <-- cannonicalizes the fraction.
                : "";

            var notation = $"{integer}.{fraction}";
            var (mantissa, scale) = notation.DeconstructFromNotation();

            return new BigDecimal___(mantissa, scale);
        }

        private static BigDecimal___ ParseFromScientific(Match match)
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
            return new BigDecimal___(mantissa, scale);
        }

        private static BigDecimal___ ParseFromDeconstructed(Match match)
        {
            var mantissa = match.Groups["mantissa"].Value;
            var scale = match.Groups["scale"].Value;

            return new BigDecimal___(
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
        public static BigDecimal___ Parse(ReadOnlySpan<char> s, IFormatProvider provider) => Parse(s.ToString()).Resolve();
        public static bool TryParse(
            ReadOnlySpan<char> s,
            IFormatProvider provider,
            [MaybeNullWhen(false)] out BigDecimal___ result)
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
        public static BigDecimal___ operator ++(BigDecimal___ value) => value + 1;
        #endregion

        #region IDecrementOperators<>
        public static BigDecimal___ operator --(BigDecimal___ value) => value - 1;
        #endregion

        #region IAdditiveIdentity<,>
        public static BigDecimal___ AdditiveIdentity => Zero;
        #endregion

        #region IMultiplicativeIdentity<,>
        public static BigDecimal___ MultiplicativeIdentity => One;
        #endregion

        #region INumberBase<>
        private static BigDecimal___ _one = new BigDecimal___(1);
        private static BigDecimal___ _zero = new BigDecimal___(0);

        public static BigDecimal___ One => _one;

        public static BigDecimal___ Zero => _zero;

        public static int Radix => 10;

        public static BigDecimal___ Abs(BigDecimal___ value) => value < 0 ? -value : value;

        public static bool IsCanonical(BigDecimal___ value) => true;

        public static bool IsComplexNumber(BigDecimal___ value) => false;

        public static bool IsEvenInteger(BigDecimal___ value) => IsInteger(value) && (value._value & 1) == 0;

        public static bool IsFinite(BigDecimal___ value) => true;

        public static bool IsImaginaryNumber(BigDecimal___ value) => false;

        public static bool IsInfinity(BigDecimal___ value) => false;

        public static bool IsInteger(BigDecimal___ value) => value._scale == 0;

        public static bool IsNaN(BigDecimal___ value) => false;

        public static bool IsNegative(BigDecimal___ value) => BigInteger.IsNegative(value._value);

        public static bool IsNegativeInfinity(BigDecimal___ value) => false;

        public static bool IsNormal(BigDecimal___ value) => value != 0;

        public static bool IsOddInteger(BigDecimal___ value) => !IsEvenInteger(value);

        public static bool IsPositive(BigDecimal___ value) => BigInteger.IsPositive(value._value);

        public static bool IsPositiveInfinity(BigDecimal___ value) => false;

        public static bool IsRealNumber(BigDecimal___ value) => true;

        public static bool IsSubnormal(BigDecimal___ value) => false;

        public static bool IsZero(BigDecimal___ value) => value == 0;

        public static  BigDecimal___ MaxMagnitude(BigDecimal___ x, BigDecimal___ y)
        {
            var ax = Abs(x);
            var ay = Abs(y);

            if (ax > ay)
                return x;

            if (ax == ay)
                return IsNegative(x) ? y : x;

            return y;
        }

        public static  BigDecimal___ MaxMagnitudeNumber(BigDecimal___ x, BigDecimal___ y) => MaxMagnitude(x, y);

        public static  BigDecimal___ MinMagnitude(BigDecimal___ x, BigDecimal___ y)
        {
            var ax = Abs(x);
            var ay = Abs(y);

            if (ax < ay)
                return x;

            if (ax == ay)
                return IsNegative(x) ? x : y;

            return y;
        }

        public static  BigDecimal___ MinMagnitudeNumber(BigDecimal___ x, BigDecimal___ y) => MinMagnitude(x, y);

        public static BigDecimal___ Parse(string s, NumberStyles style, IFormatProvider provider) => Parse(s, provider);

        public static BigDecimal___ Parse(
            ReadOnlySpan<char> s,
            NumberStyles style,
            IFormatProvider provider)
            => Parse(s, provider);

        static bool INumberBase<BigDecimal___>.TryConvertFromChecked<TOther>(
            TOther value,
            [MaybeNullWhen(false)]
            out BigDecimal___ result)
            => TryConvertFrom(value, out result);

        static bool INumberBase<BigDecimal___>.TryConvertFromSaturating<TOther>(
            TOther value,
            [MaybeNullWhen(false)] out BigDecimal___ result)
            => TryConvertFrom(value, out result);


        static bool INumberBase<BigDecimal___>.TryConvertFromTruncating<TOther>(
            TOther value,
            [MaybeNullWhen(false)] out BigDecimal___ result)
            => TryConvertFrom(value, out result);


        static bool INumberBase<BigDecimal___>.TryConvertToChecked<TOther>(
            BigDecimal___ value,
            [MaybeNullWhen(false)] out TOther result)
            => TryConvertTo(value, out result);


        static bool INumberBase<BigDecimal___>.TryConvertToSaturating<TOther>(
            BigDecimal___ value,
            [MaybeNullWhen(false)] out TOther result)
            => TryConvertTo(value, out result);


        static bool INumberBase<BigDecimal___>.TryConvertToTruncating<TOther>(
            BigDecimal___ value,
            [MaybeNullWhen(false)] out TOther result)
            => TryConvertTo(value, out result);

        private static bool TryConvertFrom<TOther>(TOther value,  out BigDecimal___ result)
        {
            var bigDecimal = value switch
            {
                byte v => new BigDecimal___(v),
                sbyte v => new BigDecimal___(v),
                char v => new BigDecimal___(v),
                short v => new BigDecimal___(v),
                ushort v => new BigDecimal___(v),
                int v => new BigDecimal___(v),
                uint v => new BigDecimal___(v),
                long v => new BigDecimal___(v),
                ulong v => new BigDecimal___(v),
                Half v => new BigDecimal___(v),
                float v => new BigDecimal___(v),
                double v => new BigDecimal___(v),
                decimal v => new BigDecimal___(v),
                _ => default(BigDecimal___?)
            };

            if (bigDecimal is null)
            {
                result = default;
                return false;
            }

            result = bigDecimal.Value;
            return true;
        }

        private static bool TryConvertTo<TOther>(BigDecimal___ value, out TOther result)
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
            [MaybeNullWhen(false)] out BigDecimal___ result)
            => TryParse(s, provider, out result);

        public static bool TryParse(
            ReadOnlySpan<char> s,
            NumberStyles style,
            IFormatProvider provider,
            [MaybeNullWhen(false)] out BigDecimal___ result)
            => TryParse(s, provider, out result);
        #endregion

        #region ISignedNumber<>
        public static BigDecimal___ NegativeOne => -One;

        static BigDecimal___ INumberBase<BigDecimal___>.One => One;

        static int INumberBase<BigDecimal___>.Radix => Radix;

        static BigDecimal___ INumberBase<BigDecimal___>.Zero => Zero;

        static BigDecimal___ IAdditiveIdentity<BigDecimal___, BigDecimal___>.AdditiveIdentity => AdditiveIdentity;

        static BigDecimal___ IMultiplicativeIdentity<BigDecimal___, BigDecimal___>.MultiplicativeIdentity => MultiplicativeIdentity;
        #endregion

        #region IModulusOperator<,,>
        public static BigDecimal___ operator %(BigDecimal___ left, BigDecimal___ right)
        {
            var (l, r) = Balance(left, right);
            return new BigDecimal___(l % r, Math.Max(left._scale, right._scale));
        }
        #endregion

        #region Implicits
        public static implicit operator BigDecimal___(byte value) => new BigDecimal___(value);
        public static implicit operator BigDecimal___(sbyte value) => new BigDecimal___(value);
        public static implicit operator BigDecimal___(char value) => new BigDecimal___(value);
        public static implicit operator BigDecimal___(short value) => new BigDecimal___(value);
        public static implicit operator BigDecimal___(ushort value) => new BigDecimal___(value);
        public static implicit operator BigDecimal___(int value) => new BigDecimal___(value);
        public static implicit operator BigDecimal___(uint value) => new BigDecimal___(value);
        public static implicit operator BigDecimal___(long value) => new BigDecimal___(value);
        public static implicit operator BigDecimal___(ulong value) => new BigDecimal___(value);
        public static implicit operator BigDecimal___(Half value) => new BigDecimal___(value);
        public static implicit operator BigDecimal___(float value) => new BigDecimal___(value);
        public static implicit operator BigDecimal___(double value) => new BigDecimal___(value);
        public static implicit operator BigDecimal___(decimal value) => new BigDecimal___(value);
        public static implicit operator BigDecimal___(BigInteger value) => new BigDecimal___(value);
        #endregion

        #region Explicits
        public static explicit operator byte(BigDecimal___ value) => (byte)value.Truncate();
        public static explicit operator sbyte(BigDecimal___ value) => (sbyte)value.Truncate();
        public static explicit operator char(BigDecimal___ value) => (char)value.Truncate();
        public static explicit operator short(BigDecimal___ value) => (short)value.Truncate();
        public static explicit operator ushort(BigDecimal___ value) => (ushort)value.Truncate();
        public static explicit operator int(BigDecimal___ value) => (int)value.Truncate();
        public static explicit operator uint(BigDecimal___ value) => (uint)value.Truncate();
        public static explicit operator long(BigDecimal___ value) => (long)value.Truncate();
        public static explicit operator ulong(BigDecimal___ value) => (ulong)value.Truncate();
        public static explicit operator Half(BigDecimal___ value) => (Half)value.ToDouble();
        public static explicit operator float(BigDecimal___ value) => (float)value.ToDouble();
        public static explicit operator double(BigDecimal___ value) => value.ToDouble();
        public static explicit operator decimal(BigDecimal___ value) => value.ToDecimal();
        public static explicit operator BigInteger(BigDecimal___ value) => (char)value.Truncate();
        #endregion

        #region Helpers

        public string ToScientificString(int decimalPoint = 1)
        {
            if (decimalPoint < 1)
                throw new ArgumentOutOfRangeException(nameof(decimalPoint));

            var digits = new StringBuilder(_value.ToString());
            var digitCount = digits.Length;

            if (digitCount > decimalPoint)
                digits.Insert(decimalPoint, '.');

            else
            {
                digits.Append(".0");
                decimalPoint = digitCount;
            }

            var exponent = digitCount - _scale - decimalPoint;

            return $"{digits}e{exponent}";
        }

        public string ToNonScientificString()
        {
            var digits = new StringBuilder(_value.ToString());
            var digitCount = digits.Length;

            return
                _scale == 0 ? $"{digits}.0" :
                _scale < digitCount ? digits.Insert(digitCount - _scale, ".").ToString() :
                $"0.{digits.ToString().PadLeft(_scale, '0')}";
        }

        public static BigDecimal___ Power(BigDecimal___ value, BigDecimal___ exponent)
        {
            if (value > double.MaxValue || value < double.MinValue)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (exponent > double.MaxValue || exponent < double.MinValue)
                throw new ArgumentOutOfRangeException(nameof(exponent));

            if (BigDecimal___.IsInteger(exponent))
            {
                var balanced = Balance(value, exponent);
                return BigInteger.Pow(balanced.first, (int)balanced.second);
            }
            else  return Math.Pow((double)value, (double)exponent);
        }

        internal static (BigInteger first, BigInteger second) Balance(BigDecimal___ first, BigDecimal___ second)
        {
            return Balance(first, second, out _);
        }

        internal static (BigInteger first, BigInteger second) Balance(BigDecimal___ first, BigDecimal___ second, out int scale)
        {
            var values = first._scale.CompareTo(second._scale) switch
            {
                0 => (first._value, second._value, first._scale),
                < 0 => (first._value * BigInteger.Pow(10, second._scale - first._scale), second._value, second._scale),
                > 0 => (first._value, second._value * BigInteger.Pow(10, first._scale - second._scale), first._scale)
            };

            scale = values._scale;
            return (values.Item1, values.Item2);
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

        public static BigDecimal___ WithContext(FormatContext context, Func<BigDecimal___> func)
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

            var allDigits = text[(sign ? 1 : 0)..];
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
        /// Specifies optional data used in representing <see cref="BigDecimal___"/> values during various operations
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
