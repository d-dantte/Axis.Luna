using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

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
            var reader = new CharSequenceReader((string) null);
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
            Assert.AreEqual<string>("test", chars);
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
            Assert.AreEqual<string>("test", chars);
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
            Assert.AreEqual<string>("test", chars);
            Assert.AreEqual(4, reader.CurrentIndex);
        }

        [TestMethod]
        public void CharSequenceReader_TryReadExactlyWithPredicate_ShouldFailIfPredicateFails()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryReadExactly(4, cs => cs == "fail", out var chars);
            Assert.IsFalse(success);
            Assert.AreEqual<string>("test", chars);
            Assert.AreEqual(0, reader.CurrentIndex); // Index should not change
        }

        [TestMethod]
        public void CharSequenceReader_TryReadWithPredicate_ShouldSucceedIfPredicatePasses()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryRead(4, cs => cs == "test", out var chars);
            Assert.IsTrue(success);
            Assert.AreEqual<string>("test", chars);
            Assert.AreEqual(4, reader.CurrentIndex);
        }

        [TestMethod]
        public void CharSequenceReader_TryPeekExactly_ShouldPeekSpecifiedCountWithoutAdvancingIndex()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryPeekExactly(4, out var chars);
            Assert.IsTrue(success);
            Assert.AreEqual<string>("test", chars);
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
            Assert.AreEqual<string>("test", chars);
            Assert.AreEqual(0, reader.CurrentIndex); // Index should not change
        }

        [TestMethod]
        public void CharSequenceReader_TryPeekExactlyWithPredicate_ShouldSucceedIfPredicatePasses()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryPeekExactly(4, cs => cs == "test", out var chars);
            Assert.IsTrue(success);
            Assert.AreEqual<string>("test", chars);
            Assert.AreEqual(0, reader.CurrentIndex); // Index should not change
        }

        [TestMethod]
        public void CharSequenceReader_TryPeekExactlyWithPredicate_ShouldFailIfPredicateFails()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryPeekExactly(4, cs => cs == "fail", out var chars);
            Assert.IsFalse(success);
            Assert.AreEqual<string>("test", chars);
            Assert.AreEqual(0, reader.CurrentIndex); // Index should not change
        }

        [TestMethod]
        public void CharSequenceReader_TryPeekWithPredicate_ShouldSucceedIfPredicatePasses()
        {
            var reader = new CharSequenceReader("test string");
            var success = reader.TryPeek(4, cs => cs == "test", out var chars);
            Assert.IsTrue(success);
            Assert.AreEqual<string>("test", chars);
            Assert.AreEqual(0, reader.CurrentIndex); // Index should not change
        }


        [TestMethod]
        public void TryPeek_NullPredicate_ThrowsArgumentNullException()
        {
            var reader = new CharSequenceReader("abc");
            Assert.ThrowsException<ArgumentNullException>(() => reader.TryPeek(null!, out _));
        }

        [TestMethod]
        public void TryPeek_EmptyReader_ReturnsFalse()
        {
            var reader = new CharSequenceReader("");
            bool result = reader.TryPeek(c => true, out var chars);
            Assert.IsFalse(result);
            Assert.AreEqual(default, chars);
            Assert.AreEqual(0, reader.CurrentIndex);
        }

        [TestMethod]
        public void TryPeek_SingleCharacterMatch_ReturnsTrueAndCharacter()
        {
            var reader = new CharSequenceReader("a");
            bool result = reader.TryPeek(c => c[0] == 'a', out var chars);
            Assert.IsTrue(result);
            Assert.AreEqual("a", chars.ToString());
            Assert.AreEqual(0, reader.CurrentIndex);
        }

        [TestMethod]
        public void TryPeek_MultipleCharacterMatch_ReturnsTrueAndString()
        {
            var reader = new CharSequenceReader("abcde");
            bool result = reader.TryPeek(c => 'a' == c[^1] || 'b' == c[^1] || 'c' == c[^1], out var chars);
            Assert.IsTrue(result);
            Assert.AreEqual("abc", chars.ToString());
            Assert.AreEqual(0, reader.CurrentIndex);
        }

        [TestMethod]
        public void TryPeek_NoCharacterMatch_ReturnsFalseAndEmptyString()
        {
            var reader = new CharSequenceReader("abcde");
            bool result = reader.TryPeek(c => c.ToString().Contains('x'), out var chars);
            Assert.IsFalse(result);
            Assert.AreEqual(0, reader.CurrentIndex);
        }

        [TestMethod]
        public void TryPeek_PartialMatchThenNoMatch_ReturnsTrueAndPartialString()
        {
            var reader = new CharSequenceReader("ab12cd");
            bool result = reader.TryPeek(c => char.IsLetter(c[^1]), out var chars);
            Assert.IsTrue(result);
            Assert.AreEqual("ab", chars.ToString());
            Assert.AreEqual(0, reader.CurrentIndex);
        }

        [TestMethod]
        public void TryPeek_AllMatch_ReturnsTrueAndFullString()
        {
            var reader = new CharSequenceReader("abcdef");
            bool result = reader.TryPeek(c => c.Length > 0, out var chars);
            Assert.IsTrue(result);
            Assert.AreEqual("abcdef", chars.ToString());
            Assert.AreEqual(0, reader.CurrentIndex);
        }

        [TestMethod]
        public void TryPeek_LongStringPartialMatch_ReturnsTrue()
        {
            string longString = new string('a', 1000) + "b" + new string('c', 1000);
            var reader = new CharSequenceReader(longString);
            bool result = reader.TryPeek(c => c.ToString().All(ch => ch == 'a'), out var chars);
            Assert.IsTrue(result);
            Assert.AreEqual(new string('a', 1000), chars.ToString());
            Assert.AreEqual(0, reader.CurrentIndex);
        }

        [TestMethod]
        public void TryRead_Tests()
        {
            CharSequenceReader reader = "";
            Assert.ThrowsException<ArgumentNullException>(
                () => reader.TryRead(null, out var t));

            reader = "abcd1234";
            var result = reader.TryRead(c => char.IsLetter(c[^1]), out var chars);
            Assert.IsTrue(result);
            Assert.AreEqual(4, reader.CurrentIndex);

            reader.Reset();
            result = reader.TryRead(c => char.IsNumber(c[^1]), out chars);
            Assert.IsFalse(result);
            Assert.AreEqual(0, reader.CurrentIndex);
        }
    }

}
