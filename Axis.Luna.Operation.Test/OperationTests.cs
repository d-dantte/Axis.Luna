using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test
{
    [TestClass]
    public class OperationTests
    {
        [TestMethod]
        public async Task FromResult_Should_ReturnValidOperation()
        {
            var result = 5;
            var op = Operation.FromResult(result);

            Assert.IsNotNull(op);
            Assert.IsTrue(op.Succeeded == true);
            Assert.IsNull(op.Error);
            Assert.IsNotNull(op.GetAwaiter());

            Assert.AreEqual(result, await op);
        }

        [TestMethod]
        public async Task FromVoid_Should_ReturnValidOperation()
        {
            var op = Operation.FromVoid();

            Assert.IsNotNull(op);
            Assert.IsNull(op.Error);
            Assert.IsNotNull(op.GetAwaiter());

            await op;
            Assert.IsTrue(op.Succeeded == true);
        }

        [TestMethod]
        public async Task Fail_WithException_Should_ReturnValidFailedOperation()
        {
            var exception = new Exception("failed operation");
            var op = Operation.Fail(exception);

            Assert.IsNotNull(op);
            Assert.IsTrue(op.Succeeded == false);
            Assert.IsNotNull(op.GetAwaiter());
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(exception, op.Error.GetException());

            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);

            op = Operation.Fail();

            Assert.IsNotNull(op);
            Assert.IsTrue(op.Succeeded == false);
            Assert.IsNotNull(op.GetAwaiter());
            Assert.IsNotNull(op.Error);

            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
        }


        [TestMethod]
        public async Task Fail_WithResultException_Should_ReturnValidFailedOperation()
        {
            var exception = new Exception("failed operation");
            var op = Operation.Fail<int>(exception);

            Assert.IsNotNull(op);
            Assert.IsTrue(op.Succeeded == false);
            Assert.IsNotNull(op.GetAwaiter());
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(exception, op.Error.GetException());

            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);

            op = Operation.Fail<int>();

            Assert.IsNotNull(op);
            Assert.IsTrue(op.Succeeded == false);
            Assert.IsNotNull(op.GetAwaiter());
            Assert.IsNotNull(op.Error);

            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
        }


        [TestMethod]
        public async Task Fail_WithErrorMessage_Should_ReturnValidFailedOperation()
        {
            var message = "failed operation";
            var op = Operation.Fail(message);

            Assert.IsNotNull(op);
            Assert.IsTrue(op.Succeeded == false);
            Assert.IsNotNull(op.GetAwaiter());
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(message, op.Error.GetException().Message);

            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
        }

        [TestMethod]
        public async Task Fail_WithResultErrorMessage_Should_ReturnValidFailedOperation()
        {
            var message = "failed operation";
            var op = Operation.Fail<int>(message);

            Assert.IsNotNull(op);
            Assert.IsTrue(op.Succeeded == false);
            Assert.IsNotNull(op.GetAwaiter());
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(message, op.Error.GetException().Message);

            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
        }



        [TestMethod]
        public async Task Try_WithFuncResult_Should_ReturnValidOperation()
        {
            var value = 3;
            var op = Operation.Try(() => value);

            Assert.IsNotNull(op);
            Assert.IsNull(op.Error);
            Assert.IsNotNull(op.GetAwaiter());

            var result = await op;
            Assert.IsTrue(op.Succeeded == true);
            Assert.AreEqual(value, result);
        }
        [TestMethod]
        public async Task Try_WithErroredFuncResult_Should_ReturnValidOperation()
        {
            var exception = new Exception();
            var op = Operation.Try(() => exception.Throw<int>());

            Assert.IsNotNull(op);
            Assert.IsNotNull(op.GetAwaiter());

            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.IsTrue(op.Succeeded == false);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(exception, op.Error.GetException());
        }


        [TestMethod]
        public async Task Try_WithAction_Should_ReturnValidOperation()
        {
            var op = Operation.Try(() => { var d = DateTime.Now; });

            Assert.IsNotNull(op);
            Assert.IsNull(op.Error);
            Assert.IsNotNull(op.GetAwaiter());

            await op;
            Assert.IsTrue(op.Succeeded == true);
        }
        [TestMethod]
        public async Task Try_WithErroredAction_Should_ReturnValidOperation()
        {
            var exception = new Exception();
            var op = Operation.Try(() => exception.Throw());

            Assert.IsNotNull(op);
            Assert.IsNotNull(op.GetAwaiter());

            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.IsTrue(op.Succeeded == false);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(exception, op.Error.GetException());
        }



        [TestMethod]
        public async Task Try_WithAsyncFuncResult_Should_ReturnValidOperation()
        {
            var value = 3;
            var op = Operation.Try(() => Task.Run(() => value));

            Assert.IsNotNull(op);
            Assert.IsNull(op.Error);
            Assert.IsNotNull(op.GetAwaiter());

            var result = await op;
            Assert.IsTrue(op.Succeeded == true);
            Assert.AreEqual(value, result);
        }
        [TestMethod]
        public async Task Try_WithErroredAsyncFuncResult_Should_ReturnValidOperation()
        {
            var exception = new Exception();
            var op = Operation.Try(() => Task.Run(() => exception.Throw<int>()));

            Assert.IsNotNull(op);
            Assert.IsNotNull(op.GetAwaiter());

            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.IsTrue(op.Succeeded == false);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(exception, op.Error.GetException());
        }



        [TestMethod]
        public async Task Try_WithAsyncAction_Should_ReturnValidOperation()
        {
            var op = Operation.Try(() => Task.Run(() => { }));

            Assert.IsNotNull(op);
            Assert.IsNull(op.Error);
            Assert.IsNotNull(op.GetAwaiter());

            await op;
            Assert.IsTrue(op.Succeeded == true);
            Assert.IsNull(op.Error);
        }
        [TestMethod]
        public async Task Try_WithErroredAsyncAction_Should_ReturnValidOperation()
        {
            var exception = new Exception();
            var op = Operation.Try(() => Task.Run(() => exception.Throw()));

            Assert.IsNotNull(op);
            Assert.IsNotNull(op.GetAwaiter());

            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.IsTrue(op.Succeeded == false);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(exception, op.Error.GetException());
        }



        [TestMethod]
        public async Task Try_WithTaskResult_Should_ReturnValidOperation()
        {
            var value = 3;
            var op = Operation.Try(Task.Run(() => value));

            Assert.IsNotNull(op);
            Assert.IsNull(op.Error);
            Assert.IsNotNull(op.GetAwaiter());

            var result = await op;
            Assert.IsTrue(op.Succeeded == true);
            Assert.AreEqual(value, result);
        }
        [TestMethod]
        public async Task Try_WithErroredTaskResult_Should_ReturnValidOperation()
        {
            var exception = new Exception();
            var op = Operation.Try(Task.Run(() => exception.Throw<int>()));

            Assert.IsNotNull(op);
            Assert.IsNotNull(op.GetAwaiter());

            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.IsTrue(op.Succeeded == false);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(exception, op.Error.GetException());
        }



        [TestMethod]
        public async Task Try_WithTask_Should_ReturnValidOperation()
        {
            var op = Operation.Try(Task.Run(() => { }));

            Assert.IsNotNull(op);
            Assert.IsNull(op.Error);
            Assert.IsNotNull(op.GetAwaiter());

            await op;
            Assert.IsTrue(op.Succeeded == true);
            Assert.IsNull(op.Error);
        }
        [TestMethod]
        public async Task Try_WithErroredTask_Should_ReturnValidOperation()
        {
            var exception = new Exception();
            var op = Operation.Try(Task.Run(() => exception.Throw()));

            Assert.IsNotNull(op);
            Assert.IsNotNull(op.GetAwaiter());

            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.IsTrue(op.Succeeded == false);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(exception, op.Error.GetException());
        }



        [TestMethod]
        public async Task Try_WithOperationResult_Should_ReturnValidOperation()
        {
            var value = 3;
            var op = Operation.Try(() => Operation.Try(() => value));

            Assert.IsNotNull(op);
            Assert.IsNull(op.Error);
            Assert.IsNotNull(op.GetAwaiter());

            var result = await op;
            Assert.IsTrue(op.Succeeded == true);
            Assert.AreEqual(value, result);
        }
        [TestMethod]
        public async Task Try_WithErroredOperationResult_Should_ReturnValidOperation()
        {
            var exception = new Exception();
            var op = Operation.Try(() => Operation.Try(() => exception.Throw<int>()));

            Assert.IsNotNull(op);
            Assert.IsNotNull(op.GetAwaiter());

            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.IsTrue(op.Succeeded == false);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(exception, op.Error.GetException());
        }



        [TestMethod]
        public async Task Try_WithOperation_Should_ReturnValidOperation()
        {
            var op = Operation.Try(() => Operation.Try(() => { }));

            Assert.IsNotNull(op);
            Assert.IsNull(op.Error);
            Assert.IsNotNull(op.GetAwaiter());

            await op;
            Assert.IsTrue(op.Succeeded == true);
            Assert.IsNull(op.Error);
        }
        [TestMethod]
        public async Task Try_WithErroredOperation_Should_ReturnValidOperation()
        {
            var exception = new Exception();
            var op = Operation.Try(() => Operation.Try(() => exception.Throw()));

            Assert.IsNotNull(op);
            Assert.IsNotNull(op.GetAwaiter());

            await Assert.ThrowsExceptionAsync<Exception>(async () => await op);
            Assert.IsTrue(op.Succeeded == false);
            Assert.IsNotNull(op.Error);
            Assert.AreEqual(exception, op.Error.GetException());
        }

    }
}
