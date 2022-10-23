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
                .Select(type =>
                {
                    var metadata = RandomMetadata();
                    var ibasicValue = NewNonDefaultValue(type, metadata);
                    var basicValue = new BasicValueWrapper(ibasicValue);
                    return (type, metadata ?? Array.Empty<Metadata>(), ibasicValue, basicValue);
                })
                .ToList()
                .ForEach(tuple =>
                {
                    var (type, metadata, ibasicValue, basicValueWrapper) = tuple;

                    Assert.IsNotNull(basicValueWrapper);
                    Assert.AreEqual(type, basicValueWrapper.Value.Type);
                    Assert.IsTrue(metadata
                        .OrderBy(m => m.Key)
                        .SequenceEqual(basicValueWrapper.Value.Metadata
                        .OrderBy(m => m.Key)));
                });
        }

        [TestMethod]
        public void DefaultConstructor_ShouldConstructValidDefaultObject()
        {
            var basicValue = new BasicValueWrapper();
            var basicValue2 = default(BasicValueWrapper);

            Assert.IsNull(basicValue.Value);
            Assert.IsNull(basicValue2.Value);
            Assert.AreEqual(basicValue, basicValue2);
        }
        #endregion

        #region Equality tests
        [TestMethod]
        public void EqualityTest()
        {
            Enum.GetValues(typeof(BasicTypes))
                .Cast<BasicTypes>()
                .Select(type =>
                {
                    var ibasicValue = NewNonDefaultValue(type);
                    IBasicValue @default = NewDefaultValue(type);
                    var basicValue1 = new BasicValueWrapper(ibasicValue);
                    var basicValue2 = new BasicValueWrapper(ibasicValue);
                    var basicValue3 = new BasicValueWrapper(@default);
                    return (basicValue1, basicValue2, basicValue3);
                })
                .ToList()
                .ForEach(tuple =>
                {
                    var (basicValue1, basicValue2, basicValue3) = tuple;

                    Assert.AreEqual(basicValue1.Value, basicValue1.Value);
                    Assert.AreEqual(basicValue1.Value, basicValue2.Value);

                    Assert.AreNotEqual(basicValue1.Value, basicValue3.Value);
                    Assert.AreNotEqual(basicValue3.Value, basicValue1.Value);
                });
        }
        #endregion


        private IBasicValue NewNonDefaultValue(BasicTypes type, params Metadata[] metadata)
        {
            return type switch
            {
                BasicTypes.Bool => new BasicBool(SecureRandom.NextBool(), metadata),

                BasicTypes.Bytes => new BasicBytes(SecureRandom.NextBytes(SecureRandom.NextInt(20)), metadata),

                BasicTypes.Date => new BasicDate(DateTimeOffset.Now, metadata),

                BasicTypes.Decimal => new BasicDecimal((decimal)SecureRandom.NextInt(100), metadata),

                BasicTypes.Guid => new BasicGuid(Guid.NewGuid(), metadata),

                BasicTypes.Int => new BasicInt(SecureRandom.NextSignedLong(), metadata),

                BasicTypes.UInt => new BasicUInt((ulong)SecureRandom.NextSignedLong(), metadata),

                BasicTypes.List => new BasicList(new BasicValueWrapper[] { "1", "2", "3", "4" }, metadata),

                BasicTypes.Real => new BasicReal(SecureRandom.NextSignedDouble(), metadata),

                BasicTypes.String => new BasicString("string value", metadata),

                BasicTypes.Struct => new BasicStruct(new BasicStruct.Initializer(metadata)
                { 
                    ["name"] = "bleh",
                    ["dob"] = DateTimeOffset.Now
                }),

                BasicTypes.TimeSpan => new BasicTimeSpan(TimeSpan.FromMinutes(SecureRandom.NextInt(100000)), metadata),

                _ => throw new ArgumentException($"Invalid basic type: {type}")
            };
        }

        private Metadata[] RandomMetadata()
        {
            return SecureRandom.NextInt(4) switch
            {
                0 => null,
                1 => Array.Empty<Metadata>(),
                2 => new Metadata[] { "me;", "you:them;", "never;" },
                3 => new Metadata[] { "stuff;" },
                _ => throw new Exception("invalid switch")
            };
        }

        private IBasicValue NewDefaultValue(BasicTypes type)
        {
            return type switch
            {
                BasicTypes.Bool => default(BasicBool),

                BasicTypes.Bytes => default(BasicBytes),

                BasicTypes.Date => default(BasicDate),

                BasicTypes.Decimal => default(BasicDecimal),

                BasicTypes.Guid => default(BasicGuid),

                BasicTypes.Int => default(BasicInt),

                BasicTypes.UInt => default(BasicUInt),

                BasicTypes.List => default(BasicList),

                BasicTypes.Real => default(BasicReal),

                BasicTypes.String => default(BasicString),

                BasicTypes.Struct => default(BasicStruct),

                BasicTypes.TimeSpan => default(BasicTimeSpan),

                _ => throw new ArgumentException()
            };
        }
    }
}
