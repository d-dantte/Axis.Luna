using Sigil;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace Axis.Luna.FInvoke
{
    public class InstanceInvoker
    {

        private static readonly string InstanceInvoker__NamePrefix = "NInvoker_";

        private static readonly ConcurrentDictionary<MethodInfo, InstanceInvoker> _invokerCache = new ConcurrentDictionary<MethodInfo, InstanceInvoker>();

        /// <summary>
        /// Delegate for invoking the underlying method dynamically
        /// </summary>
        public Func<object, object[], object> Func { get; }

        /// <summary>
        /// Invokes the function encapsulated by this invoker.
        /// <para>
        /// NOTE: when dynamically calling value-type methods, care should be taken because the value-type is boxed and passed into this API via the <paramref name="this"/> arg.
        /// The peculiarities of value-types dictate that if the method mutates the state of the value-type, the new state may be lost if the caller doesn't have access to the boxed reference.
        /// <para/>
        /// E.g
        /// <code>
        ///     SomeStruct @struct = new SomeStruct();
        ///     object boxed = @struct;
        ///     
        ///     invoker.Invoke(@struct, arg1, arg2...); // mutation is lost because boxing happens at the call site
        ///     invoker.Invoke(boxed, arg1, arg2...); // mutation is not lost because the caller has access to the boxed reference.
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="this">the instance as a ref, mainly because for <see cref="ValueType"/>s, instances are passed by value, and so any local mutations are lost on the return trip</param>
        /// <param name="arguments">the method arguments</param>
        /// <returns>the return value if any</returns>
        public object Invoke(object @this, params object[] arguments) => Func.Invoke(@this, arguments);

        /// <summary>
        /// Invokes the function with no arguments
        /// <para>
        /// NOTE: when dynamically calling value-type methods, care should be taken because the value-type is boxed and passed into this API via the <paramref name="this"/> arg.
        /// The peculiarities of value-types dictate that if the method mutates the state of the value-type, the new state may be lost if the caller doesn't have access to the boxed reference.
        /// <para/>
        /// E.g
        /// <code>
        ///     SomeStruct @struct = new SomeStruct();
        ///     object boxed = @struct;
        ///     
        ///     invoker.Invoke(@struct, arg1, arg2...); // mutation is lost because boxing happens at the call site
        ///     invoker.Invoke(boxed, arg1, arg2...); // mutation is not lost because the caller has access to the boxed reference.
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="this">The instance</param>
        /// <returns>the result value if any</returns>
        public object Invoke(object @this) => Invoke(@this, Array.Empty<object>());

        /// <summary>
        /// Creates or retrieves a new instance of the invoker. Generic-Definition methods are not accepted.
        /// </summary>
        /// <param name="method">Method to be invoked</param>
        /// <returns>The invoker instance</returns>
        public static InstanceInvoker InvokerFor(MethodInfo method)
        {
            if (method.IsGenericMethodDefinition)
                throw new ArgumentException("Cannot create an Invoker from a generic method definition");

            else if (method.DeclaringType == null)
                throw new ArgumentException($"Cannot create an Invoker for methods without declaring types");

            else if (method.IsStatic)
                throw new ArgumentException("Cannot create an Invoker for a static method");

            else
                return _invokerCache.GetOrAdd(method, _method => new InstanceInvoker(_method));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="method">Target method</param>
        private InstanceInvoker(MethodInfo method)
        {
            Func = InitInstance(method);
        }

        private static Func<object, object[], object> InitInstance(MethodInfo method)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));

            if (method.IsAbstract)
                throw new ArgumentException($"Cannot create Invoker for abstract method: {method}");

            var guid = Guid
                .NewGuid()
                .ToString()
                .Replace("-", "_");

            var dynamicMethod = new DynamicMethod(
                name: $"{InstanceInvoker__NamePrefix}_{guid}",
                returnType: typeof(object),
                parameterTypes: new[] {typeof(object), typeof(object[]) },
                m: typeof(InstanceInvoker).Module);

            var emitter = dynamicMethod.GetILGenerator();

            // declare a local variable to store the unboxed 'this' instance
            if (method.DeclaringType.IsValueType)
                _ = emitter.DeclareLocal(method.DeclaringType);

            // push 'this' unto the stack - for value-types, this is a boxed value
            emitter.Emit(OpCodes.Ldarg_0);

            // cast/unbox 'this' from object to appropariate type.
            if (method.DeclaringType.IsValueType)
            {
                emitter.Emit(OpCodes.Unbox_Any, method.DeclaringType);
                emitter.Emit(OpCodes.Stloc_0);
                emitter.Emit(OpCodes.Ldloca, 0);
            }
            else
            {
                emitter.Emit(OpCodes.Castclass, method.DeclaringType);
            }

            // push arguments unto the stack
            var arguments = method.GetParameters() ?? Array.Empty<ParameterInfo>();
            for (ushort cnt = 0; cnt < arguments.Length; cnt++)
            {
                //load the meta-arg array into memory
                emitter.Emit(OpCodes.Ldarg_1);
                emitter.Emit(OpCodes.Ldc_I4, cnt);

                if (arguments[cnt].ParameterType.IsValueType)
                    LoadBoxedValueType(emitter, arguments[cnt].ParameterType);

                else // if(!arguments[cnt].IsValueType)
                    LoadCastedRefType(emitter, arguments[cnt].ParameterType);
            }

            // call the method
            if (method.DeclaringType.IsValueType)
            {
                emitter.Emit(OpCodes.Call, method);

                // for value-types, we need to copy the locally made value back into the box-address (yes, this is possible!!!)
                // see here: https://stackoverflow.com/questions/44724042/how-to-mutate-a-boxed-value-type-primitive-or-struct-in-c-il
                emitter.Emit(OpCodes.Ldarg_0);
                emitter.Emit(OpCodes.Unbox, method.DeclaringType);
                emitter.Emit(OpCodes.Ldloc_0);
                emitter.Emit(OpCodes.Stobj, method.DeclaringType);
            }
            else
            {
                emitter.Emit(OpCodes.Callvirt, method);
            }

            // return value
            if (method.ReturnType == typeof(void))
                emitter.Emit(OpCodes.Ldnull);

            else if (method.ReturnType.IsValueType)
                emitter.Emit(OpCodes.Box, method.ReturnType);

            emitter.Emit(OpCodes.Ret);

            return dynamicMethod.CreateDelegate<Func<object, object[], object>>();
        }

        /// <summary>
        /// Loads from the meta-arg array, the specified element, and unboxes it.
        /// Note that this method assumes that the meta-arg array is already on the stack
        /// </summary>
        /// <param name="emitter">The sigil emit type used to construct the method</param>
        /// <param name="argType">The underlying argument type of the parameter</param>
        private static void LoadBoxedValueType(ILGenerator emitter, Type argType)
        {
            emitter.Emit(OpCodes.Ldelem, typeof(object));
            emitter.Emit(OpCodes.Unbox_Any, argType);
        }

        /// <summary>
        /// Loads from the meta-arg array, the specified element, and casts it.
        /// Note that this method assumes that the meta-arg array is already on the stack
        /// </summary>
        /// <param name="emitter">The sigil emit type used to construct the method</param>
        /// <param name="argType">The underlying argument type of the parameter</param>
        private static void LoadCastedRefType(ILGenerator emitter, Type argType)
        {
            emitter.Emit(OpCodes.Ldelem, typeof(object));
            emitter.Emit(OpCodes.Castclass, argType);
        }
    }
}
