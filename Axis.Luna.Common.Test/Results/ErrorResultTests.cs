using Axis.Luna.Common.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Axis.Luna.Extensions;
using System;
using System.IO;
namespace Axis.Luna.Common.Test.Results
{
    [TestClass]
    public class ErrorResultTests
    {
        [TestMethod]
        public void Construction_Tests()
        {
            var ex = new Exception();
            var result = new ErrorResult<int>(ex);

            Assert.AreEqual(ex, result.Error);
        }

        [TestMethod]
        public void Bind_Tests()
        {
            var ex = new FormatException();
            var result = new ErrorResult<string>(ex);

            Assert.ThrowsException<ArgumentNullException>(() => result.Bind<int>(null));

            var iresult2 = result.Bind(s => new DataResult<int>(s.Length));
            Assert.IsInstanceOfType<ErrorResult<int>>(iresult2);
            Assert.IsInstanceOfType<ErrorResult<int>>(iresult2);

            var eresult2 = iresult2 as ErrorResult<int>;
            Assert.IsNotNull(eresult2);
            Assert.AreEqual(ex, eresult2.Error);

            iresult2 = result.Bind(s => new EndOfStreamException().Throw<IResult<int>>());
            Assert.IsInstanceOfType<ErrorResult<int>>(iresult2);
            eresult2 = iresult2 as ErrorResult<int>;
            Assert.AreEqual(ex, eresult2.Error);
        }

        [TestMethod]
        public void Map_Tests()
        {
            var ex = new FormatException();
            var result = new ErrorResult<string>(ex);

            Assert.ThrowsException<ArgumentNullException>(() => result.Map<int>(null));

            var iresult2 = result.Map(s => s.Length);
            Assert.IsInstanceOfType<ErrorResult<int>>(iresult2);
            Assert.IsInstanceOfType<ErrorResult<int>>(iresult2);

            var eresult2 = iresult2 as ErrorResult<int>;
            Assert.IsNotNull(eresult2);
            Assert.AreEqual(ex, eresult2.Error);

            iresult2 = result.Map(s => new EndOfStreamException().Throw<int>());
            Assert.IsInstanceOfType<ErrorResult<int>>(iresult2);
            eresult2 = iresult2 as ErrorResult<int>;
            Assert.AreEqual(ex, eresult2.Error);
        }

        [TestMethod]
        public void Consume_Tests()
        {
            var result = new ErrorResult<string>(new ArrayTypeMismatchException());

            Assert.ThrowsException<ArgumentNullException>(() => result.Consume(null));

            int len = -1;
            result.Consume(s => len = s.Length);
            Assert.AreEqual(-1, len);

            result.Map(s => len = s.Length * 3);
            Assert.AreEqual(-1, len);
        }
    }
}
