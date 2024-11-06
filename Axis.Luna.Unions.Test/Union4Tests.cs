namespace Axis.Luna.Unions.Tests
{
    [TestClass]
    public class Union4Tests
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

            union = new MyRefUnion(TimeSpan.FromSeconds(44));
            Assert.IsFalse(union.Is(out int _));
            Assert.IsFalse(union.Is(out decimal _));
            Assert.IsFalse(union.Is(out string? _));
            Assert.IsTrue(union.Is(out TimeSpan t));
            Assert.AreEqual(TimeSpan.FromSeconds(44), t);
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
                m => m.ToString(),
                m => m.ToString()));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Match(
                i => i.ToString(),
                null!,
                m => m.ToString(),
                m => m.ToString()));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Match(
                m => m.ToString(),
                m => m.ToString(),
                null!,
                m => m.ToString()));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Match(
                m => m.ToString(),
                m => m.ToString(),
                m => m.ToString(),
                null!));

            var iunion = new MyRefUnion(5);
            var result = iunion.Match(
                i => i == 5,
                m => m > 8m,
                s => string.IsNullOrEmpty(s),
                t => t < TimeSpan.Zero);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5m);
            result = iunion.Match(
                i => i > 8,
                m => m == 5m,
                s => string.IsNullOrEmpty(s),
                t => t < TimeSpan.Zero);
            Assert.IsTrue(result);

            iunion = new MyRefUnion("5");
            result = iunion.Match(
                i => i > 8,
                m => m > 8,
                s => s is not null,
                t => t < TimeSpan.Zero);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(TimeSpan.Zero);
            result = iunion.Match(
                i => i > 8,
                m => m > 8,
                s => s is not null,
                t => t < TimeSpan.Zero);
            Assert.IsFalse(result);

            iunion = new MyRefUnion(null);
            result = iunion.Match(
                i => i > 8,
                m => m > 8,
                s => s.Equals("bleh"),
                t => t == TimeSpan.Zero,
                () => true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Consume_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Consume(
                null!,
                Console.WriteLine,
                Console.WriteLine,
                t => Console.WriteLine(t)));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Consume(
                Console.WriteLine,
                null!,
                Console.WriteLine,
                t => Console.WriteLine(t)));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Consume(
                Console.WriteLine,
                Console.WriteLine,
                null!,
                t => Console.WriteLine(t)));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Consume(
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                null!));

            var iunion = new MyRefUnion(5);
            var result = false;
            iunion.Consume(
                i => result = i > 8,
                m => result = m > 8,
                s => result = string.IsNullOrEmpty(s),
                t => result = t > TimeSpan.Zero);
            Assert.IsFalse(result);

            iunion = new MyRefUnion(5m);
            iunion.Consume(
                i => result = i > 8,
                m => result = m == 5,
                s => result = s is not null,
                t => result = t > TimeSpan.Zero);
            Assert.IsTrue(result);

            iunion = new MyRefUnion("5");
            iunion.Consume(
                i => result = i > 8,
                m => result = m > 8,
                s => result = s is not null,
                t => result = t > TimeSpan.Zero);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(TimeSpan.Zero);
            iunion.Consume(
                i => result = i > 8,
                m => result = m > 8,
                s => result = s is not null,
                t => result = t == TimeSpan.Zero);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            iunion.Consume(
                i => result = i > 8,
                m => result = m > 8,
                s => result = s.Equals("bleh"),
                t => result = t == TimeSpan.Zero,
                () => result = true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void With_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).With(
                null!,
                Console.WriteLine,
                Console.WriteLine,
                t => Console.WriteLine(t)));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).With(
                Console.WriteLine,
                null!,
                Console.WriteLine,
                t => Console.WriteLine(t)));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).With(
                Console.WriteLine,
                Console.WriteLine,
                null!,
                t => Console.WriteLine(t)));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).With(
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                null!));

            var iunion = new MyRefUnion(5);
            var result = false;
            iunion.With(
                i => result = i > 8,
                m => result = m > 8,
                s => result = string.IsNullOrEmpty(s),
                t => result = t > TimeSpan.Zero);
            Assert.IsFalse(result);

            iunion = new MyRefUnion(5m);
            iunion.With(
                i => result = i > 8,
                m => result = m == 5,
                s => result = s is not null,
                t => result = t > TimeSpan.Zero);
            Assert.IsTrue(result);

            iunion = new MyRefUnion("5");
            iunion.With(
                i => result = i > 8,
                m => result = m > 8,
                s => result = s is not null,
                t => result = t > TimeSpan.Zero);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(TimeSpan.Zero);
            iunion.With(
                i => result = i > 8,
                m => result = m > 8,
                s => result = s is not null,
                t => result = t == TimeSpan.Zero);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            iunion.With(
                i => result = i > 8,
                m => result = m > 8,
                s => result = s.Equals("bleh"),
                t => result = t == TimeSpan.Zero,
                () => result = true);
            Assert.IsTrue(result);
        }

        internal class MyRefUnion : IUnion<int, decimal, string, TimeSpan, MyRefUnion>
        {
            private readonly object? _value;
            object? IUnion<int, decimal, string, TimeSpan, MyRefUnion>.Value => _value;

            public MyRefUnion(object? value) { _value = value; }
        }
    }
}
