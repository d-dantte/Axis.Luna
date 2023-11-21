using Axis.Luna.Extensions;
using System;

namespace Axis.Luna.Common.Results
{
    /// <summary>
    /// A monadic tagged-union type encapsulating the binary states of any function's execution: a value, or an error.
    /// </summary>
    /// <typeparam name="TData">the encapsulated data type</typeparam>
    public interface IResult<TData>
    {
        #region Members

        IResult<TOut> Map<TOut>(Func<TData, TOut> mapper);

        IResult<TOut> Bind<TOut>(Func<TData, IResult<TOut>> binder);

        void Consume(Action<TData> consumer);

        #endregion
    }
}
