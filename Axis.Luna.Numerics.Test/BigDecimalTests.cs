using Axis.Luna.Result;
using System.Collections.Immutable;
using System.Numerics;

namespace Axis.Luna.Numerics.Test
{
    [TestClass]
    public class BigDecimalTests
    {
        [TestMethod]
        public void CompareTo_WithBigDecimal_Tests()
        {
            var d1 = "0.000000000000000000000000000000000000000000000000012345678912345678900000000000000000000000000000000000000000001";
            var d2 = "12345678912345678912345678912345678900000000000000000000000000000000000000000000.000000000000000000000000000000000000000000000000000000000000000045";
            var d3 = "12345678912345678912345678912345678900000000000000000000000000000000000000000000.123";
            var d4 = "12345678912345678912345678912345678900000000000000000000000000000000000000000000";
            var nd1 = "-0.000000000000000000000000000000000000000000000000012345678912345678900000000000000000000000000000000000000000001";
            var nd2 = "-12345678912345678912345678912345678900000000000000000000000000000000000000000000.000000000000000000000000000000000000000000000000000000000000000045";
            var nd3 = "-12345678912345678912345678912345678900000000000000000000000000000000000000000000.123";
            var nd4 = "-12345678912345678912345678912345678900000000000000000000000000000000000000000000";

            // zero
            Assert.AreEqual(0, BigDecimal.Zero.CompareTo(BigDecimal.Zero));

            Assert.AreEqual(-1, BigDecimal.Zero.CompareTo(BigDecimal.One));
            Assert.AreEqual(-1, BigDecimal.Zero.CompareTo(new BigDecimal(0.00000000000001m)));
            Assert.AreEqual(-1, BigDecimal.Zero.CompareTo(new BigDecimal(d1)));

            Assert.AreEqual(1, BigDecimal.Zero.CompareTo(new BigDecimal(nd1)));
            Assert.AreEqual(1, BigDecimal.Zero.CompareTo(new BigDecimal(nd2)));
            Assert.AreEqual(1, BigDecimal.Zero.CompareTo(new BigDecimal(nd3)));
            Assert.AreEqual(1, BigDecimal.Zero.CompareTo(new BigDecimal(nd4)));

            // positive
            Assert.AreEqual(0, BigDecimal.One.CompareTo(BigDecimal.One));
            Assert.AreEqual(0, new BigDecimal(0.00000000000001m).CompareTo(new BigDecimal(0.00000000000001m)));
            Assert.AreEqual(0, new BigDecimal(d1).CompareTo(new BigDecimal(d1)));
            Assert.AreEqual(0, new BigDecimal(d2).CompareTo(new BigDecimal(d2)));
            Assert.AreEqual(0, new BigDecimal(d3).CompareTo(new BigDecimal(d3)));
            Assert.AreEqual(0, new BigDecimal(d4).CompareTo(new BigDecimal(d4)));

            Assert.AreEqual(-1, new BigDecimal(d2).CompareTo(new BigDecimal(d3)));
            Assert.AreEqual(-1, new BigDecimal(d4).CompareTo(new BigDecimal(d2)));
            Assert.AreEqual(-1, new BigDecimal(d4).CompareTo(new BigDecimal(d3)));
            Assert.AreEqual(-1, new BigDecimal(d1).CompareTo(new BigDecimal(d4)));

            Assert.AreEqual(1, new BigDecimal(d3).CompareTo(new BigDecimal(d2)));
            Assert.AreEqual(1, new BigDecimal(d2).CompareTo(new BigDecimal(d4)));
            Assert.AreEqual(1, new BigDecimal(d3).CompareTo(new BigDecimal(d4)));
            Assert.AreEqual(1, new BigDecimal(d4).CompareTo(new BigDecimal(d1)));

            // negative
            Assert.AreEqual(0, BigDecimal.NegativeOne.CompareTo(BigDecimal.NegativeOne));
            Assert.AreEqual(0, new BigDecimal(-0.00000000000001m).CompareTo(new BigDecimal(-0.00000000000001m)));
            Assert.AreEqual(0, new BigDecimal(nd1).CompareTo(new BigDecimal(nd1)));
            Assert.AreEqual(0, new BigDecimal(nd2).CompareTo(new BigDecimal(nd2)));
            Assert.AreEqual(0, new BigDecimal(nd3).CompareTo(new BigDecimal(nd3)));
            Assert.AreEqual(0, new BigDecimal(nd4).CompareTo(new BigDecimal(nd4)));

            Assert.AreEqual(1, new BigDecimal(nd2).CompareTo(new BigDecimal(nd3)));
            Assert.AreEqual(1, new BigDecimal(nd4).CompareTo(new BigDecimal(nd2)));
            Assert.AreEqual(1, new BigDecimal(nd4).CompareTo(new BigDecimal(nd3)));
            Assert.AreEqual(1, new BigDecimal(nd1).CompareTo(new BigDecimal(nd4)));

            Assert.AreEqual(-1, new BigDecimal(nd3).CompareTo(new BigDecimal(nd2)));
            Assert.AreEqual(-1, new BigDecimal(nd2).CompareTo(new BigDecimal(nd4)));
            Assert.AreEqual(-1, new BigDecimal(nd3).CompareTo(new BigDecimal(nd4)));
            Assert.AreEqual(-1, new BigDecimal(nd4).CompareTo(new BigDecimal(nd1)));
        }

