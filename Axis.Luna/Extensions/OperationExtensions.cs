using Axis.Luna.Operation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Extensions
{
    public static class OperationExtensions
    {
        public static IOperation FoldAll(this IEnumerable<IOperation> ops, Action foldAction)
        {
            if (ops == null) return LazyOp.Fail(new NullReferenceException());
            else return LazyOp.Try(() =>
            {
                ops.ForAll(_op => _op.Resolve());

                foldAction();
            });
        }
        public static IOperation FoldAll<In>(this IEnumerable<IOperation<In>> ops, Action<IEnumerable<In>> foldAction)
        {
            if (ops == null) return LazyOp.Fail(new NullReferenceException());
            else return LazyOp.Try(() =>
            {
                ops.Select(_op => _op.Resolve())
                   .ToArray()
                   .Pipe(foldAction);
            });
        }

        public static IOperation<Out> FoldAll<Out>(this IEnumerable<IOperation> ops, Func<Out> foldAction)
        {
            if (ops == null) return LazyOp.Fail<Out>(new NullReferenceException());
            else return LazyOp.Try(() =>
            {
                ops.ForAll(_op => _op.Resolve());

                return foldAction();
            });
        }

        public static IOperation<Out> FoldAll<In, Out>(this IEnumerable<IOperation<In>> ops, Func<IEnumerable<In>, Out> foldAction)
        {
            if (ops == null) return LazyOp.Fail<Out>(new NullReferenceException());
            else return LazyOp.Try(() =>
            {
                return ops
                    .Select(_op => _op.Resolve())
                    .ToArray()
                    .Pipe(foldAction);
            });
        }
    }
}
