using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Dynamic;

namespace Axis.Luna.Test
{
    [TestClass]
    public class DynamicObjectTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            dynamic x = new Dynamic();
            var t = (IDisposable)x;
            t.Dispose();
        }
    }

    public interface ISomething
    {
        int SomeMethod();
    }

    public class Dynamic: DynamicObject
    {
        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = null;
            return true;
        }
    }
}
