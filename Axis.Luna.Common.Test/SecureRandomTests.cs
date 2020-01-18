
using Axis.Luna.Common.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Axis.Luna.Common.Test
{
    [TestClass]
    public class SecureRandomTests
    {
        [TestMethod]
        public void TestMethod()
        {
            var sb = new StringBuilder();
            using (var sr = new SecureRandom())
            {
                for (int cnt = 0; cnt < 14; cnt++)
                    sb.Append(sr.NextChar("abcdefghijklmnopqrstuvwqyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!£$%^&*()_+=-#@?"));
            }
            Console.WriteLine(sb);
        }


        [TestMethod]
        public void IntegerDividePerformanceTest()
        {
            var value = 67;
            var start = DateTime.Now;
            int r = 0;
            for (int cnt = 0; cnt < 1000000; cnt++)
            {
                r = value / 10;
            }

            Console.WriteLine($"Division performance for (1M cycles): {DateTime.Now - start}");
            Console.WriteLine(r);

            r = 0;
            for (int cnt = 0; cnt < 1000000; cnt++)
            {
                r = int.Parse(value.ToString()[0].ToString());
            }

            Console.WriteLine($"String parsing performance for (1M cycles): {DateTime.Now - start}");
            Console.WriteLine(r);
        }
    }
}
