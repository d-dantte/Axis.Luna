using Axis.Luna.Operation;
using Axis.Luna.Operation.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Playground2
{
    class Program
    {
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

        static void Main(string[] args)
        {
            var t = AwaitThem();

            Console.WriteLine("Called await them");

            var result = t.Result;

            Console.WriteLine("Result of awaiting them: " + result);


            Console.ReadKey();
        }


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
