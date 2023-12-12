using Axis.Luna.Common.Unions;
using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Axis.Luna.Common.Test.Unions
{
    [TestClass]
    public class Union2Tests
    {
        [TestMethod]
        public void GeneralTests()
        {
            var runion = new MyRefUnion(4);
            Assert.IsFalse(runion.Is(out string _));
            Assert.IsTrue(runion.Is(out int v));
            Assert.AreEqual(4, v);

            var sresult = runion.MapMatch(
                i => i.ToString(),
                s => s);
            Assert.AreEqual("4", sresult);

            Assert.ThrowsException<ArgumentNullException>(() => runion.MapMatch(null, s => s));
            Assert.ThrowsException<ArgumentNullException>(() => runion.MapMatch(i => i, null));
            Assert.ThrowsException<ArgumentNullException>(() => runion.ConsumeMatch(null, Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => runion.ConsumeMatch(Console.WriteLine, null));
            Assert.ThrowsException<ArgumentNullException>(() => runion.WithMatch(null, Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => runion.WithMatch(Console.WriteLine, null));

            runion = new MyRefUnion(null);
            Assert.IsTrue(runion.IsNull());
        }
    }

    internal class MyRefUnion : RefUnion<int, string, MyRefUnion>
    {
        public MyRefUnion(int value)
        : base(value) { }

        public MyRefUnion(string value)
        : base(value) { }
    }

    internal struct MyValueUnion :
        IUnion<int, string, MyValueUnion>,
        IUnionOf<int, string, MyValueUnion>
    {
        object IUnion<int, string, MyValueUnion>.Value { get; }

        public static MyValueUnion Of(int value)
        {
            throw new NotImplementedException();
        }

        public static MyValueUnion Of(string value)
        {
            throw new NotImplementedException();
        }

        public void ConsumeMatch(Action<int> t1Consumer, Action<string> t2Consumer, Action nullConsumer = null)
        {
            throw new NotImplementedException();
        }

        public bool Is(out int value)
        {
            throw new NotImplementedException();
        }

        public bool Is(out string value)
        {
            throw new NotImplementedException();
        }

        public bool IsNull()
        {
            throw new NotImplementedException();
        }

        public TOut MapMatch<TOut>(Func<int, TOut> t1Mapper, Func<string, TOut> t2Mapper, Func<TOut> nullMap = null)
        {
            throw new NotImplementedException();
        }

        public MyValueUnion WithMatch(Action<int> t1Consumer, Action<string> t2Consumer, Action nullConsumer = null)
        {
            throw new NotImplementedException();
        }
    }
}
