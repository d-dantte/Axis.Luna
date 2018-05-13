using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Axis.Luna.Extensions;
using System.Reflection;
using System.Collections.Generic;

namespace Axis.Luna.Test
{
    [TestClass]
    public class TypeExtensionTests
    {
        [TestMethod]
        public void MethodSignatureTest()
        {
            var d = new DummyClass();

            Action a1 = d.Method1;
            Console.WriteLine(a1.MinimalAQSignature());

            Action<int, string, DateTime> a2 = d.Method2;
            Console.WriteLine(a2.MinimalAQSignature());

            Func<string> a3 = d.Method3;
            Console.WriteLine(a3.MinimalAQSignature());

            Func<int, string, DateTime, string> a4 = d.Method4;
            Console.WriteLine(a4.MinimalAQSignature());

            a1 = d.Method5<TimeSpan>;
            Console.WriteLine(a1.MinimalAQSignature());

            a2 = d.Method6<Pointer, int>;
            Console.WriteLine(a2.MinimalAQSignature());

            a3 = d.Method7<int, float>;
            Console.WriteLine(a3.MinimalAQSignature());

            a4 = d.Method8<int, string, decimal>;
            Console.WriteLine(a4.MinimalAQSignature());
        }

        [TestMethod]
        public void ImplementsGenericInterfaceTest()
        {
            var result = typeof(HashSet<int>).ImplementsGenericInterface(typeof(ICollection<>));
            Assert.IsTrue(result);

            result = typeof(HashSet<int>).ImplementsGenericInterface(typeof(IList<>));
            Assert.IsFalse(result);

            result = typeof(HashSet<int>).HasGenericAncestor(typeof(HashSet<>));
            Assert.IsTrue(result);
        }
    }

    public class DummyClass
    {
        public void Method1()
        { }
        public void Method2(int x, string y, DateTime z)
        { }

        public string Method3() => null;
        public string Method4(int x, string y, DateTime z) => null;


        public void Method5<T>()
        { }
        public void Method6<T, R>(int x, string y, DateTime z)
        { }

        public string Method7<T, U>() => null;
        public string Method8<X, Y, Z>(int x, string y, DateTime z) => null;
    }
}
