using Axis.Luna.Extensions;
using System.Security.Cryptography;
using System.Text;

namespace Axis.Luna
{
    public static class RandomAlphaNumericGenerator
    {
        #region code map
        public static readonly char[] CodeMap = new char[]
        {
            'Q','w','D','c','s','K','Y','k','h','t','W','I','N','T','E','o','B','V','F','M','a','e','n','J','v','H',
            'L','b','C','P','x','i','d','y','u','U','S','f','m','R','q','G','j','z','O','X','g','Z','p','r','l','A',
            '8','6','3','9','4','2','0','1','7','5'
        };
        #endregion

        public static string RandomAlphaNumeric(int length)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var sb = new StringBuilder();
                for (int cnt = 0; cnt < length; cnt++)
                {
                    var randIndex = NextInt(rng, CodeMap.Length);
                    sb.Append(CodeMap[randIndex]);
                }
                return sb.ToString();
            }
        }
        public static string RandomAlpha(int length)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var sb = new StringBuilder();
                for (int cnt = 0; cnt < length; cnt++) sb.Append(CodeMap[NextInt(rng, 52)]);
                return sb.ToString();
            }
        }
        public static string RandomNumeric(int length)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var sb = new StringBuilder();
                for (int cnt = 0; cnt < length; cnt++) sb.Append(NextInt(rng, 10));
                return sb.ToString();
            }
        }

        private static int NextInt(RNGCryptoServiceProvider rng, int maxExclusive = int.MaxValue) => rng.RandomInt();
    }
}
