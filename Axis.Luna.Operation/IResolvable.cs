namespace Axis.Luna.Operation
{
	public interface IResolvable
	{
		void Resolve();

		bool TryResolve(out OperationError error);
	}

	public interface IResolvable<Result>
	{
		Result Resolve();

		bool TryResolve(out Result result, out OperationError error);
	}
}
