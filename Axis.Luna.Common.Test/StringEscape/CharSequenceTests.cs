using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Linq;

namespace Axis.Luna.Common.Test.StringEscape
{
    [TestClass]
    public class CharSequenceTests
    {
        [TestMethod]
        public void Constructor_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new CharSequence(null, 0, 0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new CharSequence("", -1, 0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new CharSequence("", 0, -2));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new CharSequence("", 0, 3));

            var seq = new CharSequence("abcd", 0, -1);
            Assert.AreEqual("abcd", seq.Ref);
            Assert.AreEqual(0, seq.Segment.Offset);
            Assert.AreEqual(4, seq.Length);
            Assert.AreEqual(4, seq.Segment.Count);

            var seq2 = new CharSequence("abcd", 0, 4);
            Assert.AreEqual(seq, seq2);

            seq2 = new CharSequence("abcd", 0);
            Assert.AreEqual(seq, seq2);

            seq2 = new CharSequence("abcd");
            Assert.AreEqual(seq, seq2);

            seq2 = new CharSequence("abcd", ..);
            Assert.AreEqual(seq, seq2);

            seq2 = new CharSequence('a');
            Assert.AreEqual("a", seq2.Ref);
            Assert.AreEqual(0, seq2.Segment.Offset);
            Assert.AreEqual(1, seq2.Length);
        }

        [TestMethod]
        public void Of_Tests()
        {
            var seq = new CharSequence("abcd", 0, -1);

            var seq2 = CharSequence.Of("abcd", 0, -1);
            Assert.AreEqual(seq, seq2);

            seq2 = CharSequence.Of("abcd", 0, 4);
            Assert.AreEqual(seq, seq2);

            seq2 = CharSequence.Of("abcd", 0);
            Assert.AreEqual(seq, seq2);

            seq2 = CharSequence.Of("abcd");
            Assert.AreEqual(seq, seq2);

            seq2 = CharSequence.Of("abcd", ..);
            Assert.AreEqual(seq, seq2);

            seq2 = CharSequence.Of('a');
            Assert.AreEqual("a", seq2.Ref);
            Assert.AreEqual(0, seq2.Segment.Offset);
            Assert.AreEqual(1, seq2.Length);
        }

        [TestMethod]
        public void Implicit_String_Tests()
        {
            var seq = new CharSequence("abcd");
            CharSequence seq2 = "abcd";

            Assert.AreEqual(seq, seq2);
        }

        [TestMethod]
        public void IsDefault_Test()
        {
            var seq = CharSequence.Of("abcd");
            Assert.IsFalse(seq.IsDefault);

            seq = CharSequence.Of("abcd", 0, 0);
            Assert.IsFalse(seq.IsDefault);

            seq = CharSequence.Default;
            Assert.IsTrue(seq.IsDefault);
        }

        [TestMethod]
        public void GetEnumerator_Tests()
        {
            var seq = CharSequence.Of("abcdef", 1, 3);
            var enm = seq.GetEnumerator();
            var enmm = ((IEnumerable)seq).GetEnumerator();

            Assert.IsNotNull(enm);
            Assert.IsNotNull(enmm);
            Assert.IsInstanceOfType<CharSequence.Enumerator>(enm);
            Assert.IsInstanceOfType<CharSequence.Enumerator>(enmm);
        }

        [TestMethod]
        public void Indexer1_Tests()
        {
            var seq = CharSequence.Of("abcde");
            var @char = seq[3];
            Assert.AreEqual('d', @char);
            Assert.ThrowsException<InvalidOperationException>(() => CharSequence.Default[0]);
        }

        [TestMethod]
        public void Indexer2_Tests()
        {
            var seq = CharSequence.Of("abcde");
            var seq2 = seq[2..4];
            Assert.AreEqual(2, seq2.Length);
            Assert.AreEqual(2, seq2.Segment.Offset);
            Assert.AreEqual("cd", seq2);
            Assert.ThrowsException<InvalidOperationException>(() => CharSequence.Default[..]);
        }

