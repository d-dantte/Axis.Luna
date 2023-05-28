using Axis.Luna.Common.Numerics;
using Axis.Luna.Common.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;

namespace Axis.Luna.Common.Test.Numerics
{
    [TestClass]
    public class BigDecimalTests
    {
        [TestMethod]
        public void Division_Tests()
        {
            var numerator = new BigDecimal(10);
            var denominator = new BigDecimal(2);
            var result = numerator / denominator;
            Console.WriteLine(result);

            numerator = new BigDecimal(49);
            denominator = new BigDecimal(8);
            result = numerator / denominator;
            Console.WriteLine(result);

            numerator = new BigDecimal(BigInteger.Parse("456765678767876789765412345678901268989912334565"));
            denominator = new BigDecimal(8);
            result = numerator / denominator;
            Console.WriteLine(result);

            numerator = new BigDecimal(BigInteger.Parse("456765678767876789765412345678901268989912334565"));
            denominator = new BigDecimal(456787678765432890L);
            result = numerator / denominator;
            Console.WriteLine(result);

            numerator = new BigDecimal(BigInteger.Parse("456765678767876789765412345678901268989912334565"));
            denominator = new BigDecimal(456787678765432890L);
            result = BigDecimal.WithContext(
                new BigDecimal.FormatContext(10),
                () => numerator / denominator);
            Console.WriteLine(result);
        }

        [TestMethod]
        public void Parse_Tests()
        {
            var bigDecimal = BigDecimal.Parse("12345678").Resolve();
            Assert.AreEqual("[Mantissa: 12345678, Scale: 0]", bigDecimal.ToString());

            bigDecimal = BigDecimal.Parse("12345.678").Resolve();
            Assert.AreEqual("[Mantissa: 12345678, Scale: 3]", bigDecimal.ToString());

            bigDecimal = BigDecimal.Parse("0.00000012345678").Resolve();
            Assert.AreEqual("[Mantissa: 12345678, Scale: 14]", bigDecimal.ToString());

            bigDecimal = BigDecimal.Parse("1.2345678E-12").Resolve();
            Assert.AreEqual("[Mantissa: 12345678, Scale: 19]", bigDecimal.ToString());

            bigDecimal = BigDecimal.Parse("123456.78E-12").Resolve();
            Assert.AreEqual("[Mantissa: 12345678, Scale: 14]", bigDecimal.ToString());

            bigDecimal = BigDecimal.Parse("123456.78E7").Resolve();
            Assert.AreEqual("[Mantissa: 1234567800000, Scale: 0]", bigDecimal.ToString());

            bigDecimal = BigDecimal.Parse("[mantissa:12345678, SCALE: 6]").Resolve();
            Assert.AreEqual("[Mantissa: 12345678, Scale: 6]", bigDecimal.ToString());

            var result = BigDecimal.Parse("errorneous value");

        }

        [TestMethod]
        public void Modulus_Tests()
        {
            var first = new BigDecimal(34.667m);
            var second = new BigDecimal(6.31m);
            var modulus = first % second;
            Assert.AreEqual(new BigDecimal(3.117m), modulus);
        }

        [TestMethod]
        public void Rounding_Tests()
        {
            var bdecimal = new BigDecimal(45.995123m);

            var rounded = bdecimal.Round(0);
            Assert.AreEqual("[Mantissa: 46, Scale: 0]", rounded.ToString());

            rounded = bdecimal.Round(1);
            Assert.AreEqual("[Mantissa: 46, Scale: 0]", rounded.ToString());

            rounded = bdecimal.Round(2);
            Assert.AreEqual("[Mantissa: 46, Scale: 0]", rounded.ToString());

            rounded = bdecimal.Round(3);
            Assert.AreEqual("[Mantissa: 45995, Scale: 3]", rounded.ToString());

            rounded = bdecimal.Round(6);
            Assert.AreEqual("[Mantissa: 45995123, Scale: 6]", rounded.ToString());

            rounded = bdecimal.Round(12);
            Assert.AreEqual("[Mantissa: 45995123, Scale: 6]", rounded.ToString());

            bdecimal = new BigDecimal(0.999m);
            rounded = bdecimal.Round(1);
            Assert.AreEqual("[Mantissa: 1, Scale: 0]", rounded.ToString());

            bdecimal = new BigDecimal(0.00999m);
            rounded = bdecimal.Round(1);
            Assert.AreEqual("[Mantissa: 0, Scale: 0]", rounded.ToString());
        }

