using System;

namespace Axis.Luna.Operation.Lazy
{

    public struct LazyAwaiter : IAwaiter
    {
        private Lazy<object> _lazy;
        private bool _completed;

        public LazyAwaiter(Lazy<object> lazy)
        {
            _lazy = lazy;
            _completed = false;
        }

        public bool IsCompleted => _completed;

        public void GetResult()
        {
            try
            {
                var unused = _lazy.Value;
            }
            finally
            {
                _completed = true;
            }
        }

        public void OnCompleted(Action continuation)
        {

            if (!IsCompleted)
            {
                try
                {
                    //resolve
                    GetResult();
                }
                catch { }
            }

            if (IsCompleted) continuation.Invoke();
        }
    }


    public struct LazyAwaiter<Result> : IAwaiter<Result>
    {
        private Lazy<Result> _lazy;
        private bool _completed;

        public LazyAwaiter(Lazy<Result> lazy)
        {
            _lazy = lazy;
            _completed = false;
        }

        public bool IsCompleted => _completed;

        public Result GetResult()
        {
            try
            {
                return _lazy.Value;
            }
            finally
            {
                _completed = true;
            }
        }

        public void OnCompleted(Action continuation)
        {
            if (!IsCompleted)
            {
                try
                {
                    //resolve
                    GetResult();
                }
                catch { }
            }

            if (IsCompleted) continuation.Invoke();
        }
    }
}
