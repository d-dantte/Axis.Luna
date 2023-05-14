using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Axis.Luna.Common
{
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="exception"></param>
        /// <param name="stackTrace"></param>
        /// <returns></returns>
        public static TException OverwriteStackTrace<TException>(this TException exception, StackTrace stackTrace)
        where TException : Exception => (TException)StackTraceSetter.Invoke(exception, stackTrace);

        #region Helpers
        private static readonly Func<Exception, StackTrace, Exception> StackTraceSetter = CreateStackTraceSetter();
        private static Func<Exception, StackTrace, Exception> CreateStackTraceSetter()
        {
            var target = Expression.Parameter(typeof(Exception));
            var stack = Expression.Parameter(typeof(StackTrace));

            var traceFormatType = typeof(StackTrace).GetNestedType("TraceFormat", BindingFlags.NonPublic);
            var toString = typeof(StackTrace).GetMethod("ToString", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { traceFormatType }, null);
            object normalTraceFormat = Enum.GetValues(traceFormatType).GetValue(0);

            MethodCallExpression stackTraceString = Expression.Call(stack, toString, Expression.Constant(normalTraceFormat, traceFormatType));
            FieldInfo stackTraceStringField = typeof(Exception).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance);
            BinaryExpression assign = Expression.Assign(Expression.Field(target, stackTraceStringField), stackTraceString);

            return Expression.Lambda<Func<Exception, StackTrace, Exception>>(Expression.Block(assign, target), target, stack).Compile();
        }
        #endregion
    }
}