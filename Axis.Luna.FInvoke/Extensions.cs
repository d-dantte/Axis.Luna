using System;
using System.Reflection;

namespace Axis.Luna.FInvoke
{
    public static class Extensions
    {
        /// <summary>
        /// Constructs an instance invoker for, and invokes the given instance method using the given parameters
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
                FInvoke.InstanceInvoker
                    .InvokerFor(method)
                    .Func(instance, @params);
            }
        }

        /// <summary>
        /// Constructs a static invoker for, and invokes the given static method using the given parameters
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
                FInvoke.StaticInvoker
                    .InvokerFor(method)
                    .Func(@params);
            }
        }

        /// <summary>
        /// Constructs an instance invoker for, and invokes the given instance method using the given parameters
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

            else if (method.ReturnType == typeof(void))
                throw new ArgumentException("Method has no return-type");

            else
            {
                return FInvoke.InstanceInvoker
                    .InvokerFor(method)
                    .Func(instance, @params);
            }
        }

        /// <summary>
        /// Constructs an instance invoker for, and invokes the given instance method using the given parameters
        /// </summary>
        /// <param name="instance">target instance for invocation</param>
        /// <param name="method">target method for invocation</param>
        /// <param name="params">params for the invocation</param>
        /// <returns>The value returned from invoking the target method, cast to the given type</returns>
        public static TResult InvokeFunc<TResult>(this
            object instance,
            MethodInfo method,
            params object[] @params) => (TResult)instance.InvokeFunc(method, @params);

        /// <summary>
        /// Constructs a static invoker for, and invokes the given static method using the given parameters
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

            else if (method.ReturnType == typeof(void))
                throw new ArgumentException("Method has no return-type");

            else
            {
                return FInvoke.StaticInvoker
                    .InvokerFor(method)
                    .Func(@params);
            }
        }

        /// <summary>
        /// Constructs a static invoker for, and invokes the given static method using the given parameters
        /// </summary>
        /// <param name="method">target method for invocation</param>
        /// <param name="params">params for the invocation</param>
        /// <returns>The value returned from invoking the target method, cast to the given type</returns>
        public static TResult InvokeFunc<TResult>(this
            MethodInfo method,
            params object[] @params) => (TResult)method.InvokeFunc(@params);

        /// <summary>
        /// Returns the <see cref="FInvoke.InstanceInvoker"/> instance for the given method.
        /// </summary>
        /// <param name="method">The method to create an instance invoker for</param>
        public static InstanceInvoker InstanceInvoker(this MethodInfo method) => FInvoke.InstanceInvoker.InvokerFor(method);

        /// <summary>
        /// Returns the <see cref="FInvoke.StaticInvoker"/> instance for the given method.
        /// </summary>
        /// <param name="method">The method to create an static invoker for</param>
        public static StaticInvoker StaticInvoker(this MethodInfo method) => FInvoke.StaticInvoker.InvokerFor(method);

        /// <summary>
        /// Checks if this type can be a valid "owner" for a dynamic method.
        /// Links:
        /// <list type="number">
        /// <item><see href="https://github.com/kevin-montrose/Sigil/blob/master/src/Sigil/Emit.cs#L647">sigil code</see></item>
        /// <item><see href="https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.dynamicmethod.-ctor?view=net-6.0#system-reflection-emit-dynamicmethod-ctor(system-string-system-type-system-type()-system-type-system-boolean)">c# DynamicMethod documentation</see></item>
        /// </list>
        /// </summary>
        /// <param name="type">The type to check</param>
        public static bool IsValidDynamicMethodOwner(this Type type)
        {
            return !type.IsArray
                && !type.IsInterface
                && !type.IsGenericTypeParameter
                && !type.IsGenericTypeDefinition;
        }
    }

}
