using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axis.Luna.Extensions.Test
{
    [TestClass]
    public class CommonExtensionsTests
    {
        [TestMethod]
        public void MiscTests()
        {
            var value = 10;
            var root = value.GetRoot(_v => _v - 1);

            Assert.AreEqual(1, root);
        }
    }
}
