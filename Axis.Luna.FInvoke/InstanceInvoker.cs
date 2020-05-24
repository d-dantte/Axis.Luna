using Sigil;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Axis.Luna.FInvoke
{
	/// <summary>
	/// Represents a dynamic invocation of an instance method
	/// </summary>
	public class InstanceInvoker
	{
		private static readonly string InstanceInvokerNamePrefix = "NInvoker_";

		private static readonly ConcurrentDictionary<MethodInfo, InstanceInvoker> _invokerCache = new ConcurrentDictionary<MethodInfo, InstanceInvoker>();

		/// <summary>
		/// Delegate for invoking the underlying method dynamically
		/// </summary>
		public Func<object, object[], object> Func { get; }

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

		private Func<object, object[], object> InitInstance(MethodInfo method)
		{
			var guid = Guid
				.NewGuid()
				.ToString()
				.Replace("-", "_");

			var emitter = Emit<Func<object, object[], object>>.NewDynamicMethod(
				method.DeclaringType,
				$"{InstanceInvokerNamePrefix}_{guid}");

			//push 'this' unto the stack
			emitter.LoadArgument(0);
			emitter.IsInstance(method.DeclaringType);

			//push arguments unto the stack
			var arguments = method.GetGenericArguments() ?? new Type[0];
			for (ushort cnt = 0; cnt < arguments.Length; cnt++)
			{
				//load the meta-arg array into memory
				emitter.LoadArgument(1);

				if (arguments[cnt].IsValueType)
					LoadBoxedValueType(emitter, arguments[cnt], cnt);

				else //if(!arguments[cnt].IsValueType)
					LoadCastedRefType(emitter, arguments[cnt], cnt);
			}

			//call the method
			emitter.CallVirtual(method);

			//return value
			if (method.ReturnType == typeof(void))
				emitter.LoadNull();

			else if (method.ReturnType.IsValueType)
				emitter.Box(method.ReturnType);

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
				.LoadElement(argType)
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
				.LoadElement(argType)
				.IsInstance(argType);
		}
	}
}
