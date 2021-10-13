using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

using static Axis.Luna.Extensions.Common;

namespace Axis.Luna.Common.Utils
{
    public class SecureRandom : IDisposable
    {
        private static readonly char[] CharacterMap = new[]
        {
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            '0','1','2','3','4','5','6','7','8','9'
        };

        private readonly RNGCryptoServiceProvider _cryptoProvider = new RNGCryptoServiceProvider();

        public void Dispose() => _cryptoProvider.Dispose();

        public byte[] NextBytes(int length) => NextBytes(new byte[length]);

        public byte[] NextBytes(byte[] bytes)
        {
            _cryptoProvider.GetBytes(bytes);
            return bytes;
        }

        /// <summary>
        /// Generate a positive integer less than the supplied value. If no value is supplied, no limit is assumed.
        /// </summary>
        /// <param name="maxExclusive">The value which will always be greater than generated integers</param>
        /// <returns>The generated integer</returns>
        public int NextInt(int maxExclusive = -1)
        {
            var intBytes = new byte[4];
            _cryptoProvider.GetBytes(intBytes);
            var @int = Math.Abs(BitConverter.ToInt32(intBytes, 0));
            if (maxExclusive > 0) return @int % maxExclusive;
            else return @int;
        }

        /// <summary>
        /// Generate a positive long less than the supplied value. If no value is supplied, no limit is assumed.
        /// </summary>
        /// <param name="maxExclusive">The value which will always be greater than generated long value</param>
        /// <returns>The generated long</returns>
        public long NextLong(long maxExclusive = -1)
        {
            var longBytes = new byte[8];
            _cryptoProvider.GetBytes(longBytes);
            var @long = Math.Abs(BitConverter.ToInt64(longBytes, 0));
            if (maxExclusive > 0) return @long % maxExclusive;
            else return @long;
        }

        public double NextSignedDouble()
        {
            var doubleBytes = new byte[8];
            _cryptoProvider.GetBytes(doubleBytes);
            return BitConverter.ToDouble(doubleBytes, 0);
        }

        public int NextSignedInt()
        {
            var intByte = new byte[4];
            _cryptoProvider.GetBytes(intByte);
            return BitConverter.ToInt32(intByte, 0);
        }

        public long NextSignedLong()
        {
            var longBytes = new byte[8];
            _cryptoProvider.GetBytes(longBytes);
            return BitConverter.ToInt64(longBytes, 0);
        }

        public int[] NextSequence(int sequenceLength, int maxExclusive = -1)
        {
            var list = new List<int>();
            for (int cnt = 0; cnt < sequenceLength; cnt++) list.Add(NextInt(maxExclusive));

            return list.ToArray();
        }

        public string NextAlphaString(int length)
        => NextSequence(length, 26)
            .Select(_r => CharacterMap[_r])
            .JoinUsing("");

        public string NextAlphaNumericString(int length)
        => NextSequence(length, 36)
            .Select(_r => CharacterMap[_r])
            .JoinUsing("");

        public char NextChar() => CharacterMap[NextInt() % 26];

        public V NextValue<V>(V[] values) => values[NextInt() % values.Length];

        public char NextChar(string values) => NextValue(values.ToCharArray());

        public bool NextBool() => NextInt() % 2 == 0;
    }
}
