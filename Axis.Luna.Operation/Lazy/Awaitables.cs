using System;

namespace Axis.Luna.Operation.Lazy
{

    public class LazyAwaiter : IAwaiter
    {
        private readonly Lazy<object> _lazy;

        public LazyAwaiter(Lazy<object> lazy)
        {
            _lazy = lazy;
            IsSuccessful = null;
        }

        /// <summary>
        /// Always true so that awaiting on this awaiter will always run synchroniously
        /// </summary>
        public bool IsCompleted => true;

        public bool? IsSuccessful { get; private set; }

        public void GetResult()
        {
            try
            {
                _ = _lazy.Value;
                IsSuccessful = true;
            }
            catch
            {
                IsSuccessful = false;
                throw;
            }
        }

        public void OnCompleted(Action continuation)
        {
            GetResult();
            continuation.Invoke();
        }
    }


    public class LazyAwaiter<Result> : IAwaiter<Result>
    {
        private readonly Lazy<Result> _lazy;

        public LazyAwaiter(Lazy<Result> lazy)
        {
            _lazy = lazy;
            IsSuccessful = null;
        }

        public bool? IsSuccessful { get; private set; }

        /// <summary>
        /// Always true so that awaiting on this awaiter will always run synchroniously
        /// </summary>
        public bool IsCompleted => true;

        public Result GetResult()
        {
            try
            {
                Result r =  _lazy.Value;
                IsSuccessful = true;

                return r;
            }
            catch
            {
                IsSuccessful = false;
                throw;
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
