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
            Assert.ThrowsException<ArgumentNullException>(() => new LazyOperation(null));
        }

        #region Awaiting
        [TestMethod]
        public async Task Await_WithValidAction_ShouldReturnProperly()
        {
            var op = new LazyOperation(() => { });
            Assert.IsNull(op.Succeeded);
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
            Assert.IsNull(op.Succeeded);
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
        }
        #endregion

        #region Resolving
        [TestMethod]
        public void Resolve_WithValidAction_ShouldReturnProperly()
        {
            var op = new LazyOperation(Extensions.Common.NoOp);
            Assert.IsNull(op.Succeeded);
            op.As<IResolvable>().Resolve();
            Assert.AreEqual(true, op.Succeeded);

            op = new LazyOperation(Extensions.Common.NoOp);
            Assert.IsNull(op.Succeeded);
            Assert.IsTrue(op.As<IResolvable>().TryResolve(out _));
            Assert.AreEqual(true, op.Succeeded);
        }

        [TestMethod]
        public void Resolve_WithFailingAction_ShouldReturnProperly()
        {
            var op = new LazyOperation(() => throw new Exception());
            Assert.IsNull(op.Succeeded);
            Assert.IsFalse(op.As<IResolvable>().TryResolve(out _));
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
            Assert.ThrowsException<Exception>(() => op.As<IResolvable>().Resolve());

            op = new LazyOperation(() => throw new Exception());
            Assert.IsNull(op.Succeeded);
            Assert.ThrowsException<Exception>(() => op.As<IResolvable>().Resolve());
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
            Assert.IsFalse(op.As<IResolvable>().TryResolve(out _));
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

            //with null action, should throw exception
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
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<IOperation>)null));

            //with failing action should fail
            op2 = op.Then(new Func<IOperation>(() => throw new Exception()));
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
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<IOperation<int>>)null));
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
            var newop = succeededOp.MapError(error => { result = 1; });
            await newop;
            Assert.AreEqual(0, result);
            Assert.IsNull(newop.Error);
            Assert.AreEqual(true, newop.Succeeded);

            //errored op with valid action, should return to success path
            newop = failedOp.MapError(error => { result = 5; });
            await newop;
            Assert.AreEqual(5, result);
            Assert.IsNull(newop.Error);
            Assert.AreEqual(true, newop.Succeeded);

            //successful op with failing error-map action, should stay on the success path
            newop = succeededOp.MapError(new Action<OperationError>(error => new Exception().Throw()));
            await newop; //no exception thrown
            Assert.AreEqual(true, newop.Succeeded);

            //failed op with failed error-map action, should stay on the error path
            newop = failedOp.MapError(new Action<OperationError>(error => new Exception().Throw()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop);
            Assert.AreEqual(false, newop.Succeeded);
        }

        [TestMethod]
        public async Task MapError_WithTask()
        {
            int result = 0;
            var succeededOp = new LazyOperation(() => { });
            var failedOp = new LazyOperation(() => throw new SpecialException());

            //successful op with valid action, should skip the action and remain on the success path
            var newop = succeededOp.MapError(error => Task.Run(() => { result = 1; }));
            await newop;
            Assert.AreEqual(0, result);
            Assert.IsNull(newop.Error);
            Assert.AreEqual(true, newop.Succeeded);

            //failed op with valid action, should return to success path
            newop = failedOp.MapError(error => Task.Run(() => { result = 5; }));
            await newop;
            Assert.AreEqual(5, result);
            Assert.IsNull(newop.Error);
            Assert.AreEqual(true, newop.Succeeded);

            //successful op with failing error-map action, should skip the error-map and remain on the success path
            newop = succeededOp.MapError(new Func<OperationError, Task>(error => throw new Exception()));
            await newop; //no exception thrown
            Assert.AreEqual(true, newop.Succeeded);

            //failed op with failing error-map action, should fail and remain on the error path
            newop = failedOp.MapError(new Func<OperationError, Task>(error => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop);
            Assert.AreEqual(false, newop.Succeeded);
        }

        [TestMethod]
        public async Task MapError_WithOperation()
        {
            int result = 0;
            var succeededOp = new LazyOperation(() => { });
            var failedOp = new LazyOperation(() => throw new SpecialException());

            //successful op with valid action, should skip the action and remain on the success path
            var newop = succeededOp.MapError(error => Operation.Try(() => { result = 1; }));
            await newop;
            Assert.AreEqual(0, result);
            Assert.IsNull(newop.Error);
            Assert.AreEqual(true, newop.Succeeded);

            //failed op with valid action, should return to success path
            newop = failedOp.MapError(error => Operation.Try(() => { result = 5; }));
            await newop;
            Assert.AreEqual(5, result);
            Assert.IsNull(newop.Error);
            Assert.AreEqual(true, newop.Succeeded);

            //successful op with failing error-map action, should skip the error-map and remain on the success path
            newop = succeededOp.MapError(new Func<OperationError, IOperation>(error => throw new Exception()));
            await newop; //no exception thrown
            Assert.AreEqual(true, newop.Succeeded);

            //failed op with failing error-map action, should fail and remain on the error path
            newop = failedOp.MapError(new Func<OperationError, IOperation>(error => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop);
            Assert.AreEqual(false, newop.Succeeded);
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
            Assert.IsNull(op.Succeeded);
            Assert.IsNull(op.Error);

            ///failed operation
            //async action
            op = new LazyOperation<string>(() => throw new Exception());
            Assert.IsNull(op.Succeeded);
            Assert.IsNull(op.Error);
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
        }

        #region Awaiting
        [TestMethod]
        public async Task AwaitResult_WithValidAction_ShouldReturnProperly()
        {
            var op = new LazyOperation<int>(() => 9);
            Assert.IsNull(op.Succeeded);
            var result = await op;
            Assert.AreEqual(true, op.Succeeded);
            Assert.AreEqual(9, result);
        }

        [TestMethod]
        public async Task AwaitResult_WithFailingAction_ShouldReturnProperly()
        {
            var op = new LazyOperation<int>(() => throw new Exception());
            Assert.IsNull(op.Succeeded);
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
        }
        #endregion

        #region Resolving
        [TestMethod]
        public void ResolveResult_WithValidAction_ShouldReturnProperly()
        {
            var op = new LazyOperation<int>(() => 9);
            Assert.IsNull(op.Succeeded);
            var result = op.As<IResolvable<int>>().Resolve();
            Assert.AreEqual(true, op.Succeeded);
            Assert.AreEqual(9, result);

            op = new LazyOperation<int>(() => 8);
            Assert.IsNull(op.Succeeded);
            Assert.IsTrue(op.As<IResolvable<int>>().TryResolve(out result, out _));
            Assert.AreEqual(true, op.Succeeded);
            Assert.AreEqual(8, result);
        }

        [TestMethod]
        public void ResolveResult_WithFailingAction_ShouldReturnProperly()
        {
            var op = new LazyOperation<int>(() => throw new Exception());
            Assert.IsNull(op.Succeeded);
            Assert.IsFalse(op.As<IResolvable<int>>().TryResolve(out _, out var error));
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(error, op.Error);

            op = new LazyOperation<int>(() => throw new Exception());
            Assert.IsNull(op.Succeeded);
            Assert.ThrowsException<Exception>(() => op.As<IResolvable<int>>().Resolve());
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
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Action<int>)null));

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
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<int, Task>)null));

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
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<int, IOperation>)null));

            //with failing action should fail
            op2 = op.Then(new Func<int, IOperation>(r => throw new Exception()));
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
            var op = new LazyOperation<int>(() =>
            {
                list.Add(1);
                return 5;
            });

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(async value =>
            {
                list.Add(2);
                await Task.Yield();
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
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<int, IOperation<int>>)null));
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
            var eop = op.MapError(error => { Console.WriteLine("mapped"); return 0; });

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
