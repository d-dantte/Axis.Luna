using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Axis.Luna.Extensions;

namespace Axis.Luna.Test
{
    /// <summary>
    /// Summary description for LazyOperationTest
    /// </summary>
    [TestClass]
    public class LazyOperationTest
    {
        [TestMethod]
        public void Test1()
        {
            var op = Operation.TryLazily(() => Console.WriteLine($"called at {DateTime.Now}"))
                .Then(() => Console.WriteLine($"THEN called again at {DateTime.Now}"));

            Console.WriteLine("After creating the lazy op");
            op.Resolve();
            Console.WriteLine("After resolving the op");
        }
    }
}
