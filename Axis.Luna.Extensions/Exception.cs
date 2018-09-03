using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace Axis.Luna.Extensions
{
    public static class ExceptionExts
    {

        public static T ThrowIf<T>(this T value, T compare, Exception ex)
        {
            if (EqualityComparer<T>.Default.Equals(value, compare))
            {
                if (ex.StackTrace == null) throw ex; //<- hasn't been thrown already
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }

            return value;
        }

        public static T ThrowIf<T>(this T value, T compare, string message = null) => value.ThrowIf(compare, new Exception(message));


        public static T? ThrowIf<T>(this T? value, T? compare, Exception ex)
        where T : struct
        {
            if ((value == null && compare == null) 
             || (value.HasValue && compare.HasValue && EqualityComparer<T>.Default.Equals(value.Value, compare.Value)))
            {
                if (ex.StackTrace == null) throw ex; //<- hasn't been thrown already
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }

            return value;
        }

        public static T? ThrowIf<T>(this T? value, T? compare, string message = null)
        where T : struct => value.ThrowIf(compare, new Exception(message));


        public static T ThrowIfNot<T>(this T value, T compare, Exception ex)
        {
            if (!EqualityComparer<T>.Default.Equals(value, compare))
            {
                if (ex.StackTrace == null) throw ex;
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }
            return value;
        }

        public static T ThrowIfNot<T>(this T value, T compare, string message = null) => value.ThrowIfNot(compare, new Exception(message));


        public static T? ThrowIfNot<T>(this T? value, T? compare, Exception ex)
        where T : struct
        {
            if ((value == null && compare == null)
             || (value.HasValue && compare.HasValue && EqualityComparer<T>.Default.Equals(value.Value, compare.Value)))
                return value;
            else
            {
                if (ex.StackTrace == null) throw ex; //<- hasn't been thrown already
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }

            return value;
        }

        public static T? ThrowIfNot<T>(this T? value, T? compare, string message = null)
        where T : struct => value.ThrowIfNot(compare, new Exception(message));

        public static T ThrowIfNull<T>(this T value, Exception ex)
        where T : class
        {
            if (value == null)
            {
                if (ex.StackTrace == null) throw ex;
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }
            return value;
        }

        public static T ThrowIfNull<T>(this T value, string message = null)
        where T : class => value.ThrowIfNull(new NullReferenceException(message));

        public static T? ThrowIfNull<T>(this T? value, Exception ex)
        where T : struct
        {
            if (value == null)
            {
                if (ex.StackTrace == null) throw ex;
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }
            return value;
        }

        public static T? ThrowIfNull<T>(this T? value, string message = null)
        where T : struct => value.ThrowIfNull(new NullReferenceException(message));


        public static T ThrowIfNotNull<T>(this T value, Exception ex)
        where T : class
        {
            if (value != null)
            {
                if (ex.StackTrace == null) throw ex;
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }
            return value;
        }

        public static T ThrowIfNotNull<T>(this T value, string message = null)
        where T : class => value.ThrowIfNotNull(new NullReferenceException(message));


        public static T? ThrowIfNotNull<T>(this T? value, Exception ex)
        where T : struct
        {
            if (value.HasValue)
            {
                if (ex.StackTrace == null) throw ex;
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }
            return value;
        }

        public static T? ThrowIfNotNull<T>(this T? value, string message = null)
        where T : struct => value.ThrowIfNotNull(new NullReferenceException(message));


        public static T ThrowIfDefault<T>(this T value, Exception ex) where T : struct => value.ThrowIf(default(T), ex);

        public static T ThrowIfDefault<T>(this T value, string message = null)
        where T : struct => value.ThrowIfDefault(new Exception(message));

        public static T ThrowIf<T>(this T value, Func<T, bool> predicate, Exception e)
        {
            if (predicate(value))
            {
                if (e.StackTrace == null) throw e;
                else ExceptionDispatchInfo.Capture(e).Throw();
            }
            return value;
        }

        public static T ThrowIf<T>(this T value, Func<T, bool> predicate, string message = null) => value.ThrowIf(predicate, new Exception(message));

        public static T ThrowIf<T>(this T value, Func<T, bool> predicate, Func<T, Exception> exception)
        {
            if (predicate(value))
            {
                var ex = exception?.Invoke(value) ?? new Exception("An exception occured");
                if (ex.StackTrace == null) throw ex;
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }
            return value;
        }

        public static T ThrowIf<T>(this T value, Func<T, bool> predicate, Func<T, string> exceptionMessage)
        => value.ThrowIf(predicate, _t => new Exception(exceptionMessage?.Invoke(_t) ?? "An Exception occured"));

        public static Exception InnermostException(this Exception ex)
        {
            var x = ex;
            while (ex.InnerException != null) x = ex.InnerException;

            return x;
        }
    }
}
