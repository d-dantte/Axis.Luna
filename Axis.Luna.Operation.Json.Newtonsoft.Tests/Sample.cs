using Axis.Luna.Operation.NewtonsoftJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Axis.Luna.Operation.Json.Newtonsoft.Tests
{
    [TestClass]
    public class Sample
    {
        [TestMethod]
        public void TestMethod1()
        {
            var op = Operation.Fail<int>(new OperationError(
                "some message",
                "AOC544",
                new Common.Types.Basic.BasicStruct
                {
                    ["me"] = "you",
                    ["them"] = 5,
                    ["something_id"] = Guid.NewGuid(),
                    ["d_day"] = new Common.Types.Basic.BasicStruct
                    {
                        ["moment_of_truth"] = DateTimeOffset.Now
                    }
                }));

            var timer = Stopwatch.StartNew();
            var json = JsonConvert.SerializeObject(op, Constants.JsonSettings);
            timer.Stop();
            Console.WriteLine($"First Serialization: {timer.Elapsed}");

            timer = Stopwatch.StartNew();
            var op2 = JsonConvert.DeserializeObject<Operation<int>>(json, Constants.JsonSettings);
            timer.Stop();
            Console.WriteLine($"First Deserialization: {timer.Elapsed}");

            Assert.AreEqual(op.Succeeded, op2.Succeeded);
            Assert.AreEqual(op.Error.Code, op2.Error.Code);
            Assert.AreEqual(op.Error.Message, op2.Error.Message);
            Assert.AreEqual(op.Error.Data, op2.Error.Data);

            timer = Stopwatch.StartNew();
            json = JsonConvert.SerializeObject(op2, Constants.JsonSettings);
            timer.Stop();
            Console.WriteLine($"Second Serialization: {timer.Elapsed}");

            timer = Stopwatch.StartNew();
            _ = JsonConvert.DeserializeObject<Operation<int>>(json, Constants.JsonSettings);
            timer.Stop();
            Console.WriteLine($"Second Deserialization: {timer.Elapsed}");
        }
    }
}
