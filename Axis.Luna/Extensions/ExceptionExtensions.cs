using static Axis.Luna.Extensions.TypeExtensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using Axis.Luna.Operation;

namespace Axis.Luna.Extensions
{
    public static class ExceptionExtensions
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
           memberAccess.Member.Cast<FieldInfo>().GetValue(memberAccess.Expression.Cast<ConstantExpression>().Value) :
           memberAccess.Expression.Cast<MemberExpression>().CapturedValue();


        public static void Throw(this Exception e)
        {
            throw new Exception("See inner exception", e);
        }
        public static R Throw<R>(this Exception e)
        {
            throw new Exception("See inner exception", e);
        }

        public static R ThrowIfNotNull<R>(this R value, string exceptionMessage)
        where R : class => value.ThrowIf(r => r != null, exceptionMessage);

        public static R ThrowIfNotNull<R>(this R value, Exception ex)
        where R : class => value.ThrowIf(r => r != null, ex);

        public static R ThrowIfNull<R>(this R value, string message = null)
        where R : class => value.ThrowIfNull(new Exception(message ?? "Null value"));

        public static R ThrowIfNull<R>(this R value, Exception ex)
        where R : class
        {
            if (value == null) throw ex ?? new Exception("Null value");
            else return value;
        }

        public static T? ThrowIfNull<T>(this T? value, Exception ex)
        where T: struct
        {
            if (value == null) throw ex ?? new Exception("null value");
            else return value;
        }

        public static T? ThrowIfNull<T>(this T? value, string message)
        where T : struct => value.ThrowIfNull(new Exception(message ?? "null value"));

        public static R ThrowIfDefault<R>(this R value, string message = null)
        where R : struct => value.ThrowIfDefault(new Exception(message ?? "Default value"));

        public static R ThrowIfDefault<R>(this R value, Exception ex)
        where R : struct
        {
            if (default(R).Equals(value)) throw ex ?? new Exception("Default Value");
            else return value;
        }

        public static R ThrowIf<R>(this R value, Func<R, bool> predicate, string exceptionMessage = null)
            => value.ThrowIf(predicate, new Exception(exceptionMessage));

        public static R ThrowIf<R>(this R value, Func<R, bool> predicate, Exception ex)
        {
            if (predicate(value)) throw ex ?? new Exception("Invalid internal state");
            else return value;
        }

        public static R ThrowIf<R>(this R value, Func<R, bool> predicate, Func<R, string> exceptionMessage = null)
        => value.ThrowIf(predicate, exceptionMessage?.Invoke(value));

        public static R ThrowIf<R>(this R value, Func<R, bool> predicate, Func<R, Exception> ex)
        => value.ThrowIf(predicate, ex?.Invoke(value));

        public static R ThrowIfFail<R>(Func<R> func, Func<Exception, Exception> exception)
        {
            try
            {
                return func.Invoke();
            }
            catch(Exception e)
            {
                if (exception != null) throw exception(e);
                else throw e;
            }
        }

        public static void ThrowIfFail(Action action, Func<Exception, Exception> exception)
        {
            try
            {
                action();
            }
            catch(Exception e)
            {
                if (exception != null) throw exception(e);
                else throw e;
            }
        }

        public static V ThrowIf<V>(this V test, V compare, string exceptionMessage = null) => test.ThrowIf(compare, new Exception(exceptionMessage));

        public static V ThrowIf<V>(this V test, V compare, Exception ex)
        {
            if (EqualityComparer<V>.Default.Equals(test, compare)) throw ex ?? new Exception($"value is: {compare}");
            else return test;
        }

        public static string FlattenMessage(this Exception e, string separator)
        => e.Enumerate(ex => ResolvedOp.Try(() => ex.InnerException.ThrowIfNull()))
            .Aggregate(new StringBuilder(), (sb, next) => sb.Append(next.Message).Append(separator))
            .ToString();
    }
}
