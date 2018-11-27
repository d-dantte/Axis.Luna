using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using static Axis.Luna.Extensions.Common;

namespace Axis.Luna.Extensions
{

    public class DynamicMethodInvoker
    {
        private static readonly ConcurrentDictionary<SanitizerKey, Delegate> _SanitizerMaps = new ConcurrentDictionary<SanitizerKey, Delegate>();


        private Func<object, object[], object> _funcInvoker { get; set; }
        private Action<object, object[]> _actionInvoker { get; set; }

        private Func<object[], object> _staticFuncInvoker { get; set; }
        private Action<object[]> _staticActionInvoker { get; set; }


        public MethodInfo TargetMethod { get; private set; }
        public bool IsFunctionInvoker => TargetMethod.ReturnType != typeof(void);
        public bool IsActionInvoker => TargetMethod.ReturnType == typeof(void);


        public DynamicMethodInvoker(MethodInfo method)
        {
            TargetMethod = method.ThrowIfNull("Invalid method");

            #region Instance
            if (!method.IsStatic)
            {
                var mparams = method.GetParameters();

                var instanceParam = Expression.Parameter(typeof(object), "instance");
                var argsParamExp = Expression.Parameter(typeof(object[]), "args");

                //target Cast: (TargetClass)instance
                var tcastExp = Expression.Convert(instanceParam, method.DeclaringType);

                //invoke action: ((TargetClass)instance).Method((T0)arg[0], (T1)arg[1], ...)
                var sanitizeParameterMethod = GetType().GetMethod(nameof(SanitizeParameter), BindingFlags.NonPublic|BindingFlags.Static) ?? throw new Exception("Sanitizer method not found");
                var cnt = 0;
                var callExp = Expression
                    .Call(tcastExp,
                          method,
                          mparams.Select(_p =>
                          {
                              var valueExp = Expression.ArrayIndex(argsParamExp, Expression.Constant(cnt++));

                              //return _p.ParameterType.IsValueType ?
                              //    Expression.Unbox(valueExp, _p.ParameterType) :
                              //    Expression.Convert(valueExp, _p.ParameterType);

                              var sanitizer = sanitizeParameterMethod.MakeGenericMethod(_p.ParameterType);
                              return Expression.Call(null, sanitizer, valueExp);
                          }));

                //lambda exp:
                if (method.ReturnType == typeof(void))
                    _actionInvoker = Expression
                        .Lambda(typeof(Action<object, object[]>), callExp, instanceParam, argsParamExp)
                        .Compile()
                        .As<Action<object, object[]>>();

                else
                {
                    Expression lambdaExpression = method.ReturnType == typeof(object) ?
                                                  callExp as Expression :
                                                  Expression.Convert(callExp, typeof(object));
                    _funcInvoker = Expression
                        .Lambda(typeof(Func<object, object[], object>), lambdaExpression, instanceParam, argsParamExp)
                        .Compile()
                        .As<Func<object, object[], object>>();
                }
            }
            #endregion

            #region Static
            else //static
            {
                var mparams = method.GetParameters();

                var argsParamExp = Expression.Parameter(typeof(object[]), "args");

                //invoke action: Method((T0)arg[0], (T1)arg[1], ...)
                var sanitizeParameterMethod = GetType().GetMethod(nameof(SanitizeParameter), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new Exception("Sanitizer method not found");
                var cnt = 0;
                var callExp = Expression
                    .Call(method,
                          mparams.Select(_p =>
                          {
                              var valueExp = Expression.ArrayIndex(argsParamExp, Expression.Constant(cnt++));

                              //return _p.ParameterType.IsValueType ?
                              //       Expression.Unbox(valueExp, _p.ParameterType) :
                              //       Expression.Convert(valueExp, _p.ParameterType);

                              var sanitizer = sanitizeParameterMethod.MakeGenericMethod(_p.ParameterType);
                              return Expression.Call(null, sanitizer, valueExp);
                          }));

                //lambda exp:
                if (method.ReturnType == typeof(void))
                    _staticActionInvoker = Expression
                        .Lambda(typeof(Action<object[]>), callExp, argsParamExp)
                        .Compile()
                        .As<Action<object[]>>();

                else
                {
                    Expression lambdaExpression = method.ReturnType == typeof(object) ?
                                                  callExp as Expression :
                                                  Expression.Convert(callExp, typeof(object));
                    _staticFuncInvoker = Expression
                        .Lambda(typeof(Func<object[], object>), lambdaExpression, argsParamExp)
                        .Compile()
                        .As<Func<object[], object>>();
                }
            }
            #endregion
        }


        private static To SanitizeParameter<To>(object arg)
        {
            if (arg == null) return default(To);

            var toType = typeof(To);
            if (!arg.GetType().IsValueType)
                return (To) arg;

            else
            {
                var fromType = arg.GetType();

                //first, unbox arg to it's natural type
                var del = (Func<object, To>) _SanitizerMaps.GetOrAdd(new SanitizerKey(fromType, toType), _ =>
                {
                    var argParam = Expression.Parameter(typeof(object), "arg");
                    var unboxed = Expression.Unbox(argParam, fromType);
                    var converted = Expression.Convert(unboxed, toType);
                    return Expression
                        .Lambda(typeof(Func<object, To>), converted, argParam)
                        .Compile();
                });
                return del.Invoke(arg);
            }
        }

        public object InvokeFunc(object instance, params object[] @params) => _funcInvoker(instance, @params);
        public R InvokeFunc<R>(object instance, params object[] @params) => (R)InvokeFunc(instance, @params);

        public object InvokeStaticFunc(params object[] @params) => _staticFuncInvoker(@params);
        public R InvokeStaticFunc<R>(params object[] @params) => (R)InvokeStaticFunc(@params);

        public void InvokeAction(object instance, params object[] @params) => _actionInvoker(instance, @params);
        public void InvokeStaticAction(params object[] @params) => _staticActionInvoker(@params);


        internal class SanitizerKey
        {
            internal Type From { get; }
            internal Type To { get; }

            public SanitizerKey(Type from, Type to)
            {
                From = from ?? throw new Exception("Invalid From-Type: null");
                To = to ?? throw new Exception("Invalid To-Type: null");
            }

            public override int GetHashCode() => ValueHash(From, To);

            public override bool Equals(object other)
            => other is SanitizerKey sanitizerKey
               && sanitizerKey.From == From
               && sanitizerKey.To == To;
        }
    }

