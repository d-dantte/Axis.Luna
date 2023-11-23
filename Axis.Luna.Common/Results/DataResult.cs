using System;
using Axis.Luna.Extensions;

namespace Axis.Luna.Common.Results
{
    /// <summary>
    /// Should this be as struct?
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    internal class DataResult<TData> : IResult<TData>
    {
        private readonly TData _data;

        internal TData Data => _data;

        internal DataResult(TData data)
        {
            _data = data;
        }

        public IResult<TOut> Bind<TOut>(Func<TData, IResult<TOut>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder);

            try
            {
                return binder.Invoke(_data);
            }
            catch(Exception e)
            {
                return new ErrorResult<TOut>(e);
            }
        }

        public void Consume(Action<TData> consumer)
        {
            ArgumentNullException.ThrowIfNull(consumer);

            consumer.Invoke(_data);
        }

        public IResult<TOut> Map<TOut>(Func<TData, TOut> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            try
            {
                return new DataResult<TOut>(mapper.Invoke(_data));
            }
            catch(Exception e)
            {
                return new ErrorResult<TOut>(e);
            }
        }

        public IResult<TOut> MapAs<TOut>() => Result.Of(_data.As<TOut>);
    }
}
