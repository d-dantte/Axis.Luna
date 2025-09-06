namespace GridLuck.Infra.AsynOps.Event
{
    public interface IEventHandler
    {
        Task Handle<TEvent>(OperationId eventId, TEvent eventData);
    }
}
