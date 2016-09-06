using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Axis.Luna.Extensions;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using static Axis.Luna.Extensions.ObjectExtensions;

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

        [TestMethod]
        public void SpliceTest()
        {
            //Enumerable.Range(0, 10)
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }
                .Splice(5)
                .ForAll((x, y) => Console.WriteLine(y));
        }


        [TestMethod]
        public void PositionOfTest()
        {
            //Enumerable.Range(0, 10)
            new int[] { 0, 1, 2, 3, 4, 4, 5, 6, 7, 8, 9, 0 }
                .PositionOf(6)
                .Pipe(t => Console.WriteLine(t));
        }
        [TestMethod]
        public void CancellationTokenTest()
        {
            var t = new Task(() => Console.WriteLine("doing stuff..."), default(CancellationToken));
            t.Start();
            t.Wait();
        }

        [TestMethod]
        public void AsyncOpTest()
        {
            //Operation.TryAsync(() => { Thread.Sleep(100); Console.WriteLine("asynchronious 1"); })
            //    .Then(op => Console.WriteLine("async 2"))
            //    .Resolve();
            //Console.WriteLine("Ending");

            var t = Task.Run(() => { throw new Exception("initial exception"); });
            Eval(() => t.Wait());

            var t2 = t.ContinueWith((_t) => Console.WriteLine("stuff"));
            Eval(() => t2.Wait());

            Thread.Sleep(3000);
            var ex1 = t.Exception;
            var ex2 = t2.Exception;

        }
    }
}
