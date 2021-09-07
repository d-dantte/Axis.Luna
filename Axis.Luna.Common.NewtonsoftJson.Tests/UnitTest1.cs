using Axis.Luna.Common.Types.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Axis.Luna.Common.NewtonsoftJson.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new StructDataJsonConverter{ OverloadedTypeEmbedingStyle = StructDataJsonConverter.OverloadedTypeOutputEmbedingStyle.Explicit }
                }
            };

            var struct1 = new StructData
            {
                ["Identity"] = System.Guid.NewGuid(),
                ["Amount"] = 56.43m,
                ["Weight"] = 65.432,
                ["Age"] = 56,
                ["Duration"] = TimeSpan.FromHours(5.3),
                ["EventDate"] = DateTimeOffset.Now,
                ["FavColors"] = new List<DataType>
                {
                    "Blue",
                    "Red",
                    "Black"
                }
            };

            var json = JsonConvert.SerializeObject(struct1, settings);

            var struct2 = JsonConvert.DeserializeObject<StructData>(json, settings);

            Assert.IsTrue(struct1.Equals(struct2));
            Assert.IsTrue(struct1 == struct2);
            Assert.AreEqual(struct1, struct2);
        }
    }
}
