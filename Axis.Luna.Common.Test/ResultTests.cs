using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;

namespace Axis.Luna.Common.Test
{

    [TestClass]
    public class ResultTests
    {
        [TestMethod]
        public void IResult_Of_CreatesResultInstance()
        {
            IResult<string> result = Result.Of("someting");

            Assert.IsNotNull(result);
            Assert.IsTrue(result is IResult<string>.DataResult);


            result = Result.Of<string>(new System.Exception());

            Assert.IsNotNull(result);
            Assert.IsTrue(result is IResult<string>.ErrorResult);
        }

        #region Error Result

        [TestMethod]
        public void Constructor_WithValidArgs_CreatesInstance()
        {
            var result = new IResult<int>.ErrorResult(new System.Exception());
            Assert.IsNotNull(result);

            result = new IResult<int>.ErrorResult(new Exception().WithErrorData(new ErrorData { Stuff = 54L }));
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

            Assert.AreEqual(exception, result.Cause());
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
            var errorData = new ErrorData
            {
                Name = "something",
                Id = Guid.NewGuid(),
                Something = 5.4m
            };
            var result = new IResult<int>.ErrorResult(exception.WithErrorData(errorData));

            Assert.AreEqual(errorData, result.ErrorData);

            result = new IResult<int>.ErrorResult(exception);
            Assert.IsNotNull(result.ErrorData);
        }

        [TestMethod]
        public void Equality_Test()
        {
            var errorData = new ErrorData
            {
                Name = "something",
                Id = Guid.NewGuid(),
                Something = 5.4m
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

        #region Json
        [TestMethod]
        public void ErrorResult_ToJson()
        {
            var result = Result.Of<string>(new Exception("Issues occured").WithErrorData(new ErrorData
            {
                Stuff = 54,
                Name = "me",
                Id = Guid.NewGuid(),
                Something = 23m
            }));

            var json = JsonConvert.SerializeObject(result);
        }
        #endregion
    }

    public class ErrorData
    {
        public long Stuff { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }
        public decimal Something { get; set; }
    }
}
