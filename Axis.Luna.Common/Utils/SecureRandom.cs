using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

using static Axis.Luna.Extensions.Common;

namespace Axis.Luna.Common.Utils
{
    public class SecureRandom
    {
        private static readonly char[] CharacterMap = new[]
        {
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            '0','1','2','3','4','5','6','7','8','9'
        };


        public static byte[] NextBytes(int length) => NextBytes(new byte[length]);

        public static byte[] NextBytes(byte[] bytes)
        {
            RandomNumberGenerator.Fill(bytes);
            return bytes;
        }

        /// <summary>
        /// Generate a positive integer less than the supplied value. If no value is supplied, no limit is assumed.
        /// </summary>
        /// <param name="maxExclusive">The value which will always be greater than generated integers</param>
        /// <returns>The generated integer</returns>
        public static int NextInt(int? maxExclusive = null)
        {
            if (maxExclusive <= 0)
                throw new ArgumentException($"Invalid {nameof(maxExclusive)}");

            var @int = Math.Abs(RandomNumberGenerator.GetInt32(maxExclusive ?? int.MaxValue));
            if (@int >= maxExclusive) return @int % maxExclusive.Value;
            else return @int;
        }

        /// <summary>
        /// Generate a positive long less than the supplied value. If no value is supplied, no limit is assumed.
        /// </summary>
        /// <param name="maxExclusive">The value which will always be greater than generated long value</param>
        /// <returns>The generated long</returns>
        public static long NextLong(long? maxExclusive = null)
        {
            if (maxExclusive <= 0)
                throw new ArgumentException($"Invalid {nameof(maxExclusive)}");

            var longBytes = new byte[8];
            RandomNumberGenerator.Fill(longBytes);
            var @long = Math.Abs(BitConverter.ToInt64(longBytes, 0));
            if (@long > maxExclusive) return @long % maxExclusive.Value;
            else return @long;
        }

        public static double NextSignedDouble()
        {
            var doubleBytes = new byte[8];
            RandomNumberGenerator.Fill(doubleBytes);
            return BitConverter.ToDouble(doubleBytes, 0);
        }

        public static int NextSignedInt()
        {
            var intBytes = new byte[4];
            RandomNumberGenerator.Fill(intBytes);
            return BitConverter.ToInt32(intBytes, 0);
        }

        public static long NextSignedLong()
        {
            var longBytes = new byte[8];
            RandomNumberGenerator.Fill(longBytes);
            return BitConverter.ToInt64(longBytes, 0);
        }

        public static int[] NextSequence(int sequenceLength, int maxExclusive = -1)
        {
            var list = new List<int>();
            for (int cnt = 0; cnt < sequenceLength; cnt++) 
                list.Add(NextInt(maxExclusive));

            return list.ToArray();
        }

        public static string NextAlphaString(int length)
            => NextSequence(length, 26)
                .Select(_r => CharacterMap[_r])
                .JoinUsing("");

        public static string NextAlphaNumericString(int length)
        => NextSequence(length, 36)
            .Select(_r => CharacterMap[_r])
            .JoinUsing("");

        public static char NextChar() => CharacterMap[NextInt() % 26];

        public static V NextValue<V>(V[] values) => values[NextInt(values.Length)];

        public static char NextChar(string values) => NextValue(values.ToCharArray());

        public static bool NextBool() => NextInt() % 2 == 0;
    }
}
