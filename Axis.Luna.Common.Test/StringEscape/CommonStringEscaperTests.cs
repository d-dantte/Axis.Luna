using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Axis.Luna.Extensions;
using Axis.Luna.Common.StringEscape;

namespace Axis.Luna.Common.Test.StringEscape
{
    [TestClass]
    public class CommonStringEscaperTests
    {
        [TestMethod]
        public void IsValidEscapeSequence_Tests()
        {
            var escaper = new CommonStringEscaper();

            // simple
            var escapes = EnumerableUtil.Of(
                "\\0", "\\a", "\\b", "\\f", "\\n", "\\r", "\\t", "\\v", "\\'", "\\\"", "\\\\");

            escapes.ForEvery(escape =>
            {
                var isValid = escaper.IsValidEscapeSequence(escape);
                Assert.IsTrue(isValid);
            });

            // ascii
            escapes = Enumerable
                .Range(0, byte.MaxValue + 1)
                .Select(v => $"\\x{v:x2}")
                .ToArray();

            escapes.ForEvery(escape =>
            {
                var isValid = escaper.IsValidEscapeSequence(escape);
                Assert.IsTrue(isValid);
            });

            // unicode
            escapes = Enumerable
                .Range(0, ushort.MaxValue + 1)
                .Select(v => $"\\u{v:x4}")
                .ToArray();

            escapes.ForEvery(escape =>
            {
                var isValid = escaper.IsValidEscapeSequence(escape);
                Assert.IsTrue(isValid);
            });

            Assert.IsFalse(escaper.IsValidEscapeSequence(default));
            Assert.IsFalse(escaper.IsValidEscapeSequence("bleh"));
            Assert.IsFalse(escaper.IsValidEscapeSequence("\\blehblehbleh"));
        }

        [TestMethod]
        public void Escape_Tests()
        {
            var escaper = new CommonStringEscaper();

            Assert.ThrowsException<ArgumentException>(() => escaper.Escape(default));

            // simple
            Assert.AreEqual<string>("\\x61\\x62\\x63\\x64", escaper.Escape("abcd"));
            Assert.AreEqual<string>("\\0", escaper.Escape("\0"));
            Assert.AreEqual<string>("\\a", escaper.Escape("\a"));
            Assert.AreEqual<string>("\\b", escaper.Escape("\b"));
            Assert.AreEqual<string>("\\f", escaper.Escape("\f"));
            Assert.AreEqual<string>("\\n", escaper.Escape("\n"));
            Assert.AreEqual<string>("\\r", escaper.Escape("\r"));
            Assert.AreEqual<string>("\\t", escaper.Escape("\t"));
            Assert.AreEqual<string>("\\v", escaper.Escape("\v"));
            Assert.AreEqual<string>("\\'", escaper.Escape("\'"));
            Assert.AreEqual<string>("\\\"", escaper.Escape("\""));
            Assert.AreEqual<string>("\\\\", escaper.Escape("\\"));

            // ascii
            Enumerable
                .Range(0, byte.MaxValue + 1)
                .Except(EnumerableUtil.Of('\0', '\a', '\b', '\f', '\n', '\r', '\t', '\v', '\'', '\"', '\\').Select(v => (int)v))
                .ForEvery(value => Assert.AreEqual<string>($"\\x{value:x2}", escaper.Escape(CharSequence.Of(Convert.ToChar(value)))));

            // unicode
            Enumerable
                .Range(byte.MaxValue + 1, ushort.MaxValue - byte.MaxValue)
                .ForEvery(value => Assert.AreEqual<string>($"\\u{value:x4}", escaper.Escape(CharSequence.Of(Convert.ToChar(value)))));
        }

        [TestMethod]
        public void Escape_WithPredicate_Tests()
        {
            var escaper = new CommonStringEscaper();

            Assert.ThrowsException<ArgumentException>(() => escaper.Escape(default, t => true));
            Assert.ThrowsException<ArgumentNullException>(() => escaper.Escape("abcd", null!));

            Assert.AreEqual<string>("abc\\x64", escaper.Escape("abcd", c => c == 'd'));
        }

        [TestMethod]
        public void Unescape_Sequence_Tests()
        {
            var escaper = new CommonStringEscaper();

            Assert.ThrowsException<ArgumentException>(() => escaper.Unescape(default));
            Assert.AreEqual<string>("abcd", escaper.Unescape("abcd"));

            // simple
            Assert.AreEqual<string>("\0", escaper.Unescape("\\0"));
            Assert.AreEqual<string>("\a", escaper.Unescape("\\a"));
            Assert.AreEqual<string>("\b", escaper.Unescape("\\b"));
            Assert.AreEqual<string>("\f", escaper.Unescape("\\f"));
            Assert.AreEqual<string>("\n", escaper.Unescape("\\n"));
            Assert.AreEqual<string>("\r", escaper.Unescape("\\r"));
            Assert.AreEqual<string>("\t", escaper.Unescape("\\t"));
            Assert.AreEqual<string>("\v", escaper.Unescape("\\v"));
            Assert.AreEqual<string>("\'", escaper.Unescape("\\'"));
            Assert.AreEqual<string>("\"", escaper.Unescape("\\\""));
            Assert.AreEqual<string>("\\", escaper.Unescape("\\\\"));

            // ascii
            Enumerable
                .Range(0, byte.MaxValue + 1)
                .Except(EnumerableUtil.Of('\0', '\a', '\b', '\f', '\n', '\r', '\t', '\v', '\'', '\"', '\\').Select(v => (int)v))
                .ForEvery(value =>
                {
                    Assert.AreEqual<string>(Convert.ToChar(value).ToString(), escaper.Unescape($"\\x{value:x2}"));
                });

            // unicode
            Enumerable
                .Range(byte.MaxValue + 1, ushort.MaxValue - byte.MaxValue)
                .ForEvery(value =>
                {
                    Assert.AreEqual<string>(Convert.ToChar(value).ToString(), escaper.Unescape($"\\u{value:x4}"));
                });
        }

        [TestMethod]
        public void UnescapeString_Tests()
        {
            var escaper = new CommonStringEscaper();

            Assert.IsNull(escaper.UnescapeString(null));

            var esString = "the\\n quick\\x0a brown\\u000a fox jumps over the lazy dog";
            var expected = "the\n quick\n brown\n fox jumps over the lazy dog";
            var result = escaper.UnescapeString(esString);
            Assert.AreEqual(expected, result);

            esString = "the quick brown fox jumps over the laxy dog\\";
            var ex = Assert.ThrowsException<InvalidEscapeSequence>(() => escaper.UnescapeString(esString));
            Assert.AreEqual<string>("\\", ex.EscapeSequence);

            esString = "the quick brown fox jumps over the laxy dog\\2s";
            ex = Assert.ThrowsException<InvalidEscapeSequence>(() => escaper.UnescapeString(esString));
            Assert.AreEqual<string>("\\2s", ex.EscapeSequence);

            esString = "the quick brown fox jumps over the laxy dog\\1234567 and other things.";
            ex = Assert.ThrowsException<InvalidEscapeSequence>(() => escaper.UnescapeString(esString));
            Assert.AreEqual<string>("\\12345", ex.EscapeSequence);

            esString = "the fox";
            expected = "the fox";
            result = escaper.UnescapeString(esString);
            Assert.AreEqual(expected, result);
            Assert.IsTrue(object.ReferenceEquals(result, esString));
        }
    }
}
