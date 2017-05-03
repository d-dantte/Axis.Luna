using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Axis.Luna.Operation;
using System.Collections.Generic;

namespace Axis.Luna.Test.Operation
{
    [TestClass]
    public class LazyOperationTests
    {

        [TestMethod]
        public void LazyResolutionTest()
        {
            var resolved = false;
            var op = LazyOp.Try(() =>
            {
                resolved = true;
            });
            Assert.IsFalse(resolved);
            op.Resolve();
            Assert.IsTrue(resolved);

            resolved = false;
            var opb = LazyOp.Try(() => resolved = true);
            Assert.IsFalse(resolved);
            opb.Resolve();
            Assert.IsTrue(resolved);
            Assert.IsTrue(opb.Result);

            op = LazyOp.Fail(new Exception("ex"));
            try
            {
                op.Resolve();
            }
            catch(Exception e)
            {
                Assert.AreEqual(e.Message, "ex");
            }

            opb = LazyOp.Fail<bool>(new Exception("ex"));
            try
            {
                opb.Resolve();
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "ex");
            }
        }

        [TestMethod]
        public void OperationStatusTest()
        {
            var op = LazyOp.Try(() => { });
            Assert.IsTrue(op.Succeeded == null);
            op.Resolve();

            var ops = LazyOp.Try(() => "done");
            Assert.IsTrue(ops.Succeeded == null);
            ops.Resolve();

            var _op = LazyOp.Try(() => { throw new Exception(); });
            Assert.IsTrue(_op.Succeeded == null);
            try { _op.Resolve(); } catch { }

            var _ops = LazyOp.Try(() =>
            {
                var @true = true;
                if (@true) throw new Exception();
                else return "done";
            });
            Assert.IsTrue(_ops.Succeeded == null);
            try { _ops.Resolve(); } catch { }

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
            var op = LazyOp.Try(() => continues.Add(1))

                //first successfull continuation
                .Then(() =>
                {
                    continues.Add(2);
                })

                //second successfull continuation that will itself fail and transition to a failed operation
                .Then(() =>
                {
                    continues.Add(3);
                    if(true)throw new Exception();
                }, ex =>
                {
                    continues.Add(-1);
                })

                //first failed operation
                .Then(() =>
                {
                    continues.Add(-1);
                }, ex =>
                {
                    continues.Add(4);
                })

                //transition back to a successful operation
                .ContinueWith(_opr =>
                {
                    if (_opr.Succeeded == false) continues.Add(5);
                    else continues.Add(-1);
                })

                .Then(() =>
                {
                    continues.Add(6);
                }, ex =>
                {
                    continues.Add(-1);
                });

            Assert.AreEqual(continues.Count, 0);
            try { op.Resolve(); } catch { }

            Assert.IsTrue(op.Succeeded == true);
            Assert.AreEqual(continues[0], 1);
            Assert.AreEqual(continues[1], 2);
            Assert.AreEqual(continues[2], 3);
            Assert.AreEqual(continues[3], 4);
            Assert.AreEqual(continues[4], 5);
            Assert.AreEqual(continues[5], 6);
        }
    }
}
