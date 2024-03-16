namespace Axis.Luna.Result.Tests
{
    [TestClass]
    public class DataResultTests
    {
        [TestMethod]
        public void Construction_Tests()
        {
            var result = new DataResult<int>(34);

            Assert.AreEqual(34, result.Data);
        }

        [TestMethod]
        public void Bind_Tests()
        {
            var result = new DataResult<string>("34");

            Assert.ThrowsException<ArgumentNullException>(() => result.Bind<int>(null));

            var iresult2 = result.Bind(s => new DataResult<int>(s.Length));
            Assert.IsInstanceOfType<DataResult<int>>(iresult2);
            Assert.IsTrue(iresult2.IsDataResult(out var r2));
            Assert.AreEqual(2, r2);

            iresult2 = result.Bind(s => new EndOfStreamException().Throw<IResult<int>>());
            Assert.IsInstanceOfType<ErrorResult<int>>(iresult2);
            Assert.IsTrue(iresult2.IsErrorResult(out var err));
            Assert.IsInstanceOfType<EndOfStreamException>(err);
        }

        [TestMethod]
        public void Map_Tests()
        {
            var result = new DataResult<string>("34");

            Assert.ThrowsException<ArgumentNullException>(() => result.Map<int>(null));

            var iresult2 = result.Map(s => s.Length);
            Assert.IsInstanceOfType<DataResult<int>>(iresult2);
            Assert.IsTrue(iresult2.IsDataResult(out var r2));
            Assert.AreEqual(2, r2);

            iresult2 = result.Map(s => new EndOfStreamException().Throw<int>());
            Assert.IsInstanceOfType<ErrorResult<int>>(iresult2);
            Assert.IsTrue(iresult2.IsErrorResult(out var r3));
            Assert.IsInstanceOfType<EndOfStreamException>(r3);
        }

        [TestMethod]
        public void Consume_Tests()
        {
            var result = new DataResult<string>("34");

            Assert.ThrowsException<ArgumentNullException>(() => result.Consume(null));

            int len = -1;
            result.Consume(s => len = s.Length);
            Assert.AreEqual(2, len);

            result.Map(s => len = s.Length * 3);
            Assert.AreEqual(6, len);
        }
    }
}
