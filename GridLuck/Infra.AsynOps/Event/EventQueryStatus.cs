namespace GridLuck.Infra.AsynOps.Event
{
    /// <summary>
    /// Error Status of an event query operation
    /// </summary>
    public enum EventQueryStatus
    {
        /// <summary>
        /// Signifies a situation where no event instance could be found that was mapped to the given <see cref="OperationId"/>
        /// </summary>
        NotFound,

        /// <summary>
        /// An event instance was found, but it couldn't be mapped to the given type
        /// </summary>
        TypeMismatch
    }
}
