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

        private RNGCryptoServiceProvider cryptoProvider = new RNGCryptoServiceProvider();

        public void Dispose() => cryptoProvider.Dispose();

        public byte[] NextBytes(int length) => NextBytes(new byte[length]);

        public byte[] NextBytes(byte[] bytes)
        {
            cryptoProvider.GetBytes(bytes);
            return bytes;
        }

        public int NextInt(int maxExclusive = -1)
        {
            var intByte = new byte[4];
            cryptoProvider.GetBytes(intByte);
            var @int = Math.Abs(BitConverter.ToInt32(intByte, 0));
            if (maxExclusive > 0) return @int % maxExclusive;
            else return @int;
        }

        public int NextSignedInt()
        {
            var intByte = new byte[4];
            cryptoProvider.GetBytes(intByte);
            return BitConverter.ToInt32(intByte, 0);
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
    }
}
