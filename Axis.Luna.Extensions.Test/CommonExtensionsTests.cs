using BenchmarkDotNet.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Axis.Luna.Extensions.Test
{
    [TestClass]
    public class CommonExtensionsUnitTests
    {
        [TestMethod]
        public void MiscTests()
        {
            var sarr = new string[] { null };
            var arr = sarr
            .HardCast<string, object>()
            .ToArray();

            sarr = null;
            Assert.ThrowsException<ArgumentNullException>(() => sarr
                .HardCast<string, object>()
                .ToArray());

            arr = sarr
                ?.HardCast<string, object>()
                .ToArray();
            Assert.IsNull(arr);
        }
    }

    [TestClass]
    public class CommonExtensionsPerfTests
    {

    }
}
