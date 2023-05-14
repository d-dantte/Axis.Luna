using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Axis.Luna.Common.NewtonsoftJson.Tests
{
    [TestClass]
    public class ResultConverterTests
    {
        [TestMethod]
        public void CanConvert_ShouldIndicatePropertly()
        {
            var converter = new ResultConverter();

            Assert.IsTrue(converter.CanConvert(typeof(IResult<int>)));
            Assert.IsTrue(converter.CanConvert(typeof(IResult<int>.DataResult)));
            Assert.IsTrue(converter.CanConvert(typeof(IResult<int>.ErrorResult)));
            Assert.IsFalse(converter.CanConvert(typeof(IResult<>)));
            Assert.IsFalse(converter.CanConvert(typeof(IResult<>.DataResult)));
            Assert.IsFalse(converter.CanConvert(typeof(IResult<>.ErrorResult)));
            Assert.IsFalse(converter.CanConvert(typeof(object)));
        }

        [TestMethod]
        public void Write()
        {
            var resultConverter = new ResultConverter();
            var basicConverter = new BasicStructJsonConverter();
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    resultConverter,
                    basicConverter
                }
            };

            var data = DateTimeOffset.Now;
            var exception = new Exception();
            var result1 = Results.Of<DateTimeOffset>(data);
            var result2 = Results.Of<DateTimeOffset>(exception);
            var result3 = Results.Of<DateTimeOffset>(exception.WithErrorData(new Types.Basic.BasicStruct.Initializer
            {
                ["Prop1"] = "bleh"
            }));

            var json1 = JsonConvert.SerializeObject(result1, settings);
            var jobject = JObject.Parse(json1);
            Assert.IsTrue(jobject.TryGetValue("Data", out var token));
            Assert.AreEqual(data, token.Value<DateTime>());

            var json2 = JsonConvert.SerializeObject(result2, settings);
            jobject = JObject.Parse(json2);
            Assert.IsTrue(jobject.TryGetValue(nameof(IResult<int>.ErrorResult.Message), out token));
            Assert.AreEqual(exception.Message, token.Value<string>());

            var json3 = JsonConvert.SerializeObject(result3, settings);
            jobject = JObject.Parse(json3);
            Assert.IsTrue(jobject.TryGetValue(nameof(IResult<int>.ErrorResult.ErrorData), out token));
            Assert.IsTrue(token.As<JObject>().TryGetValue("Prop1", out token));
            Assert.AreEqual("bleh", token.Value<string>());
        }

        [TestMethod]
        public void Read()
        {
            var resultConverter = new ResultConverter();
            var basicConverter = new BasicStructJsonConverter();
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    resultConverter,
                    basicConverter
                }
            };

            var data = DateTimeOffset.Now;
            var exception1 = new Exception();
            var exception2 = new Exception()
                .WithErrorData(new Types.Basic.BasicStruct.Initializer
                {
                    ["Prop1"] = "bleh"
                });
            var result1 = Results.Of<DateTimeOffset>(data);
            var result2 = Results.Of<DateTimeOffset>(exception1);
            var result3 = Results.Of<DateTimeOffset>(exception2);

            var json1 = JsonConvert.SerializeObject(result1, settings);
            var json2 = JsonConvert.SerializeObject(result2, settings);
            var json3 = JsonConvert.SerializeObject(result3, settings);

            var deserialied1 = JsonConvert.DeserializeObject<IResult<DateTimeOffset>>(json1, settings);
            var deserialied2 = JsonConvert.DeserializeObject<IResult<DateTimeOffset>>(json2, settings);
            var deserialied3 = JsonConvert.DeserializeObject<IResult<DateTimeOffset>>(json3, settings);

            Assert.AreEqual(result1, deserialied1);

            Assert.AreEqual(
                result2.AsError().Message,
                deserialied2.AsError().Message);
            Assert.AreEqual(
                result2.AsError().ErrorData,
                deserialied2.AsError().ErrorData);

            Assert.AreEqual(
                result3.AsError().Message,
                deserialied3.AsError().Message);
            Assert.AreEqual(
                result3.AsError().ErrorData,
                deserialied3.AsError().ErrorData);
        }
    }
}
