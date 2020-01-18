using System;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.ConfiguredTaskAwaitable;

namespace Axis.Luna.Operation.Async
{
    public class AsyncAwaiter : IAwaiter, ICriticalNotifyCompletion
    {
        public ConfiguredTaskAwaiter TaskAwaiter => TaskAwaitable.GetAwaiter();

        public ConfiguredTaskAwaitable TaskAwaitable { get; }

        public bool IsCompleted => TaskAwaiter.IsCompleted;

        public AsyncAwaiter(ConfiguredTaskAwaitable awaitable)
        {
            TaskAwaitable = awaitable;
        }

        public void GetResult() => TaskAwaiter.GetResult();

        public void OnCompleted(Action continuation) => TaskAwaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => TaskAwaiter.UnsafeOnCompleted(continuation);
    }

    public class AsyncAwaiter<Result> : IAwaiter<Result>, ICriticalNotifyCompletion
    {
        public ConfiguredTaskAwaitable<Result>.ConfiguredTaskAwaiter TaskAwaiter => TaskAwaitable.GetAwaiter();
        public ConfiguredTaskAwaitable<Result> TaskAwaitable { get; }

        public bool IsCompleted => TaskAwaiter.IsCompleted;


        public AsyncAwaiter(ConfiguredTaskAwaitable<Result> awaitable)
        {
            TaskAwaitable = awaitable;
        }

        public Result GetResult() => TaskAwaiter.GetResult();

        public void OnCompleted(Action continuation) => TaskAwaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => TaskAwaiter.UnsafeOnCompleted(continuation);
    }
}
