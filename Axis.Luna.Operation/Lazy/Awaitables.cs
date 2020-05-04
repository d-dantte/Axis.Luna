using System;

namespace Axis.Luna.Operation.Lazy
{

    public struct LazyAwaiter : IAwaiter
    {
        private readonly CustomLazy<object> _lazy;

        public LazyAwaiter(CustomLazy<object> lazy)
        {
            _lazy = lazy;
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
            _ = _lazy.Value;
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

        public LazyAwaiter(CustomLazy<Result> lazy)
        {
            _lazy = lazy;
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

        public Result GetResult() => _lazy.Value;

        public void OnCompleted(Action continuation)
        {
            //resolve
            _ = GetResult();
            continuation.Invoke();
        }
    }
}
