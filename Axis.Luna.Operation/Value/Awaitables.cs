using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Axis.Luna.Operation.Value
{
    public struct ValueAwaiter : IAwaiter, ICriticalNotifyCompletion
    {
        public Exception Exception { get; private set; }

        public bool IsCompleted => true;

        public bool? IsSuccessful => false;


        public ValueAwaiter(Exception exception)
        {
            Exception = exception ?? new Exception("General Error");
        }

        public void GetResult() => ExceptionDispatchInfo.Capture(Exception).Throw();

        public void OnCompleted(Action continuation) => continuation.Invoke();

        public void UnsafeOnCompleted(Action continuation) => continuation.Invoke();
    }


    public struct ValueAwaiter<TResult> : IAwaiter<TResult>, ICriticalNotifyCompletion
    {
        public TResult Result { get; private set; }

        public Exception Exception { get; private set; }

        public bool? IsSuccessful { get; private set; }

        public bool IsCompleted => true;


        public ValueAwaiter(TResult result)
        {
            Exception = null;
            this.Result = result;
            IsSuccessful = true;
        }
        public ValueAwaiter(Exception exception)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            this.Result = default;
            IsSuccessful = false;
        }

        public TResult GetResult()
        {
            if (Exception != null)
                ExceptionDispatchInfo.Capture(Exception).Throw();

            //else 
            return this.Result;
        }

        public void OnCompleted(Action continuation) => continuation.Invoke();

        public void UnsafeOnCompleted(Action continuation) => continuation.Invoke();
    }
}
