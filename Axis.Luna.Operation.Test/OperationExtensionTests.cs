using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.Test
{
	[TestClass]
	public class OperationExtensionTests
	{
		/// <summary>
		/// Test "<c>Operation Then(this Operation prev, * action, Actio&lt;Exception&gt; errorHandler, *)</c>"
		/// </summary>
		[TestMethod]
		public void ThenTest()
		{
			AsyncContext.Run(async () =>
			{
				//test for Action
				var capturedInt = 0;
				var op = Operation.Try(() => { capturedInt++; });
				Assert.IsNull(op.Succeeded);

				var next = op.Then(() => { capturedInt++; });
				Assert.IsNull(op.Succeeded);
				Assert.IsNull(next.Succeeded);

				next.Resolve();
				Assert.AreEqual(true, op.Succeeded);
				Assert.AreEqual(true, next.Succeeded);
				Assert.AreEqual(2, capturedInt);

				next = Operation
					.Try(() => new Exception().Throw())
					.Then(
						errorHandler: e => capturedInt++,
						action: () => { });

				Assert.IsNull(next.Succeeded);
				next.ResolveSafely();
				Assert.AreEqual(true, next.Succeeded);
				Assert.AreEqual(3, capturedInt);


				//test happy path for Task
				capturedInt = 0;
				op = Operation.Try(() => { capturedInt++; });
				Assert.IsNull(op.Succeeded);

				next = op.Then(async () =>
				{
					capturedInt++;
					await Task.Delay(1);
				});
				Assert.IsNull(op.Succeeded);
				Assert.IsNull(next.Succeeded);

				await next;
				Assert.AreEqual(true, op.Succeeded);
				Assert.AreEqual(true, next.Succeeded);
				Assert.AreEqual(2, capturedInt);

				next = Operation
					.Try(async () =>
					{
						if (true)
							new Exception().Throw();

						else
							await Task.Delay(1);
					})
					.Then(
						errorHandler: e => capturedInt++,
						action: () => { });

				Assert.IsNull(next.Succeeded);
				next.ResolveSafely();
				Assert.AreEqual(true, next.Succeeded);
				Assert.AreEqual(3, capturedInt);


				//test happy path for Operation
				capturedInt = 0;
				op = Operation.Try(() => { capturedInt++; });
				Assert.IsNull(op.Succeeded);

				next = op.Then(() =>
				{
					return Operation.Try(() => { capturedInt++; });
				});
				Assert.IsNull(op.Succeeded);
				Assert.IsNull(next.Succeeded);

				await next;
				Assert.AreEqual(true, op.Succeeded);
				Assert.AreEqual(true, next.Succeeded);
				Assert.AreEqual(2, capturedInt);

				next = Operation
					.Try(() => Operation.Try(() => new Exception().Throw()))
					.Then(
						errorHandler: e => capturedInt++,
						action: () => { });

				Assert.IsNull(next.Succeeded);
				next.ResolveSafely();
				Assert.AreEqual(true, next.Succeeded);
				Assert.AreEqual(3, capturedInt);

			});
		}

		/// <summary>
		/// Test "<c>Operation Then(this Operation&lt;In&gt; prev, * action, Actio&lt;Exception&gt; errorHandler, *)</c>"
		/// </summary>
		[TestMethod]
		public void ThenTest2()
		{
			AsyncContext.Run(async () =>
			{
				//test for Action
				var result = 1;
				var op = Operation.Try(() => 1);
				Assert.IsNull(op.Succeeded);

				var next = op.Then(i => { result += i; });
				Assert.IsNull(op.Succeeded);
				Assert.IsNull(next.Succeeded);

				next.Resolve();
				Assert.AreEqual(true, op.Succeeded);
				Assert.AreEqual(true, next.Succeeded);
				Assert.AreEqual(2, result);

				next = Operation
					.Try(() => new Exception().Throw<int>())
					.Then(
						errorHandler: e => { result += 1; },
						action: i => { });

				Assert.IsNull(next.Succeeded);
				next.ResolveSafely();
				Assert.AreEqual(true, next.Succeeded);
				Assert.AreEqual(3, result);


				//test happy path for Task
				result = 1;
				op = Operation.Try(() => 1);
				Assert.IsNull(op.Succeeded);

				next = op.Then(async i =>
				{
					result += i;
					await Task.Delay(1);
				});
				Assert.IsNull(op.Succeeded);
				Assert.IsNull(next.Succeeded);

				await next;
				Assert.AreEqual(true, op.Succeeded);
				Assert.AreEqual(true, next.Succeeded);
				Assert.AreEqual(2, result);

				next = Operation
					.Try(async () =>
					{
						if (true)
							new Exception().Throw();

						else
							await Task.Delay(1);
					})
					.Then(
						errorHandler: e => result++,
						action: () => { });

				Assert.IsNull(next.Succeeded);
				next.ResolveSafely();
				Assert.AreEqual(true, next.Succeeded);
				Assert.AreEqual(3, result);


				//test happy path for Operation
				result = 1;
				op = Operation.Try(() => 1);
				Assert.IsNull(op.Succeeded);

				next = op.Then(i =>
				{
					return Operation.Try(() => { result += i; });
				});
				Assert.IsNull(op.Succeeded);
				Assert.IsNull(next.Succeeded);

				await next;
				Assert.AreEqual(true, op.Succeeded);
				Assert.AreEqual(true, next.Succeeded);
				Assert.AreEqual(2, result);

				next = Operation
					.Try(() => Operation.Try(() => new Exception().Throw()))
					.Then(
						errorHandler: e => result++,
						action: () => { });

				Assert.IsNull(next.Succeeded);
				next.ResolveSafely();
				Assert.AreEqual(true, next.Succeeded);
				Assert.AreEqual(3, result);

			});
		}
	}
}
