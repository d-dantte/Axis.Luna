using Axis.Luna.Extensions;
using Axis.Luna.Operation.Lazy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test.Lazy
{
    [TestClass]
    public class OperationTests
    {
        #region LazyOperation
        [TestMethod]
        public void NewLazyOperation_ShouldReturnValidObject()
        {
            //action
            var op = new LazyOperation(() => { });
            Assert.IsNotNull(op);
            Assert.IsNull(op.Succeeded);
            Assert.IsNull(op.Error);

            ///failed operation
            //action
            op = new LazyOperation(() => throw new Exception());
            Assert.IsNull(op.Succeeded);
            Assert.IsNull(op.Error);
        }

        #region Awaiting
        [TestMethod]
        public async Task Await_WithValidAction_ShouldReturnProperly()
        {
            var op = new LazyOperation(() => { });
            await op;
            Assert.AreEqual(true, op.Succeeded);
        }

        [TestMethod]
        public async Task Await_WithFailingAction_ShouldReturnProperly()
        {
            var op = new LazyOperation(() =>
            {
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
            var op = new LazyOperation(() =>
            {
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
            var op = new LazyOperation(() =>
            {
                list.Add(1);
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(() =>
            {
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
            var op = new LazyOperation(() =>
            {
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
            var op = new LazyOperation(() =>
            {
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
            var op = new LazyOperation(() =>
            {
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
            var op = new LazyOperation(() =>
            {
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
        [TestMethod]
        public async Task MapError_WithAction()
        {
            int result = 0;
            var succeededOp = new LazyOperation(() => { });
            var failedOp = new LazyOperation(() => throw new SpecialException());

            //successful op with valid action, should stay on success path, and skip error map
            Operation newop = succeededOp.MapError(error => { result = 1; });
            await newop;
            Assert.AreEqual(0, result);

            //errored op with valid action, should return to success path
            newop = failedOp.MapError(error => { result = 5; });
            await newop;
            Assert.AreEqual(5, result);

            //successful op with failing error-map action, should stay on the success path
            newop = succeededOp.MapError(new Action<OperationError>(error => new Exception().Throw()));
            await newop; //no exception thrown

            //failed op with failed error-map action, should stay on the error path
            newop = failedOp.MapError(new Action<OperationError>(error => new Exception().Throw()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop);
        }

        [TestMethod]
        public async Task MapError_WithTask()
        {
            int result = 0;
            var succeededOp = new LazyOperation(() => { });
            var failedOp = new LazyOperation(() => throw new SpecialException());

            //successful op with valid action, should skip the action and remain on the success path
            Operation newop = succeededOp.MapError(error => Task.Run(() =>{ result = 1; }));
            await newop;
            Assert.AreEqual(0, result);

            //failed op with valid action, should return to success path
            newop = failedOp.MapError(error => Task.Run(() => { result = 5; }));
            await newop;
            Assert.AreEqual(5, result);

            //successful op with failing error-map action, should skip the error-map and remain on the success path
            newop = succeededOp.MapError(new Func<OperationError, Task>(error => throw new Exception()));
            await newop; //no exception thrown

            //failed op with failing error-map action, should fail and remain on the error path
            newop = failedOp.MapError(new Func<OperationError, Task>(error => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop);
        }

        [TestMethod]
        public async Task MapError_WithOperation()
        {
            int result = 0;
            var succeededOp = new LazyOperation(() => { });
            var failedOp = new LazyOperation(() => throw new SpecialException());

            //successful op with valid action, should skip the action and remain on the success path
            Operation newop = succeededOp.MapError(error => Operation.Try(() => { result = 1; }));
            await newop;
            Assert.AreEqual(0, result);

            //failed op with valid action, should return to success path
            newop = failedOp.MapError(error => Operation.Try(() => { result = 5; }));
            await newop;
            Assert.AreEqual(5, result);

            //successful op with failing error-map action, should skip the error-map and remain on the success path
            newop = succeededOp.MapError(new Func<OperationError, Operation>(error => throw new Exception()));
            await newop; //no exception thrown

            //failed op with failing error-map action, should fail and remain on the error path
            newop = failedOp.MapError(new Func<OperationError, Operation>(error => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop);
        }


        [TestMethod]
        public async Task MapErrorResult_WithAction()
        {
            int result = 0;
            var succeededOp = new LazyOperation(() => { });
            var failedOp = new LazyOperation(() => throw new SpecialException());

            //successful op with valid action, should skip the action and remain on the success path
            var newop = succeededOp.MapError(error => result = 1);
            await newop;
            Assert.AreEqual(0, result);

            //failed op with valid action, should return to success path
            newop = failedOp.MapError(error => result = 5);
            await newop;
            Assert.AreEqual(5, result);

            //successful op with failing error-map action, should skip the error-map and remain on the success path
            var newop2 = succeededOp.MapError(new Action<OperationError>(error => new Exception().Throw()));
            await newop2; //no exception thrown

            //failed op with failing error-map action, should fail and remain on the error path
            newop2 = failedOp.MapError(new Action<OperationError>(error => new Exception().Throw()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop2);
        }

        [TestMethod]
        public async Task MapErrorResult_WithTask()
        {
            int result = 0;
            var succeededOp = new LazyOperation(() => { });
            var failedOp = new LazyOperation(() => throw new SpecialException());

            //successful op with valid action, should skip the action and remain on the success path
            var newop = succeededOp.MapError(error => Task.Run(() => result = 1));
            await newop;
            Assert.AreEqual(0, result);

            //failed op with valid action, should return to success path
            newop = failedOp.MapError(error => Task.Run(() => result = 5));
            await newop;
            Assert.AreEqual(5, result);

            //successful op with failing error-map action, should skip the error-map and remain on the success path
            var newop2 = succeededOp.MapError(new Func<OperationError, Task>(error => throw new Exception()));
            await newop2; //no exception thrown

            //failed op with failing error-map action, should fail and remain on the error path
            newop2 = failedOp.MapError(new Func<OperationError, Task>(error => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop2);
        }

        [TestMethod]
        public async Task MapErrorResult_WithOperation()
        {
            int result = 0;
            var succeededOp = new LazyOperation(() => { });
            var failedOp = new LazyOperation(() => throw new SpecialException());

            //successful op with valid action, should skip the action and remain on the success path
            var newop = succeededOp.MapError(error => Operation.Try(() => result = 1));
            await newop;
            Assert.AreEqual(0, result);

            //failed op with valid action, should return to success path
            newop = failedOp.MapError(error => Operation.Try(() => result = 5));
            await newop;
            Assert.AreEqual(5, result);

            //successful op with failing error-map action, should skip the error-map and remain on the success path
            var newop2 = succeededOp.MapError(new Func<OperationError, Operation>(error => throw new Exception()));
            await newop2; //no exception thrown

            //failed op with failing error-map action, should fail and remain on the error path
            newop2 = failedOp.MapError(new Func<OperationError, Operation>(error => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop2);
        }
        #endregion

        #endregion

        #region LazyOperation<TResult>
        [TestMethod]
        public async Task NewResultLazyOperation_ShouldReturnValidObject()
        {
            //action
            var op = new LazyOperation<string>(() => "");
            Assert.IsNotNull(op);

            ///failed operation
            //async action
            op = new LazyOperation<string>(() => throw new Exception());
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(false, op.Succeeded);
        }


        #region Awaiting
        [TestMethod]
        public async Task AwaitResult_WithValidAction_ShouldReturnProperly()
        {
            var op = new LazyOperation<int>(() => 9);
            var result = await op;
            Assert.AreEqual(true, op.Succeeded);
            Assert.AreEqual(9, result);
        }

        [TestMethod]
        public async Task AwaitResult_WithFailingAction_ShouldReturnProperly()
        {
            var op = new LazyOperation<int>(() => throw new Exception());
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
        }
        #endregion

        #region Then
        [TestMethod]
        public async Task ThenResult_WithAction()
        {
            var list = new List<int>();
            var op = new LazyOperation<int>(() =>
            {
                list.Add(1);
                return 1;
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(r =>
            {
                list.Add(2);
            });
            await op2;
            Assert.IsTrue(new[] { 1, 2 }.SequenceEqual(list));

            //with null function, should return an already failed operation
            op2 = op.Then((Action<int>)null);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);

            //with failing action should fail
            op2 = op.Then(new Action<int>(r => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op2);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }

        [TestMethod]
        public async Task ThenResult_WithVoidTask()
        {
            var list = new List<int>();
            var op = new LazyOperation<int>(() =>
            {
                list.Add(1);
                return 5;
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(async r =>
            {
                list.Add(2);
                await Task.Yield();
            });
            await op2;
            Assert.IsTrue(new[] { 1, 2 }.SequenceEqual(list));

            //with null action, should fail
            op2 = op.Then((Func<int, Task>)null);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);

            //with failing action should fail
            op2 = op.Then(new Func<int, Task>(r => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op2);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }

        [TestMethod]
        public async Task ThenResult_WithVoidOperation()
        {
            var list = new List<int>();
            var op = new LazyOperation<int>(() =>
            {
                list.Add(1);
                return 3;
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(r => Operation.Try(() =>
            {
                list.Add(2);
            }));
            await op2;
            Assert.IsTrue(new[] { 1, 2 }.SequenceEqual(list));

            //with null action, should fail
            op2 = op.Then((Func<int, Operation>)null);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);

            //with failing action should fail
            op2 = op.Then(new Func<int, Operation>(r => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op2);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }

        [TestMethod]
        public async Task ThenResult_WithResult()
        {
            var list = new List<int>();
            var op = new LazyOperation<int>(() =>
            {
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
            var op = new LazyOperation<int>(() =>
            {
                list.Add(1);
                return 5;
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
            var op = new LazyOperation<int>(() =>
            {
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
        [TestMethod]
        public async Task ResultMapError_WithAction()
        {
            int result = 0;
            var succeededOp = new LazyOperation<int>(() => 4);
            var failedOp = new LazyOperation<int>(() => throw new SpecialException());

            //successful op with valid action, should stay on success path, and skip error map
            Operation newop = succeededOp.MapError(error => { result = 1; });
            await newop;
            Assert.AreEqual(0, result);

            //errored op with valid action, should return to success path
            newop = failedOp.MapError(error => { result = 5; });
            await newop;
            Assert.AreEqual(5, result);

            //successful op with failing error-map action, should stay on the success path
            newop = succeededOp.MapError(new Action<OperationError>(error => new Exception().Throw()));
            await newop; //no exception thrown

            //failed op with failed error-map action, should stay on the error path
            newop = failedOp.MapError(new Action<OperationError>(error => new Exception().Throw()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop);
        }

        [TestMethod]
        public async Task ResultMapError_WithTask()
        {
            int result = 0;
            var succeededOp = new LazyOperation<int>(() => 0);
            var failedOp = new LazyOperation<int>(() => throw new SpecialException());

            //successful op with valid action, should skip the action and remain on the success path
            Operation newop = succeededOp.MapError(error => Task.Run(() => { result = 1; }));
            await newop;
            Assert.AreEqual(0, result);

            //failed op with valid action, should return to success path
            newop = failedOp.MapError(error => Task.Run(() => { result = 5; }));
            await newop;
            Assert.AreEqual(5, result);

            //successful op with failing error-map action, should skip the error-map and remain on the success path
            newop = succeededOp.MapError(new Func<OperationError, Task>(error => throw new Exception()));
            await newop; //no exception thrown

            //failed op with failing error-map action, should fail and remain on the error path
            newop = failedOp.MapError(new Func<OperationError, Task>(error => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop);
        }

        [TestMethod]
        public async Task ResultMapError_WithOperation()
        {
            int result = 0;
            var succeededOp = new LazyOperation<int>(() => 0);
            var failedOp = new LazyOperation<int>(() => throw new SpecialException());

            //successful op with valid action, should skip the action and remain on the success path
            Operation newop = succeededOp.MapError(error => Operation.Try(() => { result = 1; }));
            await newop;
            Assert.AreEqual(0, result);

            //failed op with valid action, should return to success path
            newop = failedOp.MapError(error => Operation.Try(() => { result = 5; }));
            await newop;
            Assert.AreEqual(5, result);

            //successful op with failing error-map action, should skip the error-map and remain on the success path
            newop = succeededOp.MapError(new Func<OperationError, Operation>(error => throw new Exception()));
            await newop; //no exception thrown

            //failed op with failing error-map action, should fail and remain on the error path
            newop = failedOp.MapError(new Func<OperationError, Operation>(error => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop);
        }


        [TestMethod]
        public async Task ResultMapErrorResult_WithAction()
        {
            int result = 0;
            var succeededOp = new LazyOperation<int>(() => 0);
            var failedOp = new LazyOperation<int>(() => throw new SpecialException());

            //successful op with valid action, should skip the action and remain on the success path
            var newop = succeededOp.MapError(error => result = 1);
            await newop;
            Assert.AreEqual(0, result);

            //failed op with valid action, should return to success path
            newop = failedOp.MapError(error => result = 5);
            await newop;
            Assert.AreEqual(5, result);

            //successful op with failing error-map action, should skip the error-map and remain on the success path
            var newop2 = succeededOp.MapError(new Action<OperationError>(error => new Exception().Throw()));
            await newop2; //no exception thrown

            //failed op with failing error-map action, should fail and remain on the error path
            newop2 = failedOp.MapError(new Action<OperationError>(error => new Exception().Throw()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop2);
        }

        [TestMethod]
        public async Task ResultMapErrorResult_WithTask()
        {
            int result = 0;
            var succeededOp = new LazyOperation<int>(() => 0);
            var failedOp = new LazyOperation<int>(() => throw new SpecialException());

            //successful op with valid action, should skip the action and remain on the success path
            var newop = succeededOp.MapError(error => Task.Run(() => result = 1));
            await newop;
            Assert.AreEqual(0, result);

            //failed op with valid action, should return to success path
            newop = failedOp.MapError(error => Task.Run(() => result = 5));
            await newop;
            Assert.AreEqual(5, result);

            //successful op with failing error-map action, should skip the error-map and remain on the success path
            var newop2 = succeededOp.MapError(new Func<OperationError, Task>(error => throw new Exception()));
            await newop2; //no exception thrown

            //failed op with failing error-map action, should fail and remain on the error path
            newop2 = failedOp.MapError(new Func<OperationError, Task>(error => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop2);
        }

        [TestMethod]
        public async Task ResultMapErrorResult_WithOperation()
        {
            int result = 0;
            var succeededOp = new LazyOperation<int>(() => 0);
            var failedOp = new LazyOperation<int>(() => throw new SpecialException());

            //successful op with valid action, should skip the action and remain on the success path
            var newop = succeededOp.MapError(error => Operation.Try(() => result = 1));
            await newop;
            Assert.AreEqual(0, result);

            //failed op with valid action, should return to success path
            newop = failedOp.MapError(error => Operation.Try(() => result = 5));
            await newop;
            Assert.AreEqual(5, result);

            //successful op with failing error-map action, should skip the error-map and remain on the success path
            var newop2 = succeededOp.MapError(new Func<OperationError, Operation>(error => throw new Exception()));
            await newop2; //no exception thrown

            //failed op with failing error-map action, should fail and remain on the error path
            newop2 = failedOp.MapError(new Func<OperationError, Operation>(error => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop2);
        }
        #endregion

        #endregion

        #region Functional Tests
        [TestMethod]
        public async Task Try_ShouldNotExecutTillResolved()
        {
            var isExecuted = false;
            var op = Operation.Try(() => isExecuted = true);

            Assert.IsFalse(isExecuted);
            _ = await op;
            Assert.IsTrue(isExecuted);
        }

        [TestMethod]
        public async Task MapError_FromNonExecutedOperation_ShouldReturnANonExecutedOperation()
        {
            var op = Operation.Try(() => new Exception().Throw<int>());
            var eop = op.MapError(error => Console.WriteLine("mapped"));

            Assert.AreEqual(null, eop.Succeeded);
            await eop;
            Assert.AreEqual(true, eop.Succeeded);
            Assert.AreEqual(false, op.Succeeded);
        }
        #endregion


        public class SpecialException: Exception
        {
        }
    }
}
