using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Axis.Luna.Extensions;

namespace Axis.Luna.Test.Extensions
{
    [TestClass]
    public class ObjectExtensionTests
    {
        [TestMethod]
        public void NumericConversions()
        {
            #region
            byte b = 4;
            sbyte sb = 4;
            int i = 4;
            uint ui = 4u;
            long l = 4l;
            ulong ul = 4ul;
            short s = 4;
            ushort us = 4;
            float f = 4.0f;
            double d = 4.0d;
            decimal m = 4.0m;

            object box = b;
            AssertNumericConversions(box);

            box = sb;
            AssertNumericConversions(box);

            box = i;
            AssertNumericConversions(box);

            box = ui;
            AssertNumericConversions(box);

            box = l;
            AssertNumericConversions(box);

            box = ul;
            AssertNumericConversions(box);

            box = s;
            AssertNumericConversions(box);

            box = us;
            AssertNumericConversions(box);

            box = f;
            AssertNumericConversions(box);

            box = d;
            AssertNumericConversions(box);

            box = m;
            AssertNumericConversions(box);

            #endregion

        }

        public void AssertNumericConversions(object boxed)
        {
            Assert.AreNotEqual(0, boxed.Cast<int>());
            Assert.AreNotEqual(0, boxed.Cast<uint>());
            Assert.AreNotEqual(0, boxed.Cast<long>());
            Assert.AreNotEqual(0, boxed.Cast<ulong>());
            Assert.AreNotEqual(0, boxed.Cast<short>());
            Assert.AreNotEqual(0, boxed.Cast<ushort>());
            Assert.AreNotEqual(0, boxed.Cast<byte>());
            Assert.AreNotEqual(0, boxed.Cast<sbyte>());
            Assert.AreNotEqual(0, boxed.Cast<float>());
            Assert.AreNotEqual(0, boxed.Cast<double>());
            Assert.AreNotEqual(0, boxed.Cast<decimal>());
        }


        [TestMethod]
        public void ReferenceConversionTests()
        {
            var _1 = new ConcreteSomething();
            var _2 = new ConcreteSomething2();

            Assert.IsNull(_1.Cast<ConcreteSomething2>());
            Assert.IsNull(_1.Cast<AbstractSomething2>());
            Assert.IsNull(_1.Cast<IManyThings>());
            Assert.IsNotNull(_1.Cast<IOtherThing>());


            Assert.IsNull(_2.Cast<ConcreteSomething>());
            Assert.IsNull(_2.Cast<AbstractSomething>());
            Assert.IsNotNull(_2.Cast<AbstractSomething2>());
            Assert.IsNotNull(_2.Cast<IOtherThing>());
            Assert.IsNotNull(_2.Cast<IManyThings>());
        }
    }

    public interface ISomething
    {
        void Method();
        string Property { get; set; }
    }

    public interface IOtherThing
    { }

    public interface IManyThings
    { }

    public abstract class AbstractSomething : ISomething
    {
        public abstract void Method();

        public string Property { get; set; }
    }

    public abstract class AbstractSomething2 : ISomething
    {
        public void Method() { }

        public abstract string Property { get; set; }
    }

    public class ConcreteSomething : AbstractSomething, IOtherThing
    {
        public override void Method()
        {
            throw new NotImplementedException();
        }
    }

    public class ConcreteSomething2 : AbstractSomething2, IOtherThing, IManyThings
    {
        public override string Property
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
