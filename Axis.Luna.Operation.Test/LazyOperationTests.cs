using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test
{
    [TestClass]
    public class LazyOperationTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            SomeOperation().Wait();
        }

        [TestMethod]
        public void TestMethod2()
        {
            var result = SomeOperation2().Result;

            Assert.AreEqual(result, 6);
        }

        [TestMethod]
        public void TestMethod3()
        {
            try
            {
                FailedOperation().Wait();
            }
            catch (AggregateException e)
            {
                var ie = e.InnerException;
                Assert.IsTrue(ie.Message == "stuff");
            }
        }

        [TestMethod]
        public void TestMethod4()
        {
            var dic = new Dictionary<string, int>();
            try
            {
                FailedOperationWithRollback(dic).Wait();
            }
            catch (AggregateException e)
            {
                var ie = e.InnerException;
                Assert.IsTrue(dic["value"] == 2);
            }
        }


        private async Task SomeOperation()
        {
            await Operation.Try(() =>
            {
                //lazy operation
                Console.WriteLine("Lazy op");
            });
        }
        private async Task<int> SomeOperation2()
        {
            var op = Operation.Try(() =>
            {
                //lazy operation
                Console.WriteLine("Lazy op");
                return 6;
            });

            return await op;
        }

        private async Task FailedOperation()
        {
            await Operation.Try(() =>
            {
                var t = true;
                if (t)
                    throw new Exception("stuff");

                else return;
            });
        }
        private async Task FailedOperationWithRollback(Dictionary<string,int> value)
        {   
            await Operation.Try(() =>
            {
                value["value"] = 1;
                var t = true;
                if (t)
                    throw new Exception("stuff");

                else return;
            }, async () =>
            {
                value["value"] = 2;
            });
        }
    }
}
