using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Axis.Luna.Extensions;

namespace Axis.Luna.Common.Test
{
    [TestClass]
    public class NullableExtensionsTests
    {

        #region public static Optional<TOut> Map<TIn, TOut>(this Nullable<TIn> nullable, RefMapper<TIn, TOut> mapper, Func<TOut> nullMapper = null)

        [TestMethod]
        public void MapToOptional_WithValidMapper_ShouldWrapResultInOptional()
        {
            int? nullableValue = 6;
            var optional = nullableValue.Map(i => $"Value is: {i}");

            Assert.IsTrue(optional.HasValue);
            Assert.AreEqual($"Value is: {nullableValue}", optional.Value);
        }

        [TestMethod]
        public void MapToOptional_WithNullReturningdMapper_ShouldReturnEmptyOptional()
        {
            int? nullableValue = 6;
            var optional = nullableValue.Map(i => (string)null);

            Assert.IsFalse(optional.HasValue);
            Assert.AreEqual(null, optional.Value);
        }

        [TestMethod]
        public void MapToOptional_WithEmptyNullable_ShouldSkipTheMapper()
        {
            int? nullableValue = null;
            bool isMapperCalled = false;
            var optional = nullableValue.Map(i =>
            {
                isMapperCalled = true;
                return $"Value is: {i}";
            });

            Assert.IsFalse(isMapperCalled);
            Assert.IsFalse(optional.HasValue);
            Assert.AreEqual(null, optional.Value);
        }

        [TestMethod]
        public void MapToOptional_WithEmptyNullableAndNullMapper_ShouldSkipTheMapperAndCallNullMapper()
        {
            int? nullableValue = null;
            bool isMapperCalled = false;
            bool isNullMapperCalled = false;
            var optional = nullableValue.Map(
                mapper: i =>
                {
                    isMapperCalled = true;
                    return $"Value is: {i}";
                },
                nullMapper: () =>
                {
                    isNullMapperCalled = true;
                    return "Null mapper: bleh";
                });

            Assert.IsFalse(isMapperCalled);
            Assert.IsTrue(isNullMapperCalled);
            Assert.IsTrue(optional.HasValue);
            Assert.AreEqual($"Null mapper: bleh", optional.Value);
        }

        [TestMethod]
        public void MapToOptional_WithExceptionMapper_ShouldThrowException()
        {
            int? nullableValue = 8;

            Assert.ThrowsException<InvalidOperationException>(
                () => nullableValue.Map(i => new InvalidOperationException().Throw<string>()));
        }

        [TestMethod]
        public void MapToOptional_WithNullMapper_ShouldThrowException()
        {
            int? nullableValue = 8;

            Assert.ThrowsException<ArgumentNullException>(
                () => nullableValue.Map((NullableExtensions.RefMapper<int, string>)null));
        }
        #endregion

        #region public static Optional<TOut> Map<TIn, TOut>(this Nullable<TIn> nullable, Func<TIn, Optional<TOut>> mapper, Func<TOut> nullMapper = null)

        [TestMethod]
        public void MapToOptionalInstance_WithValidMapper_ShouldWrapResultInOptional()
        {
            int? nullableValue = 6;
            var optional = nullableValue.Map(i => $"Value is: {i}".AsOptional());

            Assert.IsTrue(optional.HasValue);
            Assert.AreEqual($"Value is: {nullableValue}", optional.Value);
        }

        [TestMethod]
        public void MapToOptionalInstance_WithEmptyeturningdMapper_ShouldReturnEmptyOptional()
        {
            int? nullableValue = 6;
            var optional = nullableValue.Map(i => Optional.Empty<string>());

            Assert.IsFalse(optional.HasValue);
            Assert.AreEqual(null, optional.Value);
        }

        [TestMethod]
        public void MapToOptionalInstance_WithEmptyNullableAndNullMapper_ShouldSkipTheMapperAndCallNullMapper()
        {
            int? nullableValue = null;
            bool isMapperCalled = false;
            bool isNullMapperCalled = false;
            var optional = nullableValue.Map(
                mapper: i =>
                {
                    isMapperCalled = true;
                    return $"Value is: {i}".AsOptional();
                },
                nullMapper: () =>
                {
                    isNullMapperCalled = true;
                    return "Null mapper: bleh";
                });

            Assert.IsFalse(isMapperCalled);
            Assert.IsTrue(isNullMapperCalled);
            Assert.IsTrue(optional.HasValue);
            Assert.AreEqual($"Null mapper: bleh", optional.Value);
        }

        [TestMethod]
        public void MapToOptionalInstance_WithEmptyNullable_ShouldSkipTheMapper()
        {
            int? nullableValue = null;
            bool isMapperCalled = false;
            var optional = nullableValue.Map(i =>
            {
                isMapperCalled = true;
                return $"Value is: {i}".AsOptional();
            });

            Assert.IsFalse(isMapperCalled);
            Assert.IsFalse(optional.HasValue);
            Assert.AreEqual(null, optional.Value);
        }

        [TestMethod]
        public void MapToOptionalInstance_WithExceptionMapper_ShouldThrowException()
        {
            int? nullableValue = 8;

            Assert.ThrowsException<InvalidOperationException>(
                () => nullableValue.Map(i => new InvalidOperationException().Throw<Optional<string>>()));
        }

        [TestMethod]
        public void MapToOptionalInstance_WithNullMapper_ShouldThrowException()
        {
            int? nullableValue = 8;

            Assert.ThrowsException<ArgumentNullException>(
                () => nullableValue.Map((Func<int, Optional<string>>)null));
        }
        #endregion

    }
}
