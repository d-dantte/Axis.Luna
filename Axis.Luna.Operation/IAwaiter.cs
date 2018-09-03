using System.Runtime.CompilerServices;

namespace Axis.Luna.Operation
{

    public interface IAwaiter : INotifyCompletion
    {
        bool IsCompleted { get; }
        void GetResult();
    }

    public interface IAwaiter<Result> : INotifyCompletion
    {
        bool IsCompleted { get; }
        Result GetResult();
    }
}
