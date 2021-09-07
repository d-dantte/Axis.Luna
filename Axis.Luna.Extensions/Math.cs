using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Axis.Luna.Extensions
{

    [DebuggerStepThrough]
    public static class MathExtensions
    {
        public static Fraction Divide(this int denominator, int numerator) => new Fraction
        {
            Remainder = numerator % denominator,
            Multiples = numerator / denominator
        };
        public static Fraction Divide(this long denominator, long numerator) => new Fraction
        {
            Remainder = numerator % denominator,
            Multiples = numerator / denominator
        };

        public static int DigitCount(long value) => (int)Math.Floor(Math.Log10(value) + 1);

        public static int DigitCount(ulong value) => (int)Math.Floor(Math.Log10(value) + 1);

        /// <summary>
        /// Linear interpolation from one value (start) to another value (end) using a ratio between 0 and 1
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public static double Lerp(this double start, double end, double ratio)
        {
            ratio.ThrowIf(r => r < 0d || r > 1d, $"ratio must be between 0.0 and 1.0");

            var rinfo = new { ratio, inverseRatio = 1 - ratio };

            return (start * rinfo.inverseRatio) + (end * rinfo.ratio);
        }


        private static readonly String[] OrdinalSuffixes = { "th", "st", "nd", "rd", "th", "th", "th", "th", "th", "th" };
        private static HashSet<Type> _numerics = new HashSet<Type>
        {
            typeof(short?), typeof(int?), typeof(long?),
            typeof(ushort?), typeof(uint?), typeof(ulong?),
            typeof(decimal?), typeof(float?), typeof(double?),
            typeof(byte?), typeof(sbyte?)
        };

        public static string OrdinalSuffix(this int value)
        {
            int n = Math.Abs(value);
            int lastTwoDigits = n % 100;
            int lastDigit = n % 10;
            int index = (lastTwoDigits >= 11 && lastTwoDigits <= 13) ? 0 : lastDigit;
            return OrdinalSuffixes[index];
        }

        public static string AsOrdinal(this int n) => $"{n}{n.OrdinalSuffix()}";

        public static bool IsNumeric(this Type type) => _numerics.Any(_n => _n.IsAssignableFrom(type));

        public static bool IsOdd(this long value) => value % 2 == 1;

        public static bool IsEven(this long value) => !value.IsOdd();

        public static bool IsOdd(this ulong value) => value % 2 == 1;

        public static bool IsEven(this ulong value) => !value.IsOdd();

    }

    public class Fraction
    {
        public long Multiples { get; set; }
        public long Remainder { get; set; }
    }
}
