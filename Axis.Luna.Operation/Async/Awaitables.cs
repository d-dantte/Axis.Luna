using System;
using System.Runtime.CompilerServices;

namespace Axis.Luna.Operation.Async
{
    public class AsyncAwaiter : IAwaiter, ICriticalNotifyCompletion
    {

        public TaskAwaiter TaskAwaiter { get; private set; }

        public bool IsCompleted => TaskAwaiter.IsCompleted;

        public AsyncAwaiter(TaskAwaiter awaiter)
        {
            TaskAwaiter = awaiter;
        }

        public void GetResult() => TaskAwaiter.GetResult();

        public void OnCompleted(Action continuation) => TaskAwaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => TaskAwaiter.UnsafeOnCompleted(continuation);
    }

    public class AsyncAwaiter<Result> : IAwaiter<Result>, ICriticalNotifyCompletion
    {

        public TaskAwaiter<Result> TaskAwaiter { get; private set; }

        public bool IsCompleted => TaskAwaiter.IsCompleted;


        public AsyncAwaiter(TaskAwaiter<Result> awaiter)
        {
            TaskAwaiter = awaiter;
        }

        public Result GetResult() => TaskAwaiter.GetResult();

        public void OnCompleted(Action continuation) => TaskAwaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => TaskAwaiter.UnsafeOnCompleted(continuation);
    }
}
