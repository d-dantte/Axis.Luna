using Axis.Luna.Operation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Axis.Luna.Test.Operation
{
    [TestClass]
    public class ResolvedOperationTests
    {

        [TestMethod]
        public void ImmediateResolutionTest()
        {
            var resolved = false;
            var op = ResolvedOp.Try(() =>
            {
                resolved = true;
            });
            Assert.IsTrue(resolved);

            resolved = false;
            var opb = ResolvedOp.Try(() => resolved = true);
            Assert.IsTrue(resolved);
            Assert.IsTrue(opb.Result);
        }

        [TestMethod]
        public void OperationStatusTest()
        {
            var op = ResolvedOp.Try(() => { });
            var ops = ResolvedOp.Try(() => "done");
            var _op = ResolvedOp.Try(() => { throw new Exception(); });
            var _ops = ResolvedOp.Try(() =>
            {
                var @true = true;
                if (@true) throw new Exception();
                else return "done";
            });

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
