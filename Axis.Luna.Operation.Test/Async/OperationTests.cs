using Axis.Luna.Operation.Async;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test.Async
{
    [TestClass]
    public class OperationTests
    {
        #region AsyncOperation
        [TestMethod]
        public async Task NewAsyncOperation_ShouldReturnValidObject()
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
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(false, op.Succeeded);

            //task
            op = new AsyncOperation(Task.FromException(new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(false, op.Succeeded);
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

            //with null action, should return an already failed operation
            op2 = op.Then((Action)null);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);

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

            //with null action, should fail
            op2 = op.Then((Func<Task>)null);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);

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

            //with null action, should fail
            op2 = op.Then((Func<Operation>)null);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);

            //with failing action should fail
            op2 = op.Then(new Func<Operation>(() => throw new Exception()));
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

            //with null action, should fail
            op2 = op.Then((Func<int>)null);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);

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

            //with null action, should fail
            op2 = op.Then((Func<Task<int>>)null);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);

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

            //with null action, should fail
            op2 = op.Then((Func<Operation<int>>)null);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }
        #endregion

        #region MapError
        #endregion

        #endregion

        #region AsyncOperation<TResult>
        [TestMethod]
        public async Task NewResultAsyncOperation_ShouldReturnValidObject()
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
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(false, op.Succeeded);

            //task
            op = new AsyncOperation<string>(Task.FromException<string>(new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(false, op.Succeeded);
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
            op2 = op.Then((Func<int, int>)null);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);

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
            op2 = op.Then((Func<int, Task<int>>)null);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);

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
            op2 = op.Then((Func<int, Operation<int>>)null);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);

            //with failing action should fail
            op2 = op.Then(new Func<int, Operation<int>>(r => throw new Exception()));
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
            op2 = op.Then((Func<int, int>)null);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);

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
            op2 = op.Then((Func<int, Task<int>>)null);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);

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
            op2 = op.Then((Func<int, Operation<int>>)null);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }
        #endregion

        #region MapError
        #endregion

        #endregion
    }
}
