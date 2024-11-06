namespace Axis.Luna.Unions.Tests
{
    [TestClass]
    public class Union3Tests
    {
        [TestMethod]
        public void Is_Tests()
        {
            var union = new MyRefUnion(5);
            Assert.IsTrue(union.Is(out int iv));
            Assert.AreEqual(5, iv);
            Assert.IsFalse(union.Is(out decimal _));
            Assert.IsFalse(union.Is(out string? _));
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion(5m);
            Assert.IsFalse(union.Is(out int _));
            Assert.IsTrue(union.Is(out decimal dv));
            Assert.AreEqual(5m, dv);
            Assert.IsFalse(union.Is(out string? _));
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion("5");
            Assert.IsFalse(union.Is(out int _));
            Assert.IsFalse(union.Is(out decimal _));
            Assert.IsTrue(union.Is(out string? sv));
            Assert.AreEqual("5", sv);
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion(null);
            Assert.IsFalse(union.Is(out int _));
            Assert.IsFalse(union.Is(out decimal _));
            Assert.IsFalse(union.Is(out string? _));
            Assert.IsTrue(union.IsNull());
        }

        [TestMethod]
        public void Match_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Match(
                null!,
                m => m.ToString(),
                s => s));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Match(
                i => i.ToString(),
                null!,
                s => s));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Match(
                i => i.ToString(),
                m => m.ToString(),
                null!));
            Assert.ThrowsException<InvalidOperationException>(() => new MyRefUnion(Guid.NewGuid()).Match(
                i => i.ToString(),
                m => m.ToString(),
                x => x.ToString()));

            var iunion = new MyRefUnion(5);
            var result = iunion.Match(
                i => i == 5,
                m => m > 8m,
                s => string.IsNullOrEmpty(s));
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5m);
            result = iunion.Match(
                i => i > 8,
                m => m == 5m,
                s => string.IsNullOrEmpty(s));
            Assert.IsTrue(result);

            iunion = new MyRefUnion("5");
            result = iunion.Match(
                i => i > 8,
                m => m > 8,
                s => s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            result = iunion.Match(
                i => i > 8,
                m => m > 8,
                s => s.Equals("bleh"),
                () => true);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            result = iunion.Match(
                i => i > 8,
                m => m > 8,
                s => s.Equals("bleh"));
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Consume_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Consume(
                null!,
                Console.WriteLine,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Consume(
                Console.WriteLine,
                null!,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Consume(
                Console.WriteLine,
                Console.WriteLine,
                null!));

            var iunion = new MyRefUnion(5);
            var result = false;
            iunion.Consume(
                i => result = i > 8,
                m => result = m > 8,
                s => result = string.IsNullOrEmpty(s));
            Assert.IsFalse(result);

            iunion = new MyRefUnion(5m);
            iunion.Consume(
                i => result = i > 8,
                m => result = m == 5,
                s => result = s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion("5");
            iunion.Consume(
                i => result = i > 8,
                m => result = m > 8,
                s => result = s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            iunion.Consume(
                i => result = i > 8,
                m => result = m > 8,
                s => result = s.Equals("bleh"),
                () => result = true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void With_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).With(
                null!,
                Console.WriteLine,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).With(
                Console.WriteLine,
                null!,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).With(
                Console.WriteLine,
                Console.WriteLine,
                null!));

            var iunion = new MyRefUnion(5);
            var result = false;
            iunion.With(
                i => result = i > 8,
                m => result = m > 8,
                s => result = string.IsNullOrEmpty(s));
            Assert.IsFalse(result);

            iunion = new MyRefUnion(5m);
            iunion.With(
                i => result = i > 8,
                m => result = m == 5,
                s => result = s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion("5");
            iunion.With(
                i => result = i > 8,
                m => result = m > 8,
                s => result = s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            iunion.With(
                i => result = i > 8,
                m => result = m > 8,
                s => result = s.Equals("bleh"),
                () => result = true);
        }

        internal class MyRefUnion : IUnion<int, decimal, string, MyRefUnion>
        {
            private readonly object? _value;
            object? IUnion<int, decimal, string, MyRefUnion>.Value => _value;

            public MyRefUnion(object? value) { _value = value; }
        }
    }
}
