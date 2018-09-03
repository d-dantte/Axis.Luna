
namespace Axis.Luna.Operation
{
    public interface IAwaitable
    {
        IAwaiter GetAwaiter();
    }

    public interface IAwaitable<Result>
    {
        IAwaiter<Result> GetAwaiter();
    }
}
