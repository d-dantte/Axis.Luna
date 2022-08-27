
namespace Axis.Luna.Operation
{
    public interface IAwaitable
    {
        IAwaiter GetAwaiter();
    }

    public interface IAwaitable<out Result>
    {
        IAwaiter<Result> GetAwaiter();
    }
}