        [TestMethod]
        public void CompareTo_WithObject_Tests()
        {
            var b10 = new BigDecimal(10);
            Assert.AreEqual(0, b10.CompareTo((object)(byte)10));
            Assert.AreEqual(0, b10.CompareTo((object)(sbyte)10));
            Assert.AreEqual(0, b10.CompareTo((object)(short)10));
            Assert.AreEqual(0, b10.CompareTo((object)(ushort)10));
            Assert.AreEqual(0, b10.CompareTo((object)(int)10));
            Assert.AreEqual(0, b10.CompareTo((object)(uint)10));
            Assert.AreEqual(0, b10.CompareTo((object)(long)10));
            Assert.AreEqual(0, b10.CompareTo((object)(ulong)10));
            Assert.AreEqual(0, b10.CompareTo((object)(Half)10));
            Assert.AreEqual(0, b10.CompareTo((object)(float)10));
            Assert.AreEqual(0, b10.CompareTo((object)(double)10));
            Assert.AreEqual(0, b10.CompareTo((object)(decimal)10));
            Assert.AreEqual(0, b10.CompareTo((object)(BigInteger)10));
            Assert.AreEqual(0, b10.CompareTo((object)(BigDecimal)10));
            _ = Assert.ThrowsException<ArgumentNullException>(() => b10.CompareTo(null));
            _ = Assert.ThrowsException<ArgumentException>(() => b10.CompareTo(new object()));
        }


        [TestMethod]
        public void Equals_WithBigDecial()
        {
            Assert.IsTrue(BigDecimal.One.Equals(BigDecimal.One));
        }

        [TestMethod]
        public void IsEvenInteger_Tests()
        {
            Assert.IsTrue(BigDecimal.IsEvenInteger(0));
            Assert.IsTrue(BigDecimal.IsEvenInteger(10));
            Assert.IsTrue(BigDecimal.IsEvenInteger(22));
            Assert.IsTrue(BigDecimal.IsEvenInteger(20));

            Assert.IsFalse(BigDecimal.IsEvenInteger(0.4m));
            Assert.IsFalse(BigDecimal.IsEvenInteger(3));
        }

        [TestMethod]
        public void IsOddInteger_Tests()
        {
            Assert.IsFalse(BigDecimal.IsOddInteger(0.4m));
            Assert.IsFalse(BigDecimal.IsOddInteger(0));
            Assert.IsFalse(BigDecimal.IsOddInteger(10));
            Assert.IsFalse(BigDecimal.IsOddInteger(22));
            Assert.IsFalse(BigDecimal.IsOddInteger(20));

            Assert.IsTrue(BigDecimal.IsOddInteger(3));
            Assert.IsTrue(BigDecimal.IsOddInteger(33));
            Assert.IsTrue(BigDecimal.IsOddInteger(new BigDecimal("10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000003")));
        }

        [TestMethod]
        public void Add_Tests()
        {
            var left = "1000000000000000000000000000000000000000000000000000000000000000000000000000000000000123.1";
            var right = "0.100000000000000000000000000000000000000000000000000000000000000123";
            var result = "1000000000000000000000000000000000000000000000000000000000000000000000000000000000000123.200000000000000000000000000000000000000000000000000000000000000123";
            Assert.AreEqual(new BigDecimal(result), BigDecimal.Add(new BigDecimal(left), new BigDecimal(right)));
            Assert.AreEqual(new BigDecimal(result), BigDecimal.Add(new BigDecimal(right), new BigDecimal(left)));

            var bdresult = BigDecimal.Add(0, 1);
            Assert.AreEqual(1, bdresult);
            
            bdresult = BigDecimal.Add(1, 0);
            Assert.AreEqual(1, bdresult);
        }

        [TestMethod]
        public void Subtract_Tests()
        {
            var left = "1000000000000000000000000000000000000000000000000000000000000000000000000000000000000123.1";
            var right = "0.100000000000000000000000000000000000000000000000000000000000000123";
            var result1 = "1000000000000000000000000000000000000000000000000000000000000000000000000000000000000122.999999999999999999999999999999999999999999999999999999999999999877";
            var result2 = "-1.000000000000000000000000000000000000000000000000000000000000000000000000000000000000122999999999999999999999999999999999999999999999999999999999999999877E87";
            Assert.AreEqual(new BigDecimal(result1), BigDecimal.Subtract(new BigDecimal(left), new BigDecimal(right)));
            Assert.AreEqual(new BigDecimal(result2), BigDecimal.Subtract(new BigDecimal(right), new BigDecimal(left)));

            var bdresult = BigDecimal.Subtract(0, 1);
            Assert.AreEqual(-1, bdresult);

            bdresult = BigDecimal.Subtract(1, 0);
            Assert.AreEqual(1, bdresult);
        }