    public static class DynamicInvokerHelper
    {
        private static readonly ConcurrentDictionary<MethodInfo, DynamicMethodInvoker> _InvokerMaps = new ConcurrentDictionary<MethodInfo, DynamicMethodInvoker>();

        /// <summary>
        /// Creates and caches (statically) a dynamic invoker for the given method.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static DynamicMethodInvoker Invoker(this MethodInfo method) => _InvokerMaps.GetOrAdd(method, _method => new DynamicMethodInvoker(_method));

        public static object CallFunc(this object instance, MethodInfo method,params object[] args)
        {
            var invoker = _InvokerMaps.GetOrAdd(method.ThrowIfNull("Invalid method"), _m => new DynamicMethodInvoker(method));

            return invoker.InvokeFunc(instance, args);
        }
        public static object CallStaticFunc(this MethodInfo method, params object[] args)
        {
            var invoker = _InvokerMaps.GetOrAdd(method.ThrowIfNull("Invalid method"), _m => new DynamicMethodInvoker(method));

            return invoker.InvokeStaticFunc(args);
        }

        public static T CallFunc<T>(this object instance, MethodInfo method, params object[] args)
        {
            var invoker = _InvokerMaps.GetOrAdd(method.ThrowIfNull("Invalid method"), _m => new DynamicMethodInvoker(method));

            return invoker.InvokeFunc<T>(instance, args);
        }
        public static T CallStaticFunc<T>(this MethodInfo method, params object[] args)
        {
            var invoker = _InvokerMaps.GetOrAdd(method.ThrowIfNull("Invalid method"), _m => new DynamicMethodInvoker(method));

            return invoker.InvokeStaticFunc<T>(args);
        }

        public static void CallAction(this object instance, MethodInfo method, params object[] args)
        {
            var invoker = _InvokerMaps.GetOrAdd(method.ThrowIfNull("Invalid method"), _m => new DynamicMethodInvoker(method));

            invoker.InvokeAction(instance, args);
        }
        public static void CallStaticAction(this MethodInfo method, params object[] args)
        {
            var invoker = _InvokerMaps.GetOrAdd(method.ThrowIfNull("Invalid method"), _m => new DynamicMethodInvoker(method));

            invoker.InvokeStaticAction(args);
        }
    }
}
