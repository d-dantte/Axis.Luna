using System;

namespace Axis.Luna.Common.Results
{
    internal readonly struct ErrorResult<TData> :
        IResult<TData>,
        IDefaultValueProvider<ErrorResult<TData>>
    {
        private readonly Exception _error;

        internal Exception Error => _error;

        #region IDefaultValueProvider
        public bool IsDefault => _error is null;

        public static ErrorResult<TData> Default => default;
        #endregion

        internal ErrorResult(Exception error)
        {
            ArgumentNullException.ThrowIfNull(error);

            _error = error;
        }

        public IResult<TOut> Bind<TOut>(Func<TData, IResult<TOut>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder);

            return new ErrorResult<TOut>(_error);
        }

        public void Consume(Action<TData> consumer)
        {
            ArgumentNullException.ThrowIfNull(consumer);
        }

        public IResult<TOut> Map<TOut>(Func<TData, TOut> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);

            return new ErrorResult<TOut>(_error);
        }

        public IResult<TOut> MapAs<TOut>() => new ErrorResult<TOut>(_error);
    }
}