        [TestMethod]
        public void Multiply_Tests()
        {
            var left = "1000000000000000000000000000000000000000000000000000000000000000000000000000000000000123.1";
            var right = "0.100000000000000000000000000000000000000000000000000000000000000123";
            var result = "100000000000000000000000000000000000000000000000000000000000000123000000000000000000012.3100000000000000000000000000000000000000000000000000000000000151413";

            Assert.AreEqual(BigDecimal.Zero, BigDecimal.Multiply(BigDecimal.Zero, new BigDecimal(right)));
            Assert.AreEqual(BigDecimal.Zero, BigDecimal.Multiply(new BigDecimal(left), BigDecimal.Zero));
            Assert.AreEqual(new BigDecimal(right), BigDecimal.Multiply(BigDecimal.One, new BigDecimal(right)));
            Assert.AreEqual(new BigDecimal(left), BigDecimal.Multiply(new BigDecimal(left), BigDecimal.One));
            Assert.AreEqual(new BigDecimal(result), BigDecimal.Multiply(new BigDecimal(left), new BigDecimal(right)));
            Assert.AreEqual(new BigDecimal(result), BigDecimal.Multiply(new BigDecimal(right), new BigDecimal(left)));

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => BigDecimal.Divide(1, 3, -3));
        }

        [TestMethod]
        public void Divive_Tests()
        {
            var left = "1000000000000000000000000000000000000000000000000000000000000000000000000000000000000123.1";
            var right = "0.100000000000000000000000000000000000000000000000000000000000000123";
            var result1 = "9999999999999999999999999999999999999999999999999999999999999987700000000000000000001231.0000000000000000000000000000000000000151289999999999999999984858";
            var result2 = "1.000000000000000000000000000000000000000000000000000000000000001229999999999999999999876899999999999999999999999999999999999999999999E-88";

            Assert.AreEqual(BigDecimal.Zero, BigDecimal.Divide(BigDecimal.Zero, new BigDecimal(right)));
            Assert.ThrowsException<DivideByZeroException>(() => BigDecimal.Divide(new BigDecimal(left), BigDecimal.Zero));
            Assert.AreEqual(new BigDecimal(left), BigDecimal.Divide(new BigDecimal(left), BigDecimal.One));
            Assert.AreEqual(new BigDecimal("0.25"), BigDecimal.Divide(new BigDecimal("1.5"), new BigDecimal("6")));
            Assert.AreEqual(new BigDecimal("0.5"), BigDecimal.Divide(BigDecimal.One, new BigDecimal("2")));
            Assert.AreEqual(new BigDecimal(result1), BigDecimal.Divide(new BigDecimal(left), new BigDecimal(right)));
            Assert.AreEqual(new BigDecimal(result2), BigDecimal.Divide(new BigDecimal(right), new BigDecimal(left), 220));
        }

        [TestMethod]
        public void MaxMagnitude_Tests()
        {
            var d1 = new BigDecimal("100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001");
            var d2 = new BigDecimal("100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002");
            var nd1 = new BigDecimal("-100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001");
            var nd2 = new BigDecimal("-100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002");

            Assert.AreEqual(d2, BigDecimal.MaxMagnitude(d1, d2));
            Assert.AreEqual(d2, BigDecimal.MaxMagnitude(d2, d1));
            Assert.AreEqual(nd2, BigDecimal.MaxMagnitude(nd1, nd2));
            Assert.AreEqual(nd2, BigDecimal.MaxMagnitude(nd2, nd1));
            Assert.AreEqual(nd2, BigDecimal.MaxMagnitude(d1, nd2));
            Assert.AreEqual(nd2, BigDecimal.MaxMagnitude(nd2, d1));
        }


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
        public void Modulus_Tests()
        {
            var result = BigDecimal.Modulus(0, 34);
            Assert.AreEqual(BigDecimal.Zero, result);

            Assert.ThrowsException<DivideByZeroException>(() => BigDecimal.Modulus(34, 0));

            result = BigDecimal.Modulus(34, 1);
            Assert.AreEqual(new BigDecimal(34), result);

            result = BigDecimal.Modulus(8, 3);
            Assert.AreEqual(2, result);

            result = BigDecimal.Modulus(-9, 6);
            Assert.AreEqual(-3, result);

            result = BigDecimal.Modulus(0.8m, 7);
            Assert.AreEqual(0.8m, result);

            result = BigDecimal.Modulus(7, -0.8m);
            Assert.AreEqual(0.6m, result);
        }


