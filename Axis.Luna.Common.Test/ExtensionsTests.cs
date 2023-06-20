using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Axis.Luna.Common.Test
{
	[TestClass]
	public class ExtensionsTests
	{
		[TestMethod]
		public void ToBitSequenceTests()
		{
			var zero = BigInteger.Zero;
			BitSequence bits = zero.ToBitSequence();
			Assert.AreEqual(0, bits.Length);

			var one = BigInteger.One;
			bits = one.ToBitSequence();
			Assert.AreEqual(1, bits.Length);

            var value = BigInteger.Parse("2");
            bits = value.ToBitSequence();
            Assert.AreEqual(2, bits.Length);

            value = BigInteger.Parse("4");
            bits = value.ToBitSequence();
            Assert.AreEqual(3, bits.Length);

            value = BigInteger.Parse("9");
            bits = value.ToBitSequence();
            Assert.AreEqual(4, bits.Length);

            value = BigInteger.Parse("19");
            bits = value.ToBitSequence();
            Assert.AreEqual(5, bits.Length);

            value = BigInteger.Parse("32");
            bits = value.ToBitSequence();
            Assert.AreEqual(6, bits.Length);

            value = BigInteger.Parse("100");
            bits = value.ToBitSequence();
            Assert.AreEqual(7, bits.Length);

            value = BigInteger.Parse("255");
            bits = value.ToBitSequence();
            Assert.AreEqual(8, bits.Length);

            value = BigInteger.Parse("256");
			bits = value.ToBitSequence();
			Assert.AreEqual(9, bits.Length);
		}
	}
}

