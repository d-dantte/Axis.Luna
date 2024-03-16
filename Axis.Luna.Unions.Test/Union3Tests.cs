namespace Axis.Luna.Unions.Tests
{
    [TestClass]
    public class Union3Tests
    {
        [TestMethod]
        public void Construction_Tests()
        {
            var runion = new MyRefUnion(4); //no exceptions thrown
            runion = new MyRefUnion(4m); //no exceptions thrown
            runion = new MyRefUnion("some string"); //no exceptions thrown

            Assert.ThrowsException<TypeInitializationException>(() => new DuplicateTypeUnion(4));
        }

        [TestMethod]
        public void Is_Tests()
        {
            var union = new MyRefUnion(5);
            Assert.IsTrue(union.Is(out int iv));
            Assert.AreEqual(5, iv);
            Assert.IsFalse(union.Is(out decimal _));
            Assert.IsFalse(union.Is(out string _));
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion(5m);
            Assert.IsFalse(union.Is(out int _));
            Assert.IsTrue(union.Is(out decimal dv));
            Assert.AreEqual(5m, dv);
            Assert.IsFalse(union.Is(out string _));
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion("5");
            Assert.IsFalse(union.Is(out int _));
            Assert.IsFalse(union.Is(out decimal _));
            Assert.IsTrue(union.Is(out string sv));
            Assert.AreEqual("5", sv);
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion(null);
            Assert.IsFalse(union.Is(out int _));
            Assert.IsFalse(union.Is(out decimal _));
            Assert.IsFalse(union.Is(out string _));
            Assert.IsTrue(union.IsNull());
        }

        [TestMethod]
        public void MapMatch_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).MapMatch(
                null,
                m => m.ToString(),
                s => s));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).MapMatch(
                i => i.ToString(),
                null,
                s => s));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).MapMatch(
                i => i.ToString(),
                m => m.ToString(),
                null));

            var iunion = new MyRefUnion(5);
            var result = iunion.MapMatch(
                i => i == 5,
                m => m > 8m,
                s => string.IsNullOrEmpty(s));
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5m);
            result = iunion.MapMatch(
                i => i > 8,
                m => m == 5m,
                s => string.IsNullOrEmpty(s));
            Assert.IsTrue(result);

            iunion = new MyRefUnion("5");
            result = iunion.MapMatch(
                i => i > 8,
                m => m > 8,
                s => s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            result = iunion.MapMatch(
                i => i > 8,
                m => m > 8,
                s => s.Equals("bleh"),
                () => true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Consume_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).ConsumeMatch(
                null,
                Console.WriteLine,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).ConsumeMatch(
                Console.WriteLine,
                null,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).ConsumeMatch(
                Console.WriteLine,
                Console.WriteLine,
                null));

            var iunion = new MyRefUnion(5);
            var result = false;
            iunion.ConsumeMatch(
                i => result = i > 8,
                m => result = m > 8,
                s => result = string.IsNullOrEmpty(s));
            Assert.IsFalse(result);

            iunion = new MyRefUnion(5m);
            iunion.ConsumeMatch(
                i => result = i > 8,
                m => result = m == 5,
                s => result = s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion("5");
            iunion.ConsumeMatch(
                i => result = i > 8,
                m => result = m > 8,
                s => result = s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            iunion.ConsumeMatch(
                i => result = i > 8,
                m => result = m > 8,
                s => result = s.Equals("bleh"),
                () => result = true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void With_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).WithMatch(
                null,
                Console.WriteLine,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).WithMatch(
                Console.WriteLine,
                null,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).WithMatch(
                Console.WriteLine,
                Console.WriteLine,
                null));

            var iunion = new MyRefUnion(5);
            var result = false;
            iunion.WithMatch(
                i => result = i > 8,
                m => result = m > 8,
                s => result = string.IsNullOrEmpty(s));
            Assert.IsFalse(result);

            iunion = new MyRefUnion(5m);
            iunion.WithMatch(
                i => result = i > 8,
                m => result = m == 5,
                s => result = s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion("5");
            iunion.WithMatch(
                i => result = i > 8,
                m => result = m > 8,
                s => result = s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            iunion.WithMatch(
                i => result = i > 8,
                m => result = m > 8,
                s => result = s.Equals("bleh"),
                () => result = true);
        }

        [TestMethod]
        public void ImplicitsAndOf_Tests()
        {
            // implicits
            All all = 5;
            Assert.IsTrue(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = 5m;
            Assert.IsFalse(all.Is(out int _));
            Assert.IsTrue(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = "5";
            Assert.IsFalse(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsTrue(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            // of
            all = All.Of(5);
            Assert.IsTrue(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = All.Of(5m);
            Assert.IsFalse(all.Is(out int _));
            Assert.IsTrue(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = All.Of("5");
            Assert.IsFalse(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsTrue(all.Is(out string _));
            Assert.IsFalse(all.IsNull());
        }

        internal class MyRefUnion
        : RefUnion<int, decimal, string, MyRefUnion>
        {
            public MyRefUnion(object value) : base(value)
            {
            }
        }

        internal class All :
            RefUnion<int, decimal, string, All>,
            IUnionImplicits<int, decimal, string, All>,
            IUnionOf<int, decimal, string, All>
        {
            public All(object value) : base(value)
            {
            }

            public static All Of(int value) => new(value);

            public static All Of(decimal value) => new(value);

            public static All Of(string value) => new(value);

            public static implicit operator All(int value) => new(value);

            public static implicit operator All(decimal value) => new(value);

            public static implicit operator All(string value) => new(value);
        }

        internal class DuplicateTypeUnion :
            RefUnion<int, int, int, DuplicateTypeUnion>,
#pragma warning disable CS1956 // Member implements interface member with multiple matches at run-time
            IUnionImplicits<int, int, int, DuplicateTypeUnion>
#pragma warning restore CS1956 // Member implements interface member with multiple matches at run-time
        {
            public DuplicateTypeUnion(object value) : base(value)
            {
            }

            public static implicit operator DuplicateTypeUnion(int value)
            {
                throw new NotImplementedException();
            }
        }

    }
}
