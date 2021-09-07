using Axis.Luna.Common.Types.Base;
using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Operation.Test.Utils
{
    [TestClass]
    public class StructDataTests
    {
        [TestMethod]
        public void SampleTest()
        {
            var @struct = new StructData
            {
                ["stuff"] = 5,
                ["multiple-stuff"] = new DataType[] { 6, "me", false },
                ["inner"] = new StructData
                {
                    ["inner-inner"] = 5.4m
                }
            };

            Assert.IsNotNull(@struct);

            @struct.Value = null;
            Assert.AreEqual(0, @struct.Count);

            @struct.Value = new KeyValuePair<string, DataType>[]
            {
                "abcd".ValuePair<string, DataType>(5)
            };
            Assert.AreEqual("abcd", @struct.Keys.First());
        }
    }
}
