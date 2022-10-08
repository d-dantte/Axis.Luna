using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Axis.Luna.Extensions;

namespace Axis.Luna.Common.Test
{
    [TestClass]
    public class OptionalTests
    {
        #region Consume
        [TestMethod]
        public void Consume_FromOptional_WithValidAction_ShouldCallActionAndSkipNullAction()
        {
            bool isActionCalled = false;
            bool isNullActionCalled = false;
            var optional = "a string".AsOptional();

            //try the action
            optional.Consume(
                nullAction: null,
                action: value =>
                {
                    Assert.AreEqual("a string", value);
                    isActionCalled = true;
                });

            Assert.IsTrue(isActionCalled);

            //try with null action
            optional.Consume(
                action: value =>
                {
                    isActionCalled = true;
                },
                nullAction: () =>
                {
                    isNullActionCalled = true;
                });

            Assert.IsTrue(isActionCalled);
            Assert.IsFalse(isNullActionCalled);
        }

        [TestMethod]
        public void Consume_FRomOptional_WithNullAction_ShouldThrowArgumentNullException()
        {
            var optional = "a string".AsOptional();

            Assert.ThrowsException<ArgumentNullException>(() => optional.Consume(null));
        }
        #endregion

        [TestMethod]
        public void Value_OnEmptyOptional_ThrowsException()
        {
            var optional = Optional.Empty<string>();
            Assert.ThrowsException<InvalidOperationException>(() => optional.Value());
        }

        #region Optional<TOut> Map<TOut>(RefMapper<TOut> mapper, Func<TOut> nullMapper = null)
        [TestMethod]
        public void Map_FromOptional_WithValidMapper_ShouldCallMapperFunction()
        {
            var @string = "a string";
            var isMapperCalled = false;
            var optional = @string.AsOptional();
            var optional2 = optional.Map(value =>
            {
                isMapperCalled = true;
                Assert.AreEqual(@string, value);

                return value.ToCharArray();
            });

            Assert.IsTrue(isMapperCalled);
            Assert.IsFalse(optional.IsEmpty);
            Assert.IsFalse(optional2.IsEmpty);
            Assert.AreEqual(@string, optional.Value());
            Assert.IsTrue(@string.ToCharArray().SequenceEqual(optional2.Value()));
        }

        [TestMethod]
        public void Map_FromEmptyOptional_WithValidMapper_ShouldSkipMapperFunction()
        {
            var isMapperCalled = false;
            var optional = Optional.Empty<string>();
            var optional2 = optional.Map(value =>
            {
                isMapperCalled = true;
                return value.ToCharArray();
            });

            Assert.IsFalse(isMapperCalled);
            Assert.IsTrue(optional.IsEmpty);
            Assert.IsTrue(optional2.IsEmpty);
        }

        [TestMethod]
        public void Map_FromOptional_WithNullReturningMapper_ShouldCallMapperFunctionAndReturnEmpty()
        {
            var @string = "a string";
            var isMapperCalled = false;
            var optional = @string.AsOptional();
            var optional2 = optional.Map(value =>
            {
                isMapperCalled = true;
                Assert.AreEqual(@string, value);

                return (object)null;
            });

            Assert.IsTrue(isMapperCalled);
            Assert.IsFalse(optional.IsEmpty);
            Assert.IsTrue(optional2.IsEmpty);
            Assert.AreEqual(@string, optional.Value());
        }

        [TestMethod]
        public void Map_FromOptional_WithExceptionThrowingMapper_ShouldThrowException()
        {
            var @string = "a string";
            var optional = @string.AsOptional();
            Assert.ThrowsException<InvalidOperationException>(
                () => optional.Map(value => new InvalidOperationException().Throw<object>()));
        }

        [TestMethod]
        public void Map_FromOptional_WithNullMapper_ShouldThrowException()
        {
            var @string = "a string";
            var optional = @string.AsOptional();
            Assert.ThrowsException<ArgumentNullException>(
                () => optional.Map((Optional<string>.RefMapper<object>)null));
        }

