using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Axis.Luna.FInvoke
{
    public static class Extensions
    {
        private static readonly ConcurrentDictionary<MethodInfo, DynamicMethodInvoker> InvokerCache = new ConcurrentDictionary<MethodInfo, DynamicMethodInvoker>();

        /// <summary>
        /// Creates and caches (statically) a dynamic invoker for the given method.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        internal static DynamicMethodInvoker Invoker(this MethodInfo method)
            => InvokerCache.GetOrAdd(method, _method => new DynamicMethodInvoker(_method));        

        internal static object CallStaticInternal(this MethodInfo method, params object[] methodArgs)
        {
            var invoker = method.Invoker();
                        
            if(invoker.IsFunctionInvoker)
                return invoker.InvokeStaticFunc(methodArgs);
            
            else
            {
                invoker.InvokeStaticAction(methodArgs);
                return null;
            }
        }

        internal static object CallInternal(this object instance, MethodInfo method, params object[] methodArgs)
        {
            var invoker = method.Invoker();

            if (invoker.IsFunctionInvoker)
                return invoker.InvokeFunc(instance, methodArgs);

            else
            {
                invoker.InvokeAction(instance, methodArgs);
                return null;
            }
        }

        internal static object ReboxAs<ValueType>(this
            object value)
            where ValueType : struct
            => value.ReboxAs(typeof(ValueType));

        internal static object ReboxAs(this object value, Type valueType)
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
        internal static object[] NormalizeArguments(this MethodInfo method, object[] argValues)
        {
            var argTypes = method.GetParameters()
                .Select(_p => _p.ParameterType)
                .ToArray();

            if (argTypes.Length != argValues.Length) 
                throw new Exception("Invalid Argument Count");

            for (int cnt = 0; cnt < argTypes.Length; cnt++)
            {
                if (!argTypes[cnt].IsValueType) continue;
                else if (argTypes[cnt] == argValues[cnt].GetType()) continue;
                else argValues[cnt] = argValues[cnt].ReboxAs(argTypes[cnt]);
            }

            return argValues;
        }


        public static object InvokeStatic(this
            MethodInfo method,
            params object[] methodArgs)
            => method.CallStaticInternal(method.NormalizeArguments(methodArgs));

        public static object Invoke(this
            object instance,
            MethodInfo method,
            params object[] methodArgs)
            //=> instance.CallInternal(method, method.NormalizeArguments(methodArgs));
            => instance.CallInternal(method, methodArgs);
    }

}
