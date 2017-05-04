using Axis.Luna.Extensions;
using Axis.Luna.Operation;
using System;
using System.Security.Cryptography;

namespace ConsolePlayground
{
    class Program
    {
        static void Main(string[] args)
        {
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

        public static IOperation<string> SomeOperation() => AsyncOp.Try((() => "yes"));
    }
}