        [TestMethod]
        public void Map_FromOptional_WithValidMapperAndValidNullMapper_ShouldCallMapperFunctionAndSkipNullMapperFunction()
        {
            var @string = "a string";
            var isMapperCalled = false;
            var isNullMapperCalled = false;
            var optional = @string.AsOptional();
            var optional2 = optional.Map(
                mapper: value =>
                {
                    isMapperCalled = true;
                    Assert.AreEqual(@string, value);

                    return value.ToCharArray();
                },
                nullMapper: () =>
                {
                    isNullMapperCalled = false;
                    return null;
                });

            Assert.IsTrue(isMapperCalled);
            Assert.IsFalse(isNullMapperCalled);
            Assert.IsFalse(optional.IsEmpty);
            Assert.IsFalse(optional2.IsEmpty);
            Assert.AreEqual(@string, optional.Value());
            Assert.IsTrue(@string.ToCharArray().SequenceEqual(optional2.Value()));
        }

        [TestMethod]
        public void Map_FromEmptyOptional_WithValidMapperAndValidNullMapper_ShouldSkipMapperFunctionAndCallNullMapperFunction()
        {
            var isMapperCalled = false;
            var isNullMapperCalled = false;
            var optional = Optional.Empty<string>();
            var optional2 = optional.Map(
                mapper: value =>
                {
                    isMapperCalled = true;

                    return value.ToCharArray();
                },
                nullMapper: () =>
                {
                    isNullMapperCalled = true;
                    return "bleh".ToCharArray();
                });

            Assert.IsFalse(isMapperCalled);
            Assert.IsTrue(isNullMapperCalled);
            Assert.IsTrue(optional.IsEmpty);
            Assert.IsFalse(optional2.IsEmpty);
            Assert.IsTrue("bleh".ToCharArray().SequenceEqual(optional2.Value()));


            var optional3 = optional.Map(
                mapper: value =>
                {
                    isMapperCalled = true;

                    return value.ToCharArray();
                },
                nullMapper: () =>
                {
                    isNullMapperCalled = true;
                    return null;
                });

            Assert.IsFalse(isMapperCalled);
            Assert.IsTrue(isNullMapperCalled);
            Assert.IsTrue(optional3.IsEmpty);
        }
        #endregion

        #region public Optional<TOut> Bind<TOut>(Func<T, Optional<TOut>> mapper, Func<TOut> nullMapper = null)

        [TestMethod]
        public void Bind_FromOptional_WithValidMapper_ShouldCallMapperFunction()
        {
            var @string = "a string";
            var isMapperCalled = false;
            var optional = @string.AsOptional();
            var optional2 = optional.Bind(value =>
            {
                isMapperCalled = true;
                Assert.AreEqual(@string, value);

                return value.ToCharArray().AsOptional();
            });

            Assert.IsTrue(isMapperCalled);
            Assert.IsFalse(optional.IsEmpty);
            Assert.IsFalse(optional2.IsEmpty);
            Assert.AreEqual(@string, optional.Value());
            Assert.IsTrue(@string.ToCharArray().SequenceEqual(optional2.Value()));
        }

        [TestMethod]
        public void Bind_FromEmptyOptional_WithValidMapper_ShouldSkipMapperFunction()
        {
            var isMapperCalled = false;
            var optional = Optional.Empty<string>();
            var optional2 = optional.Bind(value =>
            {
                isMapperCalled = true;
                return value.ToCharArray().AsOptional();
            });

            Assert.IsFalse(isMapperCalled);
            Assert.IsTrue(optional.IsEmpty);
            Assert.IsTrue(optional2.IsEmpty);
        }

        [TestMethod]
        public void Bind_FromOptional_WithNullReturningMapper_ShouldCallMapperFunctionAndReturnEmpty()
        {
            var @string = "a string";
            var isMapperCalled = false;
            var optional = @string.AsOptional();
            var optional2 = optional.Bind(value =>
            {
                isMapperCalled = true;
                Assert.AreEqual(@string, value);

                return Optional.Empty<object>();
            });

            Assert.IsTrue(isMapperCalled);
            Assert.IsFalse(optional.IsEmpty);
            Assert.IsTrue(optional2.IsEmpty);
            Assert.AreEqual(@string, optional.Value());
        }

