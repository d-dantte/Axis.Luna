using Sigil;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Axis.Luna.FInvoke
{
    public class ConstructorInvoker
	{
		private static readonly string InstanceInvokerNamePrefix = "NInvoker_";

		private static readonly ConcurrentDictionary<ConstructorInfo, ConstructorInvoker> _invokerCache = new ConcurrentDictionary<ConstructorInfo, ConstructorInvoker>();

		/// <summary>
		/// Delegate for invoking the underlying method dynamically
		/// </summary>
		public Func<object[], object> Func { get; }

		/// <summary>
		/// Invokes the encapsulated constructor.
		/// </summary>
		/// <param name="arguments">the arguments</param>
		/// <returns>The object constructed</returns>
		public object New(params object[] arguments) => Func.Invoke(arguments);

		/// <summary>
		/// Creates or retrieves a new instance of the invoker. Generic-Definition methods are not accepted.
		/// </summary>
		/// <param name="constructor">Constructor to be invoked</param>
		/// <returns>The invoker instance</returns>
		public static ConstructorInvoker InvokerFor(ConstructorInfo constructor)
		{
			if (constructor.DeclaringType == null)
				throw new ArgumentException($"Cannot create an Invoker for methods without declaring types");

			else if (constructor.IsStatic)
				throw new ArgumentException("Cannot create an Invoker for a static method");

			else
				return _invokerCache.GetOrAdd(constructor, _ctor => new ConstructorInvoker(_ctor));
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="constructor">Target method</param>
		private ConstructorInvoker(ConstructorInfo constructor)
		{
			Func = InitInstance(constructor);
		}

		private Func<object[], object> InitInstance(ConstructorInfo constructor)
		{
			var guid = Guid
				.NewGuid()
				.ToString()
				.Replace("-", "_");

			var emitter = Emit<Func<object[], object>>.NewDynamicMethod(
				name: $"{InstanceInvokerNamePrefix}_{guid}",
				doVerify: false,
				owner: constructor.DeclaringType.IsValidDynamicMethodOwner()
					? constructor.DeclaringType
					: null);

			//push arguments unto the stack
			var arguments = constructor.GetParameters() ?? new ParameterInfo[0];
			for (ushort cnt = 0; cnt < arguments.Length; cnt++)
			{
				//load the meta-arg array into memory
				emitter.LoadArgument(0);

				if (arguments[cnt].ParameterType.IsValueType)
					LoadBoxedValueType(emitter, arguments[cnt].ParameterType, cnt);

				else //if(!arguments[cnt].IsValueType)
					LoadCastedRefType(emitter, arguments[cnt].ParameterType, cnt);
			}

			//call the method
			emitter.NewObject(constructor);

            if (constructor.DeclaringType.IsValueType)
                emitter.Box(constructor.DeclaringType);

            emitter.Return();

			return emitter.CreateDelegate();
		}

		/// <summary>
		/// Loads from the meta-arg array, the specified element, and unboxes it.
		/// Note that this method assumes that the meta-arg array is already on the stack
		/// </summary>
		/// <param name="emitter">The sigil emit type used to construct the method</param>
		/// <param name="argType">The underlying argument type of the parameter</param>
		/// <param name="argIndex">The index of the parameter in the array</param>
		private void LoadBoxedValueType<TDelegate>(
			Emit<TDelegate> emitter,
			Type argType,
			ushort argIndex)
		{
			emitter
				.LoadConstant(argIndex)
				.LoadElement(typeof(object))
				.UnboxAny(argType);
		}

		/// <summary>
		/// Loads from the meta-arg array, the specified element, and casts it.
		/// Note that this method assumes that the meta-arg array is already on the stack
		/// </summary>
		/// <param name="emitter">The sigil emit type used to construct the method</param>
		/// <param name="argType">The underlying argument type of the parameter</param>
		/// <param name="argIndex">The index of the parameter in the array</param>
		private void LoadCastedRefType<TDelegate>(
			Emit<TDelegate> emitter,
			Type argType,
			ushort argIndex)
		{
			emitter
				.LoadConstant(argIndex)
				.LoadElement(typeof(object))
				.IsInstance(argType);
		}
	}
}
