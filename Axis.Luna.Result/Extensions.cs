using System.Runtime.ExceptionServices;

namespace Axis.Luna.Result
{
    internal static class Extensions
    {
        internal static T Throw<T>(this Exception e)
        {
            ExceptionDispatchInfo.Capture(e).Throw();

            //never reached
            return default;
        }

        internal static TOut ApplyTo<TIn, TOut>(this TIn @in, Func<TIn, TOut> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            return mapper.Invoke(@in);
        }
    }
}
