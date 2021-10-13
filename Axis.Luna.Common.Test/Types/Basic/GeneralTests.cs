using Axis.Luna.Common.Types.Basic;
using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Axis.Luna.Common.Test.Types.Basic
{
    [TestClass]
    public class GeneralTests
    {

        [TestMethod]
        public void GeneralTestsForIBasicValue()
        {
            var types = Enum
                .GetValues(typeof(BasicTypes))
                .Cast<BasicTypes>()
                .ToArray();

            var testConstruction = typeof(GeneralTests).GetMethod(nameof(TestConstruction));
            var testDefault = typeof(GeneralTests).GetMethod(nameof(TestDefault));
            var testEquality = typeof(GeneralTests).GetMethod(nameof(TestEquality));

            types.ForAll(t =>
            {
                RunConstructTest(t);

                RunEqualityTest(t);

                RunDefaultTest(t);
            });
        }

        private void RunConstructTest(BasicTypes type)
        {
            switch(type)
            {
                case BasicTypes.Bool:
                    TestConstruction(new BasicBool(true, "stuff;"), type, new BasicMetadata[] { "stuff;" });
                    TestConstruction(new BasicBool(true), type, new BasicMetadata[0]);
                    TestConstruction(new BasicBool(true, "stuffx;", "vlad:putin;"), type, new BasicMetadata[] { "vlad:putin;", "stuffx;" });
                    break;

                case BasicTypes.Bytes:
                    TestConstruction(new BasicBytes(new byte[] { 1, 5 }, "stuff;"), type, new BasicMetadata[] { "stuff;" });
                    TestConstruction(new BasicBytes(new byte[] { 2, 77, 51 }), type, new BasicMetadata[0]);
                    TestConstruction(new BasicBytes(new byte[0], "stuffx;", "vlad:putin;"), type, new BasicMetadata[] { "vlad:putin;", "stuffx;" });
                    break;

                case BasicTypes.Date:
                    TestConstruction(new BasicDateTime(DateTimeOffset.Now, "stuff;"), type, new BasicMetadata[] { "stuff;" });
                    TestConstruction(new BasicDateTime(DateTimeOffset.Now), type, new BasicMetadata[0]);
                    TestConstruction(new BasicDateTime(DateTimeOffset.Now, "stuffx;", "vlad:putin;"), type, new BasicMetadata[] { "vlad:putin;", "stuffx;" });
                    break;

                case BasicTypes.Decimal:
                    TestConstruction(new BasicDecimal(5m, "stuff;"), type, new BasicMetadata[] { "stuff;" });
                    TestConstruction(new BasicDecimal(9.33m), type, new BasicMetadata[0]);
                    TestConstruction(new BasicDecimal(3m, "stuffx;", "vlad:putin;"), type, new BasicMetadata[] { "vlad:putin;", "stuffx;" });
                    break;

                case BasicTypes.Guid:
                    TestConstruction(new BasicGuid(Guid.NewGuid(), "stuff;"), type, new BasicMetadata[] { "stuff;" });
                    TestConstruction(new BasicGuid(Guid.NewGuid()), type, new BasicMetadata[0]);
                    TestConstruction(new BasicGuid(Guid.NewGuid(), "stuffx;", "vlad:putin;"), type, new BasicMetadata[] { "vlad:putin;", "stuffx;" });
                    break;

                case BasicTypes.Int:
                    TestConstruction(new BasicInt(56, "stuff;"), type, new BasicMetadata[] { "stuff;" });
                    TestConstruction(new BasicInt(56), type, new BasicMetadata[0]);
                    TestConstruction(new BasicInt(56, "stuffx;", "vlad:putin;"), type, new BasicMetadata[] { "vlad:putin;", "stuffx;" });
                    break;

                case BasicTypes.List:
                    TestConstruction(new BasicList(new BasicValue[]{DateTimeOffset.Now, 5, "stuff"}, new BasicMetadata[] { "stuff;" }), type, new BasicMetadata[] { "stuff;" });
                    TestConstruction(new BasicList(new BasicValue[]{DateTimeOffset.Now, false}), type, new BasicMetadata[0]);
                    TestConstruction(new BasicList(new BasicValue[]{DateTimeOffset.Now, 65m}, new BasicMetadata[] { "stuffx;", "vlad:putin;" }), type, new BasicMetadata[] { "vlad:putin;", "stuffx;" });
                    break;

                case BasicTypes.Real:
                    TestConstruction(new BasicReal(87.09, "stuff;"), type, new BasicMetadata[] { "stuff;" });
                    TestConstruction(new BasicReal(87.09), type, new BasicMetadata[0]);
                    TestConstruction(new BasicReal(87.09, "stuffx;", "vlad:putin;"), type, new BasicMetadata[] { "vlad:putin;", "stuffx;" });
                    break;

                case BasicTypes.String:
                    TestConstruction(new BasicString("something stringy", "stuff;"), type, new BasicMetadata[] { "stuff;" });
                    TestConstruction(new BasicString("something stringy"), type, new BasicMetadata[0]);
                    TestConstruction(new BasicString("something stringy", "stuffx;", "vlad:putin;"), type, new BasicMetadata[] { "vlad:putin;", "stuffx;" });
                    break;

                case BasicTypes.Struct:
                    TestConstruction(new BasicStruct("stuff;") { ["bleh"] = true }, type, new BasicMetadata[] { "stuff;" });
                    TestConstruction(new BasicStruct() { ["bleh"] = true }, type, new BasicMetadata[0]);
                    TestConstruction(new BasicStruct("stuffx;", "vlad:putin;") { ["bleh"] = true }, type, new BasicMetadata[] { "vlad:putin;", "stuffx;" });
                    break;

                case BasicTypes.TimeSpan:
                    TestConstruction(new BasicTimeSpan(TimeSpan.FromHours(54.1), "stuff;"), type, new BasicMetadata[] { "stuff;" });
                    TestConstruction(new BasicTimeSpan(TimeSpan.FromHours(54.1)), type, new BasicMetadata[0]);
                    TestConstruction(new BasicTimeSpan(TimeSpan.FromHours(54.1), "stuffx;", "vlad:putin;"), type, new BasicMetadata[] { "vlad:putin;", "stuffx;" });
                    break;

                case BasicTypes.NullValue:
                    break;

                default:
                    throw new Exception("Invalid type: " + type);
            }
        }

        private void RunDefaultTest(BasicTypes type)
        {
            switch(type)
            {
                case BasicTypes.Bool:
                    TestDefault(default(BasicBool));
                    TestDefault(new BasicBool(null));
                    TestDefault(new BasicBool(null, "some;", "meta-label;"));
                    break;

                case BasicTypes.Bytes:
                    TestDefault(default(BasicBytes));
                    TestDefault(new BasicBytes(null));
                    TestDefault(new BasicBytes(null, "some;", "meta-label;"));
                    break;

                case BasicTypes.Date:
                    TestDefault(default(BasicDateTime));
                    TestDefault(new BasicDateTime(null));
                    TestDefault(new BasicDateTime(null, "some;", "meta-label;"));
                    break;

                case BasicTypes.Decimal:
                    TestDefault(default(BasicDecimal));
                    TestDefault(new BasicDecimal(null));
                    TestDefault(new BasicDecimal(null, "some;", "meta-label;"));
                    break;

                case BasicTypes.Guid:
                    TestDefault(default(BasicGuid));
                    TestDefault(new BasicGuid(null));
                    TestDefault(new BasicGuid(null, "some;", "meta-label;"));
                    break;

                case BasicTypes.Int:
                    TestDefault(default(BasicInt));
                    TestDefault(new BasicInt(null));
                    TestDefault(new BasicInt(null, "some;", "meta-label;"));
                    break;

                case BasicTypes.List:
                    TestDefault(default(BasicList));
                    TestDefault(new BasicList());
                    TestDefault(new BasicList((BasicValue[])null, new BasicMetadata[] { "some;", "meta-label;" }));
                    break;

                case BasicTypes.Real:
                    TestDefault(default(BasicReal));
                    TestDefault(new BasicReal(null));
                    TestDefault(new BasicReal(null, "some;", "meta-label;"));
                    break;

                case BasicTypes.String:
                    TestDefault(default(BasicString));
                    TestDefault(new BasicString(null));
                    TestDefault(new BasicString(null, "some;", "meta-label;"));
                    break;

                case BasicTypes.NullValue:
                case BasicTypes.Struct:
                    break;

                case BasicTypes.TimeSpan:
                    TestDefault(default(BasicTimeSpan));
                    TestDefault(new BasicTimeSpan(null));
                    TestDefault(new BasicTimeSpan(null, "some;", "meta-label;"));
                    break;


                default:
                    throw new Exception("Invalid type: " + type);
            }
        }

        private void RunEqualityTest(BasicTypes type)
        {
            switch(type)
            {
                case BasicTypes.Bool:
                    TestEquality(new BasicBool(true), new BasicBool(true), new BasicBool(false));
                    break;

                case BasicTypes.Bytes:
                    TestEquality(
                        new BasicBytes(new byte[] {3, 5}),
                        new BasicBytes(new byte[] { 3, 5 }),
                        new BasicBytes(new byte[0]));
                    break;

                case BasicTypes.Date:
                    TestEquality(
                        new BasicDateTime(DateTimeOffset.Parse("2021/10/14")),
                        new BasicDateTime(DateTimeOffset.Parse("2021/10/14")),
                        new BasicDateTime(DateTimeOffset.Now));
                    break;

                case BasicTypes.Decimal:
                    TestEquality(new BasicDecimal(7m), new BasicDecimal(7m), new BasicDecimal(91.37m));
                    break;

                case BasicTypes.Guid:
                    TestEquality(
                        new BasicGuid(Guid.Parse("50bed137-19eb-47b5-88ac-9dfe2ba772fd")),
                        new BasicGuid(Guid.Parse("50bed137-19eb-47b5-88ac-9dfe2ba772fd")),
                        new BasicGuid(Guid.Empty));
                    break;

                case BasicTypes.Int:
                    TestEquality(new BasicInt(6754), new BasicInt(6754), new BasicInt(11));
                    break;

                case BasicTypes.List:
                    TestEquality(
                        new BasicList(7m, true, "false"),
                        new BasicList(7m, true, "false"),
                        new BasicList("true", false, 0m));
                    break;

                case BasicTypes.Real:
                    TestEquality(new BasicReal(6754.0), new BasicReal(6754.0), new BasicReal(11d));
                    break;

                case BasicTypes.String:
                    TestEquality(
                        new BasicString("random stuff"),
                        new BasicString("random stuff"),
                        new BasicString("not so random stuff"));
                    break;

                case BasicTypes.Struct:
                    TestEquality(
                        new BasicStruct() { ["name"] = "jin"},
                        new BasicStruct() { ["name"] = "jin" },
                        new BasicStruct() { ["age"] = 32.9});
                    break;

                case BasicTypes.TimeSpan:
                    TestEquality(
                        new BasicTimeSpan(TimeSpan.FromHours(45)),
                        new BasicTimeSpan(TimeSpan.FromMinutes(45*60)),
                        new BasicTimeSpan(TimeSpan.FromSeconds(90)));
                    break;

                case BasicTypes.NullValue:
                    break;

                default:
                    throw new Exception($"Invalid type: {type}");
            }
        }

        public void TestConstruction<T>(IBasicValue<T> value, BasicTypes type, BasicMetadata[] metadata)
        {
            Assert.IsNotNull(value);
            Assert.AreNotEqual(default, value.Value);
            Assert.AreEqual(type, value.Type);

            var s1 = metadata.OrderBy(m => m.Key).ToArray();
            var s2 = value.Metadata.OrderBy(m => m.Key).ToArray();
            Assert.IsTrue(s1.SequenceEqual(s2));
        }

        public void TestDefault<T>(IBasicValue<T> @default)
        {
            Assert.AreEqual(default, @default.Value);
        }

        public void TestEquality<T>(IBasicValue<T> eq1, IBasicValue<T> eq2, IBasicValue<T> neq1)
        {
            Assert.IsTrue(eq1.Equals(eq2));
            Assert.IsTrue(eq2.Equals(eq1));

            Assert.IsFalse(eq1.Equals(neq1));
            Assert.IsFalse(neq1.Equals(eq1));

            Assert.IsFalse(eq2.Equals(neq1));
            Assert.IsFalse(neq1.Equals(eq2));

            Assert.AreEqual(eq1, eq2);
            Assert.AreNotEqual(eq1, neq1);
            Assert.AreNotEqual(eq2, neq1);
        }
    }
}
