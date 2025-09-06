using GridLuck.Common.Extensions;
using GridLuck.Infra.AsynOps.Command;
using GridLuck.Infra.AsynOps.Event;

namespace GridLuck.Infra.AsynOps
{
    internal static class TypeExtensions
    {
        internal static bool IsEventHandler(this Type type)
        {
            return type.ImplementsAny(typeof(IEventHandler));
        }

        internal static bool IsCommandHandler(this Type type)
        {
            return type.ImplementsAny(typeof(ICommandHandler));
        }

        internal static bool IsCallbackCommandHandler(this Type type)
        {
            return type.ImplementsAny(typeof(ICallbackCommandHandler));
        }
    }
}
