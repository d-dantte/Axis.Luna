using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Axis.Luna.Extensions.Test
{
    [TestClass]
    public class NullableExtensionsTests
    {
        #region AsNullable(TOut)

        [TestMethod]
        public void AsNullable_WithValidArgument_ShouldReturnValidNullable()
        {
            int i = 6;
            var nullable = i.AsNullable();

            Assert.IsNotNull(nullable);
            Assert.IsTrue(nullable.HasValue);
            Assert.AreEqual(i, nullable.Value);
        }

        #endregion

        #region TOut? Map<TIn, TOut>(TIn?, Func<TIn, TOut>, Func<TOut>)
        public static IEnumerable<object[]> Map_WithExpectedValues_ShouldReturnExpectedResults_Data => new object[][]
        {
            new object[]
            { 
                5.AsNullable(),
                TimeSpan.FromSeconds(5).AsNullable(),
                (Func<int, TimeSpan>) (value => TimeSpan.FromSeconds(value)),
                null
            },
            new object[]
            {
                null,
                TimeSpan.FromSeconds(3).AsNullable(),
                (Func<int, TimeSpan>)(value => TimeSpan.FromSeconds(value * 5)),
                (Func<TimeSpan>)(() => TimeSpan.FromSeconds(3))
            },
            new object[]
            {
                null,
                null,
                (Func<int, TimeSpan>)(value => TimeSpan.FromSeconds(value * 5)),
                null
            }
        };

        [TestMethod]
        [DynamicData(nameof(Map_WithExpectedValues_ShouldReturnExpectedResults_Data))]
        public void Map_WithExpectedValues_ShouldReturnExpectedResults(
            int? value,
            TimeSpan? result,
            Func<int, TimeSpan> mapper,
            Func<TimeSpan> nullMapper)
        {
            var nullable = value;
            var nullable2 = nullable.Map(
                mapper: mapper,
                nullMapper: nullMapper);

            Assert.AreEqual(result, nullable2);
        }

        [TestMethod]
        public void Map_WithInvalidMapper_ShouldThrowException()
        {
            int? nullable = 5;
            Assert.ThrowsException<ArgumentNullException>(() => nullable.Map((Func<int, TimeSpan>)null, null));
        }

        [TestMethod]
        public void Map_WithExceptionMapper_ShouldThrowException()
        {
            int? nullable = 5;
            Assert.ThrowsException<Exception>(() => nullable.Map(value => new Exception().Throw<TimeSpan>()));
        }

        [TestMethod]
        public void Map_WithExceptionNullMapper_ShouldThrowException()
        {
            int? nullable = null;
            Assert.ThrowsException<Exception>(() => nullable.Map(
                mapper: value => default, 
                nullMapper: () => new Exception().Throw<TimeSpan>()));
        }
        #endregion

        #region TOut? Bind<TIn, TOut>(TIn?, Func<TIn, TOut?>, Func<TOut>)
        public static IEnumerable<object[]> Bind_WithExpectedValues_ShouldReturnExpectedResults_Data => new object[][]
        {
            new object[]
            {
                5.AsNullable(),
                TimeSpan.FromSeconds(5).AsNullable(),
                (Func<int, TimeSpan?>) (value => TimeSpan.FromSeconds(value).AsNullable()),
                null
            },
            new object[]
            {
                null,
                TimeSpan.FromSeconds(3).AsNullable(),
                (Func<int, TimeSpan?>)(value => TimeSpan.FromSeconds(value * 5).AsNullable()),
                (Func<TimeSpan>)(() => TimeSpan.FromSeconds(3))
            },
            new object[]
            {
                null,
                null,
                (Func<int, TimeSpan?>)(value => TimeSpan.FromSeconds(value * 5).AsNullable()),
                null
            }
        };

        [TestMethod]
        [DynamicData(nameof(Bind_WithExpectedValues_ShouldReturnExpectedResults_Data))]
        public void Bind_WithExpectedValues_ShouldReturnExpectedResults(
            int? value,
            TimeSpan? result,
            Func<int, TimeSpan?> mapper,
            Func<TimeSpan> nullMapper)
        {
            var nullable = value;
            var nullable2 = nullable.Bind(
                mapper: mapper,
                nullMapper: nullMapper);

            Assert.AreEqual(result, nullable2);
        }

        [TestMethod]
        public void Bind_WithInvalidMapper_ShouldThrowException()
        {
            int? nullable = 5;
            Assert.ThrowsException<ArgumentNullException>(() => nullable.Bind((Func<int, TimeSpan?>)null, null));
        }

        [TestMethod]
        public void Bind_WithExceptionMapper_ShouldThrowException()
        {
            int? nullable = 5;
            Assert.ThrowsException<Exception>(() => nullable.Bind(value => new Exception().Throw<TimeSpan?>()));
        }

        [TestMethod]
        public void Bind_WithExceptionNullMapper_ShouldThrowException()
        {
            int? nullable = null;
            Assert.ThrowsException<Exception>(() => nullable.Bind(
                mapper: value => default,
                nullMapper: () => new Exception().Throw<TimeSpan>()));
        }
        #endregion
    }
}
