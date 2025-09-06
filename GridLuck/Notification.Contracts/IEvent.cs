namespace GridLuck.Notification.Contracts
{
    public interface IEvent
    {
        DateTimeOffset Timestamp { get; }
    }
}
