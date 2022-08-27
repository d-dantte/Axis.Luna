using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Luna.Extensions.Test
{
    [TestClass]
    public class AsyncExtensionTests
    {
        [TestMethod]
        public async Task Test1()
        {
            var list = new List<Task<int>>();
            var keyBase = "SomeKey";

            for(int cnt=0; cnt < 10; cnt++)
            {
                var key = keyBase + cnt;
                var t = cnt;
                list.Add(key.AsyncLock(async () =>
                {
                    Console.WriteLine("before sleeping: " + t);
                    await Task.Delay(1000);
                    Console.WriteLine("after sleeping: " + t);
                    return 0;
                }));

            }

            await Task.WhenAll(list.ToArray());
        }
    }
}