        [TestMethod]
        public void Rounding_Tests()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => BigDecimal.One.Round(-2));

            var bdecimal = new BigDecimal(45.995123m);

            var rounded = bdecimal.Round(0);
            Assert.AreEqual(new BigDecimal(46), rounded);

            rounded = bdecimal.Round(1);
            Assert.AreEqual(new BigDecimal(46), rounded);

            rounded = bdecimal.Round(2);
            Assert.AreEqual(new BigDecimal(46), rounded);

            rounded = bdecimal.Round(3);
            Assert.AreEqual(new BigDecimal(45.995m), rounded);

            rounded = bdecimal.Round(6);
            Assert.AreEqual(new BigDecimal(45.995123m), rounded);

            rounded = bdecimal.Round(12);
            Assert.AreEqual(new BigDecimal(45.995123m), rounded);

            bdecimal = new BigDecimal(0.999m);
            rounded = bdecimal.Round(1);
            Assert.AreEqual(new BigDecimal(1), rounded);

            bdecimal = new BigDecimal(0.00999m);
            rounded = bdecimal.Round(1);
            Assert.AreEqual(new BigDecimal(0), rounded);

            bdecimal = new BigDecimal(0.00999m);
            rounded = bdecimal.Round(2);
            Assert.AreEqual(new BigDecimal(0.01m), rounded);

            bdecimal = new BigDecimal(-0.00999m);
            rounded = bdecimal.Round(2);
            Assert.AreEqual(new BigDecimal(-0.01m), rounded);

            rounded = BigDecimal.One.Round(2);
            Assert.AreEqual(BigDecimal.One, rounded);
        }

        [TestMethod]
        public void Floor_Tests()
        {
            var result = BigDecimal.Parse("123.456").Resolve().Floor();
            Assert.AreEqual(new BigDecimal(123), result);

            result = BigDecimal.Parse("0.456").Resolve().Floor();
            Assert.AreEqual(BigDecimal.Zero, result);
        }

        [TestMethod]
        public void Fraction_Tests()
        {
            var result = BigDecimal.Parse("123.456").Resolve().FractionalPart();
            Assert.AreEqual(new BigDecimal(0.456m), result);


            result = BigDecimal.Parse("0.00456").Resolve().FractionalPart();
            Assert.AreEqual(new BigDecimal(0.00456m), result);
        }

        [TestMethod]
        public void Power_Tests()
        {
            var first = new BigDecimal(48);
            var second = 5;
            var result = BigDecimal.Power(first, second);
            Assert.AreEqual(new BigDecimal(254803968), result);
        }

        [TestMethod]
        public void Decimal_Rounding_Tests()
        {
            var value = new BigDecimal(45.6m);
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
        public void Multiply_Tests2()
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
        public void StringTests()
        {
            var value = BigDecimal
                .Parse("0.0005326")
                .Resolve();

            var ss = value.ToScientificString();
            var ns = value.ToNonScientificString();

            Assert.AreEqual("5.326E-4", ss);
            Assert.AreEqual("0.0005326", ns);

            // this evinces a problem in the parsing algorithm
            value = BigDecimal
                .Parse("-123456.0e-42")
                .Resolve();
            Assert.AreEqual("-1.23456E-37", value.ToScientificString());
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


        [TestMethod]
        public void DeconstructDoubleTests()
        {
            var x = BigDecimal.Deconstruct(12d);
            Assert.AreEqual(new BigInteger(12), x.value);
            Assert.AreEqual(0, x.scale);
        }

        [TestMethod]
        public void Constructor_Tests()
        {
            var bd = new BigDecimal((new BigInteger(234543000), 5));
            bd = new BigDecimal((new BigInteger(23454300), -3));
        }

        [TestMethod]
        public void DecimalShiftTest()
        {
            var result = BigDecimal.DecimalShift(123456789, 0);
            Assert.AreEqual(new BigInteger(123456789), result);

            result = BigDecimal.DecimalShift(123456789, 1);
            Assert.AreEqual(new BigInteger(1234567890), result);

            result = BigDecimal.DecimalShift(123456789, 5);
            Assert.AreEqual(new BigInteger(12345678900000m), result);

            result = BigDecimal.DecimalShift(123456789, -1);
            Assert.AreEqual(new BigInteger(12345678), result);

            result = BigDecimal.DecimalShift(123456789, -6);
            Assert.AreEqual(new BigInteger(123), result);

            result = BigDecimal.DecimalShift(123456789, -9);
            Assert.AreEqual(new BigInteger(0), result);

            result = BigDecimal.DecimalShift(123456789, -12);
            Assert.AreEqual(new BigInteger(), result);
        }

        [TestMethod]
        public void DenormalizeTest()
        {
            var result = BigDecimal.Denormalize(123456789);
            Assert.AreEqual((new BigInteger(123456789), 0), result);

            result = BigDecimal.Denormalize(1234567890);
            Assert.AreEqual((new BigInteger(1234567890), 0), result);

            result = BigDecimal.Denormalize(12345678900000);
            Assert.AreEqual((new BigInteger(12345678900000m), 0), result);

            result = BigDecimal.Denormalize(12345678.9m);
            Assert.AreEqual((new BigInteger(123456789), 1), result);

            result = BigDecimal.Denormalize(123.456789m);
            Assert.AreEqual((new BigInteger(123456789), 6), result);

            result = BigDecimal.Denormalize(0.123456789m);
            Assert.AreEqual((new BigInteger(123456789), 9), result);

            result = BigDecimal.Denormalize(0.000123456789m);
            Assert.AreEqual((new BigInteger(123456789), 12), result);
        }

        [TestMethod]
        public void Balance_Tests()
        {
            var balanced = BigDecimal.Balance(
                new BigDecimal(8),
                new BigDecimal(12.3m));

            Console.WriteLine(balanced);

            balanced = BigDecimal.Balance(
                new BigDecimal(8.9m),
                new BigDecimal(12.34m));
            Console.WriteLine(balanced);

            balanced = BigDecimal.Balance(
                new BigDecimal(8.9m),
                new BigDecimal(123000m));
            Console.WriteLine(balanced);

            balanced = BigDecimal.Balance(
                new BigDecimal(89123m),
                new BigDecimal(1.23m));
            Console.WriteLine(balanced);
        }

        [TestMethod]
        public void Miscs__()
        {
            Console.WriteLine(BigDecimal.ParseScientificNotation("-543222.1E-12"));
            Console.WriteLine(BigDecimal.ParseScientificNotation("-543222.1E12"));
            Console.WriteLine(BigDecimal.ParseScientificNotation("-0.123456E8"));
            Console.WriteLine(BigDecimal.ParseScientificNotation("000.00000123456E14"));
            Console.WriteLine(BigDecimal.ParseScientificNotation("-0.123456E0"));
            Console.WriteLine(BigDecimal.ParseScientificNotation("0E0"));
            Console.WriteLine(BigDecimal.ParseScientificNotation("0.0000E000"));
            Console.WriteLine(BigDecimal.ParseScientificNotation("-0.0000E-000"));
        }

        [TestMethod]
        public void Divide_Tests()
        {
            var numerator = new BigDecimal(8);
            var denominator = new BigDecimal(12.3m);

            var result = BigDecimal.Divide(numerator, denominator, 8);
            Console.WriteLine(result);
            Assert.AreEqual(new BigDecimal(0.65040650m), result);

            numerator = new BigDecimal(8);
            denominator = new BigDecimal(123m);

            result = BigDecimal.Divide(numerator, denominator, 8);
            Console.WriteLine(result);
            Assert.AreEqual(new BigDecimal(0.065040650m), result);

            numerator = new BigDecimal(800);
            denominator = new BigDecimal(123m);

            result = BigDecimal.Divide(numerator, denominator, 10);
            Console.WriteLine(result);
            Assert.AreEqual(new BigDecimal(6.5040650406m), result);
        }

        [TestMethod]
        public void ToScientificStringTests()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => BigDecimal.Zero.ToScientificString(-3));
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => BigDecimal.Zero.ToScientificString(0));

            var bd = new BigDecimal(0m);
            Assert.AreEqual("0.0E0", bd.ToScientificString());

            bd = new BigDecimal(10m);
            Assert.AreEqual("1.0E1", bd.ToScientificString());
            Assert.AreEqual("1.0E1", bd.ToScientificString(10));

            bd = new BigDecimal(110m);
            Assert.AreEqual("1.1E2", bd.ToScientificString());

            bd = new BigDecimal(23450m);
            Assert.AreEqual("2.345E4", bd.ToScientificString());
            Assert.AreEqual("2.345E4", bd.ToScientificString(10));

            bd = new BigDecimal(0.0000000234566639118m);
            Assert.AreEqual("2.34566639118E-8", bd.ToScientificString());
            Assert.AreEqual("2.34566639118E-8", bd.ToScientificString(20));

            bd = new BigDecimal(23.450m);
            Assert.AreEqual("2.345E1", bd.ToScientificString());
            Assert.AreEqual("2.345E1", bd.ToScientificString(10));

            bd = new BigDecimal(-2.3450m);
            Assert.AreEqual("-2.345E0", bd.ToScientificString());
            Assert.AreEqual("-2.35E0", bd.ToScientificString(2));

            bd = new BigDecimal(12.999m);
            Assert.AreEqual("1.30E1", bd.ToScientificString(2));
        }

        [TestMethod]
        public void ToNonScientificStringTests()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => BigDecimal.Zero.ToNonScientificString(-3));

            var bd = new BigDecimal(0m);
            Assert.AreEqual("0.0", bd.ToNonScientificString());
            Assert.AreEqual("0.0", bd.ToNonScientificString(0));

            bd = new BigDecimal(1);
            Assert.AreEqual("1.0", bd.ToNonScientificString());
            Assert.AreEqual("1.0", bd.ToNonScientificString(0));

            bd = new BigDecimal(10m);
            Assert.AreEqual("10.0", bd.ToNonScientificString());
            Assert.AreEqual("10.0000000000", bd.ToNonScientificString(10));

            bd = new BigDecimal(14m);
            Assert.AreEqual("14.0", bd.ToNonScientificString());
            Assert.AreEqual("14.0000000000", bd.ToNonScientificString(10));

            bd = new BigDecimal(23450m);
            Assert.AreEqual("23450.0", bd.ToNonScientificString());
            Assert.AreEqual("23450.0000000000", bd.ToNonScientificString(10));

            bd = new BigDecimal(0.0000000234566639118m);
            Assert.AreEqual("0.0000000234566639118", bd.ToNonScientificString());
            Assert.AreEqual("0.0000000234567", bd.ToNonScientificString(13));

            bd = new BigDecimal(23.450m);
            Assert.AreEqual("23.45", bd.ToNonScientificString());
            Assert.AreEqual("23.4500000000", bd.ToNonScientificString(10));

            bd = new BigDecimal(-2.3450m);
            Assert.AreEqual("-2.345", bd.ToNonScientificString());
            Assert.AreEqual("-2.35", bd.ToNonScientificString(2));
        }

        [TestMethod]
        public void PowerTest()
        {
            var bd = new BigDecimal(1.2m);
            var r = BigDecimal.Power(bd, 3);
            Console.WriteLine(r);
        }

        [TestMethod]
        public void DemotionTests()
        {
            var bd = new BigDecimal(4554.3109m);
            var d = bd.DemoteToDecimal();
            Assert.AreEqual(4554.3109m, d);

            bd = new BigDecimal(-4554.3109m);
            d = bd.DemoteToDecimal();
            Assert.AreEqual(-4554.3109m, d);
        }

        [TestMethod]
        public void EqualsObject_Tests()
        {
            var bd = new BigDecimal(43.5m);
            var result = bd.Equals("43.5m");
            Assert.IsFalse(result);

            result = bd.Equals(null!);
            Assert.IsFalse(result);

            result = bd.Equals((object)bd);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EqualsInstance_Tests()
        {
            var bd = new BigDecimal(43.5m);
            var bd2 = new BigDecimal(43.6m);
            var bd3 = new BigDecimal(43.06m);

            var result = bd.Equals(bd);
            Assert.IsTrue(result);

            result = bd.Equals(bd2);
            Assert.IsFalse(result);

            result = bd.Equals(bd3);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsDefault_Test()
        {
            var bd = default(BigDecimal);
            var bd2 = BigDecimal.Zero;
            var bd3 = new BigDecimal(0m);
            var bd4 = BigDecimal.Default;

            Assert.IsTrue(bd.IsDefault);
            Assert.IsTrue(bd2.IsDefault);
            Assert.IsTrue(bd3.IsDefault);
            Assert.IsTrue(bd4.IsDefault);

            bd = 1;
            Assert.IsFalse(bd.IsDefault);
            bd = 10000;
            Assert.IsFalse(bd.IsDefault);
        }

        [TestMethod]
        public void MaxMAgnitude_WithBigDecimal_Test()
        {
            BigDecimal bd = 3456m;
            BigDecimal bd2 = 3457m;
            BigDecimal bd3 = -3457m;

            var result = BigDecimal.MaxMagnitudeNumber(bd, bd2);
            Assert.AreEqual(bd2, result);

            result = BigDecimal.MaxMagnitudeNumber(bd, bd3);
            Assert.AreEqual(bd3, result);

            result = BigDecimal.MaxMagnitudeNumber(bd3, bd3);
            Assert.AreEqual(bd3, result);

            result = BigDecimal.MaxMagnitudeNumber(bd2, bd3);
            Assert.AreEqual(bd2, result);

            result = BigDecimal.MaxMagnitudeNumber(bd3, bd2);
            Assert.AreEqual(bd2, result);
        }

        [TestMethod]
        public void MinMAgnitude_WithBigDecimal_Test()
        {
            BigDecimal bd = 3456m;
            BigDecimal bd2 = 3457m;
            BigDecimal bd3 = -3457m;

            var result = BigDecimal.MinMagnitudeNumber(bd, bd2);
            Assert.AreEqual(bd, result);

            result = BigDecimal.MinMagnitudeNumber(bd, bd3);
            Assert.AreEqual(bd, result);

            result = BigDecimal.MinMagnitudeNumber(bd3, bd3);
            Assert.AreEqual(bd3, result);

            result = BigDecimal.MinMagnitudeNumber(bd2, bd3);
            Assert.AreEqual(bd3, result);

            result = BigDecimal.MinMagnitudeNumber(bd3, bd2);
            Assert.AreEqual(bd3, result);

            result = BigDecimal.MinMagnitudeNumber(bd2, bd);
            Assert.AreEqual(bd, result);
        }

        [TestMethod]
        public void TryConvertFrom_Tests()
        {
            var converted = BigDecimal.TryConvertFrom<byte>(4, out var bd);
            Assert.IsTrue(converted);
            Assert.AreEqual(new BigDecimal(4), bd);

            converted = BigDecimal.TryConvertFrom<sbyte>(4, out bd);
            Assert.IsTrue(converted);
            Assert.AreEqual(new BigDecimal(4), bd);

            converted = BigDecimal.TryConvertFrom<char>('4', out bd);
            Assert.IsTrue(converted);
            Assert.AreEqual(new BigDecimal(52), bd);

            converted = BigDecimal.TryConvertFrom<short>(4, out bd);
            Assert.IsTrue(converted);
            Assert.AreEqual(new BigDecimal(4), bd);

            converted = BigDecimal.TryConvertFrom<ushort>(4, out bd);
            Assert.IsTrue(converted);
            Assert.AreEqual(new BigDecimal(4), bd);

            converted = BigDecimal.TryConvertFrom<int>(4, out bd);
            Assert.IsTrue(converted);
            Assert.AreEqual(new BigDecimal(4), bd);

            converted = BigDecimal.TryConvertFrom<uint>(4, out bd);
            Assert.IsTrue(converted);
            Assert.AreEqual(new BigDecimal(4), bd);

            converted = BigDecimal.TryConvertFrom<long>(4, out bd);
            Assert.IsTrue(converted);
            Assert.AreEqual(new BigDecimal(4), bd);

            converted = BigDecimal.TryConvertFrom<ulong>(4, out bd);
            Assert.IsTrue(converted);
            Assert.AreEqual(new BigDecimal(4), bd);

            converted = BigDecimal.TryConvertFrom<Half>((byte)4, out bd);
            Assert.IsTrue(converted);
            Assert.AreEqual(new BigDecimal(4), bd);

            converted = BigDecimal.TryConvertFrom<float>(4, out bd);
            Assert.IsTrue(converted);
            Assert.AreEqual(new BigDecimal(4), bd);

            converted = BigDecimal.TryConvertFrom<double>(4, out bd);
            Assert.IsTrue(converted);
            Assert.AreEqual(new BigDecimal(4), bd);

            converted = BigDecimal.TryConvertFrom<decimal>(4, out bd);
            Assert.IsTrue(converted);
            Assert.AreEqual(new BigDecimal(4), bd);

            converted = BigDecimal.TryConvertFrom("string", out bd);
            Assert.IsFalse(converted);
            Assert.AreEqual(default(BigDecimal), bd);
        }

        [TestMethod]
        public void TryConvertTo_Tests()
        {
            var converted = BigDecimal.TryConvertTo(4, out byte bbyte);
            Assert.IsTrue(converted);
            Assert.AreEqual((byte)4, bbyte);

            converted = BigDecimal.TryConvertTo<sbyte>(4, out sbyte bsbyte);
            Assert.IsTrue(converted);
            Assert.AreEqual((sbyte)4, bsbyte);

            converted = BigDecimal.TryConvertTo<char>(4, out char bchar);
            Assert.IsTrue(converted);
            Assert.AreEqual((char)4, bchar);

            converted = BigDecimal.TryConvertTo<short>(4, out short bshort);
            Assert.IsTrue(converted);
            Assert.AreEqual((short)4, bshort);

            converted = BigDecimal.TryConvertTo<ushort>(4, out ushort bushort);
            Assert.IsTrue(converted);
            Assert.AreEqual((ushort)4, bushort);

            converted = BigDecimal.TryConvertTo<int>(4, out int bint);
            Assert.IsTrue(converted);
            Assert.AreEqual((int)4, bint);

            converted = BigDecimal.TryConvertTo<uint>(4, out uint buint);
            Assert.IsTrue(converted);
            Assert.AreEqual((uint)4, buint);

            converted = BigDecimal.TryConvertTo<long>(4, out long blong);
            Assert.IsTrue(converted);
            Assert.AreEqual((long)4, blong);

            converted = BigDecimal.TryConvertTo<ulong>(4, out ulong bulong);
            Assert.IsTrue(converted);
            Assert.AreEqual((ulong)4, bulong);

            converted = BigDecimal.TryConvertTo<Half>(4, out Half bhalf);
            Assert.IsTrue(converted);
            Assert.AreEqual((Half)4, bhalf);

            converted = BigDecimal.TryConvertTo<float>(4, out float bfloat);
            Assert.IsTrue(converted);
            Assert.AreEqual(4f, bfloat);

            converted = BigDecimal.TryConvertTo<double>(4, out double bdouble);
            Assert.IsTrue(converted);
            Assert.AreEqual(4d, bdouble);

            converted = BigDecimal.TryConvertTo<decimal>(4, out decimal bdecimal);
            Assert.IsTrue(converted);
            Assert.AreEqual(4m, bdecimal);

            converted = BigDecimal.TryConvertTo(4, out string? bstring);
            Assert.IsFalse(converted);
            Assert.IsNull(bstring);
        }

        [TestMethod]
        public void Parse_Tests()
        {
            var bigDecimal = BigDecimal.Parse("12345678").Resolve();
            Assert.AreEqual(new BigDecimal(12345678), bigDecimal);

            bigDecimal = BigDecimal.Parse("12345.678").Resolve();
            Assert.AreEqual(new BigDecimal(12345.678m), bigDecimal);

            bigDecimal = BigDecimal.Parse("0.00000012345678").Resolve();
            Assert.AreEqual(new BigDecimal(0.00000012345678m), bigDecimal);

            bigDecimal = BigDecimal.Parse("1.2345678E-12").Resolve();
            Assert.AreEqual("1.2345678E-12", bigDecimal.ToString());

            bigDecimal = BigDecimal.Parse("123456.78E-12").Resolve();
            Assert.AreEqual("1.2345678E-7", bigDecimal.ToString());

            bigDecimal = BigDecimal.Parse("123456.78E7").Resolve();
            Assert.AreEqual(new BigDecimal(1234567800000m), bigDecimal);

            bigDecimal = BigDecimal.Parse("12345678000000").Resolve();
            Assert.AreEqual(new BigDecimal(12345678000000m), bigDecimal);

            var result = BigDecimal.Parse("errorneous value");
            Assert.IsTrue(result.IsErrorResult());

            // parse with format provider
            var bd = BigDecimal.Parse("123", (IFormatProvider?)null);
            Assert.AreEqual(123, bd);

            // parse with span and format provider
            bd = BigDecimal.Parse("123".AsSpan(), (IFormatProvider?)null);
            Assert.AreEqual(123, bd);

            // try parse with span and format provider
            var parsed = BigDecimal.TryParse("123", null, out bd);
            Assert.IsTrue(parsed);
            Assert.AreEqual(123, bd);

            parsed = BigDecimal.TryParse("bleh", null, out bd);
            Assert.IsFalse(parsed);
            Assert.AreEqual(BigDecimal.Zero, bd);
        }

        [TestMethod]
        public void Ceiling_Tests()
        {
            var bd = new BigDecimal(1200);

            var result = bd.Ceiling();
            Assert.AreEqual(bd, result);

            bd = new BigDecimal(0.1m);
            result = bd.Ceiling();
            Assert.AreEqual(BigDecimal.Zero, result);

            bd = new BigDecimal(0.01m);
            result = bd.Ceiling();
            Assert.AreEqual(BigDecimal.Zero, result);

            bd = new BigDecimal(154.22900808m);
            result = bd.Ceiling();
            Assert.AreEqual(154, result);

            bd = new BigDecimal(154.52900808m);
            result = bd.Ceiling();
            Assert.AreEqual(155, result);
        }

        [TestMethod]
        public void StringShift_Tests()
        {
            var result = BigDecimal.StringShift(12345, 0);
            Assert.AreEqual(12345, result);

            result = BigDecimal.StringShift(12345, 5);
            Assert.AreEqual(1234500000, result);

            result = BigDecimal.StringShift(12345, -2);
            Assert.AreEqual(123, result);

            result = BigDecimal.StringShift(-12345, -8);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void PowerShift_Tests()
        {
            var result = BigDecimal.PowerShift(12345, 0);
            Assert.AreEqual(12345, result);

            result = BigDecimal.PowerShift(12345, 5);
            Assert.AreEqual(1234500000, result);

            result = BigDecimal.PowerShift(12345, -2);
            Assert.AreEqual(123, result);

            result = BigDecimal.PowerShift(-12345, -8);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void DecimalShift_Tests()
        {
            var result = BigDecimal.DecimalShift(12345, 0);
            Assert.AreEqual(12345, result);

            result = BigDecimal.DecimalShift(12345, 5);
            Assert.AreEqual(1234500000, result);

            result = BigDecimal.DecimalShift(12345, -2);
            Assert.AreEqual(123, result);

            result = BigDecimal.DecimalShift(-12345, -30);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void ToDoubleTest_Tests()
        {
            BigDecimal bd = double.MaxValue;
            bd = bd + 1;
            var doubleResult = bd.ToDouble();
            Assert.AreEqual(double.MaxValue, doubleResult);
            Assert.AreEqual(0d, BigDecimal.Zero.ToDouble());
        }

        [TestMethod]
        public void ToDecimalTest_Tests()
        {
            BigDecimal bd = decimal.MaxValue;
            bd = bd + 1;
            var decimalResult = bd.ToDecimal();
            Assert.AreEqual(decimal.MaxValue, decimalResult);
            Assert.AreEqual(0m, BigDecimal.Zero.ToDecimal());
        }

        [TestMethod]
        public void RoundInt_Tests()
        {
            BigInteger value = 987654321;
            BigInteger mvalue = -987654321;

            var result = BigDecimal.Round(value, 9);
            Assert.AreEqual(value, result);
            result = BigDecimal.Round(mvalue, 9);
            Assert.AreEqual(mvalue, result);

            result = BigDecimal.Round(value, 8);
            Assert.AreEqual(987654320, result);
            result = BigDecimal.Round(mvalue, 8);
            Assert.AreEqual(-987654320, result);

            result = BigDecimal.Round(value, 7);
            Assert.AreEqual(987654300, result);
            result = BigDecimal.Round(mvalue, 7);
            Assert.AreEqual(-987654300, result);

            result = BigDecimal.Round(value, 6);
            Assert.AreEqual(987654000, result);
            result = BigDecimal.Round(mvalue, 6);
            Assert.AreEqual(-987654000, result);

            result = BigDecimal.Round(value, 5);
            Assert.AreEqual(987650000, result);
            result = BigDecimal.Round(mvalue, 5);
            Assert.AreEqual(-987650000, result);

            result = BigDecimal.Round(value, 4);
            Assert.AreEqual(987700000, result);
            result = BigDecimal.Round(mvalue, 4);
            Assert.AreEqual(-987700000, result);

            result = BigDecimal.Round(value, 3);
            Assert.AreEqual(988000000, result);
            result = BigDecimal.Round(mvalue, 3);
            Assert.AreEqual(-988000000, result);

            result = BigDecimal.Round(value, 2);
            Assert.AreEqual(990000000, result);
            result = BigDecimal.Round(mvalue, 2);
            Assert.AreEqual(-990000000, result);

            result = BigDecimal.Round(value, 2, true);
            Assert.AreEqual(99, result);
            result = BigDecimal.Round(mvalue, 2, true);
            Assert.AreEqual(-99, result);

            result = BigDecimal.Round(value, 1);
            Assert.AreEqual(1000000000, result);
            result = BigDecimal.Round(mvalue, 1);
            Assert.AreEqual(-1000000000, result);

            result = BigDecimal.Round(value, 1, true);
            Assert.AreEqual(10, result);
            result = BigDecimal.Round(mvalue, 1, true);
            Assert.AreEqual(-10, result);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => BigDecimal.Round(value, 0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => BigDecimal.Round(mvalue, 0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => BigDecimal.Round(value, -1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => BigDecimal.Round(mvalue, -1));

            value = BigInteger.Parse("9999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999");
            result = BigDecimal.Round(value, 32);
            Assert.AreEqual(
                "10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
                result.ToString());
        }

        [TestMethod]
        public void FormatContext_Tests()
        {
            var context = new BigDecimal.FormatContext(4);
            Assert.AreEqual(4, context.MaxSignificantFractionalDigits);

            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new BigDecimal.FormatContext(-3));

            Assert.AreEqual(50,
                BigDecimal.FormatContext.AsyncLocal.MaxSignificantFractionalDigits);
        }

        [TestMethod]
        public void Split_Test()
        {
            var split = new BigDecimal(34.65430m).Split();
            Assert.AreEqual(1, split.Sign);
            Assert.AreEqual(34, split.IntegralPart);
            Assert.AreEqual(0.65430m, split.DecimalPart);

            split = new BigDecimal(-34.65430m).Split();
            Assert.AreEqual(-1, split.Sign);
            Assert.AreEqual(34, split.IntegralPart);
            Assert.AreEqual(0.65430m, split.DecimalPart);
        }
    }
}
