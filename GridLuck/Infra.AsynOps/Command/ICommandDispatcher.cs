using GridLuck.Common.Results;
using NLog;

namespace GridLuck.Infra.AsynOps.Command
{
    /// <summary>
    /// Contract for retrieving <see cref="CommandResult{TResult}"/> for a given <see cref="OperationId"/>
    /// </summary>
    public interface ICommandQueryable
    {
        /// <summary>
        /// Given a <paramref name="commandId"/>, query the status|result of the command
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="commandId"></param>
        /// <returns></returns>
        public Task<CommandResult<TResult>> QueryCommand<TResult>(OperationId commandId);
    }

    /// <summary>
    /// A command dispatcher contract.
    /// </summary>
    /// <remarks>
    /// Note that instances of this type are responsible for creating the <see cref="OperationId"/>
    /// </remarks>
    public interface ICommandDispatcher : ICommandQueryable
    {
        /// <summary>
        /// Invoke a command and receive it's command id. This ID can be used to query/poll for the status|result of the command.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="command"></param>
        /// <param name="commandNamespace"></param>
        /// <returns></returns>
        public Task<OperationId> DispatchCommand<TCommand>(TCommand command, OperationNamespace commandNamespace);
    }

    /// <summary>
    /// A callback-command dispatcher contract.
    /// </summary>
    /// <remarks>
    /// Note that instances of this type are responsible for creating the <see cref="OperationId"/>
    /// </remarks>
    public interface ICallbackCommandDispatcher : ICommandQueryable
    {
        /// <summary>
        /// Invoke a command that will execute a callback when the result is ready. Note that commands invoked this way
        /// are still async commands, and can be queried via the <see cref="QueryCommand{TResult, AsyncOperationIdentifier}"/> interface.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="command"></param>
        /// <param name="commandNamespace"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Task<OperationId> DispatchCallbackCommand<TCommand, TResult>(
            TCommand command,
            OperationNamespace commandNamespace,
            Func<Result<TResult>, Task> callback);
    }


    public class DefaultCommandDispatcher :
        ICommandDispatcher,
        ICallbackCommandDispatcher
    {
        private readonly CommandHandlerResolver _commandResolver;
        private readonly ICommandRepository _commandRepository;
        private readonly ILogger _logger;

        public DefaultCommandDispatcher(
            CommandHandlerResolver commandResolver,
            ICommandRepository repository,
            ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(commandResolver);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(repository);

            _commandResolver = commandResolver;
            _commandRepository = repository;
            _logger = logger;
        }

        public async Task<OperationId> DispatchCommand<TCommand>(
            TCommand command,
            OperationNamespace commandNamespace)
        {
            ArgumentNullException.ThrowIfNull(command); ;

            var opId = new OperationId(AsyncOpType.Command, commandNamespace);

            await _commandResolver
                .ResolveHandler<TCommand>()
                .Handle(opId, command);

            return opId;
        }

        public async Task<OperationId> DispatchCallbackCommand<TCommand, TResult>(
            TCommand command,
            OperationNamespace commandNamespace,
            Func<Result<TResult>, Task> callback)
        {
            ArgumentNullException.ThrowIfNull(command);

            var opId = new OperationId(AsyncOpType.Command, commandNamespace);

            await _commandResolver
                .ResolveCallbackHandler<TCommand>()
                .Handle(opId, command, callback);

            return opId;
        }

        public async Task<CommandResult<TResult>> QueryCommand<TResult>(OperationId commandId)
        {
            return await _commandRepository.GetCommandResult<TResult>(commandId);
        }

        #region Nested types

        /// <summary>
        /// Contract for the optional Command repository
        /// </summary>
        public interface ICommandRepository
        {
            Task Persist<TCommand>(OperationId CommandId, TCommand @Command);

            Task<CommandResult<TResult>> GetCommandResult<TResult>(OperationId CommandId);
        }
        #endregion
    }
}
