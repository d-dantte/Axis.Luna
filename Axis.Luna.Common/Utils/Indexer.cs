namespace Axis.Luna.Common.Utils
{
    public interface IReadonlyIndexer<TKey, TValue>
    {
        TValue this[TKey key] { get; }
    }

    public interface IWriteonlyIndexer<TKey, TValue>
    {
        TValue this[TKey key] { set; }
    }

    public interface IIndexer<TKey, TValue>: IReadonlyIndexer<TKey, TValue>, IWriteonlyIndexer<TKey, TValue>
    {
    }
}
