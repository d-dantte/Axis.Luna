using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var sd = new SomeData { Property = "kingsglave" };
            CallContext.LogicalSetData("ABCD", sd);

            var t = Task.Run(() =>
            {
                Console.WriteLine(CallContext.LogicalGetData("ABCD"));
            });

            t.Wait();

            Console.WriteLine(CallContext.LogicalGetData("ABCD"));

            Console.ReadKey();
        }
    }

    public class SomeData
    {
        public string Property { get; set; }

        public override string ToString()
        {
            return $"[property: {Property}]";
        }
    }
}
