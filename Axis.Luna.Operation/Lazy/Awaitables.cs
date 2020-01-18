using System;

namespace Axis.Luna.Operation.Lazy
{

    public struct LazyAwaiter : IAwaiter
    {
        private readonly Lazy<object> _lazy;

        public LazyAwaiter(Lazy<object> lazy)
        {
            _lazy = lazy;
        }

        public bool IsCompleted => true;

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
        private readonly Lazy<Result> _lazy;

        public LazyAwaiter(Lazy<Result> lazy)
        {
            _lazy = lazy;
        }

        public bool IsCompleted => true;

        public Result GetResult()
        {
            return _lazy.Value;
        }

        public void OnCompleted(Action continuation)
        {
            //resolve
            GetResult();
            continuation.Invoke();
        }
    }
}
