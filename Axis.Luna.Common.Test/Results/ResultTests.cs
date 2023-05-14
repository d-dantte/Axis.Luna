using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Axis.Luna.Common.Test.Results
{
    using BasicStruct = System.Collections.Generic.Dictionary<string, object>;

    [TestClass]
    public class ResultTests
    {
        [TestMethod]
        public void IResult_Of_CreatesResultInstance()
        {
            IResult<string> result = Result.Of("someting");

            Assert.IsNotNull(result);
            Assert.IsTrue(result is IResult<string>.DataResult);


            result = Result.Of<string>(new Exception());

            Assert.IsNotNull(result);
            Assert.IsTrue(result is IResult<string>.ErrorResult);
        }

        #region Error Result

        [TestMethod]
        public void Constructor_WithValidArgs_CreatesInstance()
        {
            var result = new IResult<int>.ErrorResult(new Exception());
            Assert.IsNotNull(result);

            result = new IResult<int>.ErrorResult(new Exception().WithErrorData(new BasicStruct { ["stuff"] = 54L }));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Constructor_WithInvalidArgs_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new IResult<int>.ErrorResult(null));
        }

        [TestMethod]
        public void Cause_ReturnsExceptionInstance()
        {
            var exception = new Exception("some exception");
            var result = new IResult<int>.ErrorResult(exception);

            Assert.AreEqual(exception, result.Cause().InnerException);
        }

        [TestMethod]
        public void Messge_ReturnsExceptionMessage()
        {
            var exception = new Exception("some exception");
            var result = new IResult<int>.ErrorResult(exception);

            Assert.AreEqual(exception.Message, result.Message);
        }

        [TestMethod]
        public void ErrorData_ReturnsErrorDataInstance()
        {
            var exception = new Exception("some exception");
            var errorData = new BasicStruct
            {
                ["prop1"] = "something",
                ["prop2"] = Guid.NewGuid(),
                ["prop3"] = 5.4m
            };
            var result = new IResult<int>.ErrorResult(exception.WithErrorData(errorData));

            Assert.AreEqual(errorData, result.ErrorData);

            result = new IResult<int>.ErrorResult(exception);
            Assert.IsNotNull(result.ErrorData);
        }

        [TestMethod]
        public void Equality_Test()
        {
            BasicStruct errorData = new BasicStruct
            {
                ["prop1"] = "something",
                ["prop2"] = Guid.NewGuid(),
                ["prop3"] = 5.4m
            };
            var result = new IResult<int>.ErrorResult(new Exception("some exception").WithErrorData(errorData));
            var result2 = new IResult<int>.ErrorResult(new Exception("some exception"));
            var result3 = new IResult<int>.ErrorResult(new Exception("some exception").WithErrorData(errorData));

            Assert.AreEqual(result, result);
            Assert.IsTrue(result.Equals(result));
            Assert.IsFalse(result.Equals(result2));
            Assert.IsFalse(result.Equals(result3));
        }
        #endregion

        #region Data Result

        [TestMethod]
        public void DataResult_Constructor_WithValidArgs_CreatesInstance()
        {
            var result = new IResult<int>.DataResult(45);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DataResult__Data_ReturnsExceptionMessage()
        {
            var date = DateTimeOffset.Now;
            var result = new IResult<DateTimeOffset>.DataResult(date);

            Assert.AreEqual(date, result.Data);
        }

        [TestMethod]
        public void DataResult_Equality_Test()
        {
            var result = new IResult<int>.DataResult(4);
            var result2 = new IResult<int>.DataResult(77);
            var result3 = new IResult<int>.DataResult(4);

            Assert.AreEqual(result, result);
            Assert.IsTrue(result.Equals(result));
            Assert.IsFalse(result.Equals(result2));
            Assert.IsTrue(result.Equals(result3));
        }
        #endregion

        #region Map

        [TestMethod]
        public void Map_WithValidArgs_SholdMapToOutputResult()
        {
            var inputResult = Result.Of("55");
            var outputResult = inputResult.Map(int.Parse);
            Assert.IsNotNull(outputResult);
            Assert.IsTrue(outputResult is IResult<int>.DataResult);
            Assert.AreEqual(55, outputResult.As<IResult<int>.DataResult>().Data);

            var outputResult2 = outputResult.Map(
                v => TimeSpan.FromHours(v),
                (e) => 1);
            Assert.IsNotNull(outputResult2);
            Assert.AreEqual(55, outputResult2.Resolve().TotalHours);


            inputResult = Result.Of<string>(new Exception());
            outputResult = inputResult.Map(int.Parse);
            Assert.IsNotNull(outputResult);
            Assert.IsTrue(outputResult is IResult<int>.ErrorResult);

            outputResult2 = outputResult.Map(
                v => TimeSpan.FromHours(v),
                (e) => 1);
            Assert.IsNotNull(outputResult2);
            Assert.IsTrue(outputResult2 is IResult<TimeSpan>.DataResult);
            Assert.AreEqual(1, outputResult2.Resolve().TotalHours);
        }

        [TestMethod]
        public void Map_WithFaultingMappers_ShouldReturnErrorResult()
        {
            var inputResult = Result.Of("55");
            var outputResult1 = inputResult.Map(v => new Exception().Throw<object>());
            Assert.IsNotNull(outputResult1);
            Assert.IsTrue(outputResult1 is IResult<object>.ErrorResult);

            var outputResult2 = outputResult1.Map(v => "", (e) => new Exception().Throw<string>());
            Assert.IsNotNull(outputResult2);
            Assert.IsTrue(outputResult2 is IResult<string>.ErrorResult);
        }

        [TestMethod]
        public void Map_WithInvalidArgs_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => Result.Of(2).Map((Func<int, float>)null, null));
        }

        #endregion

        #region Map Error

        [TestMethod]
        public void MapError_WithInvalidArgs_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => Result.Of(2).MapError(null));
        }

        [TestMethod]
        public void MapError_WithFaultingMappers_ShouldReturnErrorResult()
        {
            var inputResult = Result.Of<string>(new Exception());
            var outputResult1 = inputResult.MapError((e) => new Exception().Throw<string>());
            Assert.IsNotNull(outputResult1);
            Assert.IsTrue(outputResult1 is IResult<string>.ErrorResult);
        }

        [TestMethod]
        public void MapError_WithValidArgs_ShouldMapToOutputResult()
        {
            var inputResult = Result.Of<string>(new Exception());
            var outputResult1 = inputResult.MapError((e) => "blank");
            Assert.IsNotNull(outputResult1);
            Assert.AreEqual("blank", outputResult1.As<IResult<string>.DataResult>().Data);
        }
        #endregion

        #region Combine
        [TestMethod]
        public void Combine_ValidResults_Combines()
        {
            var result = Result.Of("me");
            var result2 = Result.Of(Guid.NewGuid());

            var combined = result.Combine(result2, (@string, guid) => $"name: {@string}, id: {guid}");
            Assert.IsNotNull(combined);
            Assert.IsTrue(combined is IResult<string>.DataResult);
            var data = (IResult<string>.DataResult)combined;
            Assert.AreEqual($"name: {result.Resolve()}, id: {result2.Resolve()}", data.Data);
        }

        [TestMethod]
        public void Combine_WithErroredResults_CombinesErrors()
        {
            var result = Result.Of("me");
            var exception1 = new Exception("1");
            var resultError = Result.Of<string>(exception1);

            var result2 = Result.Of(Guid.NewGuid());
            var exception2 = new Exception("2");
            var result2Error = Result.Of<Guid>(exception2);

            Func<string, Guid, string> combiner1 = (@string, guid) => $"name: {@string}, id: {guid}";
            Func<Guid, string, string> combiner2 = (guid, @string) => $"name: {@string}, id: {guid}";

            var combined1 = result.Combine(result2Error, combiner1);
            var combined2 = result2.Combine(resultError, combiner2);
            var combined3 = resultError.Combine(result2, combiner1);
            var combined4 = result2Error.Combine(result, combiner2);
            var combined5 = resultError.Combine(result2Error, combiner1);

            // combined 1
            Assert.IsTrue(combined1 is IResult<string>.ErrorResult);
            var stringError = (IResult<string>.ErrorResult)combined1;
            Assert.AreEqual(exception2, stringError.Cause().InnerException);

            // combined 2
            Assert.IsTrue(combined2 is IResult<string>.ErrorResult);
            stringError = (IResult<string>.ErrorResult)combined2;
            Assert.AreEqual(exception1, stringError.Cause().InnerException);

            // combined 3
            Assert.IsTrue(combined3 is IResult<string>.ErrorResult);
            stringError = (IResult<string>.ErrorResult)combined3;
            Assert.AreEqual(exception1, stringError.Cause().InnerException);

            // combined 4
            Assert.IsTrue(combined4 is IResult<string>.ErrorResult);
            stringError = (IResult<string>.ErrorResult)combined4;
            Assert.AreEqual(exception2, stringError.Cause().InnerException);

            // combined 5
            Assert.IsTrue(combined5 is IResult<string>.ErrorResult);
            stringError = (IResult<string>.ErrorResult)combined5;
            var aggregate = stringError.Cause().InnerException as AggregateException;
            Assert.IsNotNull(aggregate);
            Assert.IsTrue(Enumerable.SequenceEqual(
                aggregate.InnerExceptions,
                new[] { exception1, exception2}));
        }

        [TestMethod]
        public void Combine_WithInvalidArgs_ThrowsException()
        {
            var errorResult = Result.Of<string>(new Exception());
            var dataResult = Result.Of("me");

            // error result
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.Combine(
                (IResult<string>)null,
                (x, y) => ""));
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.Combine(
                Result.Of("stuff"),
                (Func<string, string, string>)null));

            // data result
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.Combine(
                (IResult<string>)null,
                (x, y) => ""));
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.Combine(
                Result.Of("stuff"),
                (Func<string, string, string>)null));
        }
        #endregion

        [TestMethod]
        public void MiscTest()
        {
            var result = Result.Of<int>(new Exception());

            var x = result
                .Map(i => true)
                .MapError(err => false)
                .Resolve();

            Assert.IsFalse(x);
        }
    }

}
