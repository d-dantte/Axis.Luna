using GridLuck.Common.Unions;

namespace GridLuck.Infra.AsynOps.Event
{
    public struct EventQueryResult<TResult> :
        IUnion<TResult, EventQueryStatus, EventQueryResult<TResult>>,
        IUnionOf<TResult, EventQueryStatus, EventQueryResult<TResult>>,
        IUnionImplicits<TResult, EventQueryStatus, EventQueryResult<TResult>>
    {
        private readonly object? _value;

        object? IUnion<TResult, EventQueryStatus, EventQueryResult<TResult>>.Value => _value;

        public EventQueryResult(TResult result) => _value = result;

        public EventQueryResult(EventQueryStatus status) => _value = status;

        public static EventQueryResult<TResult> Of(TResult value) => new(value);

        public static EventQueryResult<TResult> Of(EventQueryStatus value) => new(value);

        public static implicit operator EventQueryResult<TResult>(TResult value) => new(value);

        public static implicit operator EventQueryResult<TResult>(EventQueryStatus value) => new(value);
    }
}
