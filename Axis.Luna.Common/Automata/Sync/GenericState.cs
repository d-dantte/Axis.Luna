using System;

namespace Axis.Luna.Common.Automata.Sync
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class GenericState<TData> : IState<TData> where TData : class
    {
        private readonly Func<TData, string> _act;
        private readonly Action<string, TData> _entering;
        private readonly Action<string, TData> _leaving;

        public string StateName { get; }

        public GenericState(
            string stateName,
            Func<TData, string> act,
            Action<string, TData> entering = null,
            Action<string, TData> leaving = null)
        {
            StateName = stateName;
            _act = act ?? throw new ArgumentNullException(nameof(act));
            _entering = entering;
            _leaving = leaving;
        }


        public string Act(TData data) => _act.Invoke(data);

        public void Entering(string previousState, TData data) => _entering?.Invoke(previousState, data);

        public void Leaving(string nextState, TData data) => _leaving?.Invoke(nextState, data);
    }
}
