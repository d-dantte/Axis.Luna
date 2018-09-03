using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Axis.Luna.Operation.Sync
{
    public class SyncAwaiter<Result> : IAwaiter<Result>, ICriticalNotifyCompletion
    {
        public Result OpResult { get; private set; }
        public Exception OpException { get; private set; }

        public bool IsCompleted => true;

        public SyncAwaiter(Result result)
        {
            OpResult = result;
        }
        public SyncAwaiter(Exception exception)
        {
            OpException = exception;
        }

        public Result GetResult()
        {
            if (OpException != null)
                ExceptionDispatchInfo.Capture(OpException).Throw();

            //else 
            return OpResult;
        }

        public void OnCompleted(Action continuation) => continuation.Invoke();

        public void UnsafeOnCompleted(Action continuation) => continuation.Invoke();
    }


    public class SyncAwaiter : IAwaiter, ICriticalNotifyCompletion
    {
        public Exception OpException { get; private set; }

        public bool IsCompleted => true;

        public SyncAwaiter(Exception exception)
        {
            OpException = exception ?? new Exception("General Error");
        }

        public void GetResult()
        {
            ExceptionDispatchInfo.Capture(OpException).Throw();
        }

        public void OnCompleted(Action continuation) => continuation.Invoke();

        public void UnsafeOnCompleted(Action continuation) => continuation.Invoke();
    }
}
