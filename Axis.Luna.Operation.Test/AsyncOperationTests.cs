using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.AsyncEx;
using System;
using System.IO;
using System.Threading;
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
                var op = Operation.Try(async () =>
                {
                    await Task.Delay(500);
                });
                op.Resolve();
            });
        }

        [TestMethod]
        public void ResultAsyncOpWithReEntrantSyncContext()
        {
            AsyncContext.Run(async () =>
            {
                var op = Operation.Try(async () =>
                {
                    await Task.Delay(500);
                    return 6;
                });
                _ = op.Resolve();
            });
        }
        
        private async Task<int> SomeResultOperation(Task task = null, bool maintainSyncContext = true)
        {
            task = task ?? Task.Delay(500);
            await task.ConfigureAwait(maintainSyncContext);

            return DateTime.Now.GetHashCode();
        }
        private Task SomeVoidOperation(Task task = null, bool maintainSyncContext = true) => Task.Run(async () =>
        {
            var cxt = SynchronizationContext.Current;
            using (var writer = new StreamWriter(new FileStream("sync-context.txt", FileMode.Append)))
            {
                writer.WriteLine($"{cxt?.ToString() ?? "null"}");
                writer.Flush();
            }

            task = task ?? Task.Delay(500);
            await task.ConfigureAwait(maintainSyncContext);
        });
        private async Task SomeVoidOperationAsync(Task task = null, bool maintainSyncContext = true)
        {
            task = task ?? Task.Delay(500);
            await task.ConfigureAwait(maintainSyncContext);
        }


        private async Task SomeOperation()
        {
            await Operation.Try(async () =>
            {
                await Task.Run(() => { });
            });
        }
        private async Task<int> SomeOperation2()
        {
            return await Operation.Try(async () =>
            {
                var t = await Task.Run(() => 5);
                return t;
            });
        }

        private async Task FailedOperation()
        {
            await Operation.Try(async () =>
            {
                await Task.Run(() => throw new Exception("stuff"));
            });
        }
        private async Task<int> FailedResultOperation()
        {
            return await Operation.Try(async () =>
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
