namespace Axis.Luna.Automata.Sync
{
    public class DefaultState<TData> : IState<TData> where TData : class
    {
        private readonly Func<string?, TData, string> _act;
        private readonly Action<string?, TData>? _entering;

        public string StateName { get; }

        public DefaultState(
            string stateName,
            Func<string?, TData, string> act,
            Action<string?, TData>? entering = null)
        {
            ArgumentNullException.ThrowIfNull(stateName);
            ArgumentNullException.ThrowIfNull(act);

            StateName = stateName;
            _act = act;
            _entering = entering;
        }


        public void Entering(
            string? previousState,
            TData data) => _entering?.Invoke(previousState, data);

        public string Act(
            string? previousState,
            TData data)
            => _act.Invoke(previousState, data);
    }
}
