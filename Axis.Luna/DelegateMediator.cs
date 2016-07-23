using static Axis.Luna.Extensions.ExceptionExtensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Axis.Luna.Extensions;

namespace Axis.Luna
{
    public enum CallbackLifetime { SourceDependent, SourceIndependent }

    public abstract class DelegateMediator<CallbackDelegate>
    where CallbackDelegate : class
    {
        private static Dictionary<string, DynamicMethodDelegate> _TargetDelegates = new Dictionary<string, DynamicMethodDelegate>();
        //private static Dictionary<string, Delegate> _SourceDelegates = new Dictionary<string, Delegate>();

        internal CallbackLifetime HandlerLifetime { get; private set; } = CallbackLifetime.SourceIndependent;
        internal WeakReference WeakTarget { get; private set; }
        internal object StrongTarget { get; private set; }
        internal MethodInfo HandlerMethod { get; private set; }
        internal DynamicMethodDelegate HandlerDelegate { get; private set; }

        internal CallbackDelegate GeneratedDelegate { get; private set; }
        internal Action<CallbackDelegate> Disposer { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="delegateDisposer"></param>
        /// <param name="lifetime"></param>
        internal void Init(CallbackDelegate target, Action<CallbackDelegate> disposer, CallbackLifetime lifetime = CallbackLifetime.SourceDependent)
        {
            ThrowNullArguments(() => target, () => disposer);

            this.Disposer = disposer;
            HandlerLifetime = lifetime;
            HandlerMethod = (target as Delegate).TrueMethod().ThrowIfNull();
            HandlerDelegate = _TargetDelegates.GetOrAdd(ToMethodIdentifier(this.HandlerMethod),
                                                        mid => this.HandlerMethod.CreateDynamicDelegate());
            if (!HandlerMethod.IsStatic)
                switch (lifetime)
                {
                    case CallbackLifetime.SourceDependent: StrongTarget = (target as Delegate).TrueTarget(); break;
                    case CallbackLifetime.SourceIndependent: WeakTarget = new WeakReference((target as Delegate).TrueTarget()); break;
                }

            GeneratedDelegate = GenerateDelegate();
        }

        public object Callback(object[] args)
        {
            if (!HandlerMethod.IsStatic)
            {
                object t = null;
                t = (HandlerLifetime == CallbackLifetime.SourceIndependent) ? WeakTarget.Target : StrongTarget;
                if (t == null)
                {
                    Disposer.Invoke(GeneratedDelegate);
                    return null;
                }
                else return HandlerDelegate.Invoke(t, args);
            }
            else return HandlerDelegate.Invoke(null, args);
        }

        private string ToMethodIdentifier(MethodInfo minfo)
        {
            if (minfo.IsGenericMethodDefinition) throw new ArgumentException("method cannot be a generic method definition");
            var mparams = minfo.GetParameters()
                               .Select(p => $"{{{p.ParameterType.MinimalAQName()}}}")
                               .Aggregate("", (x, p) => $"{(x.Length > 0 ? "," : "")} {p}");
            return $"{minfo.ReturnType}  {minfo.DeclaringType.MinimalAQName()}/{minfo.Name}({mparams})";
        }
        private string ToDelegateIdentifier(Type delType)
            => ToMethodIdentifier(delType.GetRuntimeMethods().FirstOrDefault(m => m.Name == "Invoke"));

        private CallbackDelegate GenerateDelegate() => NewCallbackDelegate();
        //=> _SourceDelegates.GetOrAdd(toDelegateIdentifier(typeof(CallbackDelegate)), id => newCallbackDelegate() as Delegate) as CallbackDelegate;

        #region Generated in proxy
        public abstract CallbackDelegate NewCallbackDelegate();
        #endregion

    }
}
