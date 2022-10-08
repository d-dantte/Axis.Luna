using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Axis.Luna.Operation.Lazy
{

    public struct LazyAwaiter : IAwaiter, ICriticalNotifyCompletion
    {
        private readonly List<Action> _continuations;
        private readonly CustomLazy<object> _lazy;

        public LazyAwaiter(CustomLazy<object> lazy, Action<Exception> errorSetter)
        {
            _continuations = new List<Action>();
            _lazy = lazy;

            OnCompleted(() =>
            {
                if (lazy.State == CustomLazyState.Faulted)
                    errorSetter.Invoke(
                        lazy.Exception
                        ?? new Exception($"Lazy status is '{lazy.State}', yet no exception found that caused the fault."));
            });
        }


        /// <summary>
        /// Always true so that awaiting on this awaiter will always run synchroniously
        /// </summary>
        public bool IsCompleted => true;

        public bool? IsSuccessful
        {
            get
            {
                return _lazy.State switch
                {
                    CustomLazyState.Faulted => false,
                    CustomLazyState.Initialized => true,
                    _ => null,
                };
            }
        }

        public void GetResult()
        {
            try
            {
                _ = _lazy.Value;
            }
            finally
            {
                NotifyCompletion();
            }
        }

        /// <summary>
        /// Schedules the continuation to be run.
        /// <para>
        /// If the lazy construct has been evaluated, the continuation is executed immediately, else it is executed when the lazy is evaluated.
        /// </para>
        /// </summary>
        /// <param name="continuation"></param>
        public void OnCompleted(Action continuation)
        {
            lock(_continuations)
            {
                _continuations.Add(continuation);
            }

            NotifyCompletion();
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            lock (_continuations)
            {
                _continuations.Add(continuation);
            }

            NotifyCompletion();
        }

        private void NotifyCompletion()
        {
            if(_lazy.State == CustomLazyState.Initialized || _lazy.State == CustomLazyState.Faulted)
            {
                lock(_continuations)
                {
                    foreach(var continuation in _continuations)
                    {
                        try
                        {
                            continuation.Invoke();
                        }
                        catch
                        {
                        }
                    }

                    _continuations.Clear();
                }
            }
        }
    }


    public struct LazyAwaiter<Result> : IAwaiter<Result>, ICriticalNotifyCompletion
    {
        private readonly CustomLazy<Result> _lazy;
        private readonly List<Action> _continuations;

        public LazyAwaiter(CustomLazy<Result> lazy, Action<Exception> errorSetter)
        {
            _continuations = new List<Action>();
            _lazy = lazy;

            OnCompleted(() =>
            {
                if (lazy.State == CustomLazyState.Faulted)
                    errorSetter.Invoke(
                        lazy.Exception
                        ?? new Exception($"Lazy status is '{lazy.State}', yet no exception found that caused the fault."));
            });
        }

        public bool? IsSuccessful
        {
            get
            {
                return _lazy.State switch
                {
                    CustomLazyState.Faulted => false,
                    CustomLazyState.Initialized => true,
                    _ => null,
                };
            }
        }

        /// <summary>
        /// Always true so that awaiting on this awaiter will always run synchroniously
        /// </summary>
        public bool IsCompleted => true;

        public Result GetResult()
        {
            try
            {
                return _lazy.Value;
            }
            finally
            {
                NotifyCompletion();
            }
        }

        public void OnCompleted(Action continuation)
        {
            lock (_continuations)
            {
                _continuations.Add(continuation);
            }

            NotifyCompletion();
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            lock (_continuations)
            {
                _continuations.Add(continuation);
            }

            NotifyCompletion();
        }

        private void NotifyCompletion()
        {
            if (_lazy.State == CustomLazyState.Initialized || _lazy.State == CustomLazyState.Faulted)
            {
                lock (_continuations)
                {
                    foreach (var continuation in _continuations)
                    {
                        try
                        {
                            continuation.Invoke();
                        }
                        catch
                        {
                        }
                    }

                    _continuations.Clear();
                }
            }
        }
    }
}
