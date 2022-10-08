
namespace Axis.Luna.Operation
{
    public interface IAwaitable
    {
        /// <summary>
        /// Returns the awaiter that enables async execution
        /// </summary>
        IAwaiter GetAwaiter();
    }

    public interface IAwaitable<out Result>
    {
        /// <summary>
        /// Returns the awaiter that enables async execution
        /// </summary>
        IAwaiter<Result> GetAwaiter();
    }
}
