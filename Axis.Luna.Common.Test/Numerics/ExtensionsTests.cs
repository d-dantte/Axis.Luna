using Axis.Luna.Common.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;

namespace Axis.Luna.Common.Test.Numerics
{
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void DigitCount_Tests()
        {
            for(int cnt = 0; cnt < 5000; cnt++)
            {
                var testCase = cnt switch
                {
                    0 => (value: BigInteger.Zero, digitCount: 1),
                    _ => (value: BigInteger.Parse(new string('9', cnt)), digitCount: cnt)
                };

                var digitCount = testCase.value.DigitCount();

                if (testCase.digitCount != digitCount)
                    Console.WriteLine($"Failed at: {testCase.value}");

                Assert.AreEqual(testCase.digitCount, digitCount);
            }
        }
    }
}
