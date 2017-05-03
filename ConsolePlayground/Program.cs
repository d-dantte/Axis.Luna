using Axis.Luna.Operation;
using System;

namespace ConsolePlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            var op = LazyOp.Try(() => Console.WriteLine("resolved"));
            op.Resolve();
        }
    }
}
