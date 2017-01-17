using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Axis.Luna.Extensions.ExceptionExtensions;

namespace Axis.Luna.Test
{
    [TestClass]
    public class ExceptionExtensionsUnitTest
    {
        [TestMethod]
        public void ThrowNullArgumentTest()
        {
            var x = new SampleClass("");

        }
    }



    public class SampleClass
    {
        public SampleClass(object someParam)
        {
            ThrowNullArguments(() => someParam);
        }
    }
}
