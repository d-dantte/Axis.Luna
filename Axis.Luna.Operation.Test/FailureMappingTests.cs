using Axis.Luna.Extensions;
using Axis.Luna.Operation.Async;
using Axis.Luna.Operation.Lazy;
using Axis.Luna.Operation.Value;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test
{
    using Error = Axis.Luna.Operation.OperationError;

    [TestClass]
    public class FailureResultMappingTests
    {
        [TestMethod]
        public async Task MapError_WithResult()
        {
            // null operation
            Assert.ThrowsException<ArgumentNullException>(() => Operation.MapError(null, (Func<Error, int>)null));

            // null handler
            Assert.ThrowsException<ArgumentNullException>(() => Operation.MapError(Operation.FromResult(1), (Func<Error, int>)null));

            // successful
            var defaultOp = Operation.FromResult(-1);
            var mappedOp = defaultOp.MapError(error => 0);
            Assert.AreEqual(defaultOp, mappedOp);

            var successfulTokenSource = new CancellationTokenSource();
            var successfulValueOp = new ValueOperation<int>(1);
            var successfulLazyOp = new LazyOperation<int>(() => 2);
            var successfulAsyncOp = new AsyncOperation<int>(Cancellable(successfulTokenSource.Token, 3));

            var failedTokenSource = new CancellationTokenSource();
            var failedValueOp = new ValueOperation<int>(new Error());
            var failedLazyOp = new LazyOperation<int>(() => new Exception().Throw<int>());
            var failedAsyncOp = new AsyncOperation<int>(Cancellable<int>(failedTokenSource.Token, new Exception()));

            #region value
            // failed
            mappedOp = failedValueOp.MapError(error => 0);
            var result = await mappedOp;
            Assert.AreEqual(0, result);
            #endregion

            #region lazy
            // busy -> successful
            mappedOp = successfulLazyOp.MapError(error => 0);
            var mappedOpError = successfulLazyOp.MapError(FailingHandler<int>);
            Assert.IsTrue(mappedOp is LazyOperation<int>);
            Assert.IsTrue(mappedOpError is LazyOperation<int>);
            Assert.AreEqual(null, mappedOp.Succeeded);
            Assert.AreEqual(null, mappedOpError.Succeeded);

            result = await mappedOp;
            Assert.AreEqual(true, successfulLazyOp.Succeeded);
            Assert.AreEqual(true, mappedOp.Succeeded);
            Assert.AreEqual(2, result);

            result = await mappedOpError;
            Assert.AreEqual(true, mappedOpError.Succeeded);
            Assert.AreEqual(2, result);

            // busy -> failed
            mappedOp = failedLazyOp.MapError(error => 0);
            mappedOpError = failedLazyOp.MapError(FailingHandler<int>);
            Assert.IsTrue(mappedOp is LazyOperation<int>);
            Assert.IsTrue(mappedOpError is LazyOperation<int>);
            Assert.AreEqual(null, mappedOp.Succeeded);
            Assert.AreEqual(null, mappedOpError.Succeeded);

            result = await mappedOp;
            Assert.AreEqual(false, failedLazyOp.Succeeded);
            Assert.AreEqual(true, mappedOp.Succeeded);
            Assert.AreEqual(0, result);

            await Assert.ThrowsExceptionAsync<Exception>(async () => await mappedOpError);
            #endregion

            #region async
            // busy -> successful
            mappedOp = successfulAsyncOp.MapError(error => 0);
            mappedOpError = successfulAsyncOp.MapError(FailingHandler<int>);
            Assert.IsTrue(mappedOp is AsyncOperation<int>);
            Assert.IsTrue(mappedOpError is AsyncOperation<int>);
            Assert.AreEqual(null, mappedOp.Succeeded);
            Assert.AreEqual(null, mappedOpError.Succeeded);

            // delay because sometimes, 'task' isn't given the opportunity to run before the cancellation token is signalled.
            await Task.Delay(60);
            successfulTokenSource.Cancel();
            result = await mappedOp;
            Assert.AreEqual(true, successfulAsyncOp.Succeeded);
            Assert.AreEqual(true, mappedOp.Succeeded);
            Assert.AreEqual(3, result);

            result = await mappedOpError;
            Assert.AreEqual(true, mappedOpError.Succeeded);
            Assert.AreEqual(3, result);

            // busy -> failed
            mappedOp = failedAsyncOp.MapError(error => 0);
            mappedOpError = failedAsyncOp.MapError(FailingHandler<int>);
            Assert.IsTrue(mappedOp is AsyncOperation<int>);
            Assert.IsTrue(mappedOpError is AsyncOperation<int>);
            Assert.AreEqual(null, mappedOp.Succeeded);
            Assert.AreEqual(null, mappedOpError.Succeeded);

            // delay because sometimes, 'task' isn't given the opportunity to run before the cancellation token is signalled.
            await Task.Delay(60);
            failedTokenSource.Cancel();
            result = await mappedOp;
            Assert.AreEqual(false, failedAsyncOp.Succeeded);
            Assert.AreEqual(true, mappedOp.Succeeded);
            Assert.AreEqual(0, result);

            await Assert.ThrowsExceptionAsync<Exception>(async () => await mappedOpError);
            #endregion
        }

        [TestMethod]
        public async Task MapError_WithTaskResult()
        {
            // null operation
            Assert.ThrowsException<ArgumentNullException>(() => Operation.MapError((IOperation<int>)null, (Func<Error, Task<int>>)null));

            // null handler
            Assert.ThrowsException<ArgumentNullException>(() => Operation.MapError(Operation.FromResult(1), (Func<Error, Task<int>>)null));

            // successful
            var defaultOp = Operation.FromResult(-1);
            var mappedOp = defaultOp.MapError(error => 0);
            Assert.AreEqual(defaultOp, mappedOp);

            var successfulTokenSource = new CancellationTokenSource();
            var successfulValueOp = new ValueOperation<int>(1);
            var successfulLazyOp = new LazyOperation<int>(() => 2);
            var successfulAsyncOp = new AsyncOperation<int>(Cancellable(successfulTokenSource.Token, 3));

            var failedTokenSource = new CancellationTokenSource();
            var failedValueOp = new ValueOperation<int>(new Error());
            var failedLazyOp = new LazyOperation<int>(() => new Exception().Throw<int>());
            var failedAsyncOp = new AsyncOperation<int>(Cancellable<int>(failedTokenSource.Token, new Exception()));

            #region value
            // failed
            mappedOp = failedValueOp.MapError(error => Task.FromResult(0));
            var result = await mappedOp;
            Assert.AreEqual(0, result);
            #endregion

            #region lazy
            // busy -> successful
            mappedOp = successfulLazyOp.MapError(error => Task.FromResult(0));
            var mappedOpError = successfulLazyOp.MapError(FailingTaskHandler<int>);
            // NOTE:
            // 1. if successfulLazyOp is busy, mappedOp will be Async, and thus has a chance of resolving successfulLazyOp.
            // 2. if successfulLazyOp is resolved, mappedOp will be Lazy
            // From #1, it is not wise to assert the type here.

            result = await mappedOp;
            Assert.AreEqual(true, successfulLazyOp.Succeeded);
            Assert.AreEqual(true, mappedOp.Succeeded);
            Assert.AreEqual(2, result);

            result = await mappedOpError;
            Assert.AreEqual(true, mappedOpError.Succeeded);
            Assert.AreEqual(2, result);

            // busy -> failed
            mappedOp = failedLazyOp.MapError(error => Task.FromResult(0));
            mappedOpError = failedLazyOp.MapError(FailingTaskHandler<int>);

            result = await mappedOp;
            Assert.AreEqual(false, failedLazyOp.Succeeded);
            Assert.AreEqual(true, mappedOp.Succeeded);
            Assert.AreEqual(0, result);

            await Assert.ThrowsExceptionAsync<Exception>(async () => await mappedOpError);
            #endregion

            #region async
            // busy -> successful
            mappedOp = successfulAsyncOp.MapError(error => Task.FromResult(0));
            mappedOpError = successfulAsyncOp.MapError(FailingTaskHandler<int>);
            Assert.IsTrue(mappedOp is AsyncOperation<int>);
            Assert.IsTrue(mappedOpError is AsyncOperation<int>);
            Assert.AreEqual(null, mappedOp.Succeeded);
            Assert.AreEqual(null, mappedOpError.Succeeded);

            // delay because sometimes, 'task' isn't given the opportunity to run before the cancellation token is signalled.
            await Task.Delay(60);
            successfulTokenSource.Cancel();
            result = await mappedOp;
            Assert.AreEqual(true, successfulAsyncOp.Succeeded);
            Assert.AreEqual(true, mappedOp.Succeeded);
            Assert.AreEqual(3, result);

            result = await mappedOpError;
            Assert.AreEqual(true, mappedOpError.Succeeded);
            Assert.AreEqual(3, result);

            // busy -> failed
            mappedOp = failedAsyncOp.MapError(error => Task.FromResult(0));
            mappedOpError = failedAsyncOp.MapError(FailingTaskHandler<int>);
            Assert.IsTrue(mappedOp is AsyncOperation<int>);
            Assert.IsTrue(mappedOpError is AsyncOperation<int>);
            Assert.AreEqual(null, mappedOp.Succeeded);
            Assert.AreEqual(null, mappedOpError.Succeeded);

            // delay because sometimes, 'task' isn't given the opportunity to run before the cancellation token is signalled.
            await Task.Delay(60);
            failedTokenSource.Cancel();
            result = await mappedOp;
            Assert.AreEqual(false, failedAsyncOp.Succeeded);
            Assert.AreEqual(true, mappedOp.Succeeded);
            Assert.AreEqual(0, result);

            await Assert.ThrowsExceptionAsync<Exception>(async () => await mappedOpError);
            #endregion
        }

        [TestMethod]
        public async Task MapError_WithOperationResult()
        {
            // null operation
            Assert.ThrowsException<ArgumentNullException>(() => Operation.MapError((IOperation<int>)null, (Func<Error, IOperation<int>>)null));

            // null handler
            Assert.ThrowsException<ArgumentNullException>(() => Operation.MapError(Operation.FromResult(1), (Func<Error, IOperation<int>>)null));

            // successful
            var defaultOp = Operation.FromResult(-1);
            var mappedOp = defaultOp.MapError(error => 0);
            Assert.AreEqual(defaultOp, mappedOp);

            var successfulTokenSource = new CancellationTokenSource();
            var successfulValueOp = new ValueOperation<int>(1);
            var successfulLazyOp = new LazyOperation<int>(() => 2);
            var successfulAsyncOp = new AsyncOperation<int>(Cancellable(successfulTokenSource.Token, 3));

            var failedTokenSource = new CancellationTokenSource();
            var failedValueOp = new ValueOperation<int>(new Error());
            var failedLazyOp = new LazyOperation<int>(() => new Exception().Throw<int>());
            var failedAsyncOp = new AsyncOperation<int>(Cancellable<int>(failedTokenSource.Token, new Exception()));

            #region value
            // failed
            mappedOp = failedValueOp.MapError(error => Operation.FromResult(0));
            var result = await mappedOp;
            Assert.AreEqual(0, result);
            #endregion

            #region lazy
            // busy -> successful
            mappedOp = successfulLazyOp.MapError(error => Operation.FromResult(0));
            var mappedOpError = successfulLazyOp.MapError(FailingOperationHandler<int>);
            // NOTE:
            // 1. if successfulLazyOp is busy, mappedOp will be Async, and thus has a chance of resolving successfulLazyOp.
            // 2. if successfulLazyOp is resolved, mappedOp will be Lazy
            // From #1, it is not wise to assert the type here.

            result = await mappedOp;
            Assert.AreEqual(true, successfulLazyOp.Succeeded);
            Assert.AreEqual(true, mappedOp.Succeeded);
            Assert.AreEqual(2, result);

            result = await mappedOpError;
            Assert.AreEqual(true, mappedOpError.Succeeded);
            Assert.AreEqual(2, result);

            // busy -> failed
            mappedOp = failedLazyOp.MapError(error => Operation.FromResult(0));
            mappedOpError = failedLazyOp.MapError(FailingOperationHandler<int>);

            result = await mappedOp;
            Assert.AreEqual(false, failedLazyOp.Succeeded);
            Assert.AreEqual(true, mappedOp.Succeeded);
            Assert.AreEqual(0, result);

            await Assert.ThrowsExceptionAsync<Exception>(async () => await mappedOpError);
            #endregion

            #region async
            // busy -> successful
            mappedOp = successfulAsyncOp.MapError(error => Operation.FromResult(0));
            mappedOpError = successfulAsyncOp.MapError(FailingOperationHandler<int>);
            Assert.IsTrue(mappedOp is AsyncOperation<int>);
            Assert.IsTrue(mappedOpError is AsyncOperation<int>);
            Assert.AreEqual(null, mappedOp.Succeeded);
            Assert.AreEqual(null, mappedOpError.Succeeded);

            // delay because sometimes, 'task' isn't given the opportunity to run before the cancellation token is signalled.
            await Task.Delay(60);
            successfulTokenSource.Cancel();
            result = await mappedOp;
            Assert.AreEqual(true, successfulAsyncOp.Succeeded);
            Assert.AreEqual(true, mappedOp.Succeeded);
            Assert.AreEqual(3, result);

            result = await mappedOpError;
            Assert.AreEqual(true, mappedOpError.Succeeded);
            Assert.AreEqual(3, result);

            // busy -> failed
            mappedOp = failedAsyncOp.MapError(error => Operation.FromResult(0));
            mappedOpError = failedAsyncOp.MapError(FailingOperationHandler<int>);
            Assert.IsTrue(mappedOp is AsyncOperation<int>);
            Assert.IsTrue(mappedOpError is AsyncOperation<int>);
            Assert.AreEqual(null, mappedOp.Succeeded);
            Assert.AreEqual(null, mappedOpError.Succeeded);

            // delay because sometimes, 'task' isn't given the opportunity to run before the cancellation token is signalled.
            await Task.Delay(60);
            failedTokenSource.Cancel();
            result = await mappedOp;
            Assert.AreEqual(false, failedAsyncOp.Succeeded);
            Assert.AreEqual(true, mappedOp.Succeeded);
            Assert.AreEqual(0, result);

            await Assert.ThrowsExceptionAsync<Exception>(async () => await mappedOpError);
            #endregion
        }

        private static Func<Task<T>> Cancellable<T>(CancellationToken token, T result)
        {
            return async () =>
            {
                while (!token.IsCancellationRequested)
                    await Task.Yield();

                return result;
            };
        }

        private static Func<Task<T>> Cancellable<T>(CancellationToken token, Exception e)
        {
            return async () =>
            {
                while (!token.IsCancellationRequested)
                    await Task.Yield();

                return e.Throw<T>();
            };
        }

        private static T FailingHandler<T>(Error error) => new Exception().Throw<T>();

        private static Task<T> FailingTaskHandler<T>(Error error) => new Exception().Throw<Task<T>>();

        private static IOperation<T> FailingOperationHandler<T>(Error error) => new Exception().Throw<IOperation<T>>();
    }
}
