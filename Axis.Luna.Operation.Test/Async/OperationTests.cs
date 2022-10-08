using Axis.Luna.Extensions;
using Axis.Luna.Operation.Async;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test.Async
{
    [TestClass]
    public class OperationTests
    {
        #region AsyncOperation
        [TestMethod]
        public void NewAsyncOperation_ShouldReturnValidObject()
        {
            //async action
            var op = new AsyncOperation(async () => await Task.Yield());
            Assert.IsNotNull(op);

            //task
            op = new AsyncOperation(Task.Run(() => { }));
            Assert.IsNotNull(op);

            var task = new Task(() => { });
            Assert.AreEqual(TaskStatus.Created, task.Status);
            op = new AsyncOperation(task);
            Assert.AreNotEqual(TaskStatus.Created, task.Status);


            ///failed operation

            //async action
            op = new AsyncOperation(async () => await Task.FromException(new Exception()));
            Assert.IsNotNull(op);

            //task
            op = new AsyncOperation(Task.FromException(new Exception()));
            Assert.IsNotNull(op);
        }

        [TestMethod]
        public void NewAsyncOperation_WithInvalidInput_ShouldThrowExcption()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new AsyncOperation((Task)null));
            Assert.ThrowsException<ArgumentNullException>(() => new AsyncOperation((Func<Task>)null));
        }

        [TestMethod]
        public void NewAsyncOperation_WithFaultingTaskProducer_ShouldCreateFaultedOperation()
        {
            Func<Task> producer = () => new Exception().Throw<Task>();
            var op = new AsyncOperation(producer);

            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
            Assert.ThrowsExceptionAsync<Exception>(async () => await op);


            producer = () => new OperationException(new OperationError()).Throw<Task>();
            op = new AsyncOperation(producer);

            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
            Assert.ThrowsExceptionAsync<Exception>(async () => await op);
        }

        #region Awaiting
        [TestMethod]
        public async Task Await_WithValidAction_ShouldReturnProperly()
        {
            var op = new AsyncOperation(async () =>
            {
                await Task.Yield();
            });
            await op;
            Assert.AreEqual(true, op.Succeeded);
            Assert.IsNull(op.Error);


            op = new AsyncOperation(async () =>
            {
                await Task.Delay(50);
            });
            await op;
            Assert.AreEqual(true, op.Succeeded);
            Assert.IsNull(op.Error);


            op = new AsyncOperation(Task.Delay(10));
            await op;
            Assert.AreEqual(true, op.Succeeded);
            Assert.IsNull(op.Error);
        }

        [TestMethod]
        public async Task Await_WithFailingAction_ShouldReturnProperly()
        {
            var op = new AsyncOperation(async () =>
            {
                await Task.Yield();
                throw new Exception();
            });
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
        }
        #endregion

        #region Then
        [TestMethod]
        public async Task Then_WithVoidResult()
        {
            var list = new List<int>();
            var op = new AsyncOperation(async () =>
            {
                await Task.Yield();
                list.Add(1);
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(() =>
            {
                list.Add(2);
            });
            await op2;
            Assert.IsTrue(new[] { 1, 2 }.SequenceEqual(list));

            //with null action, throw argument exception
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Action)null));

            //with failing action should fail
            op2 = op.Then(new Action(() => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op2);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }

        [TestMethod]
        public async Task Then_WithVoidTask()
        {
            var list = new List<int>();
            var op = new AsyncOperation(async () =>
            {
                await Task.Yield();
                list.Add(1);
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(async () =>
            {
                await Task.Yield();
                list.Add(2);
            });
            await op2;
            Assert.IsTrue(new[] { 1, 2 }.SequenceEqual(list));

            //with null action, should throw argument exception
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<Task>)null));

            //with failing action should fail
            op2 = op.Then(new Func<Task>(() => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op2);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }

        [TestMethod]
        public async Task Then_WithVoidOperation()
        {
            var list = new List<int>();
            var op = new AsyncOperation(async () =>
            {
                await Task.Yield();
                list.Add(1);
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(() => Operation.Try(() =>
            {
                list.Add(2);
            }));
            await op2;
            Assert.IsTrue(new[] { 1, 2 }.SequenceEqual(list));

            //with null action, should throw argument exceptions
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<IOperation>)null));

            //succeeded operation,shold fail
            op2 = op.Then(new Func<IOperation>(() => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op2);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }

        [TestMethod]
        public async Task Then_WithResult()
        {
            var list = new List<int>();
            var op = new AsyncOperation(async () =>
            {
                await Task.Yield();
                list.Add(1);
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(() =>
            {
                list.Add(2);
                return 5;
            });
            await op2;
            Assert.IsTrue(new[] { 1, 2 }.SequenceEqual(list));

            //with null action, should throw argument exceptions
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<int>)null));

            //with failing action should fail
            op2 = op.Then(new Func<int>(() => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op2);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }

        [TestMethod]
        public async Task Then_WithResultTask()
        {
            var list = new List<int>();
            var op = new AsyncOperation(async () =>
            {
                await Task.Yield();
                list.Add(1);
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(async () =>
            {
                await Task.Yield();
                list.Add(2);
                return 5;
            });
            await op2;
            Assert.IsTrue(new[] { 1, 2 }.SequenceEqual(list));

            //with null action, should throw argument exceptions
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<Task<int>>)null));

            //with failing action should fail
            op2 = op.Then(new Func<Task<int>>(() => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op2);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }

        [TestMethod]
        public async Task Then_WithResultOperation()
        {
            var list = new List<int>();
            var op = new AsyncOperation(async () =>
            {
                await Task.Yield();
                list.Add(1);
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(() => Operation.Try(() =>
            {
                list.Add(2);
                return 5;
            }));
            await op2;
            Assert.IsTrue(new[] { 1, 2 }.SequenceEqual(list));

            //with null action, should throw argument exceptions
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<IOperation<int>>)null));
        }
        #endregion

        #region MapError
        [TestMethod]
        public async Task MapError_WithVoidOutput()
        {
            var semaphore = new SemaphoreSlim(0);
            var op = new AsyncOperation(semaphore.WaitAsync());

            // with incomplete task, return a new operation that succeeds the source
            var errorMapped = false;
            var op2 = op.MapError(error => errorMapped = true);
            Assert.IsNull(op.Succeeded);
            Assert.IsNull(op2.Succeeded);

            // force op to complete
            _ = semaphore.Release();
            await op2;
            Assert.AreEqual(true, op.Succeeded);
            Assert.AreEqual(true, op2.Succeeded);
            Assert.IsFalse(errorMapped);

            // with null failure handler, should throw exception
            Assert.ThrowsException<ArgumentNullException>(() => op.MapError((Action<OperationError>)null));

            // with succeeded source, should return the source
            op2 = op.MapError(Extensions.Common.NoOp);
            Assert.AreEqual(op2, op);

            // with failing source, should return failure handler operation
            errorMapped = false;
            string exceptionMessage = Guid.NewGuid().ToString();
            op = new AsyncOperation(Task.FromException(new Exception(exceptionMessage)));
            op2 = op.MapError(error =>
            {
                errorMapped = true;
                Assert.AreEqual(exceptionMessage, error.GetException().Message);
            });
        }

        [TestMethod]
        public async Task MapError_WithTaskOutput()
        {
            var semaphore = new SemaphoreSlim(0);
            var op = new AsyncOperation(semaphore.WaitAsync());

            // with incomplete task, return a new operation that succeeds the source
            var errorMapped = false;
            var op2 = op.MapError(error => Task.Run(() => errorMapped = true));
            Assert.IsNull(op.Succeeded);
            Assert.IsNull(op2.Succeeded);

            // force op to complete
            _ = semaphore.Release();
            await op2;
            Assert.AreEqual(true, op.Succeeded);
            Assert.AreEqual(true, op2.Succeeded);
            Assert.IsFalse(errorMapped);

            // with null failure handler, should throw exception
            Assert.ThrowsException<ArgumentNullException>(() => op.MapError((Func<OperationError, Task>)null));

            // with succeeded source, should return the source
            op2 = op.MapError(error => Task.Run(() => errorMapped = true));
            Assert.AreEqual(op2, op);

            // with failing source, should return failure handler operation
            errorMapped = false;
            string exceptionMessage = Guid.NewGuid().ToString();
            op = new AsyncOperation(Task.FromException(new Exception(exceptionMessage)));
            op2 = op.MapError(async error =>
            {
                await Task.Yield();
                errorMapped = true;
                Assert.AreEqual(exceptionMessage, error.GetException().Message);
            });
        }

        [TestMethod]
        public async Task MapError_WithOperationOutput()
        {
            var semaphore = new SemaphoreSlim(0);
            var op = new AsyncOperation(semaphore.WaitAsync());

            // with incomplete task, return a new operation that succeeds the source
            var errorMapped = false;
            var op2 = op.MapError(error => new AsyncOperation(Task.Run(() => errorMapped = true)));
            Assert.IsNull(op.Succeeded);
            Assert.IsNull(op2.Succeeded);

            // force op to complete
            _ = semaphore.Release();
            await op2;
            Assert.AreEqual(true, op.Succeeded);
            Assert.AreEqual(true, op2.Succeeded);
            Assert.IsFalse(errorMapped);

            // with null failure handler, should throw exception
            Assert.ThrowsException<ArgumentNullException>(() => op.MapError((Func<OperationError, IOperation>)null));

            // with succeeded source, should return the source
            op2 = op.MapError(error => new AsyncOperation(Task.Run(() => errorMapped = true)));
            Assert.AreEqual(op2, op);

            // with failing source, should return failure handler operation
            errorMapped = false;
            string exceptionMessage = Guid.NewGuid().ToString();
            op = new AsyncOperation(Task.FromException(new Exception(exceptionMessage)));
            op2 = op.MapError(error =>
            {
                errorMapped = true;
                Assert.AreEqual(exceptionMessage, error.GetException().Message);
                return new AsyncOperation(Task.Delay(0));
            });
        }
        #endregion

        #endregion

        #region AsyncOperation<TResult>
        [TestMethod]
        public void NewResultAsyncOperation_ShouldReturnValidObject()
        {
            //async action
            var op = new AsyncOperation<string>(async () => await Task.FromResult(""));
            Assert.IsNotNull(op);

            //task
            op = new AsyncOperation<string>(Task.Run(() => ""));
            Assert.IsNotNull(op);

            var task = new Task<string>(() => "");
            Assert.AreEqual(TaskStatus.Created, task.Status);
            op = new AsyncOperation<string>(task);
            Assert.AreNotEqual(TaskStatus.Created, task.Status);


            ///failed operation

            //async action
            op = new AsyncOperation<string>(async () => await Task.FromException<string>(new Exception()));
            Assert.IsNotNull(op);

            //task
            op = new AsyncOperation<string>(Task.FromException<string>(new Exception()));
            Assert.IsNotNull(op);
        }

        [TestMethod]
        public void NewAsyncResultOperation_WithInvalidInput_ShouldThrowExcption()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new AsyncOperation<string>((Task<string>)null));
            Assert.ThrowsException<ArgumentNullException>(() => new AsyncOperation<string>((Func<Task<string>>)null));
        }

        [TestMethod]
        public void NewAsyncResultOperation_WithFaultingTaskProducer_ShouldCreateFaultedOperation()
        {
            Func<Task<string>> producer = () => new Exception().Throw<Task<string>>();
            var op = new AsyncOperation<string>(producer);

            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
            Assert.ThrowsExceptionAsync<Exception>(async () => await op);

            producer = () => new OperationException(new OperationError()).Throw<Task<string>>();
            op = new AsyncOperation<string>(producer);

            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
            Assert.ThrowsExceptionAsync<Exception>(async () => await op);
        }

        [TestMethod]
        public async Task Await_WithValidFunc_ShouldReturnProperly()
        {
            var op = new AsyncOperation<string>(async () =>
            {
                await Task.Yield();
                return "";
            });
            await op;
            Assert.AreEqual(true, op.Succeeded);
            Assert.IsNull(op.Error);


            op = new AsyncOperation<string>(async () =>
            {
                await Task.Delay(50);
                return "";
            });
            await op;
            Assert.AreEqual(true, op.Succeeded);
            Assert.IsNull(op.Error);


            op = new AsyncOperation<string>(Task.FromResult(""));
            await op;
            Assert.AreEqual(true, op.Succeeded);
            Assert.IsNull(op.Error);
        }

        [TestMethod]
        public async Task Await_WithFailingFunc_ShouldReturnProperly()
        {
            var op = new AsyncOperation<string>(async () =>
            {
                await Task.Yield();
                throw new Exception();
            });
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
        }

        #region Awaiting
        [TestMethod]
        public async Task AwaitResult_WithValidAction_ShouldReturnProperly()
        {
            var op = new AsyncOperation<int>(async () =>
            {
                return await Task.FromResult(4);
            });
            var result = await op;
            Assert.AreEqual(true, op.Succeeded);
            Assert.AreEqual(4, result);
        }

        [TestMethod]
        public async Task AwaitResult_WithFailingAction_ShouldReturnProperly()
        {
            var op = new AsyncOperation<int>(Task.FromException<int>(new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);

            op = new AsyncOperation<int>(async () =>
            {
                await Task.Yield();
                throw new Exception();
            });
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
        }
        #endregion

        #region Then
        [TestMethod]
        public async Task ThenResult_WithVoidResult()
        {
            var list = new List<int>();
            var op = new AsyncOperation<int>(async () =>
            {
                await Task.Yield();
                list.Add(1);
                return 1;
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(r =>
            {
                list.Add(2);
                return 2;
            });
            var result = await op2;
            Assert.IsTrue(new[] { 1, 2 }.SequenceEqual(list));
            Assert.AreEqual(2, result);

            //with null function, should return an already failed operation
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<int, int>)null));

            //with failing action should fail
            op2 = op.Then(new Func<int, int>(r => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op2);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }

        [TestMethod]
        public async Task ThenResult_WithVoidTask()
        {
            var list = new List<int>();
            var op = new AsyncOperation<int>(async () =>
            {
                await Task.Yield();
                list.Add(1);
                return 5;
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(async r =>
            {
                await Task.Yield();
                list.Add(2);
                return 2;
            });
            var result = await op2;
            Assert.IsTrue(new[] { 1, 2 }.SequenceEqual(list));
            Assert.AreEqual(2, result);

            //with null action, should fail
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<int, Task<int>>)null));

            //with failing action should fail
            op2 = op.Then(new Func<int, Task<int>>(r => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op2);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }

        [TestMethod]
        public async Task ThenResult_WithVoidOperation()
        {
            var list = new List<int>();
            var op = new AsyncOperation<int>(async () =>
            {
                await Task.Yield();
                list.Add(1);
                return 3;
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(r => Operation.Try(() =>
            {
                list.Add(2);
                return 5;
            }));
            var result = await op2;
            Assert.IsTrue(new[] { 1, 2 }.SequenceEqual(list));
            Assert.AreEqual(5, result);

            //with null action, should fail
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<int, IOperation<int>>)null));

            //with failing action should fail
            op2 = op.Then(new Func<int, IOperation<int>>(r => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op2);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }

        [TestMethod]
        public async Task ThenResult_WithResult()
        {
            var list = new List<int>();
            var op = new AsyncOperation<int>(async () =>
            {
                await Task.Yield();
                list.Add(1);
                return 6;
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(value =>
            {
                list.Add(2);
                return 5;
            });
            await op2;
            Assert.IsTrue(new[] { 1, 2 }.SequenceEqual(list));

            //with null action, should fail
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<int, int>)null));

            //with failing action should fail
            op2 = op.Then(new Func<int, int>(value => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op2);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }

        [TestMethod]
        public async Task ThenResult_WithResultTask()
        {
            var list = new List<int>();
            var op = new AsyncOperation<int>(async () =>
            {
                await Task.Yield();
                list.Add(1);
                return 5;
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(async value =>
            {
                await Task.Yield();
                list.Add(2);
                return 5;
            });
            await op2;
            Assert.IsTrue(new[] { 1, 2 }.SequenceEqual(list));

            //with null action, should fail
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<int, Task<int>>)null));

            //with failing action should fail
            op2 = op.Then(new Func<int, Task<int>>(value => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op2);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }

        [TestMethod]
        public async Task ThenResult_WithResultOperation()
        {
            var list = new List<int>();
            var op = new AsyncOperation<int>(async () =>
            {
                await Task.Yield();
                list.Add(1);
                return 5;
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(value => Operation.Try(() =>
            {
                list.Add(2);
                return 5;
            }));
            await op2;
            Assert.IsTrue(new[] { 1, 2 }.SequenceEqual(list));

            //with null action, should fail
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<int, IOperation<int>>)null));
        }
        #endregion

        #region MapError
        [TestMethod]
        public async Task MapError_WithResultdOutput()
        {
            var semaphore = new SemaphoreSlim(0);
            var op = new AsyncOperation<int>(semaphore.WaitAsync().ContinueWith(t => 4));

            // with incomplete task, return a new operation that succeeds the source
            var errorMapped = false;
            var op2 = op.MapError(error => { errorMapped = true; return 0; });
            Assert.IsNull(op.Succeeded);
            Assert.IsNull(op2.Succeeded);

            // force op to complete
            _ = semaphore.Release();
            await op2;
            Assert.AreEqual(true, op.Succeeded);
            Assert.AreEqual(true, op2.Succeeded);
            Assert.IsFalse(errorMapped);

            // with null failure handler, should throw exception
            Assert.ThrowsException<ArgumentNullException>(() => op.MapError((Func<OperationError, int>)null));

            // with succeeded source, should return the source
            op2 = op.MapError(error => 0);
            Assert.AreEqual(op2, op);

            // with failing source, should return failure handler operation
            errorMapped = false;
            string exceptionMessage = Guid.NewGuid().ToString();
            op = new AsyncOperation<int>(Task.FromException<int>(new Exception(exceptionMessage)));
            op2 = op.MapError(error =>
            {
                errorMapped = true;
                Assert.AreEqual(exceptionMessage, error.GetException().Message);
                return 0;
            });
        }

        [TestMethod]
        public async Task MapError_WithTaskResultOutput()
        {
            var semaphore = new SemaphoreSlim(0);
            var op = new AsyncOperation<int>(semaphore.WaitAsync().ContinueWith(t => 0));

            // with incomplete task, return a new operation that succeeds the source
            var errorMapped = false;
            var op2 = op.MapError(error => Task.Run(() => { errorMapped = true; return 0; }));
            Assert.IsNull(op.Succeeded);
            Assert.IsNull(op2.Succeeded);

            // force op to complete
            _ = semaphore.Release();
            await op2;
            Assert.AreEqual(true, op.Succeeded);
            Assert.AreEqual(true, op2.Succeeded);
            Assert.IsFalse(errorMapped);

            // with null failure handler, should throw exception
            Assert.ThrowsException<ArgumentNullException>(() => op.MapError((Func<OperationError, Task<int>>)null));

            // with succeeded source, should return the source
            op2 = op.MapError(error => Task.Run(() => { errorMapped = true; return 0; }));
            Assert.AreEqual(op2, op);

            // with failing source, should return failure handler operation
            errorMapped = false;
            string exceptionMessage = Guid.NewGuid().ToString();
            op = new AsyncOperation<int>(Task.FromException<int>(new Exception(exceptionMessage)));
            op2 = op.MapError(async error =>
            {
                await Task.Yield();
                errorMapped = true;
                Assert.AreEqual(exceptionMessage, error.GetException().Message);
                return 0;
            });
        }

        [TestMethod]
        public async Task MapError_WithOperationResultOutput()
        {
            var semaphore = new SemaphoreSlim(0);
            var op = new AsyncOperation<int>(semaphore.WaitAsync().ContinueWith(t => 0));

            // with incomplete task, return a new operation that succeeds the source
            var errorMapped = false;
            var op2 = op.MapError(error => new AsyncOperation<int>(Task.Run(() => { errorMapped = true; return 0; })));
            Assert.IsNull(op.Succeeded);
            Assert.IsNull(op2.Succeeded);

            // force op to complete
            _ = semaphore.Release();
            await op2;
            Assert.AreEqual(true, op.Succeeded);
            Assert.AreEqual(true, op2.Succeeded);
            Assert.IsFalse(errorMapped);

            // with null failure handler, should throw exception
            Assert.ThrowsException<ArgumentNullException>(() => op.MapError((Func<OperationError, IOperation<int>>)null));

            // with succeeded source, should return the source
            op2 = op.MapError(error => new AsyncOperation<int>(Task.Run(() => { errorMapped = true; return 0; })));
            Assert.AreEqual(op2, op);

            // with failing source, should return failure handler operation
            errorMapped = false;
            string exceptionMessage = Guid.NewGuid().ToString();
            op = new AsyncOperation<int>(Task.FromException<int>(new Exception(exceptionMessage)));
            op2 = op.MapError(error =>
            {
                errorMapped = true;
                Assert.AreEqual(exceptionMessage, error.GetException().Message);
                return new AsyncOperation<int>(Task.Delay(0).ContinueWith(t => 0));
            });
        }
        #endregion

        #endregion
    }
}
