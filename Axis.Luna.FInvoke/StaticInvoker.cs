using Sigil;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Axis.Luna.FInvoke
{
	/// <summary>
	/// Represents a dynamic invocation of a static method
	/// </summary>
	public class StaticInvoker
	{
		private static readonly string StaticInvokerNamePrefix = "SInvoker_";

		private static readonly ConcurrentDictionary<MethodInfo, StaticInvoker> _invokerCache = new ConcurrentDictionary<MethodInfo, StaticInvoker>();

		/// <summary>
		/// Delegate for invoking the underlying method dynamically
		/// </summary>
		public Func<object[], object> Func { get; }

		/// <summary>
		/// Invokes the function encapsulated by this invoker
		/// </summary>
		/// <param name="arguments">the method arguments</param>
		/// <returns>the return value if any</returns>
		public object Invoke(params object[] arguments) => Func.Invoke(arguments);

		/// <summary>
		/// Invokes the function with no argument
		/// </summary>
		/// <returns>the return value if any</returns>
		public object Invoke() => Invoke(Array.Empty<object>());

		/// <summary>
		/// Creates or retrieves a new instance of the invoker. Generic-Definition methods are not accepted.
		/// </summary>
		/// <param name="method">Method to be invoked</param>
		/// <returns>The invoker instance</returns>
		public static StaticInvoker InvokerFor(MethodInfo method)
		{
			if (method.IsGenericMethodDefinition)
				throw new ArgumentException("Cannot create an Invoker from a generic method definition");

			else if (method.DeclaringType == null)
				throw new ArgumentException($"Cannot create an Invoker for methods without declaring types");

			else if (!method.IsStatic)
				throw new ArgumentException("Cannot create an Invoker for a non-static method");

			else
				return _invokerCache.GetOrAdd(method, _method => new StaticInvoker(_method));
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="method">Target method</param>
		private StaticInvoker(MethodInfo method)
		{
			Func = InitStatic(method);
		}

		private Func<object[], object> InitStatic(MethodInfo method)
		{
			var guid = Guid
				.NewGuid()
				.ToString()
				.Replace("-", "_");

			var emitter = Emit<Func<object[], object>>.NewDynamicMethod(
				name: $"{StaticInvokerNamePrefix}_{guid}",
				doVerify: false,
				owner: method.DeclaringType.IsValidDynamicMethodOwner()
					? method.DeclaringType
					: null);

			//push arguments unto the stack
			var arguments = method.GetParameters() ?? new ParameterInfo[0];
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
			emitter.Call(method);

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
