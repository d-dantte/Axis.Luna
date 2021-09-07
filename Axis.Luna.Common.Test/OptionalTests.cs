using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Axis.Luna.Extensions;

namespace Axis.Luna.Common.Test
{
    [TestClass]
    public class OptionalTests
    {
        #region Do
        [TestMethod]
        public void Do_FromOptional_WithValidAction_ShouldCallActionAndSkipNullAction()
        {
            bool isActionCalled = false;
            bool isNullActionCalled = false;
            var optional = "a string".AsOptional();

            //try the action
            optional.Do(
                nullAction: null,
                action: value =>
                {
                    Assert.AreEqual("a string", value);
                    isActionCalled = true;
                });

            Assert.IsTrue(isActionCalled);

            //try with null action
            optional.Do(
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
        public void Do_FRomOptional_WithNullAction_ShouldThrowArgumentNullException()
        {
            var optional = "a string".AsOptional();

            Assert.ThrowsException<ArgumentNullException>(() => optional.Do(null));
        }
        #endregion

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
            Assert.AreEqual(@string, optional.Value);
            Assert.IsTrue(@string.ToCharArray().SequenceEqual(optional2.Value));
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
            Assert.AreEqual(null, optional.Value);
            Assert.AreEqual(null, optional2.Value);
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
            Assert.AreEqual(@string, optional.Value);
            Assert.IsNull(optional2.Value);
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
            Assert.AreEqual(@string, optional.Value);
            Assert.IsTrue(@string.ToCharArray().SequenceEqual(optional2.Value));
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
            Assert.AreEqual(null, optional.Value);
            Assert.IsTrue("bleh".ToCharArray().SequenceEqual(optional2.Value));


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
            Assert.AreEqual(null, optional3.Value);
        }
        #endregion

        #region public Optional<TOut> Map<TOut>(Func<T, Optional<TOut>> mapper, Func<TOut> nullMapper = null)

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
            Assert.AreEqual(@string, optional.Value);
            Assert.IsTrue(@string.ToCharArray().SequenceEqual(optional2.Value));
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
            Assert.AreEqual(null, optional.Value);
            Assert.AreEqual(null, optional2.Value);
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
            Assert.AreEqual(@string, optional.Value);
            Assert.IsNull(optional2.Value);
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
            Assert.AreEqual(@string, optional.Value);
            Assert.IsTrue(@string.ToCharArray().SequenceEqual(optional2.Value));
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
            Assert.AreEqual(null, optional.Value);
            Assert.IsTrue("bleh".ToCharArray().SequenceEqual(optional2.Value));


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
            Assert.AreEqual(null, optional3.Value);
        }

        #endregion
    }
}
