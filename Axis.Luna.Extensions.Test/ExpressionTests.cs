using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Axis.Luna.Extensions.Test
{
    [TestClass]
    public class ExpressionTests
    {
        [TestMethod]
        public void TestExpressions()
        {
            var pexp = Expression.Parameter(typeof(SomeClass), "obj");
            var body = Expression.Block
            (
                typeof(string),
                new ParameterExpression[] { pexp },
                Expression.Assign(pexp, Expression.Constant(new SomeClass { ABCD = "bleh" })),
                Expression.MakeMemberAccess(pexp, typeof(SomeClass).GetProperty("ABCD"))
            );

            var method = Expression
                .Lambda<Func<string>>(body)
                .Compile();

            Console.WriteLine(method.Invoke());
        }
    }

    public class SomeClass
    {
        public string ABCD { get; set; }
    }
}
