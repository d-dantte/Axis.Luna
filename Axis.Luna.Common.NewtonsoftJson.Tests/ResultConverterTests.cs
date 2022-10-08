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
            var result1 = IResult<DateTimeOffset>.Of(data);
            var result2 = IResult<DateTimeOffset>.Of(exception);
            var result3 = IResult<DateTimeOffset>.Of(exception, new Types.Basic.BasicStruct
            {
                ["Prop1"] = "bleh"
            });

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
            var exception = new Exception();
            var result1 = IResult<DateTimeOffset>.Of(data);
            var result2 = IResult<DateTimeOffset>.Of(exception);
            var result3 = IResult<DateTimeOffset>.Of(exception, new Types.Basic.BasicStruct
            {
                ["Prop1"] = "bleh"
            });

            var json1 = JsonConvert.SerializeObject(result1, settings);
            var json2 = JsonConvert.SerializeObject(result2, settings);
            var json3 = JsonConvert.SerializeObject(result3, settings);

            var deserialied1 = JsonConvert.DeserializeObject<IResult<DateTimeOffset>>(json1, settings);
            var deserialied2 = JsonConvert.DeserializeObject<IResult<DateTimeOffset>>(json2, settings);
            var deserialied3 = JsonConvert.DeserializeObject<IResult<DateTimeOffset>>(json3, settings);

            Assert.AreEqual(result1, deserialied1);

            Assert.AreEqual(
                result2
                    .As<IResult<DateTimeOffset>.ErrorResult>()
                    .Message,
                deserialied2
                    .As<IResult<DateTimeOffset>.ErrorResult>()
                    .Message);
            Assert.AreEqual(
                result2
                    .As<IResult<DateTimeOffset>.ErrorResult>()
                    .ErrorData,
                deserialied2
                    .As<IResult<DateTimeOffset>.ErrorResult>()
                    .ErrorData);

            Assert.AreEqual(
                result3
                    .As<IResult<DateTimeOffset>.ErrorResult>()
                    .Message,
                deserialied3
                    .As<IResult<DateTimeOffset>.ErrorResult>()
                    .Message);
            Assert.AreEqual(
                result3
                    .As<IResult<DateTimeOffset>.ErrorResult>()
                    .ErrorData,
                deserialied3
                    .As<IResult<DateTimeOffset>.ErrorResult>()
                    .ErrorData);
        }
    }
}
