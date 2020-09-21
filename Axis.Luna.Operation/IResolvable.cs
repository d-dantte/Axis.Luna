namespace Axis.Luna.Operation
{
	public interface IResolvable
	{
		void ResolveSafely();

		void Resolve();

		bool TryResolve(out OperationError error);
	}

	public interface IResolvable<Result>
	{
		Result ResolveSafely();

		Result Resolve();

		bool TryResolve(out Result result, out OperationError error);
	}
}
