using GridLuck.Common.Unions;

namespace GridLuck.Infra.AsynOps.Command
{
    public readonly struct CommandResult<TResult> :
        IUnion<TResult, CommandStatus, CommandResult<TResult>>,
        IUnionOf<TResult, CommandStatus, CommandResult<TResult>>,
        IUnionImplicits<TResult, CommandStatus, CommandResult<TResult>>
    {
        private readonly object? _value;

        object? IUnion<TResult, CommandStatus, CommandResult<TResult>>.Value => _value;

        public CommandResult(TResult result) => _value = result;

        public CommandResult(CommandStatus status) => _value = status;

        public static CommandResult<TResult> Of(TResult value) => new(value);

        public static CommandResult<TResult> Of(CommandStatus value) => new(value);

        public static implicit operator CommandResult<TResult>(TResult value) => new(value);

        public static implicit operator CommandResult<TResult>(CommandStatus value) => new(value);
    }
}
