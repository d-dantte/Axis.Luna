using Axis.Luna.Common.Types.Basic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static Axis.Luna.Common.Types.Basic.BaseExtensions;

namespace Axis.Luna.Common.NewtonsoftJson.Tests
{
    [TestClass]
    public class MiscTest
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

            var struct1 = new BasicStruct
            {
                [new BasicStruct.PropertyName("Identity", "origin;", "genesis;")] = Guid.NewGuid(),
                ["Amount"] = 56.43m,
                ["Discount"] = 6.01m.AsBasicType(),
                ["Weight"] = 65.432.AsBasicType(),
                ["Age"] = 56,
                ["Duration"] = TimeSpan.FromHours(5.3),
                ["EventDate"] = DateTimeOffset.Now,
                ["FavColors"] = new List<BasicValue>
                {
                    new BasicString("Purple", "royal;", "expensive;"),
                    "Blue",
                    "Red".AsBasicType(),
                    "Black",
                    true,
                    new BasicStruct
                    {
                        ["something"] = 45.AsBasicType()
                    }
                }
            };

            var json = JsonConvert.SerializeObject(struct1, settings);

            var struct2 = JsonConvert.DeserializeObject<BasicStruct>(json, settings);

            Assert.IsTrue(struct1.Equals(struct2));
            Assert.IsTrue(struct1 == struct2);
            Assert.AreEqual(struct1, struct2);
        }
    }
}
