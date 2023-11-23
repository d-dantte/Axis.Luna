using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Axis.Luna.Common.Test.Results
{

    [TestClass]
    public class ResultTests
    {
        #region Of
        [TestMethod]
        public void Of_Tests()
        {
            var result = Result.Of(50m);
            Assert.IsInstanceOfType<DataResult<decimal>>(result);

            result = Result.Of<decimal>(new Exception());
            Assert.IsInstanceOfType<ErrorResult<decimal>>(result);

            Assert.ThrowsException<ArgumentNullException>(() => Result.Of(default(Func<decimal>)));

            result = Result.Of(() => 50m);
            Assert.IsInstanceOfType<DataResult<decimal>>(result);

            result = Result.Of(() => new Exception().Throw<decimal>());
            Assert.IsInstanceOfType<ErrorResult<decimal>>(result);

            Assert.ThrowsException<ArgumentNullException>(() => Result.Of(default(Func<IResult<decimal>>)));

            result = Result.Of(() => Result.Of(50m));
            Assert.IsInstanceOfType<DataResult<decimal>>(result);

            result = Result.Of(() => new Exception().Throw<IResult<decimal>>());
            Assert.IsInstanceOfType<ErrorResult<decimal>>(result);
        }
        #endregion

        #region Resolve
        [TestMethod]
        public void Resolve_Tests()
        {
            var result = default(IResult<int>);
            Assert.ThrowsException<ArgumentNullException>(() => result.Resolve());

            result = Result.Of(5);
            Assert.AreEqual(5, result.Resolve());

            result = Result.Of<int>(new AggregateException());
            Assert.ThrowsException<AggregateException>(() => result.Resolve());

            result = new UnknownResult<int>();
            Assert.ThrowsException<InvalidOperationException>(() => result.Resolve());
        }
        #endregion

        #region Is
        [TestMethod]
        public void Is_Tests()
        {
            var nullResult = default(IResult<string>);
            var dataResult = Result.Of("stuff");
            var errorResult = Result.Of<string>(new AccessViolationException("absolute violation"));
            var unknownResult = new UnknownResult<string>();

            // null
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.IsDataResult());
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.IsDataResult(out string _));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.IsErrorResult());
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.IsErrorResult(out Exception _));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.IsErrorResult(out AccessViolationException _));

            // data
            Assert.IsTrue(dataResult.IsDataResult());
            Assert.IsTrue(dataResult.IsDataResult(out string data) && "stuff".Equals(data));
            Assert.IsFalse(dataResult.IsErrorResult());
            Assert.IsFalse(dataResult.IsErrorResult(out Exception _));
            Assert.IsFalse(dataResult.IsErrorResult(out AccessViolationException _));
            Assert.IsFalse(dataResult.IsErrorResult(out InvalidOperationException _));

            // error
            Assert.IsFalse(errorResult.IsDataResult());
            Assert.IsFalse(errorResult.IsDataResult(out data) && "stuff".Equals(data));
            Assert.IsTrue(errorResult.IsErrorResult());
            Assert.IsTrue(errorResult.IsErrorResult(out Exception _));
            Assert.IsTrue(errorResult.IsErrorResult(out AccessViolationException ex) && "absolute violation".Equals(ex.Message));
            Assert.IsFalse(errorResult.IsErrorResult(out InvalidOperationException _));

            // unkonwn
            Assert.IsFalse(unknownResult.IsDataResult());
            Assert.IsFalse(unknownResult.IsDataResult(out string _));
            Assert.IsFalse(unknownResult.IsErrorResult());
            Assert.IsFalse(unknownResult.IsErrorResult(out Exception _));
            Assert.IsFalse(unknownResult.IsErrorResult(out AccessViolationException _));
            Assert.IsFalse(unknownResult.IsErrorResult(out InvalidOperationException _));
        }
        #endregion

        #region With
        [TestMethod]
        public void With_Tests()
        {
            var nullResult = default(IResult<string>);
            var dataResult = Result.Of("stuff");
            var errorResult = Result.Of<string>(new Ex("eee"));
            var unknownResult = new UnknownResult<string>();

            // null
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.WithData(Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.WithError(Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.WithError<string, Ex>(Console.WriteLine));

            // data
            string assignedData = null;
            var returnedResult = dataResult.WithData(d => assignedData = d);
            Assert.AreEqual("stuff", assignedData);
            Assert.AreEqual(dataResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.WithData(null));

            Exception assignedError = null;
            returnedResult = dataResult.WithError(e => assignedError = e);
            Assert.IsNull(assignedError);
            Assert.AreEqual(dataResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.WithError(null));

            assignedError = null;
            returnedResult = dataResult.WithError<string, Ex>(e => assignedError = e);
            Assert.IsNull(assignedError);
            Assert.AreEqual(dataResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.WithError<string, Ex>(null));

            assignedError = null;
            returnedResult = dataResult.WithError<string, ArgumentException>(e => assignedError = e);
            Assert.IsNull(assignedError);
            Assert.AreEqual(dataResult, returnedResult);

            // error
            assignedData = null;
            returnedResult = errorResult.WithData(d => assignedData = d);
            Assert.IsNull(assignedData);
            Assert.AreEqual(errorResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.WithData(null));

            assignedError = null;
            returnedResult = errorResult.WithError(e => assignedError = e);
            Assert.AreEqual("eee", assignedError.Message);
            Assert.IsInstanceOfType<Ex>(assignedError);
            Assert.AreEqual(errorResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.WithError(null));

            assignedError = null;
            returnedResult = errorResult.WithError<string, Ex>(e => assignedError = e);
            Assert.AreEqual("eee", assignedError.Message);
            Assert.IsInstanceOfType<Ex>(assignedError);
            Assert.AreEqual(errorResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.WithError<string, Ex>(null));

            assignedError = null;
            returnedResult = errorResult.WithError<string, ArgumentException>(e => assignedError = e);
            Assert.IsNull(assignedError);
            Assert.AreEqual(errorResult, returnedResult);

            // unknown
            Assert.ThrowsException<ArgumentException>(() => unknownResult.WithData(d => assignedData = d));
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.WithData(null));

            Assert.ThrowsException<ArgumentException>(() => unknownResult.WithError(e => assignedError = e));
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.WithError(null));

            Assert.ThrowsException<ArgumentException>(() => unknownResult.WithError<string, Ex>(e => assignedError = e));
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.WithError<string, Ex>(null));

            Assert.ThrowsException<ArgumentException>(() => unknownResult.WithError<string, ArgumentException>(e => assignedError = e));
        }
        #endregion

        #region MapError
        [TestMethod]
        public void MapError_Tests()
        {
            var nullResult = default(IResult<string>);
            var dataResult = Result.Of("stuff");
            var errorResult = Result.Of<string>(new Ex("eee"));
            var unknownResult = new UnknownResult<string>();

            // null
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.MapError(e => e.ToString()));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.MapError((Ex e) => e.ToString()));

            // data
            string mappedResult = null;
            var returnedResult = dataResult.MapError(e => mappedResult = e.Message);
            Assert.AreEqual(dataResult, returnedResult);
            Assert.IsNull(mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.MapError(null));

            mappedResult = null;
            returnedResult = dataResult.MapError((Ex e) => mappedResult = e.Message);
            Assert.AreEqual(dataResult, returnedResult);
            Assert.IsNull(mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.MapError<string, Ex>(null));

            mappedResult = null;
            returnedResult = dataResult.MapError((Fx e) => mappedResult = e.Message);
            Assert.AreEqual(dataResult, returnedResult);
            Assert.IsNull(mappedResult);


            // error
            mappedResult = null;
            returnedResult = errorResult.MapError(e => mappedResult = e.Message);
            Assert.AreNotEqual(errorResult, returnedResult);
            Assert.AreEqual("eee", mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.MapError(null));

            mappedResult = null;
            returnedResult = errorResult.MapError((Ex e) => mappedResult = e.Message);
            Assert.AreNotEqual(errorResult, returnedResult);
            Assert.AreEqual("eee", mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.MapError<string, Ex>(null));

            mappedResult = null;
            returnedResult = errorResult.MapError((Fx e) => mappedResult = e.Message);
            Assert.AreEqual(errorResult, returnedResult);
            Assert.IsNull(mappedResult);


            // error
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.MapError(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.MapError(e => e.Message));

            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.MapError<string, Ex>(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.MapError<string, Ex>(e => e.Message));
        }
        #endregion

        #region BindError
        [TestMethod]
        public void BindError_Tests()
        {
            var nullResult = default(IResult<string>);
            var dataResult = Result.Of("stuff");
            var errorResult = Result.Of<string>(new Ex("eee"));
            var unknownResult = new UnknownResult<string>();

            // null
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.BindError(e => Result.Of(e.Message)));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.BindError((Ex e) => Result.Of(e.Message)));

            // data
            string mappedResult = null;
            var returnedResult = dataResult.BindError(e => Result.Of(mappedResult = e.Message));
            Assert.AreEqual(dataResult, returnedResult);
            Assert.IsNull(mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.BindError(null));

            mappedResult = null;
            returnedResult = dataResult.BindError((Ex e) => Result.Of(mappedResult = e.Message));
            Assert.AreEqual(dataResult, returnedResult);
            Assert.IsNull(mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.BindError<string, Ex>(null));

            mappedResult = null;
            returnedResult = dataResult.BindError((Fx e) => Result.Of(mappedResult = e.Message));
            Assert.AreEqual(dataResult, returnedResult);
            Assert.IsNull(mappedResult);


            // error
            mappedResult = null;
            returnedResult = errorResult.BindError(e => Result.Of(mappedResult = e.Message));
            Assert.AreNotEqual(errorResult, returnedResult);
            Assert.AreEqual("eee", mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.BindError(null));

            mappedResult = null;
            returnedResult = errorResult.BindError((Ex e) => Result.Of(mappedResult = e.Message));
            Assert.AreNotEqual(errorResult, returnedResult);
            Assert.AreEqual("eee", mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.BindError<string, Ex>(null));

            mappedResult = null;
            returnedResult = errorResult.BindError((Fx e) => Result.Of(mappedResult = e.Message));
            Assert.AreEqual(errorResult, returnedResult);
            Assert.IsNull(mappedResult);


            // error
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.BindError(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.BindError(e => Result.Of(e.Message)));

            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.BindError<string, Ex>(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.BindError<string, Ex>(e => Result.Of(e.Message)));
        }
        #endregion

        #region ConsumeError
        [TestMethod]
        public void ConsumeError_Tests()
        {
            var nullResult = default(IResult<string>);
            var dataResult = Result.Of("stuff");
            var errorResult = Result.Of<string>(new Ex("eee"));
            var unknownResult = new UnknownResult<string>();

            // null
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.ConsumeError(e => e.ToString()));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.ConsumeError((Ex e) => e.ToString()));

            // data
            string mappedResult = null;
            dataResult.ConsumeError(e => mappedResult = e.Message);
            Assert.IsNull(mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.ConsumeError(null));

            mappedResult = null;
            dataResult.ConsumeError((Ex e) => mappedResult = e.Message);
            Assert.IsNull(mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.ConsumeError<string, Ex>(null));

            mappedResult = null;
            dataResult.ConsumeError((Fx e) => mappedResult = e.Message);
            Assert.IsNull(mappedResult);


            // error
            mappedResult = null;
            errorResult.ConsumeError(e => mappedResult = e.Message);
            Assert.AreEqual("eee", mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.ConsumeError(null));

            mappedResult = null;
            errorResult.ConsumeError((Ex e) => mappedResult = e.Message);
            Assert.AreEqual("eee", mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.ConsumeError<string, Ex>(null));

            mappedResult = null;
            errorResult.ConsumeError((Fx e) => mappedResult = e.Message);
            Assert.IsNull(mappedResult);


            // error
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.ConsumeError(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.ConsumeError(Console.Write));

            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.ConsumeError<string, Ex>(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.ConsumeError<string, Ex>(Console.Write));
        }
        #endregion

        #region TransformError
        [TestMethod]
        public void TransformError_Tests()
        {
            var nullResult = default(IResult<string>);
            var dataResult = Result.Of("stuff");
            var errorResult = Result.Of<string>(new Ex("eee"));
            var unknownResult = new UnknownResult<string>();

            // null
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.TransformError(e => new Fx(e.Message)));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.TransformError((Ex e) => new Fx(e.Message)));

            // data
            var returnedResult = dataResult.TransformError(e => new Fx(e.Message));
            Assert.AreEqual(dataResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.TransformError(null));

            returnedResult = dataResult.TransformError((Ex e) => new Fx(e.Message));
            Assert.AreEqual(dataResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.TransformError<string, Ex>(null));

            returnedResult = dataResult.TransformError((Fx e) => new Fx(e.Message));
            Assert.AreEqual(dataResult, returnedResult);


            // error
            returnedResult = errorResult.TransformError(e => new Fx(e.Message));
            Assert.AreNotEqual(errorResult, returnedResult);
            Assert.IsInstanceOfType<ErrorResult<string>>(returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.TransformError(null));

            returnedResult = errorResult.TransformError((Ex e) => new Fx(e.Message));
            Assert.AreNotEqual(errorResult, returnedResult);
            Assert.IsInstanceOfType<ErrorResult<string>>(returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.TransformError<string, Ex>(null));

            returnedResult = errorResult.TransformError((Fx e) => new Fx(e.Message));
            Assert.AreEqual(errorResult, returnedResult);


            // error
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.TransformError(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.TransformError(e => new Fx(e.Message)));

            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.TransformError<string, Ex>(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.TransformError<string, Ex>(e => new Fx(e.Message)));
        }
        #endregion

        #region Continue
        [TestMethod]
        public void Continue_Tests()
        {
            var nullResult = default(IResult<string>);
            var dataResult = Result.Of("stuff");
            var errorResult = Result.Of<string>(new Ex("eee"));
            var unknownResult = new UnknownResult<string>();

            // null
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.Continue(e => e.ToString().Length));

            // data
            var returnedResult = dataResult.Continue(e => e.ToString().Length);
            Assert.AreEqual(5, returnedResult.Resolve());
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.Continue<string, int>(null));


            // error
            returnedResult = errorResult.Continue(e => (e as Ex).Message.Length);
            Assert.AreEqual(3, returnedResult.Resolve());
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.Continue<string, int>(null));


            // unknown
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.Continue<string, int>(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.Continue(e => e.ToString().Length));
        }
        #endregion

        #region Fold
        #endregion

        #region Nested Test Types
        internal class UnknownResult<T> : IResult<T>
        {
            public IResult<TOut> Bind<TOut>(Func<T, IResult<TOut>> binder)
            {
                throw new NotImplementedException();
            }

            public void Consume(Action<T> consumer)
            {
                throw new NotImplementedException();
            }

            public IResult<TOut> Map<TOut>(Func<T, TOut> mapper)
            {
                throw new NotImplementedException();
            }
        }

        internal class Ex : Exception
        {
            public Ex(string message)
            : base(message) { }
        }

        internal class Fx : Exception
        {
            public Fx(string message)
            : base(message) { }
        }
        #endregion
    }

}
