using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Axis.Luna.Operation.Test")]

namespace Axis.Luna.Operation
{
    /// <summary>
    /// Static helper class for IOperation
    /// </summary>
    public static class Operation
    {
        #region Value
        /// <summary>
        /// Returns an already resolved operation with a value ready for returning
        /// </summary>
        /// <typeparam name="Result">type of the result</typeparam>
        /// <param name="result">result value</param>
        /// <returns>Operation of result</returns>
        public static IOperation<Result> FromResult<Result>(Result result) => new Value.ValueOperation<Result>(result);

        /// <summary>
        /// Creates a no-op operation that may or may not have been resolved, but will be successful when resolved.
        /// </summary>
        /// <returns>A no-op operation</returns>
        public static IOperation FromVoid() => new Lazy.LazyOperation(() => { }); //<-- is there a better way to do this?
        #endregion

        #region Try
        public static IOperation<Result> Try<Result>(this Func<Result> func)
        {
            if(func == null)
                throw new ArgumentNullException(nameof(func));

            return new Lazy.LazyOperation<Result>(func);
        }
        public static IOperation Try(this Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return new Lazy.LazyOperation(action);
        }


        public static IOperation<Result> Try<Result>(this Func<Task<Result>> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return new Async.AsyncOperation<Result>(func);
        }
        public static IOperation Try(this Func<Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return new Async.AsyncOperation(action);
        }

        public static IOperation<Result> Try<Result>(this Task<Result> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            return new Async.AsyncOperation<Result>(task);
        }
        public static IOperation Try(this Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            return new Async.AsyncOperation(task);
        }


        public static IOperation<Result> Try<Result>(this Func<IOperation<Result>> operationProvider)
        {
            if (operationProvider == null)
                throw new ArgumentNullException(nameof(operationProvider));

            try
            {
                return operationProvider.Invoke() ?? throw new InvalidOperationException("Cannot return a null operation");
            }
            catch (Exception e)
            {
                return Fail<Result>(e);
            }
        }
        public static IOperation Try(this Func<IOperation> operationProvider)
        {
            if (operationProvider == null)
                throw new ArgumentNullException(nameof(operationProvider));

            try
            {
                return operationProvider.Invoke() ?? throw new InvalidOperationException("Cannot return a null operation");
            }
            catch (Exception e)
            {
                return Fail(e);
            }
        }
        #endregion

        #region Fail
        public static IOperation Fail(Exception exception = null)
            => new Value.FaultedOperation(
                new OperationError(
                    code: "GeneralError",
                    message: exception?.Message,
                    exception: exception));

        public static IOperation<Result> Fail<Result>(Exception exception = null)
            => new Value.ValueOperation<Result>(
                new OperationError(
                    code: "GeneralError",
                    message: exception?.Message,
                    exception: exception));


        public static IOperation Fail(string message) => Fail(new Exception(message));

        public static IOperation<Result> Fail<Result>(string message) => Fail<Result>(new Exception(message));


        public static IOperation Fail(OperationError error)
            => new Value.FaultedOperation(error ?? throw new ArgumentNullException(nameof(error)));

        public static IOperation<Result> Fail<Result>(OperationError error)
            => new Value.ValueOperation<Result>(error ?? throw new ArgumentNullException(nameof(error)));
        #endregion

        #region Folding
        /// <summary>
        /// Folds all given operations into a single operation, aggregating any errors encountered, and exiting depending on the <see cref="FoldBias"/>.
        /// The fold operation ensures that all individual operations are given a chance to execute.
        /// </summary>
        /// <param name="operations">A list of operations to fold</param>
        /// <param name="bias">The bias to apply to the folded operations</param>
        public static IOperation Fold(this
            IEnumerable<IOperation> operations,
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
                        catch (Exception e)
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
        public static IOperation<TResult[]> Fold<TResult>(this
            IEnumerable<IOperation<TResult>> operations,
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
        public static IOperation<TOut[]> FoldWith<TIn, TOut>(this
            IEnumerable<IOperation<TIn>> operations,
            Func<TIn, TOut> mapper,
            FoldBias bias = FoldBias.Fail)
        {
            if (mapper == null)
                return Operation.Fail<TOut[]>(new ArgumentNullException(nameof(mapper)));

            return operations
                .Select(op => op?.Then(mapper))
                .Fold(bias);
        }

        #endregion

        #region Failure mappers
        public static IOperation<TResult> MapError<TResult>(this
            IOperation<TResult> operation,
            Func<OperationError, TResult> failureHandler)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (failureHandler == null)
                throw new ArgumentNullException(nameof(failureHandler));

            if (operation.Succeeded == true)
                return operation;

            if (operation.Succeeded == false)
                return Operation.Try(() => failureHandler.Invoke(operation.Error));

            return operation switch
            {
                Lazy.LazyOperation<TResult> lazyop => Operation.Try(() =>
                {
                    if (lazyop.As<IResolvable<TResult>>().TryResolve(out var result, out var error))
                        return result;

                    else return failureHandler.Invoke(error);
                }),

                Async.AsyncOperation<TResult> asyncop => Operation.Try(async () =>
                {
                    try
                    {
                        return await asyncop;
                    }
                    catch
                    {
                        return failureHandler.Invoke(asyncop.Error);
                    }
                }),

                _ => throw new ArgumentException($"Invalid operaton type: Async or Lazy expected, {operation.GetType()} found.")
            };
        }

        public static IOperation<TResult> MapError<TResult>(this
            IOperation<TResult> operation,
            Func<OperationError, Task<TResult>> failureHandler)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (failureHandler == null)
                throw new ArgumentNullException(nameof(failureHandler));

            if (operation.Succeeded == true)
                return operation;

            if (operation.Succeeded == false)
                return Operation.Try(() => failureHandler.Invoke(operation.Error));

            return operation switch
            {
                Lazy.LazyOperation<TResult> lazyop => Operation.Try(async () =>
                {
                    if (lazyop.As<IResolvable<TResult>>().TryResolve(out var result, out var error))
                        return result;

                    else return await failureHandler.Invoke(error);
                }),

                Async.AsyncOperation<TResult> asyncop => Operation.Try(async () =>
                {
                    try
                    {
                        return await asyncop;
                    }
                    catch
                    {
                        return await failureHandler.Invoke(asyncop.Error);
                    }
                }),

                _ => throw new ArgumentException($"Invalid operaton type: Async or Lazy expected, {operation.GetType()} found.")
            };
        }

        public static IOperation<TResult> MapError<TResult>(this
            IOperation<TResult> operation,
            Func<OperationError, IOperation<TResult>> failureHandler)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (failureHandler == null)
                throw new ArgumentNullException(nameof(failureHandler));

            if (operation.Succeeded == true)
                return operation;

            if (operation.Succeeded == false)
                return Operation.Try(() => failureHandler.Invoke(operation.Error));

            return operation switch
            {
                Lazy.LazyOperation<TResult> lazyop => Operation.Try(async () =>
                {
                    if (lazyop.As<IResolvable<TResult>>().TryResolve(out var result, out var error))
                        return result;

                    else return await failureHandler.Invoke(error);
                }),

                Async.AsyncOperation<TResult> asyncop => Operation.Try(async () =>
                {
                    try
                    {
                        return await asyncop;
                    }
                    catch
                    {
                        return await failureHandler.Invoke(asyncop.Error);
                    }
                }),

                _ => throw new ArgumentException($"Invalid operaton type: Async or Lazy expected, {operation.GetType()} found.")
            };
        }

        #endregion
    }
}
