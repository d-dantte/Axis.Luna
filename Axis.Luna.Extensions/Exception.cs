using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Axis.Luna.Extensions
{
    public static class ExceptionExtension
    {
        #region Throw Null Arguments - Deprecating soon
        public static void ThrowNullArguments(this IEnumerable<Expression<Func<object>>> expressions)
            => ThrowNullArguments(expressions.ToArray());

        public static void ThrowNullArguments(params Expression<Func<object>>[] expressions)
        {
            foreach (var expr in expressions)
            {
                if (expr.Body is UnaryExpression)
                {
                    var uexp = expr.Body as UnaryExpression;
                    var maccess = uexp.Operand as MemberExpression;
                    if (maccess.CapturedValue() is null)
                        throw new ArgumentNullException(maccess.Member.Name);
                }
                else if (expr.Body is MemberExpression)
                {
                    var maccess = expr.Body as MemberExpression;
                    if (maccess.CapturedValue() is null)
                        throw new ArgumentNullException(maccess.Member.Name);
                }
            }
        }

        public static void ThrowNullArguments(params KeyValuePair<string, object>[] @params)
        {
            foreach(var kvp in @params)
                if (kvp.Value is null)
                    throw new ArgumentNullException(kvp.Key);
        }

        private static object CapturedValue(this MemberExpression memberAccess)
            => memberAccess.Expression is ConstantExpression
               ? memberAccess.Member.As<FieldInfo>().GetValue(memberAccess.Expression.As<ConstantExpression>().Value)
               : memberAccess.Expression.As<MemberExpression>().CapturedValue();
        #endregion

        #region Throw If Compare
        public static T ThrowIf<T>(this T value, T compare, Exception ex)
        {
            if (EqualityComparer<T>.Default.Equals(value, compare))
            {
                if (ex.StackTrace is null) throw ex; //<- hasn't been thrown already
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }

            return value;
        }

        public static T ThrowIf<T>(this T value, T compare, string message = null) => value.ThrowIf(compare, new Exception(message));

        public static T? ThrowIf<T>(this T? value, T? compare, Exception ex)
        where T : struct
        {
            if (EqualityComparer<T?>.Default.Equals(value.Value, compare.Value))
            {
                if (ex.StackTrace is null) throw ex; //<- hasn't been thrown already
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }

            return value;
        }

        public static T? ThrowIf<T>(this T? value, T? compare, string message = null)
        where T : struct => value.ThrowIf(compare, new Exception(message));
        #endregion

        #region Throw If Not Compare
        public static T ThrowIfNot<T>(this T value, T compare, Exception ex)
        {
            if (!EqualityComparer<T>.Default.Equals(value, compare))
            {
                if (ex.StackTrace is null) throw ex;
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }
            return value;
        }

        public static T ThrowIfNot<T>(this T value, T compare, string message = null) => value.ThrowIfNot(compare, new Exception(message));

        public static T? ThrowIfNot<T>(this T? value, T? compare, Exception ex)
        where T : struct
        {
            if (!EqualityComparer<T?>.Default.Equals(value.Value, compare.Value))
            {
                if (ex.StackTrace is null) throw ex; //<- hasn't been thrown already
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }

            return value;
        }

        public static T? ThrowIfNot<T>(this T? value, T? compare, string message = null)
        where T : struct => value.ThrowIf(compare, new Exception(message));
        #endregion

        #region Throw If Predicate
        public static T ThrowIf<T>(this T value, Func<T, bool> predicate, Exception e)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            if (predicate.Invoke(value))
            {
                if (e.StackTrace is null) throw e;
                else ExceptionDispatchInfo.Capture(e).Throw();
            }
            return value;
        }

        public static T ThrowIf<T>(
            this T value,
            Func<T, bool> predicate,
            string message = null)
            => value.ThrowIf(predicate, new Exception(message));

        public static T ThrowIf<T>(
            this T value,
            Func<T, bool> predicate,
            Func<T, Exception> exception)
        {
            if (predicate(value))
            {
                var ex = exception?.Invoke(value) ?? new System.Exception("An exception occured");
                if (ex.StackTrace is null) throw ex;
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }
            return value;
        }

        public static T ThrowIf<T>
            (this T value,
            Func<T, bool> predicate,
            Func<T, string> exceptionMessage)
            => value.ThrowIf(
                predicate,
                value => new Exception(exceptionMessage?.Invoke(value) ?? "An Exception occured"));

        #endregion

        #region Throw If Not Predicate
        public static T ThrowIfNot<T>(this
            T value,
            Func<T, bool> predicate,
            Exception exception)
            => value.ThrowIfNot(predicate, _ => exception);

        public static T ThrowIfNot<T>(this
            T value,
            Func<T, bool> predicate,
            Func<T, Exception> exceptionProvider)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            if (exceptionProvider is null)
                throw new ArgumentNullException(nameof(exceptionProvider));

            if (!predicate.Invoke(value))
            {
                var exception = exceptionProvider.Invoke(value);
                if (exception.StackTrace is null) throw exception;
                else ExceptionDispatchInfo.Capture(exception).Throw();
            }

            return value;
        }

        public static T? ThrowIfNot<T>(this
            T? value,
            Func<T?, bool> predicate,
            Exception exception)
            where T : struct
            => value.ThrowIfNot(predicate, _ => exception);

        public static T? ThrowIfNot<T>(this
            T? value,
            Func<T?, bool> predicate,
            Func<T?, Exception> exceptionProvider)
            where T : struct
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            if (exceptionProvider is null)
                throw new ArgumentNullException(nameof(exceptionProvider));

            if (!predicate.Invoke(value))
            {
                var exception = exceptionProvider.Invoke(value);
                if (exception.StackTrace is null) throw exception;
                else ExceptionDispatchInfo.Capture(exception).Throw();
            }

            return value;
        }
        #endregion

        #region Throw If Enumerable
        public static IEnumerable<TItem> ThrowIfNone<TItem>(this
            IEnumerable<TItem> items,
            Func<TItem, bool> predicate,
            Exception exception = null)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

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
                    .Capture(exception ?? new Exception("No element matched the predicate"))
                    .Throw();
            }
        }

        public static IEnumerable<TItem> ThrowIfAll<TItem>(this
            IEnumerable<TItem> items,
            Func<TItem, bool> predicate,
            Exception exception = null)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

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
                    .Capture(exception ?? new Exception("No element matched the predicate"))
                    .Throw();
            }
        }

        public static IEnumerable<TItem> ThrowIfAny<TItem>(this
            IEnumerable<TItem> items,
            Func<TItem, bool> predicate,
            Exception exception = null)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            foreach (var item in items)
            {
                if (predicate.Invoke(item))
                    ExceptionDispatchInfo
                        .Capture(exception ?? new Exception("No element matched the predicate"))
                        .Throw();

                yield return item;
            }
        }
        #endregion

        #region Throw if null
        public static T ThrowIfNull<T>(this T value, System.Exception ex)
        where T : class
        {
            if (value is null)
            {
                if (ex.StackTrace is null) throw ex;
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }
            return value;
        }

        public static T ThrowIfNull<T>(this T value, string message = null)
        where T : class => value.ThrowIfNull(new NullReferenceException(message));

        public static T? ThrowIfNull<T>(this T? value, System.Exception ex)
        where T : struct
        {
            if (value is null)
            {
                if (ex.StackTrace is null) throw ex;
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }
            return value;
        }

        public static T? ThrowIfNull<T>(this T? value, string message = null)
        where T : struct => value.ThrowIfNull(new NullReferenceException(message));
        #endregion

        #region Throw if not null
        public static T ThrowIfNotNull<T>(this T value, System.Exception ex)
        where T : class
        {
            if (value is not null)
            {
                if (ex.StackTrace is null) throw ex;
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }
            return value;
        }

        public static T ThrowIfNotNull<T>(this T value, string message = null)
        where T : class => value.ThrowIfNotNull(new Exception(message));


        public static T? ThrowIfNotNull<T>(this T? value, System.Exception ex)
        where T : struct
        {
            if (value.HasValue)
            {
                if (ex.StackTrace is null) throw ex;
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }
            return value;
        }

        public static T? ThrowIfNotNull<T>(this T? value, string message = null)
        where T : struct => value.ThrowIfNotNull(new Exception(message));
        #endregion

        #region Throw if default
        public static T ThrowIfDefault<T>(this T value, System.Exception ex) where T : struct => value.ThrowIf(default(T), ex);

        public static T ThrowIfDefault<T>(this T value, string message = null)
        where T : struct => value.ThrowIfDefault(new Exception(message));

        public static T? ThrowIfDefault<T>(this T? value, System.Exception ex) where T : struct
        {
            if (default(T?).Equals(value.Value))
                return ex.Throw<T?>();

            return value;
        }
        #endregion

        #region Throw
        public static T Throw<T>(this ExceptionDispatchInfo edi)
        {
            edi.Throw();

            //never reached
            return default;
        }

        public static void Throw(this ExceptionDispatchInfo edi) => edi.Throw();

        public static T Throw<T>(this Exception e)
        {
            if (e.StackTrace is null)
                throw e; //<- hasn't been thrown already

            ExceptionDispatchInfo.Capture(e).Throw();

            //never reached
            return default;
        }

        public static void Throw(this Exception e)
        {
            if (e.StackTrace is null)
                throw e; //<- hasn't been thrown already

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
