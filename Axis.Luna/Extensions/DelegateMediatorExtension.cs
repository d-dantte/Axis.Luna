using static Axis.Luna.Extensions.ObjectExtensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Axis.Luna.Extensions
{

    public static class DelegateMediatorExtensions
    {
        public static object TrueTarget(this Delegate del)
        {
            //if (Eval(() => del.Target as Delegate) != null) return (del.Target as Delegate).TrueTarget();
            if (del.Target.Is<Delegate>()) return (del.Target as Delegate).TrueTarget();
            else return del.Target;
        }
        public static MethodInfo TrueMethod(this Delegate del)
        {
            //if (Eval(() => del.Target as Delegate) != null) return (del.Target as Delegate).TrueMethod();
            if (del.Target.Is<Delegate>()) return (del.Target as Delegate).TrueMethod();
            else return del.Method;
        }

        public static MethodInfo DelegateSignature(this Type delegateType)
            => delegateType.GetRuntimeMethods().FirstOrDefault(m => m.Name == "Invoke");

        public static CallbackDelegate ManagedCallback<CallbackDelegate>(CallbackDelegate target, Action<CallbackDelegate> disposer)
        where CallbackDelegate : class => ManagedCallback(CallbackLifetime.SourceDependent, target, disposer);

        public static CallbackDelegate ManagedCallback<CallbackDelegate>(CallbackLifetime lifetime,
                                                                         CallbackDelegate target,
                                                                         Action<CallbackDelegate> disposer)
        where CallbackDelegate : class
        {
            var mediatorGenerator = MediatorFor<CallbackDelegate>();
            var mediator = mediatorGenerator.Invoke();
            mediator.Init(target, disposer, lifetime);
            return mediator.GeneratedDelegate;
        }
        public static CallbackDelegate RemoveManagedCallback<CallbackDelegate>(CallbackDelegate source, CallbackDelegate target)
        where CallbackDelegate : class
        {
            var method = (target as Delegate).TrueMethod();

            var toRemove = (source as Delegate).GetInvocationList()
                                               .Where(d => d.TrueTarget() is DelegateMediator<CallbackDelegate>)
                                               .Select(d => new { Target = d.TrueTarget() as DelegateMediator<CallbackDelegate>, Delegate = d })
                                               .FirstOrDefault(d => d.Target.HandlerMethod == method);

            if (toRemove != null) return Delegate.Remove(source as Delegate, toRemove.Delegate as Delegate) as CallbackDelegate;
            else return source;
        }

        public static bool IsManaged(this Delegate del)
            => del?.TrueTarget()?.GetType().Name.StartsWith(DynamicDelegateTypeNamePrefix) ?? false;

        private static string DynamicDelegateAssemblyNamePrefix = "__AxisDynAssembly_";
        private static string DynamicDelegateTypeNamePrefix = "__AxisDynType_";
        private static AssemblyBuilder _asmBuilder =
            AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName($"{DynamicDelegateAssemblyNamePrefix}{Guid.NewGuid().ToString().Replace("-", "_")}"),
                                                          AssemblyBuilderAccess.Run);
        private static ModuleBuilder _moduleBuilder = _asmBuilder.DefineDynamicModule("exec");

        internal static Func<DelegateMediator<CallbackDelegate>> MediatorFor<CallbackDelegate>()
        where CallbackDelegate : class
        {
            return _ProxyCache.GetOrAdd(typeof(CallbackDelegate), cbdt =>
            {
                #region public class __AxisDynType_DelegateMediator[*]{...}
                var cbdSignature = cbdt.DelegateSignature();
                var cbdParams = cbdSignature.GetParameters();
                DelegateMediator<CallbackDelegate> _this = null;
                var dmType = typeof(DelegateMediator<CallbackDelegate>);
                var callbackMinfo = dmType.GetRuntimeMethod(nameof(_this.Callback), new[] { typeof(object[]) });
                var tbuilder = _moduleBuilder.DefineType($"{DynamicDelegateTypeNamePrefix}DelegateMediator_{Guid.NewGuid().ToString().Replace("-", "_")}",
                                                         TypeAttributes.Public |
                                                         TypeAttributes.Class |
                                                         TypeAttributes.AutoClass |
                                                         TypeAttributes.AnsiClass |
                                                         TypeAttributes.AutoLayout,
                                                         dmType);
                #endregion

                #region public <Type> invokeCallBack(<Args>){...}
                var icbBuilder = tbuilder.DefineMethod("invokeCallback",
                                                       MethodAttributes.Public |
                                                       MethodAttributes.HideBySig |
                                                       MethodAttributes.Virtual,
                                                       cbdSignature.ReturnType,
                                                       cbdParams.Select(p => p.ParameterType).ToArray());
                var il = icbBuilder.GetILGenerator();
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, cbdParams.Count());
                il.Emit(OpCodes.Newarr, typeof(object));
                cbdParams.ForAll((cnt, p) =>
                {
                    il.Emit(OpCodes.Dup); //duplicate the array on the stack
                    il.Emit(OpCodes.Ldc_I4, cnt); //copy the array index to the stack
                    il.Emit(OpCodes.Ldarg, cnt + 1); //copy the methods corresponding argument to the stack
                    if (p.ParameterType.IsValueType) il.Emit(OpCodes.Box, p.ParameterType); //box it if it is a value type since this is an object arrays
                    il.Emit(OpCodes.Stelem_Ref); //assign it to the array. not that this pops the array off the stack along with the arguments
                });
                il.Emit(OpCodes.Call, callbackMinfo); //invoke callback(array);

                if (cbdSignature.ReturnType == typeof(void)) il.Emit(OpCodes.Pop); //remove the "this" pointer from the stack
                else
                {
                    il.Emit(OpCodes.Isinst, cbdSignature.ReturnType);
                    il.Emit(OpCodes.Unbox_Any, cbdSignature.ReturnType);
                }
                il.Emit(OpCodes.Ret);

                #endregion

                #region public override CallbackDelegate newCallbackDelegate() => new CallbackDelegate(_this.invokeCallback);
                var delCtor = cbdt.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                var ncbdBuilder = tbuilder.DefineMethod(nameof(_this.NewCallbackDelegate),
                                                        MethodAttributes.Public |
                                                        MethodAttributes.HideBySig |
                                                        MethodAttributes.Virtual,
                                                        cbdt,
                                                        Type.EmptyTypes);

                il = ncbdBuilder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldftn, icbBuilder);
                il.Emit(OpCodes.Newobj, delCtor);
                il.Emit(OpCodes.Ret);
                #endregion

                var type = tbuilder.CreateType();
                Func<DelegateMediator<CallbackDelegate>> creator = () => Activator.CreateInstance(type) as DelegateMediator<CallbackDelegate>;
                return creator;
            })
            as Func<DelegateMediator<CallbackDelegate>>;
        }

        private static Dictionary<Type, Delegate> _ProxyCache = new Dictionary<Type, Delegate>();
    }
}
