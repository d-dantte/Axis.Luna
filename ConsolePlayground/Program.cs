using Axis.Luna.Extensions;
using Axis.Luna.Operation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace ConsolePlayground
{
    class Program
    {
        static void _Main(string[] args)
        {
            Task<int> t;
            var op = LazyOp.Try(() =>
            {
                Console.WriteLine("starting");
            })
            .Then(() => SomeOperation(), null)
            .Then(_v =>
            {
                Console.WriteLine($"last one returned: {_v}");
                return _v;
            })
            .Then(_v =>
            {
                throw new Exception("oops");
            })
            .ContinueWith(_op =>
            {
                Console.WriteLine($"thrown exception is {_op.GetException()}");
                return "new value";
            });
            
            Console.WriteLine($"Before resolving, Result = {op.Result}");
            op.Resolve();
            Console.WriteLine($"After resolving, Result = {op.Result}");
            
            Console.ReadKey();
        }


        static int[][] clockwise(int[][] s, int rotations)
        {
            var newArr = new int[3][] { s[0].Clone() as int[], s[1].Clone() as int[], s[2].Clone() as int[] };
            for (int cnt = 0; cnt < rotations*2; cnt++)
            {
                var interim = newArr[0][0];
                newArr[0][0] = newArr[1][0];
                newArr[1][0] = newArr[2][0];
                newArr[2][0] = newArr[2][1];
                newArr[2][1] = newArr[2][2];
                newArr[2][2] = newArr[1][2];
                newArr[1][2] = newArr[0][2];
                newArr[0][2] = newArr[0][1];
                newArr[0][1] = interim;
            }

            return newArr;
        }
        static int[][] flip(int[][] s)
        {
            return new int[3][] { s[2].Clone() as int[], s[1].Clone() as int[], s[0].Clone() as int[] };
        }
        static int cost(int[][] sourceSquare, int[][] targetSquare)
        {
            var rank = 0;
            for (int cnty = 0; cnty < 3; cnty++)
            {
                for (int cntx = 0; cntx < 3; cntx++)
                {
                    if (sourceSquare[cnty][cntx] != targetSquare[cnty][cntx]) rank += targetSquare[cnty][cntx] - sourceSquare[cnty][cntx];
                }
            }
            return rank;
        }

        static int formingMagicSquare(int[][] s)
        {
            // Complete this function
            int[][] origin = new int[3][] { new[] { 8, 3, 4 }, new[] { 1, 5, 9 }, new[] { 6, 7, 2 } };
            var match = Enumerable.Range(0, 8)
                .Select(_cnt =>
                {
                    if (_cnt < 4)
                    {
                        var psquare = clockwise(origin, _cnt);
                        return new
                        {
                            PSquare = psquare,
                            Cost = cost(s, psquare)
                        };
                    }
                    else
                    {
                        var psquare = clockwise(flip(origin), _cnt - 4);
                        return new
                        {
                            PSquare = psquare,
                            Cost = cost(s, psquare)
                        };
                    }
                })
                .OrderBy(_v => _v.Cost)
                .ToList();

            return match.Select(_r => _r.Cost).FirstOrDefault();
        }

        public static void __Main(string[] args)
        {
            int[][] origin = new int[3][] { new[] { 4, 8, 2 }, new[] { 4, 5, 7 }, new[] { 6, 1, 6 } };
            Console.WriteLine(formingMagicSquare(origin));

            Console.ReadKey();
        }

        public static IOperation<string> SomeOperation() => AsyncOp.Try((() => "yes"));


        public static void Main(string[] args)
        {
            //var task = Task.FromResult(1);
            //task.GetAwaiter().OnCompleted(() => Console.WriteLine("called"));

            //int? t = null;
            //task = new Task<int>(() => t ?? throw new Exception());
            //task.GetAwaiter().OnCompleted(() => Console.WriteLine("called"));
            //task.GetAwaiter().OnCompleted(() => Console.WriteLine("called2"));

            //task.RunSynchronously();

            //Thread.Sleep(500);

            //try
            //{
            //    var r = task.Result;
            //}
            //catch(Exception e)
            //{ }

            OtherFunc();

            var task = Task.Run(() => Thread.Sleep(1000));
            task.Start();

            Console.ReadKey();
        }

        public static async void OtherFunc()
        {
            var obj = await SomeFunc();

            Console.WriteLine("things: " + obj);
        }


        public static LazyAwaitable SomeFunc()
        {
            return new LazyAwaitable(() =>
            {
                Console.Write("Lazy called");
                return true;
            });
        }
    }

    public struct LazyAwaiter: INotifyCompletion
    {
        private Lazy<object> _lazy;
        private List<Pair> _continuation;

        public LazyAwaiter(Lazy<object> lazy)
        {
            _lazy = lazy;
            _continuation = new List<Pair>(); ;
        }

        public bool IsCompleted => _lazy.IsValueCreated;

        public object GetResult() => _lazy.Value;

        public void OnCompleted(Action continuation)
        {
            var pair = new Pair { IsCalled = false, Action = continuation };
            _continuation.Add(pair);

            if (!IsCompleted)
            {
                try
                {
                    var t = _lazy.Value;
                }
                catch { }
            }

            if(IsCompleted)
            {
                pair.IsCalled = true;
                pair.Action.Invoke();
            }
        }

        public class Pair
        {
            public bool IsCalled { get; set; }
            public Action Action { get; set; }
        }
    }

    public class LazyAwaitable
    {
        public Lazy<object> _lazy;
        private LazyAwaiter _awaiter;

        public LazyAwaitable(Func<object> func)
        {
            this._lazy = new Lazy<object>(func, true);
            _awaiter = new LazyAwaiter(_lazy);
        }

        public LazyAwaiter GetAwaiter() => _awaiter;
    }
}