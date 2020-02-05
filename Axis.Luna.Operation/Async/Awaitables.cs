using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static System.Runtime.CompilerServices.ConfiguredTaskAwaitable;

namespace Axis.Luna.Operation.Async
{
    public class AsyncAwaiter : IAwaiter, ICriticalNotifyCompletion
    {
        public ConfiguredTaskAwaiter TaskAwaiter => TaskAwaitable.GetAwaiter();

        public ConfiguredTaskAwaitable TaskAwaitable { get; }

        public Task Task { get; }

        public bool IsCompleted => TaskAwaiter.IsCompleted;

        public bool? IsSuccessful
        {
            get
            {
                switch(Task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        return true;

                    case TaskStatus.Canceled:
                    case TaskStatus.Faulted:
                        return false;

                    default:
                        return null;
                }
            }
        }

        public AsyncAwaiter(Task task)
        {
            Task = task;
            TaskAwaitable = task.ConfigureAwait(false);
        }

        public void GetResult() => TaskAwaiter.GetResult();

        public void OnCompleted(Action continuation) => TaskAwaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => TaskAwaiter.UnsafeOnCompleted(continuation);
    }

    public class AsyncAwaiter<Result> : IAwaiter<Result>, ICriticalNotifyCompletion
    {
        public ConfiguredTaskAwaitable<Result>.ConfiguredTaskAwaiter TaskAwaiter => TaskAwaitable.GetAwaiter();

        public ConfiguredTaskAwaitable<Result> TaskAwaitable { get; }

        public Task<Result> Task { get; }

        public bool IsCompleted => TaskAwaiter.IsCompleted;

        public bool? IsSuccessful
        {
            get
            {
                switch (Task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        return true;

                    case TaskStatus.Canceled:
                    case TaskStatus.Faulted:
                        return false;

                    default:
                        return null;
                }
            }
        }


        public AsyncAwaiter(Task<Result> task)
        {
            Task = task;
            TaskAwaitable = Task.ConfigureAwait(false);
        }

        public Result GetResult() => TaskAwaiter.GetResult();

        public void OnCompleted(Action continuation) => TaskAwaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => TaskAwaiter.UnsafeOnCompleted(continuation);
    }
}
