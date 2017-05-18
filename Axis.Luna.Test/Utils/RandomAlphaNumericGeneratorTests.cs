using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Axis.Luna.Test.Utils
{
    [TestClass]
    public class RandomAlphaNumericGeneratorTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var random = RandomAlphaNumericGenerator.RandomAlpha(10);
            Assert.AreEqual(10, random.Length);
        }
    }
}
