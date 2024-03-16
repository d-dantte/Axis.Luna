namespace Axis.Luna.Common.Indexers
{
    public interface IWriteonlyIndexer<TKey, TValue>
    {
        TValue this[TKey key] { set; }
    }
}
