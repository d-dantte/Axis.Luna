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
                    //(maccess.Member as FieldInfo).GetValue((maccess.Expression as ConstantExpression).Value)
                    //                             .ThrowIfNull(new ArgumentException(maccess.Member.Name));
                    maccess.CapturedValue().ThrowIfNull(new ArgumentNullException(maccess.Member.Name));
                }
                else if (expr.Body is MemberExpression)
                {
                    var maccess = expr.Body as MemberExpression;
                    //(maccess.Member as FieldInfo).GetValue((maccess.Expression as ConstantExpression).Value)
                    //                             .ThrowIfNull(new ArgumentException(maccess.Member.Name));
                    maccess.CapturedValue().ThrowIfNull(new ArgumentNullException(maccess.Member.Name));
                }
            }
        }

        private static object CapturedValue(this MemberExpression memberAccess)
        => (memberAccess.Expression is ConstantExpression) ?
           memberAccess.Member.As<FieldInfo>().GetValue(memberAccess.Expression.As<ConstantExpression>().Value) :
           memberAccess.Expression.As<MemberExpression>().CapturedValue();

        public static T ThrowIf<T>(this T value, T compare, System.Exception ex)
        {
            if (EqualityComparer<T>.Default.Equals(value, compare))
            {
                if (ex.StackTrace == null) throw ex; //<- hasn't been thrown already
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }

            return value;
        }

        public static T ThrowIf<T>(this T value, T compare, string message = null) => value.ThrowIf(compare, new Exception(message));


        public static T? ThrowIf<T>(this T? value, T? compare, System.Exception ex)
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


        public static T ThrowIfNot<T>(this T value, T compare, System.Exception ex)
        {
            if (!EqualityComparer<T>.Default.Equals(value, compare))
            {
                if (ex.StackTrace == null) throw ex;
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }
            return value;
        }

        public static T ThrowIfNot<T>(this T value, T compare, string message = null) => value.ThrowIfNot(compare, new Exception(message));


        public static T? ThrowIfNot<T>(this T? value, T? compare, System.Exception ex)
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

        public static T ThrowIfNull<T>(this T value, System.Exception ex)
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

        public static T? ThrowIfNull<T>(this T? value, System.Exception ex)
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


        public static T ThrowIfNotNull<T>(this T value, System.Exception ex)
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


        public static T? ThrowIfNotNull<T>(this T? value, System.Exception ex)
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


        public static T ThrowIfDefault<T>(this T value, System.Exception ex) where T : struct => value.ThrowIf(default(T), ex);

        public static T ThrowIfDefault<T>(this T value, string message = null)
        where T : struct => value.ThrowIfDefault(new Exception(message));

        public static T ThrowIf<T>(this T value, Func<T, bool> predicate, System.Exception e)
        {
            if (predicate(value))
            {
                if (e.StackTrace == null) throw e;
                else ExceptionDispatchInfo.Capture(e).Throw();
            }
            return value;
        }

        public static T ThrowIf<T>(this T value, Func<T, bool> predicate, string message = null) => value.ThrowIf(predicate, new Exception(message));

        public static T ThrowIf<T>(this T value, Func<T, bool> predicate, Func<T, System.Exception> exception)
        {
            if (predicate(value))
            {
                var ex = exception?.Invoke(value) ?? new System.Exception("An exception occured");
                if (ex.StackTrace == null) throw ex;
                else ExceptionDispatchInfo.Capture(ex).Throw();
            }
            return value;
        }

        public static T ThrowIf<T>(this T value, Func<T, bool> predicate, Func<T, string> exceptionMessage)
        => value.ThrowIf(predicate, _t => new Exception(exceptionMessage?.Invoke(_t) ?? "An Exception occured"));

        public static Exception InnermostException(this System.Exception ex)
        {
            var x = ex;
            while (ex.InnerException != null) x = ex.InnerException;

            return x;
        }
    }
}
