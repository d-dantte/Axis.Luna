using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace Axis.Luna.Extensions
{
    public static class ExceptionExtension
    {
        #region Throw If Compare
        public static T ThrowIf<T>(this
            T value,
            T compare,
            Func<T, Exception> exceptionProvider)
        {
            ArgumentNullException.ThrowIfNull(exceptionProvider);

            if (EqualityComparer<T>.Default.Equals(value, compare))
            {
                ExceptionDispatchInfo
                    .Capture(exceptionProvider.Invoke(value)
                        ?? new InvalidOperationException($"'null' returns forbidden from {nameof(exceptionProvider)}"))
                    .Throw();
            }

            return value;
        }

        public static T ThrowIf<T>(
            this T value,
            T compare,
            string message = null)
            => value.ThrowIf(compare, _ => new Exception(message));

        public static T? ThrowIf<T>(this
            T? value,
            T? compare,
            Func<T?, Exception> exceptionProvider)
        where T : struct
        {
            if (EqualityComparer<T?>.Default.Equals(value.Value, compare.Value))
            {
                ExceptionDispatchInfo
                    .Capture(exceptionProvider.Invoke(value)
                        ?? new InvalidOperationException($"'null' returns forbidden from {nameof(exceptionProvider)}"))
                    .Throw();
            }

            return value;
        }

        public static T? ThrowIf<T>(this
            T? value,
            T? compare,
            string message = null)
            where T : struct
            => value.ThrowIf(compare, _ => new Exception(message));
        #endregion

        #region Throw If Not Compare
        public static T ThrowIfNot<T>(this
            T value,
            T compare,
            Func<Exception> exceptionProvider)
        {
            ArgumentNullException.ThrowIfNull(exceptionProvider);

            if (!EqualityComparer<T>.Default.Equals(value, compare))
            {
                ExceptionDispatchInfo
                    .Capture(exceptionProvider.Invoke()
                        ?? new InvalidOperationException($"'null' returns forbidden from {nameof(exceptionProvider)}"))
                    .Throw();
            }
            return value;
        }

        public static T ThrowIfNot<T>(this
            T value,
            T compare,
            string message = null)
            => value.ThrowIfNot(compare, () => new Exception(message));

        public static T? ThrowIfNot<T>(this
            T? value,
            T? compare,
            Func<Exception> exceptionProvider)
            where T : struct
        {
            if (!EqualityComparer<T?>.Default.Equals(value.Value, compare.Value))
            {
                ExceptionDispatchInfo
                    .Capture(exceptionProvider.Invoke()
                        ?? new InvalidOperationException($"'null' returns forbidden from {nameof(exceptionProvider)}"))
                    .Throw();
            }

            return value;
        }

        public static T? ThrowIfNot<T>(this
            T? value,
            T? compare,
            string message = null)
            where T : struct
            => value.ThrowIfNot(compare, () => new Exception(message));
        #endregion

        #region Throw If Predicate
        public static T ThrowIf<T>(
            this T value,
            Func<T, bool> predicate,
            string message = null)
            => value.ThrowIf(predicate, _ => new Exception(message));

        public static T ThrowIf<T>(
            this T value,
            Func<T, bool> predicate,
            Func<T, Exception> exceptionProvider)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(exceptionProvider);

            if (predicate(value))
            {
                ExceptionDispatchInfo
                    .Capture(exceptionProvider.Invoke(value)
                        ?? new InvalidOperationException($"'null' returns forbidden from {nameof(exceptionProvider)}"))
                    .Throw();
            }
            return value;
        }

        #endregion

        #region Throw If Not Predicate

        public static T ThrowIfNot<T>(this
            T value,
            Func<T, bool> predicate,
            Func<T, Exception> exceptionProvider)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(exceptionProvider);

            if (!predicate.Invoke(value))
            {
                ExceptionDispatchInfo
                    .Capture(exceptionProvider.Invoke(value)
                        ?? new InvalidOperationException($"'null' returns forbidden from {nameof(exceptionProvider)}"))
                    .Throw();
            }

            return value;
        }

        public static T? ThrowIfNot<T>(this
            T? value,
            Func<T?, bool> predicate,
            Func<T?, Exception> exceptionProvider)
            where T : struct
        {
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(exceptionProvider);

            if (!predicate.Invoke(value))
            {
                ExceptionDispatchInfo
                    .Capture(exceptionProvider.Invoke(value)
                        ?? new InvalidOperationException($"'null' returns forbidden from {nameof(exceptionProvider)}"))
                    .Throw();
            }

            return value;
        }
        #endregion

        #region Throw If Enumerable
        public static IEnumerable<TItem> ThrowIfNone<TItem>(this
            IEnumerable<TItem> items,
            Func<TItem, bool> predicate,
            Func<IEnumerable<TItem>, Exception> exceptionProvider)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(exceptionProvider);

            var found = false;
            foreach (var item in items)
            {
                if (predicate.Invoke(item))
                    found = true;

                yield return item;
            }

            if (!found)
            {
                ExceptionDispatchInfo
                    .Capture(exceptionProvider.Invoke(items)
                        ?? new InvalidOperationException($"'null' returns forbidden from {nameof(exceptionProvider)}"))
                    .Throw();
            }
        }

        public static IEnumerable<TItem> ThrowIfAll<TItem>(this
            IEnumerable<TItem> items,
            Func<TItem, bool> predicate,
            Func<IEnumerable<TItem>, Exception> exceptionProvider)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(exceptionProvider);

            var allMatch = true;
            var empty = true;
            foreach (var item in items)
            {
                if (empty)
                    empty = false;

                allMatch &= predicate.Invoke(item);

                yield return item;
            }

            if (allMatch && !empty)
            {
                ExceptionDispatchInfo
                    .Capture(exceptionProvider.Invoke(items)
                        ?? new InvalidOperationException($"'null' returns forbidden from {nameof(exceptionProvider)}"))
                    .Throw();
            }
        }

        public static IEnumerable<TItem> ThrowIfAny<TItem>(this
            IEnumerable<TItem> items,
            Func<TItem, bool> predicate,
            Func<TItem, Exception> exceptionProvider)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(exceptionProvider);

            foreach (var item in items)
            {
                if (predicate.Invoke(item))
                    ExceptionDispatchInfo
                        .Capture(exceptionProvider.Invoke(item) 
                            ?? new InvalidOperationException($"'null' returns forbidden from {nameof(exceptionProvider)}"))
                        .Throw();

                yield return item;
            }
        }
        #endregion

        #region Throw if null
        public static T ThrowIfNull<T>(this T value, Func<Exception> exceptionProvider)
        where T : class
        {
            if (value is null)
            {
                ExceptionDispatchInfo
                    .Capture(exceptionProvider.Invoke()
                        ?? new InvalidOperationException($"'null' returns forbidden from {nameof(exceptionProvider)}"))
                    .Throw();
            }
            return value;
        }

        public static T ThrowIfNull<T>(this
            T value,
            string message = null)
            where T : class
            => value.ThrowIfNull(() => new NullReferenceException(message));

        public static T? ThrowIfNull<T>(this T? value, Func<Exception> exceptionProvider)
        where T : struct
        {
            if (value is null)
            {
                var e = exceptionProvider.Invoke();
                ExceptionDispatchInfo
                    .Capture(e)
                    .Throw();
            }
            return value;
        }

        public static T? ThrowIfNull<T>(this
            T? value,
            string message = null)
            where T : struct
            => value.ThrowIfNull(() => new NullReferenceException(message));
        #endregion

        #region Throw if not null
        public static T ThrowIfNotNull<T>(this T value, Func<T, Exception> exceptionProvider)
        where T : class
        {
            if (value is not null)
            {
                ExceptionDispatchInfo
                    .Capture(exceptionProvider.Invoke(value)
                        ?? new InvalidOperationException($"'null' returns forbidden from {nameof(exceptionProvider)}"))
                    .Throw();
            }
            return value;
        }

        public static T ThrowIfNotNull<T>(this
            T value,
            string message = null)
            where T : class
            => value.ThrowIfNotNull(_ => new Exception(message));

        public static T? ThrowIfNotNull<T>(this
            T? value,
            Func<T?, Exception> exceptionProvider)
            where T : struct
        {
            if (value.HasValue)
            {
                ExceptionDispatchInfo
                    .Capture(exceptionProvider.Invoke(value)
                        ?? new InvalidOperationException($"'null' returns forbidden from {nameof(exceptionProvider)}"))
                    .Throw();
            }
            return value;
        }

        public static T? ThrowIfNotNull<T>(this
            T? value,
            string message = null)
            where T : struct
            => value.ThrowIfNotNull(_ => new Exception(message));
        #endregion

        #region Throw if default
        public static T ThrowIfDefault<T>(this
            T value,
            Func<T, Exception> ex)
            where T : struct
            => value.ThrowIf(default(T), ex);

        public static T ThrowIfDefault<T>(this
            T value,
            string message = null)
            where T : struct
            => value.ThrowIfDefault(_ => new Exception(message));

        public static T? ThrowIfDefault<T>(this T? value, Func<T?, Exception> exceptionProvider)
        where T : struct
        {
            ArgumentNullException.ThrowIfNull(exceptionProvider);

            if (default(T?).Equals(value.Value))
            {
                ExceptionDispatchInfo
                    .Capture(exceptionProvider.Invoke(value)
                        ?? new InvalidOperationException($"'null' returns forbidden from {nameof(exceptionProvider)}"))
                    .Throw();
            }

            return value;
        }
        #endregion

        #region Throw
        public static T Throw<T>(this ExceptionDispatchInfo edi)
        {
            ArgumentNullException.ThrowIfNull(edi);

            edi.Throw();

            //never reached
            return default;
        }

        public static void Throw(this ExceptionDispatchInfo edi)
        {
            ArgumentNullException.ThrowIfNull(edi);

            edi.Throw();
        }

        public static T Throw<T>(this Exception e)
        {
            ExceptionDispatchInfo.Capture(e).Throw();

            //never reached
            return default;
        }

        public static void Throw(this Exception e)
        {
            ExceptionDispatchInfo.Capture(e).Throw();
        }
        #endregion

        public static Exception InnermostException(this Exception ex)
        {
            var x = ex;
            while (ex.InnerException is not null) x = ex.InnerException;

            return x;
        }
    }
}
