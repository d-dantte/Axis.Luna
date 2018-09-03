using Axis.Luna.Operation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Axis.Luna.Extensions
{
    [DebuggerStepThrough]
    public static class OperationExtensions
    {
        public static IOperation Chain(this IEnumerable<IOperation> ops)
        {
            if (ops == null) return LazyOp.Fail(new NullReferenceException());

            var op = ops.FirstOrDefault() ?? ResolvedOp.Try(() => { });

            foreach(var _op in ops.Skip(1)) op = op.Then(() => _op);

            return op;
        }

        public static IOperation Chain(this IEnumerable<Func<IOperation>> opProducers)
        {
            if (opProducers == null) return LazyOp.Fail(new NullReferenceException());

            var op = opProducers.FirstOrDefault()?.Invoke() ?? ResolvedOp.Try(() => { });

            foreach (var _op in opProducers.Skip(1)) op = op.Then(_op);

            return op;
        }


        public static IOperation<Out> Fold<In, Out>(this IEnumerable<IOperation<In>> ops, Func<In, Out, Out> foldOperation)
        => LazyOp.Try(() =>
        {
            if (ops == null) throw new NullReferenceException();

            var op = LazyOp.Try(() => default(Out)) as IOperation<Out>;
            foreach (var _op in ops) op = op.Then(_ => foldOperation.Invoke(_op.Resolve(), _));

            return op.Resolve();
        });

        public static IOperation<Out> Fold<In, Out>(this IEnumerable<Func<IOperation<In>>> opProducers, Func<In, Out, Out> foldOperation)
        => LazyOp.Try(() =>
        {
            if (opProducers == null) throw new NullReferenceException();

            var op = LazyOp.Try(() => default(Out)) as IOperation<Out>;
            foreach (var _op in opProducers) op = op.Then(_ => foldOperation.Invoke(_op.Invoke().Resolve(), _));

            return op.Resolve();
        });

        public static void ResolveSafely(this IOperation op)
        {
            try
            {
                op.Resolve();
            }
            catch { }
        }

        public static void ResolveSafely<R>(this IOperation<R> op)
        {
            try
            {
                op.Resolve();
            }
            catch { }
        }
    }
}
