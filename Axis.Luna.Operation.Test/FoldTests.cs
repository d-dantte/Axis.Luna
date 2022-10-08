using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test
{
    [TestClass]
    public class FoldTests
    {
        [TestMethod]
        public async Task OprationFold_WithPassingOperations_MustRunAllOperations()
        {
            var completeCount = 0;
            var ops = new[]
            {
                Operation.Try(() => { Interlocked.Increment(ref completeCount); }),
                Operation.Try(() => { Interlocked.Increment(ref completeCount); }),
                Operation.Try(Task.Run(() => { Interlocked.Increment(ref completeCount); })),
                Operation.Try(Task.Run(() =>
                {
                    Thread.Sleep(50);
                    Interlocked.Increment(ref completeCount);
                }))
            };

            await ops.Fold(FoldBias.Pass);

            Assert.AreEqual(ops.Length, completeCount);
        }

        [TestMethod]
        public async Task OprationFold_WithFailingOperations_MustAggregateAllExceptions()
        {
            var ops = new[]
            {
                Operation.Fail(new Exception("1")),
                Operation.Fail(new Exception("2")),
                Operation.Try(Task.FromException(new Exception("3"))),
                Operation.Try(Task.Run(() =>
                {
                    Thread.Sleep(50);
                    throw new Exception("4");
                }))
            };

            var folded = ops.Fold(FoldBias.Fail);
            try
            {
                await folded;
            }
            catch { }

            var exception = folded.Error.GetException();
            Assert.IsInstanceOfType(exception, typeof(AggregateException));
            Assert.AreEqual(ops.Length, (exception as AggregateException).InnerExceptions.Count);
        }

        [TestMethod]
        public async Task OprationFold_WithMixedOperations_MustObeyBias()
        {
            var ops = new[]
            {
                Operation.Fail(new Exception("1")),
                Operation.Fail(new Exception("2")),
                Operation.FromVoid()
            };

            var folded = ops.Fold(FoldBias.Fail);
            try
            {
                await folded;
            }
            catch { }

            var exception = folded.Error?.GetException() as AggregateException;
            Assert.AreEqual(false, folded.Succeeded);
            Assert.IsNotNull(exception);
            Assert.AreEqual(2, exception.InnerExceptions.Count);


            folded = ops.Fold(FoldBias.Pass);
            try
            {
                await folded;
            }
            catch { }

            exception = folded.Error?.GetException() as AggregateException;
            Assert.AreEqual(true, folded.Succeeded);
            Assert.IsNull(exception);
        }

        [TestMethod]
        public async Task OprationTFold_WithPassingOperations_MustRunAllOperations()
        {
            var completeCount = 0;
            var ops = new[]
            {
                Operation.Try(() => Interlocked.Increment(ref completeCount)),
                Operation.Try(() => Interlocked.Increment(ref completeCount)),
                Operation.Try(Task.Run(() => Interlocked.Increment(ref completeCount))),
                Operation.Try(Task.Run(() =>
                {
                    Thread.Sleep(50);
                    return Interlocked.Increment(ref completeCount);
                }))
            };

            var results = await ops.Fold(FoldBias.Pass);

            Assert.AreEqual(ops.Length, completeCount);
        }

        [TestMethod]
        public async Task OprationTFold_WithFailingOperations_MustAggregateAllExceptions()
        {
            var ops = new[]
            {
                Operation.Fail<int>(new Exception("1")),
                Operation.Fail<int>(new Exception("2")),
                Operation.Try(Task.FromException<int>(new Exception("3")))
            };

            var folded = ops.Fold(FoldBias.Fail);
            try
            {
                await folded;
            }
            catch { }

            var exception = folded.Error.GetException();
            Assert.IsInstanceOfType(exception, typeof(AggregateException));
            Assert.AreEqual(ops.Length, (exception as AggregateException).InnerExceptions.Count);
        }

        [TestMethod]
        public async Task OprationTFold_WithMixedOperations_MustObeyBias()
        {
            var ops = new[]
            {
                Operation.Fail<int>(new Exception("1")),
                Operation.Fail<int>(new Exception("2")),
                Operation.FromResult(3)
            };

            var folded = ops.Fold(FoldBias.Fail);
            try
            {
                await folded;
            }
            catch { }

            var exception = folded.Error?.GetException() as AggregateException;
            Assert.AreEqual(false, folded.Succeeded);
            Assert.IsNotNull(exception);
            Assert.AreEqual(2, exception.InnerExceptions.Count);


            folded = ops.Fold(FoldBias.Pass);
            try
            {
                await folded;
            }
            catch { }

            exception = folded.Error?.GetException() as AggregateException;
            Assert.AreEqual(true, folded.Succeeded);
            Assert.IsNull(exception);
        }
    }
}
