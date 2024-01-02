namespace Axis.Luna.Unions.Types
{
    public interface IUnion<T1, T2, TSelf>
    where TSelf : IUnion<T1, T2, TSelf>
    {
        object Value { get; }

        bool Is(out T1 value);

        bool Is(out T2 value);

        bool IsNull();

        public TOut MapMatch<TOut>(
            Func<T1, TOut> t1Mapper,
            Func<T2, TOut> t2Mapper,
            Func<TOut>? nullMap = null);

        public void ConsumeMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action? nullConsumer = null);

        public TSelf WithMatch(
            Action<T1> t1Consumer,
            Action<T2> t2Consumer,
            Action? nullConsumer = null);

        abstract static TSelf Of(T1 value);

        abstract static TSelf Of(T2 value);
    }

    public readonly struct SampleUnion : IUnion<int, string, SampleUnion>
    {
        public object Value => throw new NotImplementedException();

        public static SampleUnion Of(int value)
        {
            throw new NotImplementedException();
        }

        public static SampleUnion Of(string value)
        {
            throw new NotImplementedException();
        }

        public bool Is(out int value)
        {
            throw new NotImplementedException();
        }

        public bool Is(out string value)
        {
            throw new NotImplementedException();
        }

        public bool IsNull()
        {
            throw new NotImplementedException();
        }

        public TOut MapMatch<TOut>(Func<int, TOut> t1Mapper, Func<string, TOut> t2Mapper, Func<TOut>? nullMap = null)
        {
            throw new NotImplementedException();
        }

        public void ConsumeMatch(Action<int> t1Consumer, Action<string> t2Consumer, Action? nullConsumer = null)
        {
            throw new NotImplementedException();
        }

        public SampleUnion WithMatch(Action<int> t1Consumer, Action<string> t2Consumer, Action? nullConsumer = null)
        {
            throw new NotImplementedException();
        }
    }
}
