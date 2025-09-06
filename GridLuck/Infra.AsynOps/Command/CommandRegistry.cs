using GridLuck.Common.Extensions;
using System.Collections.Immutable;

namespace GridLuck.Infra.AsynOps.Command
{
    public class CommandHandlerRegistry
    {
        private readonly Dictionary<Type, Type> _handlerMap = [];
        private readonly Dictionary<Type, Type> _callbackHandlerMap = [];

        /// <summary>
        /// Registers a handler for the Command type
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <returns></returns>
        public CommandHandlerRegistry RegisterHandler<TCommand, THandler>()
        {
            return RegisterHandler(typeof(TCommand), typeof(THandler));
        }

        /// <summary>
        /// Registers a handler for the Command type
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <returns></returns>
        public CommandHandlerRegistry RegisterCallbackHandler<TCommand, THandler>()
        {
            return RegisterCallbackHandler(typeof(TCommand), typeof(THandler));
        }

        /// <summary>
        /// Registers a handler for the Command type
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="handlerType"></param>
        /// <returns></returns>
        public CommandHandlerRegistry RegisterHandler(Type commandType, Type handlerType)
        {
            ArgumentNullException.ThrowIfNull(commandType);
            ArgumentNullException.ThrowIfNull(handlerType);

            handlerType.ThrowIfNot(
                t => t.IsCommandHandler(),
                t => new ArgumentException($"Invalid {nameof(handlerType)}: does not implement {typeof(ICommandHandler)}"));

            if (_handlerMap.ContainsKey(commandType))
                throw new InvalidOperationException($"Command type is already registered: {commandType}");

            _handlerMap[commandType] = handlerType;
            return this;
        }

        /// <summary>
        /// Registers a handler for the Command type
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="handlerType"></param>
        /// <returns></returns>
        public CommandHandlerRegistry RegisterCallbackHandler(Type commandType, Type handlerType)
        {
            ArgumentNullException.ThrowIfNull(commandType);
            ArgumentNullException.ThrowIfNull(handlerType);

            handlerType.ThrowIfNot(
                t => t.IsCallbackCommandHandler(),
                t => new ArgumentException($"Invalid {nameof(handlerType)}: does not implement {typeof(ICallbackCommandHandler)}"));

            if (_callbackHandlerMap.ContainsKey(commandType))
                throw new InvalidOperationException($"Command type is already registered: {commandType}");

            _callbackHandlerMap[commandType] = handlerType;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public CommandHandlerResolver BuildResolver(
            IServiceProvider serviceProvider)
            => new(serviceProvider, _handlerMap, _callbackHandlerMap);
    }

    public class CommandHandlerResolver
    {
        private readonly ImmutableDictionary<Type, Type> _handlerMap;
        private readonly ImmutableDictionary<Type, Type> _callbackHandlerMap;
        private readonly IServiceProvider _serviceProvider;

        internal CommandHandlerResolver(
            IServiceProvider serviceProvider,
            IEnumerable<KeyValuePair<Type, Type>> handlerMap,
            IEnumerable<KeyValuePair<Type, Type>> callbackHandlerMap)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(handlerMap);

            _serviceProvider = serviceProvider;

            _handlerMap = handlerMap
                .ThrowIfAny(
                    IsInvalidHandlerMap,
                    kvp => new ArgumentException($"Invalid handler-map: contains null set, or null type"))
                .ToImmutableDictionary();

            _callbackHandlerMap = callbackHandlerMap
                .ThrowIfAny(
                    IsInvalidCallbackHandlerMap,
                    kvp => new ArgumentException($"Invalid handler-map: contains null set, or null type"))
                .ToImmutableDictionary(); ;
        }

        private static bool IsInvalidHandlerMap(KeyValuePair<Type, Type> handlerMap)
        {
            if (handlerMap.Key is null)
                return true;

            else if (handlerMap.Value is null)
                return true;

            else if (!handlerMap.Value.IsCommandHandler())
                return true;

            else return false;
        }

        private static bool IsInvalidCallbackHandlerMap(KeyValuePair<Type, Type> handlerMap)
        {
            if (handlerMap.Key is null)
                return true;

            else if (handlerMap.Value is null)
                return true;

            else if (!handlerMap.Value.IsCallbackCommandHandler())
                return true;

            else return false;
        }

        public ICommandHandler ResolveHandler<TCommand>() => ResolveHandler(typeof(TCommand));

        public ICommandHandler ResolveHandler(Type CommandType)
        {
            ArgumentNullException.ThrowIfNull(CommandType);

            if (!_handlerMap.TryGetValue(CommandType, out var handler))
                throw new KeyNotFoundException($"Invalid Command-type: no handlers mapped for the Command type");

            return _serviceProvider
                .GetService(handler)
                .As<ICommandHandler>();
        }

        public ICallbackCommandHandler ResolveCallbackHandler<TCommand>() => ResolveCallbackHandler(typeof(TCommand));

        public ICallbackCommandHandler ResolveCallbackHandler(Type CommandType)
        {
            ArgumentNullException.ThrowIfNull(CommandType);

            if (!_callbackHandlerMap.TryGetValue(CommandType, out var handler))
                throw new KeyNotFoundException($"Invalid Command-type: no handlers mapped for the Command type");

            return _serviceProvider
                .GetService(handler)
                .As<ICallbackCommandHandler>();
        }
    }
}
