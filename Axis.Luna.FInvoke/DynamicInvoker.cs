using Sigil;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Axis.Luna.FInvoke
{
	public class DynamicInvoker
	{
		private static readonly string StaticInvokerNamePrefix = "SDInvoker_";
		private static readonly string InstanceInvokerNamePrefix = "IDInvoker_";

		private static readonly ConcurrentDictionary<MethodInfo, DynamicInvoker> _invokerCache = new ConcurrentDictionary<MethodInfo, DynamicInvoker>();

		private readonly MethodInfo _targetMethod;
		private readonly Func<object[], object> _staticFunc;
		public readonly Func<object, object[], object> _instanceFunc;

		public bool IsStaticInvoker => _targetMethod.IsStatic;

		public bool IsInstanceInvoker => !_targetMethod.IsStatic;


		public static DynamicInvoker InvokerFor(MethodInfo method)
		{
			if (method.IsGenericMethodDefinition)
				throw new InvalidOperationException("Cannot create DynamicInvoker from a generic method definition");

			else 
				return _invokerCache.GetOrAdd(method, _method => new DynamicInvoker(_method));
		}

		internal DynamicInvoker(MethodInfo method)
		{
			if (method.DeclaringType == null)
				throw new Exception($"Cannot create invoker for methods without declaring types");

			_targetMethod = method;

			if (_targetMethod.IsStatic)
			{
				_staticFunc = InitStatic();
				_instanceFunc = null;
			}
			else
			{
				_instanceFunc = InitInstance();
				_staticFunc = null;
			}
		}

		public object InvokeStatic(params object[] @params) => _staticFunc.Invoke(@params);

		public object InvokeInstance(object instance, params object[] @params) => _instanceFunc.Invoke(instance, @params);

		private Func<object[], object> InitStatic()
		{
			var guid = Guid
				.NewGuid()
				.ToString()
				.Replace("-", "_");

			var emitter = Emit<Func<object[], object>>.NewDynamicMethod(
				_targetMethod.DeclaringType,
				$"{StaticInvokerNamePrefix}_{guid}");

			//push arguments unto the stack
			var arguments = _targetMethod.GetGenericArguments() ?? new Type[0];
			for (ushort cnt = 0; cnt < arguments.Length; cnt++)
			{
				//load the meta-arg array into memory
				emitter.LoadArgument(0);

				if (arguments[cnt].IsValueType)
					LoadBoxedValueType(emitter, arguments[cnt], cnt);

				else //if(!arguments[cnt].IsValueType)
					LoadCastedRefType(emitter, arguments[cnt], cnt);
			}

			//call the method
			emitter.Call(_targetMethod);

			//return value
			if (_targetMethod.ReturnType == typeof(void))
				emitter.LoadNull();

			else if (_targetMethod.ReturnType.IsValueType)
				emitter.Box(_targetMethod.ReturnType);

			emitter.Return();

			return emitter.CreateDelegate();
		}

		private Func<object, object[], object> InitInstance()
		{
			var guid = Guid
				.NewGuid()
				.ToString()
				.Replace("-", "_");

			var emitter = Emit<Func<object, object[], object>>.NewDynamicMethod(
				_targetMethod.DeclaringType,
				$"{InstanceInvokerNamePrefix}_{guid}");

			//push 'this' unto the stack
			emitter.LoadArgument(0);
			emitter.IsInstance(_targetMethod.DeclaringType);

			//push arguments unto the stack
			var arguments = _targetMethod.GetGenericArguments() ?? new Type[0];
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
			emitter.CallVirtual(_targetMethod);

			//return value
			if (_targetMethod.ReturnType == typeof(void))
				emitter.LoadNull();

			else if (_targetMethod.ReturnType.IsValueType)
				emitter.Box(_targetMethod.ReturnType);

			emitter.Return();

			return emitter.CreateDelegate();
		}

		/// <summary>
		/// Loads from the meta-arg array, the specified element, and unboxes it.
		/// Note that this method assumes that the meta-arg array is already on the stack
		/// </summary>
		/// <param name="emitter"></param>
		/// <param name="argType"></param>
		/// <param name="argIndex"></param>
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
		/// <param name="emitter"></param>
		/// <param name="argType"></param>
		/// <param name="argIndex"></param>
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
