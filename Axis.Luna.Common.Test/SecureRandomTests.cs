
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
                for (int cnt = 0; cnt < 10; cnt++)
                    sb.Append(sr.NextChar("abcdefghijklmnopqrstuvwqyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!£$%^&*()_+=-#@?"));
            }
            Console.WriteLine(sb);
        }
    }
}
