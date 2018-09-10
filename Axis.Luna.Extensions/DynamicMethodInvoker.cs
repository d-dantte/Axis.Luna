using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Axis.Luna.Extensions
{

    public class DynamicMethodInvoker
    {
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
                var cnt = 0;
                var callExp = Expression
                    .Call(tcastExp,
                          method,
                          mparams.Select(_p =>
                          {
                              var valueExp = Expression.ArrayIndex(argsParamExp, Expression.Constant(cnt++));
                              return _p.ParameterType.IsValueType ?
                                     Expression.Unbox(valueExp, _p.ParameterType) :
                                     Expression.Convert(valueExp, _p.ParameterType);
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
                var cnt = 0;
                var callExp = Expression
                    .Call(method,
                          mparams.Select(_p =>
                          {
                              var valueExp = Expression.ArrayIndex(argsParamExp, Expression.Constant(cnt++));
                              return _p.ParameterType.IsValueType ?
                                     Expression.Unbox(valueExp, _p.ParameterType) :
                                     Expression.Convert(valueExp, _p.ParameterType);
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

        public object InvokeFunc(object instance, params object[] @params) => _funcInvoker(instance, @params);
        public R InvokeFunc<R>(object instance, params object[] @params) => (R)InvokeFunc(instance, @params);

        public object InvokeStaticFunc(params object[] @params) => _staticFuncInvoker(@params);
        public R InvokeStaticFunc<R>(params object[] @params) => (R)InvokeStaticFunc(@params);

        public void InvokeAction(object instance, params object[] @params) => _actionInvoker(instance, @params);
        public void InvokeStaticAction(params object[] @params) => _staticActionInvoker(@params);
    }

    public static class DynamicInvokerHelper
    {
        private static ConcurrentDictionary<MethodInfo, DynamicMethodInvoker> _InvokerCache = new ConcurrentDictionary<MethodInfo, DynamicMethodInvoker>();

        /// <summary>
        /// Creates and caches (statically) a dynamic invoker for the given method.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static DynamicMethodInvoker Invoker(this MethodInfo method) => _InvokerCache.GetOrAdd(method, _method => new DynamicMethodInvoker(_method));

        public static object CallFunc(this object instance, MethodInfo method,params object[] args)
        {
            var invoker = _InvokerCache.GetOrAdd(method.ThrowIfNull("Invalid method"), _m => new DynamicMethodInvoker(method));

            return invoker.InvokeFunc(instance, args);
        }
        public static object CallStaticFunc(this MethodInfo method, params object[] args)
        {
            var invoker = _InvokerCache.GetOrAdd(method.ThrowIfNull("Invalid method"), _m => new DynamicMethodInvoker(method));

            return invoker.InvokeStaticFunc(args);
        }

        public static T CallFunc<T>(this object instance, MethodInfo method, params object[] args)
        {
            var invoker = _InvokerCache.GetOrAdd(method.ThrowIfNull("Invalid method"), _m => new DynamicMethodInvoker(method));

            return invoker.InvokeFunc<T>(instance, args);
        }
        public static T CallStaticFunc<T>(this MethodInfo method, params object[] args)
        {
            var invoker = _InvokerCache.GetOrAdd(method.ThrowIfNull("Invalid method"), _m => new DynamicMethodInvoker(method));

            return invoker.InvokeStaticFunc<T>(args);
        }

        public static void CallAction(this object instance, MethodInfo method, params object[] args)
        {
            var invoker = _InvokerCache.GetOrAdd(method.ThrowIfNull("Invalid method"), _m => new DynamicMethodInvoker(method));

            invoker.InvokeAction(instance, args);
        }
        public static void CallStaticAction(this MethodInfo method, params object[] args)
        {
            var invoker = _InvokerCache.GetOrAdd(method.ThrowIfNull("Invalid method"), _m => new DynamicMethodInvoker(method));

            invoker.InvokeStaticAction(args);
        }


        public static object ReboxAs<ValueType>(this object value) where ValueType : struct => value.ReboxAs(typeof(ValueType));
        public static object ReboxAs(this object value, Type valueType)
        {
            if (!valueType.IsValueType) throw new Exception("the given value must be reboxed into a value-type");
            if (!value.GetType().IsValueType) throw new Exception("the given value must be a value-type");

            return Convert.ChangeType(value, valueType);
        }


        /// <summary>
        /// Ensures that (for now) value-types are boxed in the exact type that the method is expecting.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="argValues"></param>
        /// <returns></returns>
        public static object[] NormalizeArguments(this MethodInfo method, object[] argValues)
        {
            var argTypes = method.GetParameters().Select(_p => _p.ParameterType).ToArray();
            if (argTypes.Length != argValues.Length) throw new Exception("Invalid Argument Count");
            for (int cnt = 0; cnt < argTypes.Length; cnt++)
            {
                if (!argTypes[cnt].IsValueType) continue;
                else if (argTypes[cnt] == argValues[cnt].GetType()) continue;
                else argValues[cnt] = argValues[cnt].ReboxAs(argTypes[cnt]);
            }

            return argValues;
        }

        public static object CallNormalizedFunc(this object instance, MethodInfo method, params object[] methodArgs) => instance.CallFunc(method, method.NormalizeArguments(methodArgs));
        public static object CallStaticNormalizedFunc(this MethodInfo method, params object[] methodArgs) => method.CallStaticFunc(method.NormalizeArguments(methodArgs));

        public static T CallNormalizedFunc<T>(this object instance, MethodInfo method, params object[] methodArgs) => instance.CallFunc<T>(method, method.NormalizeArguments(methodArgs));
        public static T CallStaticNormalizedFunc<T>(this MethodInfo method, params object[] methodArgs) => method.CallStaticFunc<T>(method.NormalizeArguments(methodArgs));

        public static void CallNormalizedAction(this object instance, MethodInfo method, params object[] methodArgs) => instance.CallAction(method, method.NormalizeArguments(methodArgs));
        public static void CallStaticNormalizedAction(this MethodInfo method, params object[] methodArgs) => method.CallStaticAction(method.NormalizeArguments(methodArgs));
    }
}
