using Axis.Luna.Operation;
using Axis.Luna.Operation.Utils;
using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Playground2
{
    class Program
    {
        public event Action<string> SomeEvent;

        public void whatever()
        {
            SomeEvent += x => Console.WriteLine(x);
        }

        public static void Main(string[] args)
        {
            var p = new Program();
            p.whatever();
        }


        static void Main_(string[] args)
        {

            var op = Operation.Try(() =>
            {
                Console.WriteLine("1st");
            })
            .Then(async () =>
            {
                Console.WriteLine("2nd");
                return 2;
            })
            .Then(_p =>
            {
                Console.WriteLine($"3nd   [{_p}]");
                return 3 + _p;
            });

            Thread.Sleep(400);
            Console.WriteLine("After operation construction");

            var x = op.Resolve();

            Console.WriteLine($"After operation resolution  {x}");



            Console.ReadKey();
        }

        static void Main__(string[] args)
        {
            var t = AwaitThem();

            Console.WriteLine("Called await them");

            var result = t.Result;

            Console.WriteLine("Result of awaiting them: " + result);


            Console.ReadKey();
        }

        static void Main___(string[] args)
        {
            var task = Task.Run(() => Console.WriteLine("task has run"));
            task.GetAwaiter().OnCompleted(() => Console.WriteLine($"first task completed as: {task.Status}"));
            //Console.WriteLine($"task's status after waiting: {task.Status}");

            task = Task.Run(() => throw new Exception("faulted"));
            task.GetAwaiter().OnCompleted(() => Console.WriteLine($"first task completed as: {task.Status}"));
            try
            {
            }
            catch(Exception e)
            {
            }

            Console.ReadKey();
        }

        public static Exception Raise() => new NullReferenceException();


        static async Task<string> AwaitThem()
        {
            var i = await Operation.Try(Task.Run(() =>
            {
                Thread.Sleep(300);
                return DateTime.Now.Millisecond;
            }));

            Console.WriteLine("after the first async operation");

            var b = await Operation.Try(() => "me");

            Console.WriteLine("after the first lazy operation");

            return await Operation.Try(() => i + b);
        }

        static async Task<R> AwaitIt<R>(IOperation<R> op)
        {
            Thread.Sleep(5000);
            Console.WriteLine("Thread woken");
            var value = await op;

            Thread.Sleep(5000);
            Console.WriteLine("awaited the operation..");

            return value;
        }
    }
}
