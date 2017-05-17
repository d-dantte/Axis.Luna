using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Axis.Luna.Extensions.ExceptionExtensions;

namespace Axis.Luna.Test.Extensions
{
    [TestClass]
    public class ExceptionExtensionTests
    {
        [TestMethod]
        public void ThrowNullReferenceTest()
        {
            var x = new object();
            var y = new object();
            string z = null;

            try
            {
                ThrowNullArguments(() => x);
                ThrowNullArguments(() => x, () => y);

                ThrowNullArguments(() => z);
            }
            catch(Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(ArgumentNullException));
                Assert.AreEqual((e as ArgumentNullException).ParamName, "z");
            }
        }        
    }
}
