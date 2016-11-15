using System;
using System.Text;

namespace Axis.Luna
{
    public static class RandomAlphaNumericGenerator
    {
        #region code map
        public static readonly char[] CodeMap = new char[]
        {
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            '0','1','2','3','4','5','6','7','8','9'
        };
        #endregion

        public static string RandomAlphaNumeric(int length, Random r = null)
        {
            var random = r ?? new Random(Guid.NewGuid().GetHashCode());
            var sb = new StringBuilder();
            for (int cnt = 0; cnt < length; cnt++) sb.Append(CodeMap[random.Next(CodeMap.Length)]);
            return sb.ToString();
        }
        public static string RandomAlpha(int length, Random r = null)
        {
            var random = r ?? new Random(Guid.NewGuid().GetHashCode());
            var sb = new StringBuilder();
            for (int cnt = 0; cnt < length; cnt++) sb.Append(CodeMap[random.Next(52)]);
            return sb.ToString();
        }
        public static string RandomNumeric(int length, Random r = null)
        {
            var random = r ?? new Random(Guid.NewGuid().GetHashCode());
            var sb = new StringBuilder();
            for (int cnt = 0; cnt < length; cnt++) sb.Append(random.Next(10));
            return sb.ToString();
        }
    }
}
