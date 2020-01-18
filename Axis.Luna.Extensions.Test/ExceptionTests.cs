using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

using static Axis.Luna.Extensions.ExceptionExtension;

namespace Axis.Luna.Extensions.Test
{
    [TestClass]
    public class ExceptionTests
    {
        [TestMethod]
        public void ThrowsNullArgument1()
        {
            string a = "k",
                   b = null,
                   c = "";

            var ex = Assert.ThrowsException<ArgumentNullException>(() => ThrowNullArguments(
                () => a,
                () => b,
                () => c));

            Console.WriteLine(ex);
        }

        [TestMethod]
        public void ThrowsNullArgument2()
        {
            string a = "k",
                   b = null,
                   c = "";

            var ex = Assert.ThrowsException<ArgumentNullException>(() => ThrowNullArguments(
                nameof(a).ObjectPair(a),
                nameof(b).ObjectPair(b),
                nameof(c).ObjectPair(c)));

            Console.WriteLine(ex);
        }


        [TestMethod]
        public void ThrowsNullArgumentPerformance()
        {
            int a = 0,
                b = 0,
                c = 0,
                d = 0;

            var now = DateTime.Now;
            ThrowNullArguments(
                () => a,
                () => b,
                () => c,
                () => d);
            var time = DateTime.Now - now;
            Console.WriteLine("Use of expression: " + time);

            now = DateTime.Now;
            ThrowNullArguments(
                nameof(a).ObjectPair(a),
                nameof(b).ObjectPair(b),
                nameof(c).ObjectPair(c),
                nameof(d).ObjectPair(d));
            time = DateTime.Now - now;
            Console.WriteLine("Use of kvp: " + time);

            now = DateTime.Now;
            ThrowNullArguments(
                () => a,
                () => b,
                () => c,
                () => d);
            time = DateTime.Now - now;
            Console.WriteLine("Use of expression: " + time);

            now = DateTime.Now;
            ThrowNullArguments(
                nameof(a).ObjectPair(a),
                nameof(b).ObjectPair(b),
                nameof(c).ObjectPair(c),
                nameof(d).ObjectPair(d));
            time = DateTime.Now - now;
            Console.WriteLine("Use of kvp: " + time);
        }
    }
}
