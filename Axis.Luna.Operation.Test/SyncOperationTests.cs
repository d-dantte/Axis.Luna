using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test
{
    [TestClass]
    public class SyncOperationTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            try
            {
                SomeOperation().Wait();
            }
            catch(AggregateException e)
            {
                var ie = e.InnerException;
                Assert.AreEqual(ie.Message, "stuff");
            }
        }

        [TestMethod]
        public void TestMethod2()
        {
            var result = SomeOperation2().Result;

            Assert.AreEqual(5, result);
        }


        private async Task SomeOperation()
        {
            await Operation.Fail(new Exception("stuff"));
        }

        private async Task<int> SomeOperation2()
        {
            return await Operation.FromResult(5);
        }
    }
}
