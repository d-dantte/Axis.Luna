using Axis.Luna.Operation.Value;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test.Value
{
    [TestClass]
    public class AwaitablesTests
    {
        #region Lazy Awaiter
        [TestMethod]
        public void AwaiterConstructor_ShouldReturnValidAwaiter()
        {
            //faulted
            var awaiter = new ValueAwaiter(new Exception());

            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsFalse(awaiter.IsSuccessful.Value);

            Assert.ThrowsException<Exception>(() => awaiter.GetResult());
            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsTrue(awaiter.IsSuccessful == false);
        }

        [TestMethod]
        public async Task OnCompleted_ShouldNotifyDelegatesProperly()
        {
            var awaiter = new ValueAwaiter(new Exception());

            var notificationCount = 0;
            Action completion = () => notificationCount++;

            awaiter.OnCompleted(completion);
            Assert.AreEqual(1, notificationCount);

            Assert.ThrowsException<Exception>(() => awaiter.GetResult());
            Assert.AreEqual(1, notificationCount);

            Assert.ThrowsException<Exception>(() => awaiter.GetResult());
            Assert.AreEqual(1, notificationCount);

            awaiter.OnCompleted(completion);
            Assert.AreEqual(2, notificationCount);

            awaiter.OnCompleted(completion);
            Assert.AreEqual(3, notificationCount);

            var awaitable = new SampleAwaitable(awaiter);
            await Assert.ThrowsExceptionAsync<Exception>(async () => await awaitable);

            Assert.AreEqual(3, notificationCount);
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
            var awaiter = new ValueAwaiter<int>(5);

            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsTrue(awaiter.IsSuccessful == true);
            Assert.AreEqual(5, awaiter.GetResult());

            //failed task
            awaiter = new ValueAwaiter<int>(new Exception());

            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsFalse(awaiter.IsSuccessful.Value);

            Assert.ThrowsException<Exception>(() => awaiter.GetResult());
            Assert.IsTrue(awaiter.IsCompleted);
            Assert.IsTrue(awaiter.IsSuccessful == false);
        }

        [TestMethod]
        public async Task GenericOnCompleted_ShouldNotifyDelegatesProperly()
        {
            var awaiter = new ValueAwaiter<int>(new Exception());

            var notificationCount = 0;
            Action completion = () => notificationCount++;

            awaiter.OnCompleted(completion);
            Assert.AreEqual(1, notificationCount);

            Assert.ThrowsException<Exception>(() => awaiter.GetResult());
            Assert.AreEqual(1, notificationCount);

            Assert.ThrowsException<Exception>(() => awaiter.GetResult());
            Assert.AreEqual(1, notificationCount);

            awaiter.OnCompleted(completion);
            Assert.AreEqual(2, notificationCount);

            awaiter.OnCompleted(completion);
            Assert.AreEqual(3, notificationCount);

            var awaitable = new SampleAwaitable<int>(awaiter);
            await Assert.ThrowsExceptionAsync<Exception>(async () => await awaitable);

            Assert.AreEqual(3, notificationCount);
        }
        #endregion
    }

    internal class SampleAwaitable
    {
        private ValueAwaiter _awaiter;

        public SampleAwaitable(ValueAwaiter awaiter)
        {
            _awaiter = awaiter;
        }

        public ValueAwaiter GetAwaiter() => _awaiter;
    }

    internal class SampleAwaitable<T>
    {
        private ValueAwaiter<T> _awaiter;

        public SampleAwaitable(ValueAwaiter<T> awaiter)
        {
            _awaiter = awaiter;
        }

        public ValueAwaiter<T> GetAwaiter() => _awaiter;
    }
}
