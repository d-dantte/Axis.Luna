using Axis.Luna.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Axis.Luna.Utils
{
    class DynamicMethodInvoker
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
                        .Cast<Action<object, object[]>>();

                else
                    _funcInvoker = Expression
                        .Lambda(typeof(Func<object, object[], object>), callExp, instanceParam, argsParamExp)
                        .Compile()
                        .Cast<Func<object, object[], object>>();
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
                        .Cast<Action<object[]>>();

                else
                    _staticFuncInvoker = Expression
                        .Lambda(typeof(Func<object[], object>), callExp, argsParamExp)
                        .Compile()
                        .Cast<Func<object[], object>>();
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
}