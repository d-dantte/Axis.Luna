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
using System.Linq.Expressions;

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

        [TestMethod]
        public void TestNodeTraversal()
        {
            var node = new Node();

            node.Left = new Node();
            node.Right = new Node();

            node.Right.Left = new Node();
            node.Right.Right = new Node();

            node.Right.Right.Left = new Node();
            node.Right.Right.Right = new Node();

            node.Right.Right.Right.Left = new Node();
            node.Right.Right.Right.Right = new Node();


            Console.WriteLine(BreathFirstCount(node));
        }

        public long BreathFirstCount(Node node)
        {
            if (node == null) return 0;
            else
            {
                IEnumerable<Node> level = node == null ? new Node[0] : new[] { node };
                var count = 0L;
                var ccache = 0l;
                while ((ccache = level.Count()) > 0)
                {
                    count += ccache;
                    level = level.SelectMany(_x => _x.Nodes()); //<-- this line makes it possible!!
                }

                return count;
            }
        }

        [TestMethod]
        public void TestCopyTo()
        {
            Expression<Func<Node, bool>> expr = n => n.Left == null || n.Right == null || n.Left.GetHashCode() == 0;

            var tokens = new string[] { "abcd", "efgh" };
            var props = new string[] { "Name", "Title" };
            Expression exp = null;
            var param = Expression.Parameter(typeof(Node), "n");
            foreach(var t in tokens)
            {
                foreach (var p in props)
                {
                    var propAccess = Expression.PropertyOrField(param, p);
                    var callExp = Expression.Call(propAccess, typeof(string).GetMethod("Contains"), Expression.Constant(t));
                    if (exp == null) exp = callExp;
                    else exp = Expression.OrElse(exp, callExp);
                }
            }

            var lambda = Expression.Lambda(exp, param);
        }

        [TestMethod]
        public void SequencePageSerializationTest()
        {
            var sp = new SequencePage<System.IO.Stream>(new System.IO.Stream[0], 0, 30, 0);
            var json = JsonConvert.SerializeObject(sp);
            Console.WriteLine(json);
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

    public class Node
    {
        public Node Left { get; set; }
        public Node Right { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }

        public IEnumerable<Node> Nodes()
        {
            if (Left != null) yield return Left;
            if (Right != null) yield return Right;
            else yield break;
        }

        public void SomeFunction(Source<int, Source<string, Source<long>>> p)
        {

        }
    }

    public class Source
    { }

    public class Source<T>: Source
    {
        public T Value { get; set; }

        public Source(T value)
        {
            this.Value = value;
        }
    }

    public class Source<T, R>: Source<T>
    where R: Source
    {
        public R InnerSources { get; private set; }

        public IEnumerable<object> Values()
        {
            ///use some clever reflection or other techniques to bunch together this objects "Value", and subsequent inner-source's values

            throw new NotImplementedException();
        }

        public Source(T value, R aggregate): base(value)
        {
            this.InnerSources = aggregate;
        }
    }

    public interface IResolver<Source>
    {

    }
    

    public class Merger<TDestination>
    {
        public TDestination Merge(TDestination destination, Source source)
        {
            ///merging code here

            throw new NotImplementedException();
        }
    }


    public class BusinessObject1 { }
    public class BusinessObject2 { }
    public class BusinessObject3 { }
    public class BusinessObject4 { }


    public class SomeClassHavingMergingLogic
    {
        public void SomeMethod()
        {
            var merger = new Merger<BusinessObject4>();

            //this may also be achieved with some fluent-api to make it more visually palatable
            var sources = new Source<BusinessObject1, Source<BusinessObject2, Source<BusinessObject3>>>(new BusinessObject1(),
                          new Source<BusinessObject2, Source<BusinessObject3>>(new BusinessObject2(),
                          new Source<BusinessObject3>(new BusinessObject3())));

            var merged = merger.Merge(new BusinessObject4(), sources);

            //do with merged as you please
        }
    }

}
