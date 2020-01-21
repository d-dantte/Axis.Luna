using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test
{
	[TestClass]
	public class ResultInfoTestClass
	{
		[TestMethod]
		public void DelegateTest()
		{

		}

		public void OtherFunc(Operation func)
		{

		}

		public void SomeFunc(Func<ErrorHandlerResult> func)
		{
			Console.WriteLine($"ErrorHandlerTaskResult: {func is Func<ErrorHandlerTaskResult>}");
			Console.WriteLine($"ErrorHandlerOperationResult: {func is Func<ErrorHandlerOperationResult>}");
		}
		public void SomeFunc<T>(Func<ErrorHandlerResult<T>> func)
		{
			Console.WriteLine($"ErrorHandlerOperationResult<T>: {func is Func<ErrorHandlerOperationResult<T>>}");
			Console.WriteLine($"ErrorHandlerTasknResult<T>: {func is Func<ErrorHandlerTaskResult<T>>}");
			Console.WriteLine($"ErrorHandlerValueResult<T>: {func is Func<ErrorHandlerValueResult<T>>}");
		}
		public void SomeFunc<T>(T t, Func<ErrorHandlerResult<T>> func)
		{
			Console.WriteLine($"ErrorHandlerOperationResult<T>: {func is Func<ErrorHandlerOperationResult<T>>}");
			Console.WriteLine($"ErrorHandlerTasknResult<T>: {func is Func<ErrorHandlerTaskResult<T>>}");
			Console.WriteLine($"ErrorHandlerValueResult<T>: {func is Func<ErrorHandlerValueResult<T>>}");
		}
	}

}
