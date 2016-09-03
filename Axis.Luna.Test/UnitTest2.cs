using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Axis.Luna.Extensions;
using System.Linq;

namespace Axis.Luna.Test
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestMethod1()
        {
            var seq = 100.GenerateSequence(v => v).ToArray();

            Console.WriteLine("\nLeft edge [0]");
            var page = seq.Paginate(0, 5);
            page.AdjacentIndexes(2)
                .ForAll((x, y) => Console.Write($"[{y}] "));

            Console.WriteLine("\n\nClose to Left edge [1]");
            page = seq.Paginate(1, 5);
            page.AdjacentIndexes(2)
                .ForAll((x, y) => Console.Write($"[{y}] "));

            Console.WriteLine("\n\nMiddle [7]");
            page = seq.Paginate(7, 5);
            page.AdjacentIndexes(2)
                .ForAll((x, y) => Console.Write($"[{y}] "));

            Console.WriteLine("\n\nClose to Right edge [18]");
            page = seq.Paginate(18, 5);
            page.AdjacentIndexes(2)
                .ForAll((x, y) => Console.Write($"[{y}] "));

            Console.WriteLine("\n\nRight edge [19]");
            page = seq.Paginate(19, 5);
            page.AdjacentIndexes(2)
                .ForAll((x, y) => Console.Write($"[{y}] "));
        }
    }
}
