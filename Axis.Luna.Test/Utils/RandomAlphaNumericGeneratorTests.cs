using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Axis.Luna.Test.Utils
{
    [TestClass]
    public class RandomAlphaNumericGeneratorTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            string random = null;

            //teset alphabet generation
            for(int cnt=0;cnt<1000;cnt++)
            {
                random = RandomAlphaNumericGenerator.RandomAlpha(200);
                Assert.AreEqual(200, random.Length);
                Assert.IsTrue(random.All(_char => char.IsLetter(_char)));
            }

            //test numeric generation
            for (int cnt = 0; cnt < 1000; cnt++)
            {
                random = RandomAlphaNumericGenerator.RandomNumeric(200);
                Assert.AreEqual(200, random.Length);
                Assert.IsTrue(random.All(_char => char.IsDigit(_char)));
            }

            //test alphanum generation
            for (int cnt = 0; cnt < 1000; cnt++)
            {
                random = RandomAlphaNumericGenerator.RandomAlphaNumeric(200);
                Assert.AreEqual(200, random.Length);
                Assert.IsTrue(random.All(_char => char.IsLetterOrDigit(_char)));
            }
        }
    }
}