        [TestMethod]
        public void Bind_FromOptional_WithExceptionThrowingMapper_ShouldThrowException()
        {
            var @string = "a string";
            var optional = @string.AsOptional();
            Assert.ThrowsException<InvalidOperationException>(
                () => optional.Map(value => new InvalidOperationException().Throw<object>()));
        }

        [TestMethod]
        public void Bind_FromOptional_WithNullMapper_ShouldThrowException()
        {
            var @string = "a string";
            var optional = @string.AsOptional();
            Assert.ThrowsException<ArgumentNullException>(
                () => optional.Map((Optional<string>.RefMapper<object>)null));
        }

        [TestMethod]
        public void Bind_FromOptional_WithValidMapperAndValidNullMapper_ShouldCallMapperFunctionAndSkipNullMapperFunction()
        {
            var @string = "a string";
            var isMapperCalled = false;
            var isNullMapperCalled = false;
            var optional = @string.AsOptional();
            var optional2 = optional.Bind(
                mapper: value =>
                {
                    isMapperCalled = true;
                    Assert.AreEqual(@string, value);

                    return value.ToCharArray().AsOptional();
                },
                nullMapper: () =>
                {
                    isNullMapperCalled = false;
                    return null;
                });

            Assert.IsTrue(isMapperCalled);
            Assert.IsFalse(isNullMapperCalled);
            Assert.IsFalse(optional.IsEmpty);
            Assert.IsFalse(optional2.IsEmpty);
            Assert.AreEqual(@string, optional.Value());
            Assert.IsTrue(@string.ToCharArray().SequenceEqual(optional2.Value()));
        }

        [TestMethod]
        public void Bind_FromEmptyOptional_WithValidMapperAndValidNullMapper_ShouldSkipMapperFunctionAndCallNullMapperFunction()
        {
            var isMapperCalled = false;
            var isNullMapperCalled = false;
            var optional = Optional.Empty<string>();
            var optional2 = optional.Bind(
                mapper: value =>
                {
                    isMapperCalled = true;

                    return value.ToCharArray().AsOptional();
                },
                nullMapper: () =>
                {
                    isNullMapperCalled = true;
                    return "bleh".ToCharArray();
                });

            Assert.IsFalse(isMapperCalled);
            Assert.IsTrue(isNullMapperCalled);
            Assert.IsTrue(optional.IsEmpty);
            Assert.IsFalse(optional2.IsEmpty);
            Assert.IsTrue("bleh".ToCharArray().SequenceEqual(optional2.Value()));


            var optional3 = optional.Bind(
                mapper: value =>
                {
                    isMapperCalled = true;

                    return value.ToCharArray().AsOptional();
                },
                nullMapper: () =>
                {
                    isNullMapperCalled = true;
                    return null;
                });

            Assert.IsFalse(isMapperCalled);
            Assert.IsTrue(isNullMapperCalled);
            Assert.IsTrue(optional3.IsEmpty);
        }

        #endregion


        #region Nullable<TOut> Map<TOut>(StructMapper<TOut> mapper, Func<TOut> nullMapper = null)
        [TestMethod]
        public void MapToNullable_FromOptional_WithValidMapper_ShouldCallMapperFunction()
        {
            var @string = "a string";
            var isMapperCalled = false;
            var optional = @string.AsOptional();
            var nullable = optional.Map(value =>
            {
                isMapperCalled = true;
                Assert.AreEqual(@string, value);

                return value.Length;
            });

            Assert.IsTrue(isMapperCalled);
            Assert.IsFalse(optional.IsEmpty);
            Assert.IsTrue(nullable.HasValue);
            Assert.AreEqual(@string, optional.Value());
            Assert.AreEqual(@string.Length, nullable.Value);
        }

        [TestMethod]
        public void MapToNullable_FromEmptyOptional_WithValidMapper_ShouldSkipMapperFunction()
        {
            var isMapperCalled = false;
            var optional = Optional.Empty<string>();
            var nullable = optional.Map(value =>
            {
                isMapperCalled = true;
                return value.Length;
            });

            Assert.IsFalse(isMapperCalled);
            Assert.IsTrue(optional.IsEmpty);
            Assert.IsFalse(nullable.HasValue);
        }

