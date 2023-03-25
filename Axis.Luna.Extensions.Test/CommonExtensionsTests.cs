using BenchmarkDotNet.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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

        [TestMethod]
        public void As_WhenValueIsNull_ShouldReturnDefault()
        {
            object value = null;

            Assert.AreEqual(default, value.As<int>());
            Assert.AreEqual(default, value.As<string>());
        }

        [TestMethod]
        public void As_WhenReturnTypeIsInterface()
        {
            object value = new List<int>();

            Assert.IsNotNull(value.As<IEnumerable<int>>());
            Assert.IsNull(value.As<IEnumerable<double>>());
        }

        [TestMethod]
        public void As_WhenReturnTypeIsSuperClass()
        {
            object value = new CustomList<int>();

            Assert.IsNotNull(value.As<List<int>>());
            Assert.IsNull(value.As<List<double>>());
        }

        [TestMethod]
        public void As_WhenConvertible()
        {
            object value = 54;

            Assert.AreEqual(54L, value.As<long>());
            Assert.AreEqual(default, value.As<Unit>());
        }

        [TestMethod]
        public void As_WhenExplicitConverterExists()
        {
            var value = 554;

            Assert.AreEqual(new Rune(554), value.As<Rune>());
        }

        [TestMethod]
        public void As_WhenImplicitConverterExists()
        {
            var value = 39;

            Assert.AreEqual(new Index(39), value.As<Index>());
        }

        [TestMethod]
        public void As_WhenReturnTypeIsValueType()
        {
            var value = 39;

            Assert.AreEqual(39, value.As<int>());
        }
    }

    [TestClass]
    public class CommonExtensionsPerfTests
    {
        /*
          System.ArraySegment`1
    System.DateTimeOffset
    System.Decimal
    System.Runtime.InteropServices.GCHandle
    System.Half
    System.Runtime.InteropServices.HandleRef
    System.Index
    System.IntPtr
    System.Memory`1
    System.Runtime.InteropServices.NFloat
    System.Nullable`1
    System.ReadOnlyMemory`1
    System.ReadOnlySpan`1
    System.Text.Rune
    System.Span`1
    System.Buffers.StandardFormat
    System.String
    System.UIntPtr
    System.Numerics.Vector`1
         */
    }

    public class CustomList<T> : List<T> { }

    public struct Unit { }
}
