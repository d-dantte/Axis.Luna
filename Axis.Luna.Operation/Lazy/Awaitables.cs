using System;

namespace Axis.Luna.Operation.Lazy
{

    public struct LazyAwaiter : IAwaiter
    {
        private readonly CustomLazy<object> _lazy;
        private readonly Action<Exception> _errorSetter;

        public LazyAwaiter(CustomLazy<object> lazy, Action<Exception> errorSetter)
        {
            _lazy = lazy;
            _errorSetter = errorSetter;
        }

        /// <summary>
        /// Always true so that awaiting on this awaiter will always run synchroniously
        /// </summary>
        public bool IsCompleted => true;

        public bool? IsSuccessful
        {
            get
            {
                switch(_lazy.State)
                {
                    case CustomLazyState.Faulted: return false;
                    case CustomLazyState.Initialized: return true;
                    case CustomLazyState.Uninitialized:
                    default: return null;
                }
            }
        }

        public void GetResult()
        {
            switch(_lazy.State)
            {
                case CustomLazyState.Faulted:
                case CustomLazyState.Initialized:
                    _ = _lazy.Value;
                    break;

                case CustomLazyState.Uninitialized:
                    try
                    {
                        _ = _lazy.Value;
                    }
                    catch(Exception e)
                    {
                        _errorSetter.Invoke(e);
                        throw;
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Invalid lazy state: {_lazy.State}");
            }
        }

        public void OnCompleted(Action continuation)
        {
            GetResult();
            continuation.Invoke();
        }
    }


    public struct LazyAwaiter<Result> : IAwaiter<Result>
    {
        private readonly CustomLazy<Result> _lazy;
        private readonly Action<Exception> _errorSetter;

        public LazyAwaiter(CustomLazy<Result> lazy, Action<Exception> errorSetter)
        {
            _lazy = lazy;
            _errorSetter = errorSetter;
        }

        public bool? IsSuccessful
        {
            get
            {
                switch (_lazy.State)
                {
                    case CustomLazyState.Faulted: return false;
                    case CustomLazyState.Initialized: return true;
                    case CustomLazyState.Uninitialized:
                    default: return null;
                }
            }
        }

        /// <summary>
        /// Always true so that awaiting on this awaiter will always run synchroniously
        /// </summary>
        public bool IsCompleted => true;

        public Result GetResult()
        {
            switch (_lazy.State)
            {
                case CustomLazyState.Faulted:
                case CustomLazyState.Initialized:
                    return _lazy.Value;

                case CustomLazyState.Uninitialized:
                    try
                    {
                        return _lazy.Value;
                    }
                    catch (Exception e)
                    {
                        _errorSetter.Invoke(e);
                        throw;
                    }

                default:
                    throw new InvalidOperationException($"Invalid lazy state: {_lazy.State}");
            }
        }

        public void OnCompleted(Action continuation)
        {
            //resolve
            _ = GetResult();
            continuation.Invoke();
        }
    }
}
