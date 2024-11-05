using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Axis.Luna.Common.Test
{

    [TestClass]
    public class CharSequenceReaderTests
    {
        [TestMethod]
        public void CharSequenceReader_Constructor_ShouldInitializeCorrectly()
        {
            var reader = new CharSequenceReader("test string");
            Assert.AreEqual(0, reader.CurrentIndex);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CharSequenceReader_Constructor_ShouldThrowOnNull()
        {
            var reader = new CharSequenceReader(null);
        }

        [TestMethod]
        public void CharSequenceReader_Reset_ShouldResetIndexToZero()
        {
            var reader = new CharSequenceReader("test string").Advance(5);
            reader.Reset();
            Assert.AreEqual(0, reader.CurrentIndex);
        }

        [TestMethod]
        public void CharSequenceReader_Reset_ShouldSetIndexToGivenValue()
        {
            var reader = new CharSequenceReader("test string").Advance(3);
            reader.Reset(2);
            Assert.AreEqual(2, reader.CurrentIndex);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CharSequenceReader_Reset_ShouldThrowOnNegativeIndex()
        {
            var reader = new CharSequenceReader("test string").Reset(-1);
        }

        [TestMethod]
        public void CharSequenceReader_Advance_ShouldAdvanceByOne()
        {
            var reader = new CharSequenceReader("test string");
            reader.Advance();
            Assert.AreEqual(1, reader.CurrentIndex);
        }

        [TestMethod]
        public void CharSequenceReader_Advance_ShouldAdvanceByGivenSteps()
        {
            var reader = new CharSequenceReader("test string");
            reader.Advance(4);
            Assert.AreEqual(4, reader.CurrentIndex);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CharSequenceReader_Advance_ShouldThrowOnNegativeSteps()
        {
            var reader = new CharSequenceReader("test string");
            reader.Advance(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CharSequenceReader_Advance_ShouldThrowOnStepsExceedingLength()
        {
            var reader = new CharSequenceReader("test string");
            reader.Advance(50);
        }

        [TestMethod]
        public void CharSequenceReader_Back_ShouldMoveBackByOne()
        {
            var reader = new CharSequenceReader("test string").Advance(5);
            reader.Back();
            Assert.AreEqual(4, reader.CurrentIndex);
        }

        [TestMethod]
        public void CharSequenceReader_Back_ShouldMoveBackByGivenSteps()
        {
            var reader = new CharSequenceReader("test string").Advance(5);
            reader.Back(3);
            Assert.AreEqual(2, reader.CurrentIndex);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CharSequenceReader_Back_ShouldThrowOnNegativeSteps()
        {
            var reader = new CharSequenceReader("test string").Back(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CharSequenceReader_Back_ShouldThrowOnStepsExceedingCurrentIndex()
        {
            var reader = new CharSequenceReader("test string").Back(1);
        }

        [TestMethod]
        public void CharSequenceReader_TryReadExactly_ShouldReadSpecifiedCount()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryReadExactly(4, out var chars);
            Assert.IsTrue(success);
            Assert.AreEqual("test", chars);
            Assert.AreEqual(4, reader.CurrentIndex);
        }

        [TestMethod]
        public void CharSequenceReader_TryReadExactly_ShouldReturnFalseOnInsufficientCharacters()
        {
            var reader = new CharSequenceReader("test string").Advance(9);
            var success = reader.TryReadExactly(5, out var chars);
            Assert.IsFalse(success);
            Assert.AreEqual(default, chars);
            Assert.AreEqual(9, reader.CurrentIndex); // Index should not change
        }

        [TestMethod]
        public void CharSequenceReader_TryRead_ShouldReadUpToMaxCount()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryRead(4, out var chars);
            Assert.IsTrue(success);
            Assert.AreEqual("test", chars);
            Assert.AreEqual(4, reader.CurrentIndex);
        }

        [TestMethod]
        public void CharSequenceReader_TryRead_ShouldReturnFalseIfNothingToRead()
        {
            var reader = new CharSequenceReader("test string").Advance(11); // Reached end
            var success = reader.TryRead(5, out var chars);
            Assert.IsFalse(success);
            Assert.AreEqual(default, chars);
        }

        [TestMethod]
        public void CharSequenceReader_TryReadExactlyWithPredicate_ShouldSucceedIfPredicatePasses()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryReadExactly(4, cs => cs == "test", out var chars);
            Assert.IsTrue(success);
            Assert.AreEqual("test", chars);
            Assert.AreEqual(4, reader.CurrentIndex);
        }

        [TestMethod]
        public void CharSequenceReader_TryReadExactlyWithPredicate_ShouldFailIfPredicateFails()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryReadExactly(4, cs => cs == "fail", out var chars);
            Assert.IsFalse(success);
            Assert.AreEqual("test", chars);
            Assert.AreEqual(0, reader.CurrentIndex); // Index should not change
        }

        [TestMethod]
        public void CharSequenceReader_TryReadWithPredicate_ShouldSucceedIfPredicatePasses()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryRead(4, cs => cs == "test", out var chars);
            Assert.IsTrue(success);
            Assert.AreEqual("test", chars);
            Assert.AreEqual(4, reader.CurrentIndex);
        }

        [TestMethod]
        public void CharSequenceReader_TryPeekExactly_ShouldPeekSpecifiedCountWithoutAdvancingIndex()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryPeekExactly(4, out var chars);
            Assert.IsTrue(success);
            Assert.AreEqual("test", chars);
            Assert.AreEqual(0, reader.CurrentIndex); // Index should not change
        }

        [TestMethod]
        public void CharSequenceReader_TryPeekExactly_ShouldReturnFalseOnInsufficientCharacters()
        {
            var reader = new CharSequenceReader("test string").Advance(9);
            var success = reader.TryPeekExactly(5, out var chars);
            Assert.IsFalse(success);
            Assert.AreEqual(default, chars);
            Assert.AreEqual(9, reader.CurrentIndex); // Index should not change
        }

        [TestMethod]
        public void CharSequenceReader_TryPeek_ShouldPeekUpToMaxCountWithoutAdvancingIndex()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryPeek(4, out var chars);
            Assert.IsTrue(success);
            Assert.AreEqual("test", chars);
            Assert.AreEqual(0, reader.CurrentIndex); // Index should not change
        }

        [TestMethod]
        public void CharSequenceReader_TryPeekExactlyWithPredicate_ShouldSucceedIfPredicatePasses()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryPeekExactly(4, cs => cs == "test", out var chars);
            Assert.IsTrue(success);
            Assert.AreEqual("test", chars);
            Assert.AreEqual(0, reader.CurrentIndex); // Index should not change
        }

        [TestMethod]
        public void CharSequenceReader_TryPeekExactlyWithPredicate_ShouldFailIfPredicateFails()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryPeekExactly(4, cs => cs == "fail", out var chars);
            Assert.IsFalse(success);
            Assert.AreEqual("test", chars);
            Assert.AreEqual(0, reader.CurrentIndex); // Index should not change
        }

        [TestMethod]
        public void CharSequenceReader_TryPeekWithPredicate_ShouldSucceedIfPredicatePasses()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryPeek(4, cs => cs == "test", out var chars);
            Assert.IsTrue(success);
            Assert.AreEqual("test", chars);
            Assert.AreEqual(0, reader.CurrentIndex); // Index should not change
        }
    }

}
