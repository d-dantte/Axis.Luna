﻿using Axis.Luna.Common.Unions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Axis.Luna.Common.Test.Unions
{
    [TestClass]
    public class Union2Tests
    {
        [TestMethod]
        public void Construction_Tests()
        {
            var runion = new MyRefUnion(4); //no exceptions thrown
            runion = new MyRefUnion("some string"); //no exceptions thrown

            Assert.ThrowsException<TypeInitializationException>(() => new DuplicateTypeUnion(4));
            Assert.ThrowsException<TypeInitializationException>(() => new DuplicateTypeUnion("stuff"));
        }

        [TestMethod]
        public void Is_Tests()
        {
            var union = new MyRefUnion(5);
            Assert.IsFalse(union.Is(out string _));
            Assert.IsTrue(union.Is(out int iv));
            Assert.AreEqual(4, iv);

            union = new MyRefUnion("5");
            Assert.IsFalse(union.Is(out string sv));
            Assert.IsTrue(union.Is(out int _));
            Assert.AreEqual("5", sv);

            union = new MyRefUnion(null);
            Assert.IsFalse(union.Is(out string _));
            Assert.IsFalse(union.Is(out int _));
            Assert.IsTrue(union.IsNull());
        }

        [TestMethod]
        public void MapMatch_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).MapMatch(null, s => s));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).MapMatch(i => i, null));

            var iunion = new MyRefUnion(5);
            var result = iunion.MapMatch(
                i => i > 8,
                s => string.IsNullOrEmpty(s));
            Assert.IsFalse(result);

            iunion = new MyRefUnion("5");
            result = iunion.MapMatch(
                i => i > 8,
                s => s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            result = iunion.MapMatch(
                i => i > 8,
                s => s.Equals("bleh"),
                () => true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Consume_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).ConsumeMatch(null, Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).ConsumeMatch(Console.WriteLine, null));

            var iunion = new MyRefUnion(5);
            var result = false;
            iunion.ConsumeMatch(
                i => result = i > 8,
                s => result = string.IsNullOrEmpty(s));
            Assert.IsFalse(result);

            iunion = new MyRefUnion("5");
            iunion.ConsumeMatch(
                i => result = i > 8,
                s => result = s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            iunion.ConsumeMatch(
                i => result = i > 8,
                s => result = s.Equals("bleh"),
                () => result = true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void With_Tests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).WithMatch(null, Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => new MyRefUnion(5).WithMatch(Console.WriteLine, null));

            var iunion = new MyRefUnion(5);
            var result = false;
            iunion.WithMatch(
                i => result = i > 8,
                s => result = string.IsNullOrEmpty(s));
            Assert.IsFalse(result);

            iunion = new MyRefUnion("5");
            iunion.WithMatch(
                i => result = i > 8,
                s => result = s is not null);
            Assert.IsTrue(result);

            iunion = new MyRefUnion(null);
            iunion.WithMatch(
                i => result = i > 8,
                s => result = s.Equals("bleh"),
                () => result = true);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ImplicitsAndOf_Tests()
        {
            // implicits
            All all = 5;
            Assert.IsTrue(all.Is(out int _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = "5";
            Assert.IsFalse(all.Is(out int _));
            Assert.IsTrue(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            // of
            all = All.Of(5);
            Assert.IsTrue(all.Is(out int _));
            Assert.IsFalse(all.Is(out string _));
            Assert.IsFalse(all.IsNull());

            all = All.Of("5");
            Assert.IsFalse(all.Is(out int _));
            Assert.IsTrue(all.Is(out string _));
            Assert.IsFalse(all.IsNull());
        }
    }

    internal class MyRefUnion(object value)
    : RefUnion<int, string, MyRefUnion>(value)
    {
    }

    internal class All(object value) :
        RefUnion<int, string, All>(value),
        IUnionImplicits<int, string, All>,
        IUnionOf<int, string, All>
    {
        public static All Of(int value) => new(value);

        public static All Of(string value) => new(value);

        public static implicit operator All(int value) => new(value);

        public static implicit operator All(string value) => new(value);
    }

    internal struct MyValueUnion :
        IUnion<int, string, MyValueUnion>,
        IUnionOf<int, string, MyValueUnion>
    {
        object IUnion<int, string, MyValueUnion>.Value { get; }

        public static MyValueUnion Of(int value)
        {
            throw new NotImplementedException();
        }

        public static MyValueUnion Of(string value)
        {
            throw new NotImplementedException();
        }

        public void ConsumeMatch(Action<int> t1Consumer, Action<string> t2Consumer, Action nullConsumer = null)
        {
            throw new NotImplementedException();
        }

        public bool Is(out int value)
        {
            throw new NotImplementedException();
        }

        public bool Is(out string value)
        {
            throw new NotImplementedException();
        }

        public bool IsNull()
        {
            throw new NotImplementedException();
        }

        public TOut MapMatch<TOut>(Func<int, TOut> t1Mapper, Func<string, TOut> t2Mapper, Func<TOut> nullMap = null)
        {
            throw new NotImplementedException();
        }

        public MyValueUnion WithMatch(Action<int> t1Consumer, Action<string> t2Consumer, Action nullConsumer = null)
        {
            throw new NotImplementedException();
        }
    }

    internal class DuplicateTypeUnion(object value) :
        RefUnion<int, int, DuplicateTypeUnion>(value),
#pragma warning disable CS1956 // Member implements interface member with multiple matches at run-time
        IUnionImplicits<int, int, DuplicateTypeUnion>
#pragma warning restore CS1956 // Member implements interface member with multiple matches at run-time
    {
        public static implicit operator DuplicateTypeUnion(int value)
        {
            throw new NotImplementedException();
        }
    }
}
