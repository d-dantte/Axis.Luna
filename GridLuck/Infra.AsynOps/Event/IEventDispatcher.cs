using GridLuck.Common.Extensions;
using NLog;

namespace GridLuck.Infra.AsynOps.Event
{
    public interface IEventDispatcher
    {
        /// <summary>
        /// Fire and forget the event. If there are no handlers, no error occurs
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        public Task<OperationId> DispatchEvent<TEvent>(TEvent @event, OperationNamespace @namespace = default);

        public Task<EventQueryResult<TEvent>> QueryEvent<TEvent>(OperationId eventId);
    }

    /// <summary>
    /// A dispatcher that does not persist/cache the event instance
    /// </summary>
    public class DefaultEventDispatcher : IEventDispatcher
    {
        private readonly EventHandlerResolver _eventResolver;
        private readonly IEventRepository? _eventRepository;
        private readonly ILogger _logger;

        public DefaultEventDispatcher(
            EventHandlerResolver eventResolver,
            IEventRepository? repository,
            ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(eventResolver);
            ArgumentNullException.ThrowIfNull(logger);

            _eventResolver = eventResolver;
            _eventRepository = repository;
            _logger = logger;
        }

        public async Task<OperationId> DispatchEvent<TEvent>(TEvent @event, OperationNamespace @namespace = default)
        {
            ArgumentNullException.ThrowIfNull(@event);

            var opId = new OperationId(AsyncOpType.Event, @namespace);

            // Persist the event
            var persistEventTask = _eventRepository?
                .Persist(opId, @event)
                .Then(() => { }, ex =>
                {
                    _logger.Error(
                        message: "Error while persisting event [event-id {0}, event {1}]", // may need to remove the event object from here, in case there is sensitive data within
                        exception: ex,
                        args: [opId, @event]);

                    ex.Throw();
                })
                ?? Task.CompletedTask;
            await persistEventTask;

            // Fire off event
            try
            {
                _eventResolver
                    .Resolve(typeof(TEvent))
                    .ForAll(handler =>
                    {
                        try
                        {
                            _ = handler
                                .Handle(opId, @event)
                                .Then(() => { }, ex =>
                                {
                                    _logger.Error(
                                        message: "Error while executing event handler [event-id {0}, handler {1}]",
                                        exception: ex,
                                        args: [opId, handler]);
                                });
                        }
                        catch (Exception error) // exception from Handle(xxx, xxx);
                        {
                            _logger.Error(
                                message: "Error while executing event handler [event-id {0}, handler {1}]",
                                exception: error,
                                args: [opId, handler]);
                        }
                    });

                return opId;
            }
            catch (Exception ex) // Exception from resolving the event handlers
            {
                _logger.Error(
                    message: "Error while resolving event handlers [event-id {0}]",
                    exception: ex,
                    args: [opId]);

                throw;
            }
        }

        public async Task<EventQueryResult<TEvent>> QueryEvent<TEvent>(OperationId eventId)
        {
            if (_eventRepository is null)
                return EventQueryResult<TEvent>.Of(EventQueryStatus.NotFound);

            else return await _eventRepository!.GetEvent<TEvent>(eventId);
        }


        #region Nested types

        /// <summary>
        /// Contract for the optional event repository
        /// </summary>
        public interface IEventRepository
        {
            Task Persist<TEvent>(OperationId eventId, TEvent @event);

            Task<EventQueryResult<TEvent>> GetEvent<TEvent>(OperationId eventId);
        }
        #endregion
    }
}
