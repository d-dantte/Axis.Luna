using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test
{
    [TestClass]
    public class MiscTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            Nito.AsyncEx.AsyncContext.Run(async () =>
            {
                var l = new List<Task>();
                var t = Delay(1000, l);
                l.Add(t);
                Console.WriteLine($"While delaying: {t.Status}");
                await t;
            });


            Console.WriteLine("exiting...");
        }

        public async Task Delay(int milliseconds, List<Task> tlist)
        {
            await Task.Delay(milliseconds);
            Console.WriteLine($"After delaying, before returning from the delay method: {tlist[0].Status}");
        }
    }
}
