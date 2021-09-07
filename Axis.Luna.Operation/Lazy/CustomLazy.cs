using System;

namespace Axis.Luna.Operation.Lazy
{
	public enum CustomLazyState
	{
		/// <summary>
		/// Lazy value has not been initialized
		/// </summary>
		Uninitialized,

		/// <summary>
		/// Lazy value has been initialized
		/// </summary>
		Initialized,

		/// <summary>
		/// Lazy value faulted during initialization
		/// </summary>
		Faulted
	}


	public class CustomLazy<TValue>
	{
		private readonly Lazy<TValue> _lazy;

		public Exception Exception { get; private set; }

		public CustomLazyState State { get; private set; }

		public CustomLazy(Lazy<TValue> lazy)
		{
			_lazy = lazy ?? throw new ArgumentNullException(nameof(lazy));
		}

		public CustomLazy(Func<TValue> function)
        {
			_lazy = new Lazy<TValue>(function);
        }

		public TValue Value
		{
			get
			{
				if(State == CustomLazyState.Uninitialized)
				{
					try
					{
						var value = _lazy.Value;
						State = CustomLazyState.Initialized;
						return value;
					}
					catch(Exception e)
					{
						State = CustomLazyState.Faulted;
						Exception = e;
						throw;
					}
				}

				return _lazy.Value;
			}
		}

		public static implicit operator CustomLazy<TValue>(Lazy<TValue> lazy) => new CustomLazy<TValue>(lazy);

		public static implicit operator Lazy<TValue>(CustomLazy<TValue> customLazy) => customLazy._lazy;
	}
}
