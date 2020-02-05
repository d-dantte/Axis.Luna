using System.Runtime.CompilerServices;

namespace Axis.Luna.Operation
{

    public interface IAwaiter : INotifyCompletion
    {
        /// <summary>
        /// Indicates if the operation has completed
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Indicates if the operation was successful or not.
        /// NOTE:
        /// True = successful;
        /// False = unsuccessful;
        /// Null = not completed;
        /// </summary>
        bool? IsSuccessful { get; }

        /// <summary>
        /// Evaluates the operation
        /// </summary>
        void GetResult();
    }

    public interface IAwaiter<Result> : INotifyCompletion
    {
        /// <summary>
        /// Indicates if the operation has completed
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Indicates if the operation was successful or not.
        /// NOTE:
        /// True = successful;
        /// False = unsuccessful;
        /// Null = not evaluated;
        /// </summary>
        bool? IsSuccessful { get; }

        /// <summary>
        /// Evaluates the operation and returns the result
        /// </summary>
        Result GetResult();
    }
}
