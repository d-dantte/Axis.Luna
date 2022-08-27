using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axis.Luna.Operation
{
    public static class OperationExtensions
    {
        /// <summary>
        /// Folds all given operations into a single operation, aggregating any errors encountered, and exiting depending on the <see cref="FoldBias"/>.
        /// The fold operation ensures that all individual operations are given a chance to execute.
        /// </summary>
        /// <param name="operations">A list of operations to fold</param>
        /// <param name="bias">The bias to apply to the folded operations</param>
        public static Operation Fold(this
            IEnumerable<Operation> operations,
            FoldBias bias = FoldBias.Fail)
            => Operation.Try(async () =>
            {
                if (operations == null)
                    throw new ArgumentException(nameof(operations));

                var oparray = operations.ToArray();
                if (oparray.Length == 0)
                    return;

                var exceptions = new List<Exception>();
                await oparray
                    .Select(async op =>
                    {
                        try
                        {
                            await op;
                        }
                        catch(Exception e)
                        {
                            exceptions.Add(e);
                        }
                    })
                    .ApplyTo(Task.WhenAll);

                if (bias == FoldBias.Fail && exceptions.Count > 0)
                    throw new AggregateException(exceptions);

                else if (bias == FoldBias.Pass && oparray.Length == exceptions.Count)
                    throw new AggregateException(exceptions);
            });

        /// <summary>
        /// Folds all given operations into a single operation, aggregating any errors encountered, and exiting depending on the <see cref="FoldBias"/>.
        /// The fold operation ensures that all individual operations are given a chance to execute.
        /// </summary>
        /// <param name="operations">A list of operations to fold</param>
        /// <param name="bias">The bias to apply to the folded operations</param>
        public static Operation<TResult[]> Fold<TResult>(this
            IEnumerable<Operation<TResult>> operations,
            FoldBias bias = FoldBias.Fail)
            => Operation.Try(async () =>
            {
                if (operations == null)
                    throw new ArgumentException(nameof(operations));

                var oparray = operations.ToArray();
                if (oparray.Length == 0)
                    return Array.Empty<TResult>();

                var exceptions = new List<Exception>();
                var results = await oparray
                    .Where(op => op != null)
                    .Select(async op =>
                    {
                        try
                        {
                            return (await op, (Exception)null);
                        }
                        catch (Exception e)
                        {
                            exceptions.Add(e);
                            return (default, e);
                        }
                    })
                    .ApplyTo(Task.WhenAll);

                if (bias == FoldBias.Fail && exceptions.Count > 0)
                    throw new AggregateException(exceptions);

                else if (bias == FoldBias.Pass && oparray.Length == exceptions.Count)
                    throw new AggregateException(exceptions);

                else return results
                    .Where(result => result.Item2 == null)
                    .Select(result => result.Item1)
                    .ToArray();
            });

        /// <summary>
        /// Folds all given operations into a single operation, aggregating any errors encountered, and exiting depending on the <see cref="FoldBias"/>.
        /// The fold operation ensures that all individual operations are given a chance to execute.
        /// </summary>
        /// <param name="operations">A list of operations to fold</param>
        /// <param name="mapper">A mapping fundtion to be applied during the fold operation</param>
        /// <param name="bias">The bias to apply to the folded operations</param>
        public static Operation<TOut[]> FoldWith<TIn, TOut>(this
            IEnumerable<Operation<TIn>> operations,
            Func<TIn, TOut> mapper,
            FoldBias bias = FoldBias.Fail)
        {
            if (mapper == null)
                return Operation.Fail<TOut[]>(new ArgumentNullException(nameof(mapper)));

            return operations
                .Select(op => op?.Then(mapper))
                .Fold(bias);
        }
    }
}
