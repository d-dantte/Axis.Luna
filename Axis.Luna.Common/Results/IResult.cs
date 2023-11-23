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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="mapper"></param>
        /// <returns></returns>
        IResult<TOut> Map<TOut>(Func<TData, TOut> mapper);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <returns></returns>
        IResult<TOut> MapAs<TOut>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="binder"></param>
        /// <returns></returns>
        IResult<TOut> Bind<TOut>(Func<TData, IResult<TOut>> binder);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumer"></param>
        void Consume(Action<TData> consumer);

        #endregion
    }
}
