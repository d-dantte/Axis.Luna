using GridLuck.Common.Extensions;
using System.Collections.Immutable;

namespace GridLuck.Infra.AsynOps.Event
{
    public class EventHandlerRegistry
    {
        private readonly Dictionary<Type, HashSet<Type>> _handlerMap = [];

        /// <summary>
        /// Registers a handler for the event type
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <returns></returns>
        public EventHandlerRegistry RegisterHandler<TEvent, THandler>()
        {
            return RegisterHandler(typeof(TEvent), typeof(THandler));
        }

        /// <summary>
        /// Registers a handler for the event type
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handlerType"></param>
        /// <returns></returns>
        public EventHandlerRegistry RegisterHandler(Type eventType, Type handlerType)
        {
            ArgumentNullException.ThrowIfNull(eventType);
            ArgumentNullException.ThrowIfNull(handlerType);

            handlerType.ThrowIfNot(
                t => t.IsEventHandler(),
                t => new ArgumentException($"Invalid {nameof(handlerType)}: does not implement {typeof(IEventHandler)}"));

            var handlerSet = _handlerMap.GetOrAdd(eventType, _ => []);
            handlerSet.Add(handlerType);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public EventHandlerResolver BuildResolver(
            IServiceProvider serviceProvider)
            => new(serviceProvider, _handlerMap);
    }

    public class EventHandlerResolver
    {
        private readonly ImmutableDictionary<Type, ImmutableHashSet<Type>> _handlerMap;
        private readonly IServiceProvider _serviceProvider;

        internal EventHandlerResolver(
            IServiceProvider serviceProvider,
            IEnumerable<KeyValuePair<Type, HashSet<Type>>> handlerMap)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(handlerMap);

            _serviceProvider = serviceProvider;
            _handlerMap = handlerMap
                .ThrowIfAny(
                    IsInvalidHandlerMap,
                    kvp => new ArgumentException($"Invalid handler-map: contains null set, or null type"))
                .ToImmutableDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToImmutableHashSet());
        }

        private static bool IsInvalidHandlerMap(KeyValuePair<Type, HashSet<Type>> handlerMap)
        {
            if (handlerMap.Key is null)
                return false;

            else if (handlerMap.Value is null)
                return false;

            else if (handlerMap.Value.Any(t => t is null))
                return false;

            else return true;
        }

        public IEnumerable<IEventHandler> Resolve<TEvent>() => Resolve(typeof(TEvent));

        public IEnumerable<IEventHandler> Resolve(Type eventType)
        {
            ArgumentNullException.ThrowIfNull(eventType);

            if (!_handlerMap.TryGetValue(eventType, out var handlers))
                throw new KeyNotFoundException($"Invalid event-type: no handlers mapped for the event type");

            return handlers
                .Select(_serviceProvider.GetService)
                .Cast<IEventHandler>();
        }
    }
}
  