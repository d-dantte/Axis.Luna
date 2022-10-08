using Axis.Luna.Common;
using Axis.Luna.Extensions;
using System;
using System.Threading.Tasks;

namespace Axis.Luna.Operation
{
    /// <summary>
    /// Extension methods for <see cref="IResult<TData>"/>
    /// </summary>
    public static class ResultExtensions
    {

        public static IResult<TData> ResolveResult<TData>(this IOperation<TData> operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            return operation switch
            {
                Value.ValueOperation<TData> valueOp =>
                    valueOp.Succeeded == true
                    ? IResult<TData>.Of(valueOp.As<IResolvable<TData>>().Resolve())
                    : IResult<TData>.Of(valueOp.Error.GetException(), valueOp.Error.Data),

                Lazy.LazyOperation<TData> lazyOp =>
                    lazyOp.Succeeded == true ? IResult<TData>.Of(lazyOp.As<IResolvable<TData>>().Resolve()) :
                    lazyOp.Succeeded == false ? IResult<TData>.Of(lazyOp.Error.GetException(), lazyOp.Error.Data) :
                    lazyOp.As<IResolvable<TData>>().TryResolve(out var data, out var error) ? IResult<TData>.Of(data) :
                    IResult<TData>.Of(error.GetException(), error.Data),

                Async.AsyncOperation<TData> asyncOp =>
                    asyncOp.Succeeded == true ? IResult<TData>.Of(asyncOp.As<IResolvable<Task<TData>>>().Resolve().Result) :
                    asyncOp.Succeeded == false ? asyncOp.Error.ApplyTo(error => IResult<TData>.Of(error.GetException(), error.Data)) :
                    throw new InvalidOperationException($"the supplied async operation is not yet resolved"),

                _ => throw new ArgumentException($"unknown operation type: {operation.GetType()}")
            };
        }


        public static async Task<IResult<TData>> ResolveResultAsync<TData>(this IOperation<TData> operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            try
            {
                var result = await operation;
                return IResult<TData>.Of(result);
            }
            catch
            {
                var error = operation.Error;
                return IResult<TData>.Of(error.GetException(), error.Data);
            }
        }

    }
}
