using Axis.Luna.Common.Types.Basic2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using static Axis.Luna.Common.Types.Basic2.IBasicValue.BasicStruct;

namespace Axis.Luna.Common.Test.Types.Basic
{
    [TestClass]
    public class BasicStructTest
    {
        [TestMethod]
        public void PropertyName_Parse_WithInvalidArgs_ShouldThrow()
        {
            PropertyName pname;
            string name = null;
            var ex = Assert.ThrowsException<ArgumentException>(() => pname = name);

            name = "";
            ex = Assert.ThrowsException<ArgumentException>(() => pname = name);

            name = " ";
            ex = Assert.ThrowsException<ArgumentException>(() => pname = name);

            name = " \n\r\t";
            ex = Assert.ThrowsException<ArgumentException>(() => pname = name);

            name = "[";
            var fex = Assert.ThrowsException<FormatException>(() => pname = name);

            name = "[sdrf";
            fex = Assert.ThrowsException<FormatException>(() => pname = name);

            name = "[]";
            fex = Assert.ThrowsException<FormatException>(() => pname = name);

            name = "[fdd]";
            fex = Assert.ThrowsException<FormatException>(() => pname = name);
        }

        [TestMethod]
        public void PropertyName_Parse_WithValidArgs_ShouldParse()
        {
            string name = "prop-name";
            var pname = PropertyName.Parse(name);
            Assert.IsNotNull(pname);
            Assert.AreEqual(name, pname.Name);
            Assert.AreEqual(0, pname.Metadata.Length);

            name = "[]prop-name";
            pname = PropertyName.Parse(name);
            Assert.IsNotNull(pname);
            Assert.AreEqual("prop-name", pname.Name);
            Assert.AreEqual(name, pname.ToString());
            Assert.AreEqual(0, pname.Metadata.Length);

            name = "[]prop-name";
            pname = PropertyName.Parse(name);
            Assert.IsNotNull(pname);
            Assert.AreEqual("prop-name", pname.Name);
            Assert.AreEqual(name, pname.ToString());
            Assert.AreEqual(0, pname.Metadata.Length);

            name = "[abcd]prop-name";
            pname = PropertyName.Parse(name);
            Assert.IsNotNull(pname);
            Assert.AreEqual("prop-name", pname.Name);
            Assert.AreEqual("[abcd;]prop-name", pname.ToString());
            Assert.AreEqual(1, pname.Metadata.Length);
        }

        [TestMethod]
        public void PropertyName_Equality()
        {
            PropertyName n1 = "stuff";
            PropertyName n2 = "stuff";
            PropertyName n3 = "[meta]stuff";
            PropertyName n4 = "[meta]stuff";

            Assert.AreEqual(n1, n2);
            Assert.AreEqual(n3, n4);
            Assert.AreNotEqual(n1, n3);
            Assert.AreNotEqual(n2, n4);
        }

        [TestMethod]
        public void BasicValueWrapper_ImplicitTests()
        {
            BasicValueWrapper wrapper;

            #region int
            // sbyte
            wrapper = (sbyte)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);
            wrapper = (sbyte?)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);

            // short
            wrapper = (short)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);
            wrapper = (short?)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);

            // int
            wrapper = 3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);
            wrapper = (int?)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);

            // long
            wrapper = 3L;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);
            wrapper = (long?)3L;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);
            #endregion

            #region uint
            // sbyte
            wrapper = (sbyte)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);
            wrapper = (sbyte?)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);

            // short
            wrapper = (short)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);
            wrapper = (short?)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);

            // int
            wrapper = 3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);
            wrapper = (int?)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);

            // long
            wrapper = 3L;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);
            wrapper = (long?)3L;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicInt);
            #endregion

            // real
            wrapper = 3.4;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicReal);
            wrapper = (double?)3.4;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicReal);

            // decimal
            wrapper = 3.4m;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicDecimal);
            wrapper = (decimal?)3.4m;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicDecimal);

            // bool
            wrapper = true;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicBool);
            wrapper = (bool?)true;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicBool);

            // string
            wrapper = "some value";
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicString);
            wrapper = (string)null;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicString);

            // date time
            wrapper = DateTimeOffset.Now;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicDate);
            wrapper = (DateTimeOffset?)null;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicDate);

            // time span
            wrapper = TimeSpan.FromSeconds(454);
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicTimeSpan);
            wrapper = (TimeSpan?)null;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicTimeSpan);

            // guid
            wrapper = Guid.NewGuid();
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicGuid);
            wrapper = (Guid?)null;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicGuid);

            // bytes
            wrapper = Array.Empty<byte>();
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicBytes);
            wrapper = (byte[])null;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicBytes);

            // list
            wrapper = Array.Empty<IBasicValue>();
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicList);
            wrapper = (IBasicValue[])null;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicList);

            // list
            wrapper = Array.Empty<BasicValueWrapper>();
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicList);
            wrapper = (BasicValueWrapper[])null;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicList);

            // struct
            wrapper = default(IBasicValue.BasicStruct);
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicStruct);

            // struct
            wrapper = new Initializer() { ["stuff"] = 3 };
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is IBasicValue.BasicStruct);
        }

        [TestMethod]
        public void Initializer_IndexerTests()
        {
            var initializer = new Initializer(new Metadata("bleh"))
            {
                ["stuff"] = 5,
                ["stuff1"] = 5.2m,
                ["stuff2"] = false,
                ["stuff3"] = "other people",
                ["other-stuff"] = DateTimeOffset.Now,
                ["[meta]main-stuff"] = new Initializer
                {
                    ["inner"] = new BasicValueWrapper[]
                    {
                        7,
                        TimeSpan.FromDays(5.32)
                    }
                }
            };
            IBasicValue.BasicStruct @struct = initializer;

            Assert.IsNotNull(initializer);
            Assert.AreNotEqual(default, @struct);
            Assert.AreEqual(initializer.Properties.Length, @struct.PropertyCount);
            Assert.AreEqual(initializer.Map["stuff"].Value, @struct["stuff"]);
            Assert.AreEqual(initializer.Map["stuff1"].Value, @struct["stuff1"]);
            Assert.AreEqual(initializer.Map["stuff2"].Value, @struct["stuff2"]);
            Assert.AreEqual(initializer.Map["stuff3"].Value, @struct["stuff3"]);
            Assert.AreEqual(initializer.Map["[meta]main-stuff"].Value, @struct["main-stuff"]);
            Assert.AreEqual(1, initializer.Metadata.Length);


            IBasicValue.BasicStruct struct2 = initializer;
            Assert.AreEqual(@struct.GetHashCode(), struct2.GetHashCode());
        }
    }
}
