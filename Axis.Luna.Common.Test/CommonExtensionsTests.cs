using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Axis.Luna.Common.Test
{
    [TestClass]
	public class CommonExtensionsTests
	{
		[TestMethod]
		public void UsingExt_ShouldDisposeProperly()
		{
			object stuff = new List<int> { 4, 3, 2, 56 };
			List<int> x = stuff.As<List<int>>();
			Assert.IsNotNull(x);

			stuff = null;
			x = stuff.As<List<int>>();
			Assert.IsNull(x);
		}
	}
}
