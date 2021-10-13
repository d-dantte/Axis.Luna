using Axis.Luna.Common.Types.Basic;
using Axis.Luna.Common.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Axis.Luna.Common.Test.Types.Basic
{
    [TestClass]
    public class BasicValueTests
    {
        #region Constructor tests
        [TestMethod]
        public void Constructor_ShouldConstructValidObject()
        {
            Enum.GetValues(typeof(BasicTypes))
                .Cast<BasicTypes>()
                .Where(type => type != BasicTypes.NullValue)
                .Select(type =>
                {
                    var metadata = RandomMetadata();
                    var ibasicValue = NewNonDefaultValue(type, metadata);
                    var basicValue = new BasicValue(ibasicValue);
                    return (type, metadata ?? Array.Empty<BasicMetadata>(), ibasicValue, basicValue);
                })
                .ToList()
                .ForEach(tuple =>
                {
                    var (type, metadata, ibasicValue, basicValue) = tuple;

                    Assert.IsNotNull(basicValue);
                    Assert.AreEqual(type, basicValue.Type);
                    Assert.IsTrue(metadata
                        .OrderBy(m => m.Key)
                        .SequenceEqual(basicValue.Metadata
                        .OrderBy(m => m.Key)));
                });
        }

        [TestMethod]
        public void DefaultConstructor_ShouldConstructValidDefaultObject()
        {
            var basicValue = new BasicValue();
            var basicValue2 = default(BasicValue);

            Assert.AreEqual(basicValue.Type, basicValue2.Type);
            Assert.AreEqual(BasicTypes.NullValue, basicValue.Type);
            Assert.AreEqual(basicValue, basicValue2);
        }
        #endregion

        #region Equality tests
        [TestMethod]
        public void EqualityTest()
        {
            Enum.GetValues(typeof(BasicTypes))
                .Cast<BasicTypes>()
                .Where(type => type != BasicTypes.NullValue)
                .Select(type =>
                {
                    var ibasicValue = NewNonDefaultValue(type);
                    IBasicValue @default = NewDefaultValue(type);
                    var basicValue1 = new BasicValue(ibasicValue);
                    var basicValue2 = new BasicValue(ibasicValue);
                    var basicValue3 = new BasicValue(@default);
                    return (basicValue1, basicValue2, basicValue3);
                })
                .ToList()
                .ForEach(tuple =>
                {
                    var (basicValue1, basicValue2, basicValue3) = tuple;

                    Assert.AreEqual(basicValue1, basicValue1);
                    Assert.AreEqual(basicValue1, basicValue2);
                    Assert.AreEqual(basicValue2, basicValue1);

                    Assert.AreNotEqual(basicValue1, basicValue3);
                    Assert.AreNotEqual(basicValue3, basicValue1);
                });
        }
        #endregion

        #region ToString tests
        [TestMethod]
        public void ToStringTest()
        {
            Enum.GetValues(typeof(BasicTypes))
                .Cast<BasicTypes>()
                .Where(type => type != BasicTypes.NullValue)
                .Select(type =>
                {
                    var ibasicValue = NewNonDefaultValue(type);
                    var basicValue1 = new BasicValue(ibasicValue);
                    return (ibasicValue, basicValue1);
                })
                .ToList()
                .ForEach(tuple =>
                {
                    var (ibasicValue, basicValue1) = tuple;

                    Assert.AreEqual(ibasicValue.ToString(), basicValue1.ToString());
                });
        }
        #endregion

        #region Typecheck tests
        #endregion

        #region AsXxx tests
        #endregion

        #region implicit conversion tests
        #endregion

        #region explicit conversion tests
        #endregion


        private IBasicValue NewNonDefaultValue(BasicTypes type, params BasicMetadata[] metadata)
        {
            using var random = new SecureRandom();
            return type switch
            {
                BasicTypes.Bool => new BasicBool(random.NextBool(), metadata),

                BasicTypes.Bytes => new BasicBytes(random.NextBytes(random.NextInt(20)), metadata),

                BasicTypes.Date => new BasicDateTime(DateTimeOffset.Now, metadata),

                BasicTypes.Decimal => new BasicDecimal((decimal)random.NextInt(100), metadata),

                BasicTypes.Guid => new BasicGuid(Guid.NewGuid(), metadata),

                BasicTypes.Int => new BasicInt(random.NextSignedLong(), metadata),

                BasicTypes.List => new BasicList(new BasicValue[] { "1", "2", "3", "4" }, metadata),

                BasicTypes.Real => new BasicReal(random.NextSignedDouble(), metadata),

                BasicTypes.String => new BasicString("string value", metadata),

                BasicTypes.Struct => new BasicStruct(metadata)
                { 
                    ["name"] = "bleh",
                    ["dob"] = DateTimeOffset.Now
                },

                BasicTypes.TimeSpan => new BasicTimeSpan(TimeSpan.FromMinutes(random.NextInt(100000)), metadata),

                _ => throw new ArgumentException($"Invalid basic type: {type}")
            };
        }

        private BasicMetadata[] RandomMetadata()
        {
            using var random = new SecureRandom();
            return random.NextInt(4) switch
            {
                0 => null,
                1 => Array.Empty<BasicMetadata>(),
                2 => new BasicMetadata[] { "me;", "you:them;", "never;" },
                3 => new BasicMetadata[] { "stuff;" },
                _ => throw new Exception("invalid switch")
            };
        }

        private IBasicValue NewDefaultValue(BasicTypes type)
        {
            return type switch
            {
                BasicTypes.Bool => default(BasicBool),

                BasicTypes.Bytes => default(BasicBytes),

                BasicTypes.Date => default(BasicDateTime),

                BasicTypes.Decimal => default(BasicDecimal),

                BasicTypes.Guid => default(BasicGuid),

                BasicTypes.Int => default(BasicInt),

                BasicTypes.List => default(BasicList),

                BasicTypes.Real => default(BasicReal),

                BasicTypes.String => default(BasicString),

                BasicTypes.Struct => default(BasicStruct),

                BasicTypes.TimeSpan => default(BasicTimeSpan),

                _ => (IBasicValue) default(BasicValue)
            };
        }
    }
}
