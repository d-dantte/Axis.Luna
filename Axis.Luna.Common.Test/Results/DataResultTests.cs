﻿using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Axis.Luna.Common.Test.Results
{
    [TestClass]
    public class DataResultTests
    {
        [TestMethod]
        public void Construction_Tests()
        {
            var result = new DataResult<int>(34);

            Assert.AreEqual(34, result.Data);
        }

        [TestMethod]
        public void Bind_Tests()
        {
            var result = new DataResult<string>("34");

            Assert.ThrowsException<ArgumentNullException>(() => result.Bind<int>(null));

            var iresult2 = result.Bind(s => new DataResult<int>(s.Length));
            Assert.IsInstanceOfType<DataResult<int>>(iresult2);
            var result2 = iresult2 as DataResult<int>;
            Assert.AreEqual(2, result2.Data);

            iresult2 = result.Bind(s => new EndOfStreamException().Throw<IResult<int>>());
            Assert.IsInstanceOfType<ErrorResult<int>>(iresult2);
            var eresult2 = iresult2 as ErrorResult<int>;
            Assert.IsInstanceOfType<EndOfStreamException>(eresult2.Error);
        }

        [TestMethod]
        public void Map_Tests()
        {
            var result = new DataResult<string>("34");

            Assert.ThrowsException<ArgumentNullException>(() => result.Map<int>(null));

            var iresult2 = result.Map(s => s.Length);
            Assert.IsInstanceOfType<DataResult<int>>(iresult2);
            var result2 = iresult2 as DataResult<int>;
            Assert.AreEqual(2, result2.Data);

            iresult2 = result.Map(s => new EndOfStreamException().Throw<int>());
            Assert.IsInstanceOfType<ErrorResult<int>>(iresult2);
            var eresult2 = iresult2 as ErrorResult<int>;
            Assert.IsInstanceOfType<EndOfStreamException>(eresult2.Error);
        }

        [TestMethod]
        public void Consume_Tests()
        {
            var result = new DataResult<string>("34");

            Assert.ThrowsException<ArgumentNullException>(() => result.Consume(null));

            int len = -1;
            result.Consume(s => len = s.Length);
            Assert.AreEqual(2, len);

            result.Map(s => len = s.Length * 3);
            Assert.AreEqual(6, len);
        }
    }
}