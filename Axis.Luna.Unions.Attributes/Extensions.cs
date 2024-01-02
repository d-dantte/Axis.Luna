using System.Runtime.ExceptionServices;

namespace Axis.Luna.Unions.Attributes
{
    public static class Extensions
    {
        public static T Throw<T>(this Exception ex)
        {
            ExceptionDispatchInfo
                .Capture(ex)
                .Throw();

            return default;
        }

        public static void Throw(this Exception ex)
        {
            ExceptionDispatchInfo
                .Capture(ex)
                .Throw();
        }

        public static IEnumerable<T> ThrowIfAny<T>(this
            IEnumerable<T> sequence,
            Func<T, bool> predicate,
            Func<T, Exception>? exceptionProvider = null)
        {
            ArgumentNullException.ThrowIfNull(sequence);
            ArgumentNullException.ThrowIfNull(predicate);

            return sequence
                .Select(t =>
                {
                    if (predicate.Invoke(t))
                    {
                        var ex = exceptionProvider?.Invoke(t) ?? new Exception("Illegal value found in sequene");
                        ExceptionDispatchInfo
                            .Capture(ex)
                            .Throw();
                    }

                    return t;
                });
        }
    }
}