        [TestMethod]
        public void AsSpan_Tests()
        {
            Assert.ThrowsException<InvalidOperationException>(() => CharSequence.Default.AsSpan(..));
            Assert.ThrowsException<InvalidOperationException>(() => CharSequence.Default.AsSpan(1, 3));
            Assert.ThrowsException<InvalidOperationException>(() => CharSequence.Default.AsSpan(1));
            Assert.ThrowsException<InvalidOperationException>(() => CharSequence.Default.AsSpan());

            var seq = CharSequence.Of("abcde");
            var span = seq.AsSpan(2..^1);
            Assert.AreEqual(2, span.Length);
            Assert.IsTrue(System.Linq.Enumerable.SequenceEqual(
                span.ToArray(),
                "cd"));

            span = seq.AsSpan(2, 2);
            Assert.AreEqual(2, span.Length);
            Assert.IsTrue(System.Linq.Enumerable.SequenceEqual(
                span.ToArray(),
                "cd"));

            span = seq.AsSpan(2);
            Assert.AreEqual(3, span.Length);
            Assert.IsTrue(System.Linq.Enumerable.SequenceEqual(
                span.ToArray(),
                "cde"));

            span = seq.AsSpan();
            Assert.AreEqual(5, span.Length);
            Assert.IsTrue(System.Linq.Enumerable.SequenceEqual(
                span.ToArray(),
                "abcde"));
        }

        [TestMethod]
        public void Equals_Tests()
        {
            var str = "abcde";
            var seq = CharSequence.Of(str);
            var seqq = CharSequence.Of(str);
            var seqq1 = CharSequence.Of(str, 1, 2);
            var seqq2 = CharSequence.Of(str, 3, 2);
            var seq2 = CharSequence.Of("abc");
            var seq3 = CharSequence.Of("de");

            Assert.AreEqual(CharSequence.Default, default);
            Assert.AreNotEqual(CharSequence.Default, seq);
            Assert.AreNotEqual(seq2, seq);
            Assert.AreEqual(seq, seqq);
            Assert.AreNotEqual(seqq2, seqq1);
            Assert.AreEqual(seqq2, seq3);
        }

        [TestMethod]
        public void Object_Equals_Tests()
        {
            var seq = CharSequence.Of("abc");
            Assert.IsTrue(seq.Equals((object)seq));
            Assert.IsFalse(seq.Equals((object)"abc"));
        }

        [TestMethod]
        public void ToString_Test()
        {
            Assert.IsNull(CharSequence.Default.ToString());
            Assert.AreEqual("abc", CharSequence.Of("abc").ToString());
        }

        [TestMethod]
        public void GetHashCode_Tests()
        {
            var seq = CharSequence.Of("abc");
            var hashCode = seq.GetHashCode();
            var expected = "abc".Aggregate(0, HashCode.Combine);
            Assert.AreEqual(expected, hashCode);
            Assert.AreEqual(0, CharSequence.Default.GetHashCode());
        }

        [TestMethod]
        public void FullHash_Tests()
        {
            var seq = CharSequence.Of("abc");
            var hashCode = seq.GetHashCode();
            var expected = HashCode.Combine(hashCode, seq.Segment);
            Assert.AreEqual(expected, seq.FullHash());
        }

        [TestMethod]
        public void EqualsOperator_Tests()
        {
            var seq = CharSequence.Of("abcd");
            var seq2 = CharSequence.Of("abcd");
            var seq3 = CharSequence.Of("abcdef");

            Assert.IsTrue(CharSequence.Default == default);
            Assert.IsTrue(seq == seq2);
            Assert.IsTrue(seq != seq3);
        }

        [TestMethod]
        public void CharSequence_Enumerator_Construction()
        {
            var seq = CharSequence.Of("abcd");
            var tor = new CharSequence.Enumerator(seq);
            Assert.IsNotNull(tor);
        }

        [TestMethod]
        public void General_Tests()
        {
            var seq = CharSequence.Of("abcd");
            var tor = new CharSequence.Enumerator(seq);
            var dtor = new CharSequence.Enumerator(default);

            Assert.ThrowsException<InvalidOperationException>(() => tor.Current);
            Assert.IsTrue(tor.MoveNext());
            Assert.IsFalse(dtor.MoveNext());
            Assert.AreEqual('a', tor.Current);
            Assert.AreEqual('a', ((IEnumerator)tor).Current);

            tor.Reset();
            tor.Dispose();
            Assert.ThrowsException<InvalidOperationException>(() => tor.Current);
        }
    }
}