        [TestMethod]
        public void MapToNullable_FromOptional_WithExceptionThrowingMapper_ShouldThrowException()
        {
            var @string = "a string";
            var optional = @string.AsOptional();
            Assert.ThrowsException<InvalidOperationException>(
                () => optional.Map(value => new InvalidOperationException().Throw<int>()));
        }

        [TestMethod]
        public void MapToNullable_FromOptional_WithNullMapper_ShouldThrowException()
        {
            var @string = "a string";
            var optional = @string.AsOptional();
            Assert.ThrowsException<ArgumentNullException>(
                () => optional.Map((Optional<string>.StructMapper<int>)null));
        }

        [TestMethod]
        public void MapToNullable_FromOptional_WithValidMapperAndValidNullMapper_ShouldCallMapperFunctionAndSkipNullMapperFunction()
        {
            var @string = "a string";
            var isMapperCalled = false;
            var isNullMapperCalled = false;
            var optional = @string.AsOptional();
            var nullable = optional.Map(
                mapper: value =>
                {
                    isMapperCalled = true;
                    Assert.AreEqual(@string, value);

                    return value.Length;
                },
                nullMapper: () =>
                {
                    isNullMapperCalled = false;
                    return 0;
                });

            Assert.IsTrue(isMapperCalled);
            Assert.IsFalse(isNullMapperCalled);
            Assert.IsFalse(optional.IsEmpty);
            Assert.IsTrue(nullable.HasValue);
            Assert.AreEqual(@string, optional.Value());
            Assert.AreEqual(@string.Length, nullable.Value);
        }

        [TestMethod]
        public void MapToNullable_FromEmptyOptional_WithValidMapperAndValidNullMapper_ShouldSkipMapperFunctionAndCallNullMapperFunction()
        {
            var isMapperCalled = false;
            var isNullMapperCalled = false;
            var optional = Optional.Empty<string>();
            var nullable = optional.Map(
                mapper: value =>
                {
                    isMapperCalled = true;

                    return value.Length;
                },
                nullMapper: () =>
                {
                    isNullMapperCalled = true;
                    return "bleh".Length;
                });

            Assert.IsFalse(isMapperCalled);
            Assert.IsTrue(isNullMapperCalled);
            Assert.IsTrue(optional.IsEmpty);
            Assert.IsTrue(nullable.HasValue);
            Assert.AreEqual("bleh".Length, nullable.Value);


            var nullable2 = optional.Map(
                mapper: value =>
                {
                    isMapperCalled = true;

                    return value.Length;
                },
                nullMapper: () =>
                {
                    isNullMapperCalled = true;
                    return 0;
                });

            Assert.IsFalse(isMapperCalled);
            Assert.IsTrue(isNullMapperCalled);
            Assert.IsTrue(nullable2.HasValue);
            Assert.AreEqual(0, nullable2.Value);
        }
        #endregion

        #region Nullable<TOut> Map<TOut>(Func<T, Nullable<TOut>> mapper, Func<TOut> nullMapper = null)
        [TestMethod]
        public void MapToNullable_FromOptional_WithValidNullableMapper_ShouldCallMapperFunction()
        {
            var @string = "a string";
            var isMapperCalled = false;
            var optional = @string.AsOptional();
            var nullable = optional.Map(value =>
            {
                isMapperCalled = true;
                Assert.AreEqual(@string, value);

                return value.Length.AsNullable();
            });

            Assert.IsTrue(isMapperCalled);
            Assert.IsFalse(optional.IsEmpty);
            Assert.IsTrue(nullable.HasValue);
            Assert.AreEqual(@string, optional.Value());
            Assert.AreEqual(@string.Length, nullable.Value);
        }

        [TestMethod]
        public void MapToNullable_FromEmptyOptional_WithValidNullableMapper_ShouldSkipMapperFunction()
        {
            var isMapperCalled = false;
            var optional = Optional.Empty<string>();
            var nullable = optional.Map(value =>
            {
                isMapperCalled = true;
                return value.Length.AsNullable();
            });

            Assert.IsFalse(isMapperCalled);
            Assert.IsTrue(optional.IsEmpty);
            Assert.IsFalse(nullable.HasValue);
        }

