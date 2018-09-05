using System;
using System.Runtime.CompilerServices;

namespace Axis.Luna.Operation
{
    public interface IAwaiter: INotifyCompletion
    {
        bool IsCompleted { get; }
        void GetResult();
    }

    public interface IAwaiter<Result>: INotifyCompletion
    {
        bool IsCompleted { get; }
        Result GetResult();
    }


    public interface IAwaitable
    {
        IAwaiter GetAwaiter();
    }

    public interface IAwaitable<Result>
    {
        IAwaiter<Result> GetAwaiter();
    }


    public interface IOperation<Result>: IAwaitable<Result>
    {
        Result Resolve();
        Exception GetException();
        bool? Succeeded { get; }
    }

    public interface IOperation: IAwaitable
    {
        void Resolve();
        Exception GetException();
        bool? Succeeded { get; }
    }
}
