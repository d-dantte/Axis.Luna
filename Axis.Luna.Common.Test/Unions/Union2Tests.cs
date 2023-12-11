using Axis.Luna.Common.Unions;
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
        private readonly object _value;

        object IUnion<int, string, MyValueUnion>.Value => _value;

        public MyValueUnion(int value) => _value = value;

        public MyValueUnion(string value) => _value = value;

        public static MyValueUnion Of(int value) => new(value);

        public static MyValueUnion Of(string value) => new(value);

        public bool Is(out int value)
        {
            throw new NotImplementedException();
        }

        public bool Is(out string value)
        {
            throw new NotImplementedException();
        }

        public TOut MapMatch<TOut>(Func<int, TOut> t1Mapper, Func<string, TOut> t2Mapper, Func<TOut> nullMap = null)
        {
            throw new NotImplementedException();
        }

        public void ConsumeMatch(Action<int> t1Consumer, Action<string> t2Consumer)
        {
            throw new NotImplementedException();
        }

        public MyValueUnion WithMatch(Action<int> t1Consumer, Action<string> t2Consumer)
        {
            throw new NotImplementedException();
        }
    }
}