        [TestMethod]
        public void MapToNullable_FromOptional_WithNullReturningNullableMapper_ShouldCallMapperFunctionAndReturnEmpty()
        {
            var @string = "a string";
            var isMapperCalled = false;
            var optional = @string.AsOptional();
            var nullable = optional.Map(value =>
            {
                isMapperCalled = true;
                Assert.AreEqual(@string, value);

                return (int?)null;
            });

            Assert.IsTrue(isMapperCalled);
            Assert.IsFalse(optional.IsEmpty);
            Assert.IsFalse(nullable.HasValue);
            Assert.AreEqual(@string, optional.Value());
        }

        [TestMethod]
        public void MapToNullable_FromOptional_WithExceptionThrowingNullableMapper_ShouldThrowException()
        {
            var @string = "a string";
            var optional = @string.AsOptional();
            Assert.ThrowsException<InvalidOperationException>(
                () => optional.Map(value => new InvalidOperationException().Throw<int?>()));
        }

        [TestMethod]
        public void MapToNullable_FromOptional_WithNullNullableMapper_ShouldThrowException()
        {
            var @string = "a string";
            var optional = @string.AsOptional();
            Assert.ThrowsException<ArgumentNullException>(
                () => optional.Map((Func<string, int?>)null));
        }

        [TestMethod]
        public void MapToNullable_FromOptional_WithValidMapperAndValidNullNullableMapper_ShouldCallMapperFunctionAndSkipNullMapperFunction()
        {
            var @string = "a string";
            var isMapperCalled = false;
            var isNullMapperCalled = false;
            var optional = @string.AsOptional();
            var nullable = optional.Map(
                mapper: value =>
                {
                    isMapperCalled = true;
                    Assert.AreEqual(@string, value);

                    return value.Length.AsNullable();
                },
                nullMapper: () =>
                {
                    isNullMapperCalled = false;
                    return 0;
                });

            Assert.IsTrue(isMapperCalled);
            Assert.IsFalse(isNullMapperCalled);
            Assert.IsFalse(optional.IsEmpty);
            Assert.IsTrue(nullable.HasValue);
            Assert.AreEqual(@string, optional.Value());
            Assert.AreEqual(@string.Length, nullable.Value);
        }

        [TestMethod]
        public void MapToNullable_FromEmptyOptional_WithValidMapperAndValidNullNullableMapper_ShouldSkipMapperFunctionAndCallNullMapperFunction()
        {
            var isMapperCalled = false;
            var isNullMapperCalled = false;
            var optional = Optional.Empty<string>();
            var nullable = optional.Map(
                mapper: value =>
                {
                    isMapperCalled = true;

                    return value.Length.AsNullable();
                },
                nullMapper: () =>
                {
                    isNullMapperCalled = true;
                    return "bleh".Length;
                });

            Assert.IsFalse(isMapperCalled);
            Assert.IsTrue(isNullMapperCalled);
            Assert.IsTrue(optional.IsEmpty);
            Assert.IsTrue(nullable.HasValue);
            Assert.AreEqual("bleh".Length, nullable.Value);


            var nullable2 = optional.Map(
                mapper: value =>
                {
                    isMapperCalled = true;

                    return value.Length;
                },
                nullMapper: () =>
                {
                    isNullMapperCalled = true;
                    return 0;
                });

            Assert.IsFalse(isMapperCalled);
            Assert.IsTrue(isNullMapperCalled);
            Assert.IsTrue(nullable2.HasValue);
            Assert.AreEqual(0, nullable2.Value);
        }
        #endregion

        [TestMethod]
        public void NullableContextTests()
        {
            Exception e = null;
            Optional<string>? t = e?.InnerException?.Message.AsOptional();

            Assert.IsFalse(t.HasValue);
            Assert.AreEqual(default, t);

            // escape the nullable context by using the brackets
            Optional<string> t2 = (e?.InnerException?.Message).AsOptional();
            Assert.IsFalse(t2.HasValue);
            Assert.AreEqual(default, t2);


            // the point of this test is to highlight how using null-coalescing operator greedily converts
            // values to nullable types, unless a bracket is used to disable the greedy behavior
        }
    }
}
