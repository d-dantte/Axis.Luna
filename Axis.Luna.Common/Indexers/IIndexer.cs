namespace Axis.Luna.Common.Indexers
{
    public interface IIndexer<TKey, TValue> : IReadonlyIndexer<TKey, TValue>, IWriteonlyIndexer<TKey, TValue>
    {
    }
}
