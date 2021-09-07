using Axis.Luna.Operation.Lazy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test.Lazy
{
    [TestClass]
    public class AwaitableTests
    {
        #region Lazy Awaiter
        [TestMethod]
        public void AwaiterConstructor_ShouldReturnValidAwaiter()
        {
            var errorCount = 0;
            Action<Exception> errorSetter = (Exception e) =>
            {
                errorCount++;
            };

            //succeeding lazy
            var lazy = new CustomLazy<object>(() => null);
            var awaiter = new LazyAwaiter(lazy, errorSetter);
            _ = lazy.Value;

            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsTrue(awaiter.IsSuccessful == true);


            //failed task
            lazy = new CustomLazy<object>(() => throw new Exception());
            awaiter = new LazyAwaiter(lazy, errorSetter);

            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsNull(awaiter.IsSuccessful);

            Assert.ThrowsException<Exception>(() => awaiter.GetResult());
            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsTrue(awaiter.IsSuccessful == false);
            Assert.AreEqual(1, errorCount);

            //ensure that errorSetter is called only once
            Assert.ThrowsException<Exception>(() => awaiter.GetResult());
            Assert.AreEqual(1, errorCount);
        }

        [TestMethod]
        public async Task OnCompleted_ShouldNotifyDelegatesProperly()
        {
            var awaiter = new LazyAwaiter(
                errorSetter: ex => { },
                lazy: new CustomLazy<object>(() => null));

            var notificationCount = 0;
            Action completion = () => notificationCount++;

            awaiter.OnCompleted(completion);
            Assert.AreEqual(0, notificationCount);

            awaiter.GetResult();
            Assert.AreEqual(1, notificationCount);

            awaiter.GetResult();
            Assert.AreEqual(1, notificationCount);

            awaiter.OnCompleted(completion);
            Assert.AreEqual(2, notificationCount);

            var awaitable = new SampleAwaitable(awaiter);
            await awaitable;

            Assert.AreEqual(2, notificationCount);
        }
        #endregion

        #region Generic Lazy Awaiter
        [TestMethod]
        public void GenericAwaiterConstructor_ShouldReturnValidAwaiter()
        {
            var errorCount = 0;
            Action<Exception> errorSetter = (Exception e) =>
            {
                errorCount++;
            };

            //succeeding lazy
            var lazy = new CustomLazy<int>(() => 5);
            var awaiter = new LazyAwaiter<int>(lazy, errorSetter);
            _ = lazy.Value;

            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsTrue(awaiter.IsSuccessful == true);
            Assert.AreEqual(5, awaiter.GetResult());


            //failed task
            lazy = new CustomLazy<int>(() => throw new Exception());
            awaiter = new LazyAwaiter<int>(lazy, errorSetter);

            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsNull(awaiter.IsSuccessful);

            Assert.ThrowsException<Exception>(() => awaiter.GetResult());
            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsTrue(awaiter.IsSuccessful == false);
            Assert.AreEqual(1, errorCount);

            //ensure that errorSetter is called only once
            Assert.ThrowsException<Exception>(() => awaiter.GetResult());
            Assert.AreEqual(1, errorCount);
        }

        [TestMethod]
        public async Task GenericOnCompleted_ShouldNotifyDelegatesProperly()
        {
            var awaiter = new LazyAwaiter<object>(
                errorSetter: ex => { },
                lazy: new CustomLazy<object>(() => null));

            var notificationCount = 0;
            Action completion = () => notificationCount++;

            awaiter.OnCompleted(completion);
            Assert.AreEqual(0, notificationCount);

            awaiter.GetResult();
            Assert.AreEqual(1, notificationCount);

            awaiter.GetResult();
            Assert.AreEqual(1, notificationCount);

            awaiter.OnCompleted(completion);
            Assert.AreEqual(2, notificationCount);

            var awaitable = new SampleAwaitable<object>(awaiter);
            await awaitable;

            Assert.AreEqual(2, notificationCount);
        }
        #endregion
    }

    internal class SampleAwaitable
    {
        private LazyAwaiter _awaiter;

        public SampleAwaitable(LazyAwaiter awaiter)
        {
            _awaiter = awaiter;
        }

        public LazyAwaiter GetAwaiter() => _awaiter;
    }

    internal class SampleAwaitable<T>
    {
        private LazyAwaiter<T> _awaiter;

        public SampleAwaitable(LazyAwaiter<T> awaiter)
        {
            _awaiter = awaiter;
        }

        public LazyAwaiter<T> GetAwaiter() => _awaiter;
    }
}
