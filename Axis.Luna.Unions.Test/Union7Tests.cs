namespace Axis.Luna.Unions.Tests
{
    [TestClass]
    public class Union7Tests
    {
        [TestMethod]
        public void Construction_Tests()
        {
            var runion = new MyRefUnion(4); //no exceptions thrown
            runion = new MyRefUnion(4m); //no exceptions thrown
            runion = new MyRefUnion(4f); //no exceptions thrown
            runion = new MyRefUnion(4d); //no exceptions thrown
            runion = new MyRefUnion(4L); //no exceptions thrown
            runion = new MyRefUnion(4uL); //no exceptions thrown
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
            Assert.IsFalse(union.Is(out float _));
            Assert.IsFalse(union.Is(out double _));
            Assert.IsFalse(union.Is(out long _));
            Assert.IsFalse(union.Is(out ulong _));
            Assert.IsFalse(union.Is(out string _));
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion(5m);
            Assert.IsFalse(union.Is(out int _));
            Assert.IsTrue(union.Is(out decimal dv));
            Assert.AreEqual(5m, dv);
            Assert.IsFalse(union.Is(out float _));
            Assert.IsFalse(union.Is(out double _));
            Assert.IsFalse(union.Is(out long _));
            Assert.IsFalse(union.Is(out ulong _));
            Assert.IsFalse(union.Is(out string _));
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion(5f);
            Assert.IsFalse(union.Is(out int _));
            Assert.IsFalse(union.Is(out decimal _));
            Assert.IsTrue(union.Is(out float fv));
            Assert.AreEqual(5f, fv);
            Assert.IsFalse(union.Is(out double _));
            Assert.IsFalse(union.Is(out long _));
            Assert.IsFalse(union.Is(out ulong _));
            Assert.IsFalse(union.Is(out string _));
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion(5d);
            Assert.IsFalse(union.Is(out int _));
            Assert.IsFalse(union.Is(out decimal _));
            Assert.IsFalse(union.Is(out float _));
            Assert.IsTrue(union.Is(out double ddv));
            Assert.AreEqual(5d, ddv);
            Assert.IsFalse(union.Is(out long _));
            Assert.IsFalse(union.Is(out ulong _));
            Assert.IsFalse(union.Is(out string _));
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion(5L);
            Assert.IsFalse(union.Is(out int _));
            Assert.IsFalse(union.Is(out decimal _));
            Assert.IsFalse(union.Is(out float _));
            Assert.IsFalse(union.Is(out double _));
            Assert.IsTrue(union.Is(out long lv));
            Assert.AreEqual(5L, lv);
            Assert.IsFalse(union.Is(out ulong _));
            Assert.IsFalse(union.Is(out string _));
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion(5uL);
            Assert.IsFalse(union.Is(out int _));
            Assert.IsFalse(union.Is(out decimal _));
            Assert.IsFalse(union.Is(out float _));
            Assert.IsFalse(union.Is(out double _));
            Assert.IsFalse(union.Is(out long _));
            Assert.IsTrue(union.Is(out ulong ulv));
            Assert.AreEqual(5uL, ulv);
            Assert.IsFalse(union.Is(out string _));
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion("5");
            Assert.IsFalse(union.Is(out int _));
            Assert.IsFalse(union.Is(out decimal _));
            Assert.IsFalse(union.Is(out float _));
            Assert.IsFalse(union.Is(out double _));
            Assert.IsFalse(union.Is(out long _));
            Assert.IsFalse(union.Is(out ulong _));
            Assert.IsTrue(union.Is(out string sv));
            Assert.AreEqual("5", sv);
            Assert.IsFalse(union.IsNull());

            union = new MyRefUnion(null);
            Assert.IsFalse(union.Is(out int _));
            Assert.IsFalse(union.Is(out decimal _));
            Assert.IsFalse(union.Is(out float _));
            Assert.IsFalse(union.Is(out double _));
            Assert.IsFalse(union.Is(out long _));
            Assert.IsFalse(union.Is(out ulong _));
            Assert.IsFalse(union.Is(out string _));
            Assert.IsTrue(union.IsNull());
        }

        [TestMethod]
        public void MapMatch_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).MapMatch(
                null,
                m => m.ToString(),
                f => f.ToString(),
                d => d.ToString(),
                l => l.ToString(),
                ul => ul.ToString(),
                s => s));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).MapMatch(
                i => i.ToString(),
                null,
                f => f.ToString(),
                d => d.ToString(),
                l => l.ToString(),
                ul => ul.ToString(),
                s => s));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).MapMatch(
                i => i.ToString(),
                m => m.ToString(),
                null,
                d => d.ToString(),
                l => l.ToString(),
                ul => ul.ToString(),
                s => s));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).MapMatch(
                i => i.ToString(),
                m => m.ToString(),
                f => f.ToString(),
                null,
                l => l.ToString(),
                ul => ul.ToString(),
                s => s));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).MapMatch(
                i => i.ToString(),
                m => m.ToString(),
                f => f.ToString(),
                d => d.ToString(),
                null,
                ul => ul.ToString(),
                s => s));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).MapMatch(
                i => i.ToString(),
                m => m.ToString(),
                f => f.ToString(),
                d => d.ToString(),
                l => l.ToString(),
                null,
                s => s));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).MapMatch(
                i => i.ToString(),
                m => m.ToString(),
                f => f.ToString(),
                d => d.ToString(),
                l => l.ToString(),
                ul => ul.ToString(),
                null));

            var iunion = new MyRefUnion(5);
            var result = iunion.MapMatch(
                i => i == 5,
                m => false,
                f => false,
                d => false,
                l => false,
                ul => false,
                s => false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5m);
            result = iunion.MapMatch(
                i => false,
                m => m == 5m,
                f => false,
                d => false,
                l => false,
                ul => false,
                s => false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5f);
            result = iunion.MapMatch(
                i => false,
                m => false,
                f => f == 5f,
                d => false,
                l => false,
                ul => false,
                s => false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5d);
            result = iunion.MapMatch(
                i => false,
                m => false,
                f => false,
                d => d == 5d,
                l => false,
                ul => false,
                s => false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5L);
            result = iunion.MapMatch(
                i => false,
                m => false,
                f => false,
                d => false,
                l => l == 5L,
                ul => false,
                s => false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5uL);
            result = iunion.MapMatch(
                i => false,
                m => false,
                f => false,
                d => false,
                l => false,
                ul => ul == 5uL,
                s => false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion("5");
            result = iunion.MapMatch(
                i => false,
                m => false,
                f => false,
                d => false,
                l => false,
                ul => false,
                s => s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            result = iunion.MapMatch(
                i => false,
                m => false,
                f => false,
                d => false,
                l => false,
                ul => false,
                s => false,
                () => true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Consume_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).ConsumeMatch(
                null,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).ConsumeMatch(
                Console.WriteLine,
                null,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).ConsumeMatch(
                Console.WriteLine,
                Console.WriteLine,
                null,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).ConsumeMatch(
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                null,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).ConsumeMatch(
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                null,
                Console.WriteLine,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).ConsumeMatch(
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                null,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).ConsumeMatch(
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                null));

            var iunion = new MyRefUnion(5);
            var result = false;
            iunion.ConsumeMatch(
                i => result = i == 5,
                m => result = false,
                f => result = false,
                d => result = false,
                l => result = false,
                ul => result = false,
                s => result = false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5m);
            iunion.ConsumeMatch(
                i => result = false,
                m => result = m == 5m,
                f => result = false,
                d => result = false,
                l => result = false,
                ul => result = false,
                s => result = false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5f);
            iunion.ConsumeMatch(
                i => result = false,
                m => result = false,
                f => result = f == 5f,
                d => result = false,
                l => result = false,
                ul => result = false,
                s => result = false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5d);
            iunion.ConsumeMatch(
                i => result = false,
                m => result = false,
                f => result = false,
                d => result = d == 5d,
                l => result = false,
                ul => result = false,
                s => result = false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5L);
            iunion.ConsumeMatch(
                i => result = false,
                m => result = false,
                f => result = false,
                d => result = false,
                l => result = l == 5L,
                ul => result = false,
                s => result = false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5uL);
            iunion.ConsumeMatch(
                i => result = false,
                m => result = false,
                f => result = false,
                d => result = false,
                l => result = false,
                ul => result = ul == 5uL,
                s => result = false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion("5");
            iunion.ConsumeMatch(
                i => result = false,
                m => result = false,
                f => result = false,
                d => result = false,
                l => result = false,
                ul => result = false,
                s => result = s == "5");
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            iunion.ConsumeMatch(
                i => result = false,
                m => result = false,
                m => result = false,
                d => result = false,
                l => result = false,
                ul => result = false,
                s => result = false,
                () => result = true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void With_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).WithMatch(
                null,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).WithMatch(
                Console.WriteLine,
                null,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).WithMatch(
                Console.WriteLine,
                Console.WriteLine,
                null,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).WithMatch(
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                null,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).WithMatch(
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                null,
                Console.WriteLine,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).WithMatch(
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                null,
                Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).WithMatch(
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                Console.WriteLine,
                null));

            var iunion = new MyRefUnion(5);
            var result = false;
            iunion.WithMatch(
                i => result = i == 5,
                m => result = false,
                f => result = false,
                d => result = false,
                l => result = false,
                ul => result = false,
                s => result = false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5m);
            iunion.WithMatch(
                i => result = false,
                m => result = m == 5m,
                f => result = false,
                d => result = false,
                l => result = false,
                ul => result = false,
                s => result = false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5f);
            iunion.WithMatch(
                i => result = false,
                m => result = false,
                f => result = f == 5f,
                d => result = false,
                l => result = false,
                ul => result = false,
                s => result = false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5d);
            iunion.WithMatch(
                i => result = false,
                m => result = false,
                f => result = false,
                d => result = d == 5d,
                l => result = false,
                ul => result = false,
                s => result = false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5L);
            iunion.WithMatch(
                i => result = false,
                m => result = false,
                f => result = false,
                d => result = false,
                l => result = l == 5L,
                ul => result = false,
                s => result = false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(5uL);
            iunion.WithMatch(
                i => result = false,
                m => result = false,
                f => result = false,
                d => result = false,
                l => result = false,
                ul => result = ul == 5uL,
                s => result = false);
            Assert.IsTrue(result);

            iunion = new MyRefUnion("5");
            iunion.WithMatch(
                i => result = false,
                m => result = false,
                f => result = false,
                d => result = false,
                l => result = false,
                ul => result = false,
                s => result = s == "5");
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            iunion.WithMatch(
                i => result = false,
                m => result = false,
                m => result = false,
                d => result = false,
                l => result = false,
                ul => result = false,
                s => result = false,
                () => result = true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ImplicitsAndOf_Tests()
        {
            // implicits
            All all = 5;
            Assert.IsTrue(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out float _));
            Assert.IsFalse(all.Is(out double _));
            Assert.IsFalse(all.Is(out long _));
            Assert.IsFalse(all.Is(out ulong _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = 5m;
            Assert.IsFalse(all.Is(out int _));
            Assert.IsTrue(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out float _));
            Assert.IsFalse(all.Is(out double _));
            Assert.IsFalse(all.Is(out long _));
            Assert.IsFalse(all.Is(out ulong _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = 5f;
            Assert.IsFalse(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsTrue(all.Is(out float _));
            Assert.IsFalse(all.Is(out double _));
            Assert.IsFalse(all.Is(out long _));
            Assert.IsFalse(all.Is(out ulong _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = 5d;
            Assert.IsFalse(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out float _));
            Assert.IsTrue(all.Is(out double _));
            Assert.IsFalse(all.Is(out long _));
            Assert.IsFalse(all.Is(out ulong _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = 5L;
            Assert.IsFalse(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out float _));
            Assert.IsFalse(all.Is(out double _));
            Assert.IsTrue(all.Is(out long _));
            Assert.IsFalse(all.Is(out ulong _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = 5uL;
            Assert.IsFalse(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out float _));
            Assert.IsFalse(all.Is(out double _));
            Assert.IsFalse(all.Is(out long _));
            Assert.IsTrue(all.Is(out ulong _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = "5";
            Assert.IsFalse(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out float _));
            Assert.IsFalse(all.Is(out double _));
            Assert.IsFalse(all.Is(out long _));
            Assert.IsFalse(all.Is(out ulong _));
            Assert.IsTrue(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            // of
            all = All.Of(5);
            Assert.IsTrue(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out float _));
            Assert.IsFalse(all.Is(out double _));
            Assert.IsFalse(all.Is(out long _));
            Assert.IsFalse(all.Is(out ulong _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = All.Of(5m);
            Assert.IsFalse(all.Is(out int _));
            Assert.IsTrue(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out float _));
            Assert.IsFalse(all.Is(out double _));
            Assert.IsFalse(all.Is(out long _));
            Assert.IsFalse(all.Is(out ulong _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = All.Of(5f);
            Assert.IsFalse(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsTrue(all.Is(out float _));
            Assert.IsFalse(all.Is(out double _));
            Assert.IsFalse(all.Is(out long _));
            Assert.IsFalse(all.Is(out ulong _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = All.Of(5d);
            Assert.IsFalse(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out float _));
            Assert.IsTrue(all.Is(out double _));
            Assert.IsFalse(all.Is(out long _));
            Assert.IsFalse(all.Is(out ulong _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = All.Of(5L);
            Assert.IsFalse(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out float _));
            Assert.IsFalse(all.Is(out double _));
            Assert.IsTrue(all.Is(out long _));
            Assert.IsFalse(all.Is(out ulong _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = All.Of(5uL);
            Assert.IsFalse(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out float _));
            Assert.IsFalse(all.Is(out double _));
            Assert.IsFalse(all.Is(out long _));
            Assert.IsTrue(all.Is(out ulong _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = All.Of("5");
            Assert.IsFalse(all.Is(out int _));
            Assert.IsFalse(all.Is(out decimal _));
            Assert.IsFalse(all.Is(out float _));
            Assert.IsFalse(all.Is(out double _));
            Assert.IsFalse(all.Is(out long _));
            Assert.IsFalse(all.Is(out ulong _));
            Assert.IsTrue(all.Is(out string _));
            Assert.IsFalse(all.IsNull());
        }

        internal class MyRefUnion
        : RefUnion<int, decimal, float, double, long, ulong, string, MyRefUnion>
        {
            public MyRefUnion(object value) : base(value)
            {
            }
        }

        internal class All :
            RefUnion<int, decimal, float, double, long, ulong, string, All>,
            IUnionImplicits<int, decimal, float, double, long, ulong, string, All>,
            IUnionOf<int, decimal, float, double, long, ulong, string, All>
        {
            public All(object value) : base(value)
            {
            }

            public static All Of(int value) => new(value);

            public static All Of(decimal value) => new(value);

            public static All Of(float value) => new(value);

            public static All Of(double value) => new(value);

            public static All Of(long value) => new(value);

            public static All Of(ulong value) => new(value);

            public static All Of(string value) => new(value);

            public static implicit operator All(int value) => new(value);

            public static implicit operator All(decimal value) => new(value);

            public static implicit operator All(float value) => new(value);

            public static implicit operator All(double value) => new(value);

            public static implicit operator All(long value) => new(value);

            public static implicit operator All(ulong value) => new(value);

            public static implicit operator All(string value) => new(value);
        }

        internal class DuplicateTypeUnion :
            RefUnion<int, int, int, int, int, int, int, DuplicateTypeUnion>,
#pragma warning disable CS1956 // Member implements interface member with multiple matches at run-time
            IUnionImplicits<int, int, int, int, int, int, int, DuplicateTypeUnion>
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
