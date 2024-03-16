namespace Axis.Luna.Optional
{
    public static class EnumerableExtensions
    {
        public static Optional<TItem> FirstOrOptional<TItem>(this
            IEnumerable<TItem> items,
            Func<TItem, bool> predicate)
            where TItem : class
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(predicate);

            return items.FirstOrDefault(predicate);
        }

        public static Optional<TItem> FirstOrOptional<TItem>(this
            IEnumerable<TItem> items)
            where TItem : class
        {
            ArgumentNullException.ThrowIfNull(items);

            return items.FirstOrDefault();
        }
        public static TItem? FirstOrNull<TItem>(this
            IEnumerable<TItem> items,
            Func<TItem, bool> predicate)
            where TItem : struct
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(predicate);

            using var enumerator = items.Where(predicate).GetEnumerator();

            if (!enumerator.MoveNext())
                return null;

            return enumerator.Current;
        }

        public static TItem? FirstOrNull<TItem>(this
            IEnumerable<TItem> items)
            where TItem : struct
        {
            ArgumentNullException.ThrowIfNull(items);

            using var enumerator = items.GetEnumerator();

            if (!enumerator.MoveNext())
                return null;

            return enumerator.Current;
        }
    }
}
