namespace GridLuck.Infra.AsynOps.Command
{
    public interface ICommandHandler
    {
        /// <summary>
        /// Executes the given command.
        /// </summary>
        /// <typeparam name="TCommand">The command type</typeparam>
        /// <param name="opId">The operation ID</param>
        /// <param name="command">The command instance</param>
        /// <returns>A task that completes immediately</returns>
        public Task Handle<TCommand>(OperationId opId, TCommand command);
    }

    public interface ICallbackCommandHandler
    {
        /// <summary>
        /// Executes the given command.
        /// </summary>
        /// <typeparam name="TCommand">The command type</typeparam>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <param name="opId">The operation ID</param>
        /// <param name="command">The command instance</param>
        /// <param name="callback">The callback called when the result is available</param>
        /// <returns>A task that completes immediately</returns>
        public Task Handle<TCommand, TResult>(
            OperationId opId,
            TCommand command,
            Func<TResult, Task> callback);
    }
}
