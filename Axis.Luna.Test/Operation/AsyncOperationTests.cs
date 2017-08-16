using Axis.Luna.Extensions;
using Axis.Luna.Operation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Axis.Luna.Test.Operation
{
    [TestClass]
    public class AsyncOperationTests
    {

        [TestMethod]
        public void AsyncResolutionTest()
        {
            var resolved = false;
            var op = AsyncOp.Try(() =>
            {
                Thread.Sleep(100);
                resolved = true;
            });
            Assert.IsFalse(resolved);
            op.Resolve();
            Assert.IsTrue(resolved);

            resolved = false;
            var op_ = AsyncOp.Try(() =>
            {
                Thread.Sleep(100);
                return resolved = true;
            });
            Assert.IsFalse(resolved);
            op_.Resolve();
            Assert.IsTrue(resolved);

            op = AsyncOp.Fail(new Exception("ex"));
            try
            {
                op.Resolve();
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "ex");
            }

            op_ = AsyncOp.Fail<bool>(new Exception("ex"));
            try
            {
                op_.Resolve();
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "ex");
            }
        }

        [TestMethod]
        public void OperationStatusTest()
        {
            var op =   AsyncOp.Try(() => Thread.Sleep(100));
            Assert.IsTrue(op.Succeeded == null);

            var ops =  AsyncOp.Try(() => "done".UsingValue(_d => Thread.Sleep(100)));
            Assert.IsTrue(ops.Succeeded == null);

            var _op =  AsyncOp.Try(() => 
            {
                Thread.Sleep(100);
                if (true) throw new Exception();
                else return;
            });
            Assert.IsTrue(_op.Succeeded == null);

            var _ops = AsyncOp.Try(() =>
            {
                Thread.Sleep(100);
                var @true = true;
                if (@true) throw new Exception();
                else return "done";
            });
            Assert.IsTrue(_ops.Succeeded == null);

            Thread.Sleep(1000);
            Assert.IsTrue(op.Succeeded == true);
            Assert.IsTrue(ops.Succeeded == true);
            Assert.IsTrue(_op.Succeeded == false);
            Assert.IsTrue(_ops.Succeeded == false);
        }

        [TestMethod]
        public void ContinuationPassingTest()
        {
            var continues = new List<int>();
            var @params = new List<int>();
            var op = ResolvedOp.Try(() => continues.Add(1))

                //first successfull continuation
                .Then(() => continues.Add(2))

                //second successfull continuation that will itself fail and transition to a failed operation
                .Then(() =>
                {
                    continues.Add(3);
                    throw new Exception();
                }, ex => continues.Add(-1))

                //first failed operation
                .Then(() => continues.Add(-1), ex => continues.Add(4))

                //transition back to a successful operation
                .ContinueWith(_opr =>
                {
                    if (_opr.Succeeded == false) continues.Add(5);
                    else continues.Add(-1);
                })

                .Then(() => continues.Add(6), ex => continues.Add(-1));

            Thread.Sleep(1000);
            Assert.IsTrue(op.Succeeded == true);
            Assert.AreEqual(continues[0], 1);
            Assert.AreEqual(continues[1], 2);
            Assert.AreEqual(continues[2], 3);
            Assert.AreEqual(continues[3], 4);
            Assert.AreEqual(continues[4], 5);
            Assert.AreEqual(continues[5], 6);
        }


        [TestMethod]
        public void AwaitTest()
        {
            AwaitInner().Wait();
        }

        private async Task AwaitInner()
        {
            var x = await AsyncOp
                .Try(() => 6)
                .Then(_ => 5)
                .Cast<AsyncOperation<int>>();

            Assert.AreNotEqual(6, x);
            Assert.AreEqual(5, x);
        }

        [TestMethod]
        public void TaskExtensionTest()
        {
            TaskExtensionInner().Wait();
        }

        private async Task TaskExtensionInner()
        {
            var x = await Task
                .Run(() => 6)
                .Then(_ => 5);

            Assert.AreNotEqual(6, x);
            Assert.AreEqual(5, x);
        }
    }
}
