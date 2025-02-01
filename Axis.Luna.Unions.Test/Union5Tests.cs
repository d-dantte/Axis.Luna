namespace Axis.Luna.Unions.Tests
{
    [TestClass]
    public class Union5Tests
    {
        [TestMethod]
        public void Is_Tests()
        {
            var union = new MyRefUnion((sbyte)5);
            Assert.IsTrue(union.IsOf(out sbyte v));
            Assert.AreEqual(5, v);
            Assert.IsFalse(union.IsOf(out byte _));
            Assert.IsFalse(union.IsOf(out short _));
            Assert.IsFalse(union.IsOf(out ushort _));
            Assert.IsFalse(union.IsOf(out int _));
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion((byte)5);
            Assert.IsFalse(union.IsOf(out sbyte _));
            Assert.IsTrue(union.IsOf(out byte b));
            Assert.AreEqual(5, b);
            Assert.IsFalse(union.IsOf(out short _));
            Assert.IsFalse(union.IsOf(out ushort _));
            Assert.IsFalse(union.IsOf(out int _));
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion((short)5);
            Assert.IsFalse(union.IsOf(out sbyte _));
            Assert.IsFalse(union.IsOf(out byte _));
            Assert.IsTrue(union.IsOf(out short s));
            Assert.AreEqual(5, s);
            Assert.IsFalse(union.IsOf(out ushort _));
            Assert.IsFalse(union.IsOf(out int _));
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion((ushort)5);
            Assert.IsFalse(union.IsOf(out sbyte _));
            Assert.IsFalse(union.IsOf(out byte _));
            Assert.IsFalse(union.IsOf(out short _));
            Assert.IsTrue(union.IsOf(out ushort us));
            Assert.AreEqual(5, us);
            Assert.IsFalse(union.IsOf(out int _));
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion(5);
            Assert.IsFalse(union.IsOf(out sbyte _));
            Assert.IsFalse(union.IsOf(out byte _));
            Assert.IsFalse(union.IsOf(out short _));
            Assert.IsFalse(union.IsOf(out ushort _));
            Assert.IsTrue(union.IsOf(out int i));
            Assert.AreEqual(5, i);
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion(null);
            Assert.IsFalse(union.IsOf(out sbyte _));
            Assert.IsFalse(union.IsOf(out byte _));
            Assert.IsFalse(union.IsOf(out short _));
            Assert.IsFalse(union.IsOf(out ushort _));
            Assert.IsFalse(union.IsOf(out int _));
            Assert.IsTrue(union.IsNull());
        }

        [TestMethod]
        public void Match_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Match(
                null!,
                m => m.ToString(),
                m => m.ToString(),
                m => m.ToString(),
                m => m.ToString()));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Match(
                i => i.ToString(),
                null!,
                m => m.ToString(),
                m => m.ToString(),
                m => m.ToString()));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Match(
                m => m.ToString(),
                m => m.ToString(),
                null!,
                m => m.ToString(),
                m => m.ToString()));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Match(
                m => m.ToString(),
                m => m.ToString(),
                m => m.ToString(),
                null!,
                m => m.ToString()));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Match(
                m => m.ToString(),
                m => m.ToString(),
                m => m.ToString(),
                m => m.ToString(),
                null!));

            var iunion = new MyRefUnion((sbyte)5);
            var result = iunion.Match(
                v => v == 5,
                v => v != 5,
                v => v != 5,
                v => v != 5,
                v => v != 5);
            Assert.IsTrue(result);

            iunion = new MyRefUnion((byte)5);
            result = iunion.Match(
                v => v != 5,
                v => v == 5,
                v => v != 5,
                v => v != 5,
                v => v != 5);
            Assert.IsTrue(result);

            iunion = new MyRefUnion((short)5);
            result = iunion.Match(
                v => v != 5,
                v => v != 5,
                v => v == 5,
                v => v != 5,
                v => v != 5);
            Assert.IsTrue(result);

            iunion = new MyRefUnion((ushort)5);
            result = iunion.Match(
                v => v != 5,
                v => v != 5,
                v => v != 5,
                v => v == 5,
                v => v != 5);
            Assert.IsTrue(result);

            iunion = new MyRefUnion((int)5);
            result = iunion.Match(
                v => v != 5,
                v => v != 5,
                v => v != 5,
                v => v != 5,
                v => v == 5);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            result = iunion.Match(
                v => v == 5,
                v => v == 5,
                v => v == 5,
                v => v == 5,
                v => v == 5,
                () => true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Consume_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Consume(
                null!,
                ConsoleWrite,
                ConsoleWrite,
                ConsoleWrite,
                ConsoleWrite));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Consume(
                ConsoleWrite,
                null!,
                ConsoleWrite,
                ConsoleWrite,
                ConsoleWrite));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Consume(
                ConsoleWrite,
                ConsoleWrite,
                null!,
                ConsoleWrite,
                ConsoleWrite));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Consume(
                ConsoleWrite,
                ConsoleWrite,
                ConsoleWrite,
                null!,
                ConsoleWrite));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).Consume(
                ConsoleWrite,
                ConsoleWrite,
                ConsoleWrite,
                ConsoleWrite,
                null!));

            var iunion = new MyRefUnion((sbyte)5);
            var result = false;
            iunion.Consume(
                v => result = v == 5,
                v => result = v != 5,
                v => result = v != 5,
                v => result = v != 5,
                v => result = v != 5);
            Assert.IsTrue(result);

            iunion = new MyRefUnion((byte)5);
            iunion.Consume(
                v => result = v != 5,
                v => result = v == 5,
                v => result = v != 5,
                v => result = v != 5,
                v => result = v != 5);
            Assert.IsTrue(result);

            iunion = new MyRefUnion((short)5);
            iunion.Consume(
                v => result = v != 5,
                v => result = v != 5,
                v => result = v == 5,
                v => result = v != 5,
                v => result = v != 5);
            Assert.IsTrue(result);

            iunion = new MyRefUnion((ushort)5);
            iunion.Consume(
                v => result = v != 5,
                v => result = v != 5,
                v => result = v != 5,
                v => result = v == 5,
                v => result = v != 5);
            Assert.IsTrue(result);

            iunion = new MyRefUnion((int)5);
            iunion.Consume(
                v => result = v != 5,
                v => result = v != 5,
                v => result = v != 5,
                v => result = v != 5,
                v => result = v == 5);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            iunion.Consume(
                v => result = v == 5,
                v => result = v == 5,
                v => result = v == 5,
                v => result = v == 5,
                v => result = v == 5,
                () => result = true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void With_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).With(
                null!,
                ConsoleWrite,
                ConsoleWrite,
                ConsoleWrite,
                ConsoleWrite));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).With(
                ConsoleWrite,
                null!,
                ConsoleWrite,
                ConsoleWrite,
                ConsoleWrite));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).With(
                ConsoleWrite,
                ConsoleWrite,
                null!,
                ConsoleWrite,
                ConsoleWrite));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).With(
                ConsoleWrite,
                ConsoleWrite,
                ConsoleWrite,
                null!,
                ConsoleWrite));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).With(
                ConsoleWrite,
                ConsoleWrite,
                ConsoleWrite,
                ConsoleWrite,
                null!));

            var iunion = new MyRefUnion((sbyte)5);
            var result = false;
            iunion.With(
                v => result = v == 5,
                v => result = v != 5,
                v => result = v != 5,
                v => result = v != 5,
                v => result = v != 5);
            Assert.IsTrue(result);

            iunion = new MyRefUnion((byte)5);
            iunion.With(
                v => result = v != 5,
                v => result = v == 5,
                v => result = v != 5,
                v => result = v != 5,
                v => result = v != 5);
            Assert.IsTrue(result);

            iunion = new MyRefUnion((short)5);
            iunion.With(
                v => result = v != 5,
                v => result = v != 5,
                v => result = v == 5,
                v => result = v != 5,
                v => result = v != 5);
            Assert.IsTrue(result);

            iunion = new MyRefUnion((ushort)5);
            iunion.With(
                v => result = v != 5,
                v => result = v != 5,
                v => result = v != 5,
                v => result = v == 5,
                v => result = v != 5);
            Assert.IsTrue(result);

            iunion = new MyRefUnion((int)5);
            iunion.With(
                v => result = v != 5,
                v => result = v != 5,
                v => result = v != 5,
                v => result = v != 5,
                v => result = v == 5);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            iunion.With(
                v => result = v == 5,
                v => result = v == 5,
                v => result = v == 5,
                v => result = v == 5,
                v => result = v == 5,
                () => result = true);
            Assert.IsTrue(result);
        }

        private static void ConsoleWrite<T>(T obj)
        {
            Console.WriteLine(obj);
        }

        internal class MyRefUnion : IUnion<sbyte, byte, short, ushort, int, MyRefUnion>
        {
            private readonly object? _value;
            object? IUnion<sbyte, byte, short, ushort, int, MyRefUnion>.Value => _value;

            public MyRefUnion(object? value) { _value = value; }
        }
    }
}
