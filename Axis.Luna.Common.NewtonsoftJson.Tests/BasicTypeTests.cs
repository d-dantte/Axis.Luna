using Axis.Luna.Common.Types.Basic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Axis.Luna.Common.NewtonsoftJson.Tests
{
    [TestClass]
    public class BasicTypeTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new BasicStructJsonConverter()
                }
            };

            BasicStruct struct1 = new BasicStruct.Initializer
            {
                ["[origin;genesis;]identity"] = Guid.NewGuid(),
                ["Amount"] = 56.43m,
                ["Discount"] = 6.01m,
                ["Weight"] = 65.432,
                ["Age"] = 56,
                ["Duration"] = TimeSpan.FromHours(5.3),
                ["[hen]EventDate"] = DateTimeOffset.Now,
                ["FavColors"] = new BasicValueWrapper[]
                {
                    IBasicValue.Of("Purple", "royal;", "expensive;").Wrap(),
                    "Blue",
                    "Red",
                    "Black",
                    true,
                    new BasicStruct.Initializer
                    {
                        ["something"] = 45
                    }
                }
            };

            var json = JsonConvert.SerializeObject(struct1, settings);

            var struct2 = JsonConvert.DeserializeObject<BasicStruct>(json, settings);

            Assert.IsTrue(struct1.Equals(struct2));
            Assert.IsTrue(struct1 == struct2);
            Assert.AreEqual(struct1, struct2);
        }

        [TestMethod]
        public void EqualityTest()
        {
            BasicStruct st1 = new BasicStruct.Initializer { };
            BasicStruct defaultStruct = default;

            var areEquals = st1.Equals(defaultStruct);
            Assert.IsFalse(areEquals);
        }
    }
}
