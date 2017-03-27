using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Luna.Test
{
    [TestClass]
    public class WeakCacheTest
    {
        [TestMethod]
        public void GCTest()
        {
            var weakCache = new WeakCache();

            Func<string, XYZClass> generator = _k =>
            {
                return new XYZClass { Name = _k };
            };

            var v = weakCache.GetOrAdd("a", generator);
            Assert.AreEqual("a", v.Name);
            v = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsNull(weakCache.Get<XYZClass>("a"));
        }


        [TestMethod]
        public void GetOrAddTest()
        {
            var weakCache = new WeakCache();

            Func<string, XYZClass> generator = _k =>
            {
                return new XYZClass { Name = _k };
            };

            //get or add test
            var x = weakCache.GetOrAdd("5", generator);
            Assert.AreEqual("5", x.Name);
            x = null;

            //gc collection test
            GC.Collect();
            GC.WaitForPendingFinalizers();
            x = weakCache.Get<XYZClass>("5");
            Assert.IsNull(x);

            //refresh test
            weakCache.Refresh<XYZClass>("5");
            Assert.IsNotNull(weakCache.Get<XYZClass>("5"));

            //invalidate test
            weakCache.Invalidate("5");
            Assert.IsNull(weakCache.Get<XYZClass>("5"));
        }

        [TestMethod]
        public void ConcurrencyDictTest()
        {
            var dict = new ConcurrentDictionary<string, string>();
            var v1 = dict.GetOrAdd("abcd", _k =>
            {
                Console.WriteLine("called 1");
                return 1.ToString();
            });

            var v2 = dict.GetOrAdd("abcd", _k =>
            {
                Console.WriteLine("called 2");
                return 2.ToString();
            });


        }
        [TestMethod]
        public void GCCollectionTest()
        {
            using (var rr = new RRR())
            {
                var @ref = new WeakReference<XYZClass>(new XYZClass { Name = "bleh" });

                GC.Collect();
                GC.WaitForPendingFinalizers();

                XYZClass t;
                @ref.TryGetTarget(out t);

                Console.WriteLine(t == null);
                Console.WriteLine(t.Name);
            }

            try
            {
                RRR g;
            }
            finally
            {
            }
        }
    }

    public class XYZClass
    {
        public string Name { get; set; }
    }

    public class RRR : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
