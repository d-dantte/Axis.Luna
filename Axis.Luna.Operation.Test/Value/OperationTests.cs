using Axis.Luna.Extensions;
using Axis.Luna.Operation.Value;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test.Value
{
    [TestClass]
    public class OperationTests
    {
        #region FaultedOperation
        [TestMethod]
        public void NewFaultedOperation_ShouldReturnValidObject()
        {            
            var op = new FaultedOperation(new OperationError());
            Assert.IsNotNull(op);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(false, op.Succeeded);

            // null error
            Assert.ThrowsException<ArgumentNullException>(() => new FaultedOperation(null));
        }

        #region Awaiting
        [TestMethod]
        public async Task Await_WithValidAction_ShouldReturnProperly()
        {
            FaultedOperation op = new OperationError();
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
        }
        #endregion

        #region Resolving
        [TestMethod]
        public void Resolve_WithValidAction_ShouldReturnProperly()
        {
            FaultedOperation op = new OperationError();
            Assert.IsFalse(op.As<IResolvable>().TryResolve(out var error));
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(error, op.Error);
            Assert.ThrowsException<Exception>(() => op.As<IResolvable>().Resolve());

            op = new OperationError();
            Assert.ThrowsException<Exception>(() => op.As<IResolvable>().Resolve());
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
            Assert.IsFalse(op.As<IResolvable>().TryResolve(out _));
        }
        #endregion

        #region Then
        [TestMethod]
        public void Then_WithVoidResult()
        {
            var op = new FaultedOperation(new OperationError());

            //with valid action, should return the original operation
            var op2 = op.Then(Extensions.Common.NoOp);
            Assert.AreEqual(op, op2);
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);

            //with null action, should throw exception
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Action)null));

            //with failing action, should return the original operation
            op2 = op.Then(new Action(() => throw new Exception()));
            Assert.AreEqual(op, op2);
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
        }

        [TestMethod]
        public void Then_WithVoidTask()
        {
            var op = new FaultedOperation(new OperationError());

            //with valid action, should return the original operation
            var op2 = op.Then(() => Task.Delay(0));
            Assert.AreEqual(op, op2);
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);

            //with null action, should throw exception
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<Task>)null));

            //with failing action, should return the original operation
            op2 = op.Then(() => new Exception().Throw<Task>());
            Assert.AreEqual(op, op2);
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
        }

        [TestMethod]
        public void Then_WithVoidOperation()
        {
            var op = new FaultedOperation(new OperationError());

            //with valid action, should return the original operation
            var op2 = op.Then(Operation.FromVoid);
            Assert.AreEqual(op, op2);
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);

            //with null action, should throw exception
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<IOperation>)null));

            //with failing action, should return the original operation
            op2 = op.Then(() => new Exception().Throw<IOperation>());
            Assert.AreEqual(op, op2);
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
        }

        [TestMethod]
        public void Then_WithResult()
        {
            var op = new FaultedOperation(new OperationError());

            //with valid action, should return the original operation
            var op2 = op.Then(() => 2);
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);

            //with null action, should throw exception
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<int>)null));

            //with failing action, should return the original operation
            op2 = op.Then(() => new Exception().Throw<int>());
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);
        }

        [TestMethod]
        public void Then_WithResultTask()
        {
            var list = new List<int>();
            var op = new FaultedOperation(new OperationError());

            //with valid action, should return the original operation
            var op2 = op.Then(() => Task.FromResult(2));
            Assert.AreEqual(false, op2.Succeeded);
            Assert.IsNotNull(op2.Error);

            //with null action, should throw exception
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<Task<int>>)null));

            //with failing action, should return the original operation
            op2 = op.Then(() => new Exception().Throw<Task<int>>());
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
        }

        [TestMethod]
        public void Then_WithResultOperation()
        {
            var op = new FaultedOperation(new OperationError());

            //with valid action, should return the original operation
            var op2 = op.Then(() => Operation.FromResult(22));
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);

            //with null action, should throw exception
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<IOperation<int>>)null));

            //with failing action, should return the original operation
            op2 = op.Then(() => new Exception().Throw<IOperation<int>>());
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
        }
        #endregion

        #region MapError
        [TestMethod]
        public async Task MapError_WithAction()
        {
            int result = 0;
            var failedOp = new FaultedOperation(new OperationError());

            //errored op with valid action, should return to success path
            var newop = failedOp.MapError(error => { result = 5; });
            await newop;
            Assert.AreEqual(5, result);
            Assert.IsNull(newop.Error);
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
            var failedOp = new FaultedOperation(new OperationError());

            //failed op with valid action, should return to success path
            var newop = failedOp.MapError(error => Task.Run(() => { result = 5; }));
            await newop;
            Assert.AreEqual(5, result);
            Assert.IsNull(newop.Error);
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
            var failedOp = new FaultedOperation(new OperationError());

            //failed op with valid action, should return to success path
            var newop = failedOp.MapError(error => Operation.Try(() => { result = 5; }));
            await newop;
            Assert.AreEqual(5, result);
            Assert.IsNull(newop.Error);
            Assert.AreEqual(true, newop.Succeeded);

            //failed op with failing error-map action, should fail and remain on the error path
            newop = failedOp.MapError(new Func<OperationError, IOperation>(error => throw new Exception()));
            await Assert.ThrowsExceptionAsync<Exception>(async () => await newop);
            Assert.AreEqual(false, newop.Succeeded);
        }
        #endregion

        #endregion

        #region ValueOperation<TResult>
        [TestMethod]
        public void NewValueOperation_ShouldReturnValidObject()
        {
            //action
            var op = new ValueOperation<string>("");
            Assert.IsNotNull(op);
            Assert.AreEqual(true, op.Succeeded);
            Assert.IsNull(op.Error);

            ///failed operation
            op = new ValueOperation<string>(new OperationError());
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
            Assert.ThrowsException<ArgumentNullException>(() => new ValueOperation<string>((OperationError)null));
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
        }

        #region Awaiting
        [TestMethod]
        public async Task AwaitResult_WithValidAction_ShouldReturnProperly()
        {
            var op = new ValueOperation<int>(9);
            Assert.AreEqual(true, op.Succeeded);
            var result = await op;
            Assert.AreEqual(true, op.Succeeded);
            Assert.AreEqual(9, result);
        }

        [TestMethod]
        public async Task AwaitResult_WithFailingAction_ShouldReturnProperly()
        {
            var op = new ValueOperation<int>(new OperationError());
            Assert.AreEqual(false, op.Succeeded);
            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
        }
        #endregion

        #region Resolving
        [TestMethod]
        public void ResolveResult_WithValidAction_ShouldReturnProperly()
        {
            var op = new ValueOperation<int>(9);
            Assert.AreEqual(true, op.Succeeded);
            var result = op.As<IResolvable<int>>().Resolve();
            Assert.AreEqual(true, op.Succeeded);
            Assert.AreEqual(9, result);

            op = new ValueOperation<int>(8);
            Assert.AreEqual(true, op.Succeeded);
            Assert.IsTrue(op.As<IResolvable<int>>().TryResolve(out result, out _));
            Assert.AreEqual(true, op.Succeeded);
            Assert.AreEqual(8, result);
        }

        [TestMethod]
        public void ResolveResult_WithFailingAction_ShouldReturnProperly()
        {
            var op = new ValueOperation<int>(new OperationError());
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsFalse(op.As<IResolvable<int>>().TryResolve(out _, out var error));
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(error, op.Error);

            op = new ValueOperation<int>(new OperationError());
            Assert.AreEqual(false, op.Succeeded);
            Assert.ThrowsException<Exception>(() => op.As<IResolvable<int>>().Resolve());
            Assert.AreEqual(false, op.Succeeded);
            Assert.IsNotNull(op.Error);
        }
        #endregion

        #region Then
        [TestMethod]
        public async Task ThenResult_WithAction()
        {
            int value = 0;
            var op = new ValueOperation<int>(1);

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(r =>
            {
                value = 1;
            });

            await op2;
            Assert.AreEqual(1, value);

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
            var value = 0;
            var op = new ValueOperation<int>(1);

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(async r =>
            {
                await Task.Yield();
                value = r;
            });

            await op2;
            Assert.AreEqual(1, value);

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
            int value = 0;
            var op = new ValueOperation<int>(1);

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(r => Operation.Try(() => { value = r; }));
            await op2;
            Assert.AreEqual(1, value);

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
            var value = 0;
            var op = new ValueOperation<int>(3);

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(r => value = r);
            await op2;
            Assert.AreEqual(3, value);

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
            var value = 0;
            var op = new ValueOperation<int>(4);

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(async r =>
            {
                await Task.Yield();
                return value = r;
            });
            await op2;
            Assert.AreEqual(4, value);

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
            var value = 0;
            var op = new ValueOperation<int>(5);

            //with valid action, should execute sequentially after original operation
            var op2 = op.Then(r => Operation.Try(async () =>
            {
                await Task.Yield();
                return value = r;
            }));
            await op2;
            Assert.AreEqual(5, value);

            //with null action, should fail
            Assert.ThrowsException<ArgumentNullException>(() => op.Then((Func<int, IOperation<int>>)null));
        }
        #endregion

        #endregion
    }
}
