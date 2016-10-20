using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Axis.Luna.Extensions;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using static Axis.Luna.Extensions.ObjectExtensions;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using Axis.Luna.MetaTypes;

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
            var r = 
            Operation.TryAsync(() => { Thread.Sleep(100); Console.WriteLine("asynchronious 1"); throw new Exception("exceptioned"); })
                .Then(op => Console.WriteLine("async 2"))
                //.Resolve();
                .Result;
                //;
            Console.WriteLine("Ending");

        }

        [TestMethod]
        public void Base64Test()
        {
            var x = Convert.ToBase64String(Encoding.Unicode.GetBytes("1234567890"));
            Console.WriteLine(x);

        }

        [TestMethod]
        public void JsonConversionTest()
        {
            var obj = new SomeClass();
            obj.Numbers.Add(4);
            obj.Numbers.Add(5);
            obj.Numbers.Add(1);

            var json = JsonConvert.SerializeObject(obj);

            Console.WriteLine(json);

            var obj2 = JsonConvert.DeserializeObject<SomeClass>(json);

            var d = JsonConvert.SerializeObject(new BinaryData().ReferenceData("http://placehold.it/300", "profileImage.jpg"));


        }
    }

    public class SomeClass
    {
        public TimeSpan Duration { get; set; } = TimeSpan.FromDays(3.53);
        public List<int> Numbers { get; private set; } = new List<int>();
    }

    public class ABCD
    {
        public Operation Op1() => Operation.Try(() => { });
        public Operation<int> Op2() => Operation.FromValue(3);
        public Operation<@void> Op3() => Operation.FromValue(Void.@void);

        public ABCD()
        {
        }
    }
}
