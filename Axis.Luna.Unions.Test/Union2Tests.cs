namespace Axis.Luna.Unions.Tests
{
    [TestClass]
    public class Union2Tests
    {
        [TestMethod]
        public void Is_Tests()
        {
            var union = new NewUnion<int, string>(4);
            Assert.IsFalse(union.Is(out string? _));
            Assert.IsTrue(union.Is(out int iv));
            Assert.AreEqual(4, iv);

            union = new NewUnion<int, string>("5");
            Assert.IsFalse(union.Is(out int _));
            Assert.IsTrue(union.Is(out string? sv));
            Assert.AreEqual("5", sv);

            union = new NewUnion<int, string>(null);
            Assert.IsFalse(union.Is(out string? _));
            Assert.IsFalse(union.Is(out int _));
            Assert.IsTrue(union.IsNull());
        }

        [TestMethod]
        public void MapMatch_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Match(null!, s => s));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Match(i => i, null!));

            var iunion = new MyRefUnion(5);
            var result = iunion.Match(
                i => i > 8,
                s => string.IsNullOrEmpty(s));
            Assert.IsFalse(result);

            iunion = new MyRefUnion("5");
            result = iunion.Match(
                i => i > 8,
                s => s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            result = iunion.Match(
                i => i > 8,
                s => s.Equals("bleh"),
                () => true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Consume_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Consume(null!, Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Consume(Console.WriteLine, null!));

            var iunion = new MyRefUnion(5);
            var result = false;
            iunion.Consume(
                i => result = i > 8,
                s => result = string.IsNullOrEmpty(s));
            Assert.IsFalse(result);

            iunion = new MyRefUnion("5");
            iunion.Consume(
                i => result = i > 8,
                s => result = s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            iunion.Consume(
                i => result = i > 8,
                s => result = s.Equals("bleh"),
                () => result = true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void With_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).With(null!, Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).With(Console.WriteLine, null!));

            var iunion = new MyRefUnion(5);
            var result = false;
            iunion.With(
                i => result = i > 8,
                s => result = string.IsNullOrEmpty(s));
            Assert.IsFalse(result);

            iunion = new MyRefUnion("5");
            iunion.With(
                i => result = i > 8,
                s => result = s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            iunion.With(
                i => result = i > 8,
                s => result = s.Equals("bleh"),
                () => result = true);
            Assert.IsTrue(result);
        }

        internal readonly struct NewUnion<T1, T2> : IUnion<T1, T2, NewUnion<T1, T2>>
        {
            private readonly object? _value;

            object? IUnion<T1, T2, NewUnion<T1, T2>>.Value => _value;

            public NewUnion(object? value)
            {
                _value = value;
            }
        }

        internal class MyRefUnion : IUnion<int, string, MyRefUnion>
        {
            private readonly object? _value;

            object? IUnion<int, string, MyRefUnion>.Value => _value;

            public MyRefUnion(object? value)
            {
                _value = value;
            }
        }
    }
}
