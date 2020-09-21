using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static System.Runtime.CompilerServices.ConfiguredTaskAwaitable;

namespace Axis.Luna.Operation.Async
{
    public struct AsyncAwaiter : IAwaiter, ICriticalNotifyCompletion
    {
        private readonly Action<Exception> _errorSetter;

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

        public AsyncAwaiter(Task task, Action<Exception> errorSetter)
        {
            Task = task;
            TaskAwaitable = task.ConfigureAwait(false);
            _errorSetter = errorSetter;
        }

        public void GetResult() => TaskAwaiter.GetResult();

        public void OnCompleted(Action continuation)
        {
            var _this = this;
            TaskAwaiter.OnCompleted(() =>
            {
                if (_this.IsSuccessful == false)
                    _this._errorSetter.Invoke(_this.Task.Exception.InnerException);

                continuation.Invoke();
            });
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            var _this = this;
            TaskAwaiter.OnCompleted(() =>
            {
                if (_this.IsSuccessful == false)
                    _this._errorSetter.Invoke(_this.Task.Exception.InnerException);

                continuation.Invoke();
            });
        }
    }


    public struct AsyncAwaiter<Result> : IAwaiter<Result>, ICriticalNotifyCompletion
    {
        private readonly Action<Exception> _errorSetter; 

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


        public AsyncAwaiter(Task<Result> task, Action<Exception> errorSetter)
        {
            Task = task;
            TaskAwaitable = Task.ConfigureAwait(false);
            _errorSetter = errorSetter;
        }

        public Result GetResult() => TaskAwaiter.GetResult();

        public void OnCompleted(Action continuation)
        {
            var _this = this;
            TaskAwaiter.OnCompleted(() =>
            {
                if (_this.IsSuccessful == false)
                    _this._errorSetter.Invoke(_this.Task.Exception.InnerException);

                continuation.Invoke();
            });
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            var _this = this;
            TaskAwaiter.OnCompleted(() =>
            {
                if (_this.IsSuccessful == false)
                    _this._errorSetter.Invoke(_this.Task.Exception.InnerException);

                continuation.Invoke();
            });
        }
    }
}
