using Axis.Luna.Common.Types.Basic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Axis.Luna.Operation.Test.Utils
{
    [TestClass]
    public class StructDataTests
    {
        [TestMethod]
        public void SampleTest()
        {
            var @struct = new BasicStruct.Initializer
            {
                ["stuff"] = 5,
                ["multiple-stuff"] = new BasicValueWrapper[] { 6, "me", false },
                ["inner"] = new BasicStruct.Initializer
                {
                    ["inner-inner"] = 5.4m
                }
            };
            Assert.IsNotNull(@struct);
        }
    }
}
