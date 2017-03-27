using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Axis.Luna.Test
{
    [TestClass]
    public class StackTraceTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var start = DateTime.Now;
            var trace = new StackTrace(0);
            Console.WriteLine(DateTime.Now - start);

            start = DateTime.Now;
            trace = new StackTrace(0);
            Console.WriteLine(DateTime.Now - start);

            start = DateTime.Now;
            trace = new StackTrace(0);
            Console.WriteLine(DateTime.Now - start);

            start = DateTime.Now;
            trace = new StackTrace(0);
            Console.WriteLine(DateTime.Now - start);

            start = DateTime.Now;
            trace = new StackTrace(0);
            Console.WriteLine(DateTime.Now - start);

            start = DateTime.Now;
            trace = new StackTrace(0);
            Console.WriteLine(DateTime.Now - start);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var callee = new Callee();

            Console.WriteLine(callee.LineNumbered());
            Console.WriteLine(callee.LineNumbered());

            Thread t;
        }
    }

    public class Callee
    {
        public int LineNumbered([CallerLineNumber] int lineNumber = 0)
        {
            return lineNumber;
        }
        public int LineNumbered2(string something, [CallerLineNumber] int lineNumber = 0)
        {
            return lineNumber;
        }
    }
}
