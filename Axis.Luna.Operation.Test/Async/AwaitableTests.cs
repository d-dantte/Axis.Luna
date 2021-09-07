using Axis.Luna.Operation.Async;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test.Async
{
    [TestClass]
    public class AwaitableTests
    {
        #region AsyncAwaiter
        [TestMethod]
        public async Task AwaiterConstructor_ShouldReturnValidAwaiter()
        {
            //completed task
            var task = Task.FromResult(5);
            var awaiter = new AsyncAwaiter(task);

            Assert.IsNotNull(awaiter.Task);
            Assert.AreNotEqual(default, awaiter.TaskAwaitable);
            Assert.AreNotEqual(default, awaiter.TaskAwaiter);
            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsTrue(awaiter.IsSuccessful == true);


            //long running task
            var cancellationSource = new CancellationTokenSource();
            task = Task.Run(
                cancellationToken: cancellationSource.Token,
                function: async () =>
                {
                    while (!cancellationSource.IsCancellationRequested)
                        await Task.Yield();

                    return 5;
                });

            awaiter = new AsyncAwaiter(task);

            Assert.IsNotNull(awaiter.Task);
            Assert.AreNotEqual(default, awaiter.TaskAwaitable);
            Assert.AreNotEqual(default, awaiter.TaskAwaiter);
            Assert.IsFalse(awaiter.IsCompleted);
            Assert.IsNull(awaiter.IsSuccessful);

            await Task.Yield(); //force this current method to yield so 'task' can run
            cancellationSource.Cancel();
            await Task.WhenAll(task);
            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsTrue(awaiter.IsSuccessful == true);


            //failed task
            task = Task.FromException<int>(new Exception());
            awaiter = new AsyncAwaiter(task);
            var awaitable = new SampleAwaitable(awaiter);

            Assert.IsNotNull(awaiter.Task);
            Assert.AreNotEqual(default, awaiter.TaskAwaitable);
            Assert.AreNotEqual(default, awaiter.TaskAwaiter);
            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsTrue(awaiter.IsSuccessful == false);
            Assert.ThrowsException<Exception>(() => awaiter.GetResult());
        }

        [TestMethod]
        public async Task OnCompleted_ShouldNotifyDelegatesProperly()
        {
            var task = Task.FromException(new Exception());
            var awaiter = new AsyncAwaiter(task);

            var awaitable = new SampleAwaitable(awaiter);

            //if awaiting this awaitable behaves properly, then the continuation is working.
            await Assert.ThrowsExceptionAsync<Exception>(async() => await awaitable);
            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsTrue(awaiter.IsSuccessful == false);
        }
        #endregion

        #region AsyncAwaiter<TResult>
        [TestMethod]
        public async Task GenericAwaiterConstructor_ShouldReturnValidAwaiter()
        {
            //completed task
            var task = Task.FromResult(5);
            var awaiter = new AsyncAwaiter<int>(task);

            Assert.IsNotNull(awaiter.Task);
            Assert.AreNotEqual(default, awaiter.TaskAwaitable);
            Assert.AreNotEqual(default, awaiter.TaskAwaiter);
            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsTrue(awaiter.IsSuccessful == true);
            Assert.AreEqual(5, awaiter.GetResult());


            //long running task
            var cancellationSource = new CancellationTokenSource();
            task = Task.Run(
                cancellationToken: cancellationSource.Token,
                function: async () =>
                {
                    while (!cancellationSource.IsCancellationRequested)
                        await Task.Yield();

                    return 5;
                });

            awaiter = new AsyncAwaiter<int>(task);

            Assert.IsNotNull(awaiter.Task);
            Assert.AreNotEqual(default, awaiter.TaskAwaitable);
            Assert.AreNotEqual(default, awaiter.TaskAwaiter);
            Assert.IsFalse(awaiter.IsCompleted);
            Assert.IsNull(awaiter.IsSuccessful);

            await Task.Yield(); //force this current method to yield so 'task' can run
            cancellationSource.Cancel();
            Task.WaitAll(task);
            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsTrue(awaiter.IsSuccessful == true);
            Assert.AreEqual(5, awaiter.GetResult());


            //failed task
            task = Task.FromException<int>(new Exception());
            awaiter = new AsyncAwaiter<int>(task);

            Assert.IsNotNull(awaiter.Task);
            Assert.AreNotEqual(default, awaiter.TaskAwaitable);
            Assert.AreNotEqual(default, awaiter.TaskAwaiter);
            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsTrue(awaiter.IsSuccessful == false);
            Assert.ThrowsException<Exception>(() => awaiter.GetResult());
        }

        [TestMethod]
        public async Task GenericOnCompleted_ShouldNotifyDelegatesProperly()
        {
            var task = Task.FromException<int>(new Exception());
            var awaiter = new AsyncAwaiter<int>(task);

            var awaitable = new SampleAwaitable<int>(awaiter);

            //if awaiting this awaitable behaves properly, then the continuation is working.
            await Assert.ThrowsExceptionAsync<Exception>(async () => await awaitable);
            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsTrue(awaiter.IsSuccessful == false);
        }
        #endregion
    }

    internal class SampleAwaitable
    {
        private AsyncAwaiter _awaiter;

        public SampleAwaitable(AsyncAwaiter awaiter)
        {
            _awaiter = awaiter;
        }

        public AsyncAwaiter GetAwaiter() => _awaiter;
    }

    internal class SampleAwaitable<T>
    {
        private AsyncAwaiter<T> _awaiter;

        public SampleAwaitable(AsyncAwaiter<T> awaiter)
        {
            _awaiter = awaiter;
        }

        public AsyncAwaiter<T> GetAwaiter() => _awaiter;
    }
}
