using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
        public void TestMethod()
        {
            var weakCache = new WeakCache();

            Func<string, XYZClass> generator = _k => new XYZClass { Name = RandomAlphaNumericGenerator.RandomAlpha(10) };


            Console.WriteLine(weakCache.GetOrAdd("1", generator));

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine(weakCache.GetOrAdd("1", generator));
        }
    }

    public class XYZClass
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return $"XYZClass[ Name = {Name}]";
        }
    }
}
