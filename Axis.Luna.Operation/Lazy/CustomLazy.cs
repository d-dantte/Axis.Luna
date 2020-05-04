using System;

namespace Axis.Luna.Operation.Lazy
{
	public enum CustomLazyState
	{
		Uninitialized,
		Initialized,
		Faulted
	}


	public class CustomLazy<TValue>
	{
		private readonly Lazy<TValue> _lazy;

		public CustomLazyState State { get; private set; }

		public CustomLazy(Lazy<TValue> lazy)
		{
			_lazy = lazy ?? throw new ArgumentNullException(nameof(lazy));
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
					catch
					{
						State = CustomLazyState.Faulted;
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