        [TestMethod]
        public void Floor_Tests()
        {
            var result = BigDecimal.Parse("123.456").Resolve().Floor();
            Assert.AreEqual("[Mantissa: 123, Scale: 0]", result.ToString());


            result = BigDecimal.Parse("0.456").Resolve().Floor();
            Assert.AreEqual("[Mantissa: 0, Scale: 0]", result.ToString());
        }

        [TestMethod]
        public void Fraction_Tests()
        {
            var result = BigDecimal.Parse("123.456").Resolve().Fraction();
            Assert.AreEqual("[Mantissa: 456, Scale: 3]", result.ToString());


            result = BigDecimal.Parse("0.00456").Resolve().Fraction();
            Assert.AreEqual("[Mantissa: 456, Scale: 5]", result.ToString());
        }

        [TestMethod]
        public void DigitAtDecimalPlace_Tests()
        {
            var result = BigDecimal.Parse("123.456").Resolve().DigitAtDecimalPlace(1);
            Assert.AreEqual((byte)4, result);

            result = BigDecimal.Parse("123.456").Resolve().DigitAtDecimalPlace(2);
            Assert.AreEqual((byte)5, result);

            result = BigDecimal.Parse("123.456").Resolve().DigitAtDecimalPlace(3);
            Assert.AreEqual((byte)6, result);

            result = BigDecimal.Parse("123.456").Resolve().DigitAtDecimalPlace(6);
            Assert.AreEqual((byte)0, result);

            result = BigDecimal.Parse("0.000123456").Resolve().DigitAtDecimalPlace(6);
            Assert.AreEqual((byte)3, result);

            result = BigDecimal.Parse("0.000123456").Resolve().DigitAtDecimalPlace(2);
            Assert.AreEqual((byte)0, result);
        }

        [TestMethod]
        public void Power_Tests()
        {
            var first = new BigDecimal(48);
            var second = new BigDecimal(5);
            var result = BigDecimal.Power(first, second);
            Assert.AreEqual(new BigDecimal(254803968), result);

            first = new BigDecimal(25);
            second = new BigDecimal(0.5);
            result = BigDecimal.Power(first, second);
            Assert.AreEqual(new BigDecimal(5), result);
        }

        [TestMethod]
        public void Decimal_Rounding_Tests()
        {
            var value = new BigDecimal(45.6);
            var d = (decimal)value;
            Assert.AreEqual(45.6m, d);

            value = new BigDecimal(45);
            d = (decimal)value;
            Assert.AreEqual(45m, d);
        }

        [TestMethod]
        public void Double_Rounding_Tests()
        {
            var value = new BigDecimal(45.6);
            var d = (double)value;
            Assert.AreEqual(45.6d, d);

            value = new BigDecimal(45);
            d = (double)value;
            Assert.AreEqual(45d, d);
        }

        [TestMethod]
        public void Multiply_Tests()
        {
            var value = BigDecimal.Parse("0.7631257631257631257631257631").Resolve();
            var value2 = new BigDecimal(2.5m);
            var result = value * value2;
            Assert.AreEqual(BigDecimal.Parse("1.90781440781440781440781440775").Resolve(), result);

            value = new BigDecimal(-45);
            result = value * value2;
            Assert.AreEqual(new BigDecimal(-112.5m), result);
        }

        [TestMethod]
        public void Modulo_Tests()
        {
            var value = BigDecimal.Parse("0.7631257631257631257631257631").Resolve();
            var value2 = new BigDecimal(2.5m);
            var result = value % value2;
            Assert.AreEqual(value, result);

            value = new BigDecimal(-45);
            result = value % value2;
            Assert.AreEqual(new BigDecimal(0), result);

            value = new BigDecimal(7);
            result = value % value2;
            Assert.AreEqual(new BigDecimal(2), result);
        }

        [TestMethod]
        public void Misc_Test_2()
        {
            var value = new BigDecimal(0m);
            Assert.AreEqual(BigDecimal.Zero, value);
        }


        [TestMethod]
        public void miscTest()
        {
            var balanced = BigDecimal.Balance(34.667m, 6.13m);
            Console.WriteLine(balanced);
        }
    }
}
