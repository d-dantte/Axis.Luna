using Axis.Luna.Operation.NewtonsoftJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Axis.Luna.Operation.Json.Newtonsoft.Tests
{
    [TestClass]
    public class Sample
    {
        [TestMethod]
        public void TestMethod1()
        {
            var jobj = JValue.CreateNull();

            var op = Operation.Fail<int>(new OperationError(
                "some message",
                "AOC544",
                new Common.Types.Base.StructData
                {
                    ["me"] = "you",
                    ["them"] = 5,
                    ["something_id"] = Guid.NewGuid(),
                    ["d_day"] = new Common.Types.Base.StructData
                    {
                        ["moment_of_truth"] = DateTimeOffset.Now
                    }
                }));

            var json = JsonConvert.SerializeObject(op, Constants.JsonSettings);

            var op2 = JsonConvert.DeserializeObject<Operation<int>>(json, Constants.JsonSettings);

            Assert.AreEqual(op.Succeeded, op2.Succeeded);
            Assert.AreEqual(op.Error.Code, op2.Error.Code);
            Assert.AreEqual(op.Error.Message, op2.Error.Message);
            Assert.AreEqual(op.Error.Data, op2.Error.Data);
        }
    }
}
