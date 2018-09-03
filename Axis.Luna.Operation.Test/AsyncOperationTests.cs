using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test
{
    [TestClass]
    public class AsyncOperationTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            SomeOperation().Wait();
        }

        [TestMethod]
        public void TestMethod2()
        {
            var result = SomeOperation2().Result;

            Assert.AreEqual(5, result);
        }


        [TestMethod]
        public void FailedOpTest()
        {
            try
            {
                var result = FailedResultOperation().Result;
            }
            catch(AggregateException ex)
            {
                Assert.AreEqual("stuff", ex.InnerException.Message);
            }
        }

        [TestMethod]
        public void FailedResultOpTest()
        {
            try
            {
                FailedOperation().Wait();
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual("stuff", ex.InnerException.Message);
            }
        }

        [TestMethod]
        public void AsyncOpWithReEntrantSyncContext()
        {
            AsyncContext.Run(async () =>
            {
                var op = Operation.Create(async () =>
                {
                    await Task.Delay(500);
                });
                op.Resolve();

                Assert.IsTrue(op.Succeeded == true);
            });
        }

        [TestMethod]
        public void ResultAsyncOpWithReEntrantSyncContext()
        {
            AsyncContext.Run(async () =>
            {
                var op = Operation.Create(async () =>
                {
                    await Task.Delay(500);
                    return 5;
                });
                var r = op.Resolve();

                Assert.AreEqual(5, r);
            });
        }


        private async Task SomeOperation()
        {
            await Operation.Create(async () =>
            {
                await Task.Run(() => { });
            });
        }
        private async Task<int> SomeOperation2()
        {
            return await Operation.Create(async () =>
            {
                var t = await Task.Run(() => 5);
                return t;
            });
        }

        private async Task FailedOperation()
        {
            await Operation.Create(async () =>
            {
                await Task.Run(() => throw new Exception("stuff"));
            });
        }
        private async Task<int> FailedResultOperation()
        {
            return await Operation.Create(async () =>
            {
                return await Task.Run(() =>
                {
                    if (true) throw new Exception("stuff");
                    else return 5;
                });
            });
        }
    }
}
