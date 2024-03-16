namespace Axis.Luna.Common.Indexers
{
    public interface IReadonlyIndexer<TKey, TValue>
    {
        TValue this[TKey key] { get; }
    }
}
