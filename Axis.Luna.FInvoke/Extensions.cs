using System;
using System.Reflection;

namespace Axis.Luna.FInvoke
{
    public static class Extensions
    {
        /// <summary>
        /// Constructs an invoker for, and invokes the given instance method using the given parameters
        /// </summary>
        /// <param name="instance">target instance for invocation</param>
        /// <param name="method">target method for invocation</param>
        /// <param name="params">params for the invocation</param>
        public static void InvokeAction(this
            object instance,
            MethodInfo method,
            params object[] @params)
        {
            if (method.IsStatic)
                throw new ArgumentException("Method is static");

            else if (method.ReturnType != typeof(void))
                throw new ArgumentException("Method has return-type");

            else
            {
                InstanceInvoker
                    .InvokerFor(method)
                    .Func(instance, @params);
            }
        }

        /// <summary>
        /// Constructs an invoker for, and invokes the given static method using the given parameters
        /// </summary>
        /// <param name="method">target method for invocation</param>
        /// <param name="params">params for the invocation</param>
        public static void InvokeAction(this
            MethodInfo method,
            params object[] @params)
        {
            if (!method.IsStatic)
                throw new ArgumentException("Method is not static");

            else if (method.ReturnType != typeof(void))
                throw new ArgumentException("Method has return-type");

            else
            {
                StaticInvoker
                    .InvokerFor(method)
                    .Func(@params);
            }
        }

        /// <summary>
        /// Constructs an invoker for, and invokes the given instance method using the given parameters
        /// </summary>
        /// <param name="instance">target instance for invocation</param>
        /// <param name="method">target method for invocation</param>
        /// <param name="params">params for the invocation</param>
        /// <returns>The value returned from invoking the target method</returns>
        public static object InvokeFunc(this
            object instance,
            MethodInfo method,
            params object[] @params)
        {
            if (method.IsStatic)
                throw new ArgumentException("Method is static");

            else if (method.ReturnType != typeof(void))
                throw new ArgumentException("Method has return-type");

            else
            {
                return InstanceInvoker
                    .InvokerFor(method)
                    .Func(instance, @params);
            }
        }

        /// <summary>
        /// Constructs an invoker for, and invokes the given instance method using the given parameters
        /// </summary>
        /// <param name="instance">target instance for invocation</param>
        /// <param name="method">target method for invocation</param>
        /// <param name="params">params for the invocation</param>
        /// <returns>The value returned from invoking the target method</returns>
        public static object InvokeFunc<TResult>(this
            object instance,
            MethodInfo method,
            params object[] @params) => (TResult)instance.InvokeFunc(method, @params);

        /// <summary>
        /// Constructs an invoker for, and invokes the given static method using the given parameters
        /// </summary>
        /// <param name="method">target method for invocation</param>
        /// <param name="params">params for the invocation</param>
        /// <returns>The value returned from invoking the target method</returns>
        public static object InvokeFunc(this
            MethodInfo method,
            params object[] @params)
        {
            if (!method.IsStatic)
                throw new ArgumentException("Method is not static");

            else if (method.ReturnType != typeof(void))
                throw new ArgumentException("Method has return-type");

            else
            {
                return StaticInvoker
                    .InvokerFor(method)
                    .Func(@params);
            }
        }

        /// <summary>
        /// Constructs an invoker for, and invokes the given static method using the given parameters
        /// </summary>
        /// <param name="method">target method for invocation</param>
        /// <param name="params">params for the invocation</param>
        /// <returns>The value returned from invoking the target method</returns>
        public static object InvokeFunc<TResult>(this
            MethodInfo method,
            params object[] @params) => (TResult)method.InvokeFunc(@params);
    }

}
