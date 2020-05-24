using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;


namespace Axis.Luna.FInvoke
{
    internal class DynamicMethodInvoker
    {
        #region Properties and locals
        private Func<object, object[], object> _funcInvoker { get; set; }
        private Action<object, object[]> _actionInvoker { get; set; }

        private Func<object[], object> _staticFuncInvoker { get; set; }
        private Action<object[]> _staticActionInvoker { get; set; }

        internal MethodInfo TargetMethod { get; private set; }
        internal bool IsFunctionInvoker => TargetMethod.ReturnType != typeof(void);
        internal bool IsActionInvoker => TargetMethod.ReturnType == typeof(void);
        #endregion

        internal DynamicMethodInvoker(MethodInfo method)
        {
            TargetMethod = method ?? throw new Exception("Invalid method");

            var mparams = TargetMethod.GetParameters();
            var argsParamExp = Expression.Parameter(typeof(object[]), "args");

            #region Instance
            if (!TargetMethod.IsStatic)
            {
                var instanceParam = Expression.Parameter(typeof(object), "instance");

                //target Cast: (TargetClass)instance
                var tcastExp = Expression.Convert(instanceParam, TargetMethod.DeclaringType);

                //invoke action: ((TargetClass)instance).Method((T0)arg[0], (T1)arg[1], ...)
                var cnt = 0;
                var callExp = Expression
                    .Call(tcastExp,
                          TargetMethod,
                          mparams.Select(_p =>
                          {
                              var valueExp = Expression.ArrayIndex(argsParamExp, Expression.Constant(cnt++));
                              return _p.ParameterType.IsValueType ?
                                     Expression.Unbox(valueExp, _p.ParameterType) :
                                     Expression.Convert(valueExp, _p.ParameterType);
                          }));

                //lambda exp:
                if (TargetMethod.ReturnType == typeof(void))
                    _actionInvoker = Expression
                       .Lambda(typeof(Action<object, object[]>), callExp, instanceParam, argsParamExp)
                       .Compile() as Action<object, object[]>;

                else
                {
                    var lambdaExp = !TargetMethod.ReturnType.IsValueType ?
                                    callExp as Expression :
                                    Expression.Convert(callExp, typeof(object));

                    _funcInvoker = Expression
                        .Lambda(typeof(Func<object, object[], object>), lambdaExp, instanceParam, argsParamExp)
                        .Compile() as Func<object, object[], object>;
                }
            }
            #endregion

            #region Static
            else //static
            {
                //invoke action: Type.Method((T0)arg[0], (T1)arg[1], ...)
                var cnt = 0;
                var callExp = Expression
                    .Call(TargetMethod,
                          mparams.Select(_p =>
                          {
                              var valueExp = Expression.ArrayIndex(argsParamExp, Expression.Constant(cnt++));
                              return _p.ParameterType.IsValueType ?
                                     Expression.Unbox(valueExp, _p.ParameterType) :
                                     Expression.Convert(valueExp, _p.ParameterType);
                          }));

                //lambda exp:
                if (TargetMethod.ReturnType == typeof(void))
                    _staticActionInvoker = Expression
                       .Lambda(typeof(Action<object[]>), callExp, argsParamExp)
                       .Compile() as Action<object[]>;

                else
                {
                    var lambdaExp = !TargetMethod.ReturnType.IsValueType ?
                                    callExp as Expression :
                                    Expression.Convert(callExp, typeof(object));

                    _staticFuncInvoker = Expression
                        .Lambda(typeof(Func<object[], object>), lambdaExp, argsParamExp)
                        .Compile() as Func<object[], object>;
                }
            }
            #endregion
        }

        #region Methods
        internal object InvokeFunc(object instance, params object[] @params) => _funcInvoker(instance, @params);
        internal R InvokeFunc<R>(object instance, params object[] @params) => (R)InvokeFunc(instance, @params);

        internal object InvokeStaticFunc(params object[] @params) => _staticFuncInvoker(@params);
        internal R InvokeStaticFunc<R>(params object[] @params) => (R)InvokeStaticFunc(@params);

        internal void InvokeAction(object instance, params object[] @params) => _actionInvoker(instance, @params);

        internal void InvokeStaticAction(params object[] @params) => _staticActionInvoker(@params);
        #endregion
    }
}
