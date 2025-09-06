namespace GridLuck.Infra.AsynOps.Command
{
    public enum CommandStatus
    {
        /// <summary>
        /// Command is still being executed
        /// </summary>
        Pending,

        /// <summary>
        /// Command has ceased executing because of an error
        /// </summary>
        Faulted,

        /// <summary>
        /// Command has completed. In place of this, the actual result is sent back
        /// </summary>
        Completed,

        Unknown
    }
}
