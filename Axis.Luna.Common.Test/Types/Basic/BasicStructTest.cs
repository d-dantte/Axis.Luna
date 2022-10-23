using Axis.Luna.Common.Types.Basic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Axis.Luna.Common.Types.Basic.BasicStruct;

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
            Assert.IsTrue(wrapper.Value is BasicInt);
            wrapper = (sbyte?)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicInt);

            // short
            wrapper = (short)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicInt);
            wrapper = (short?)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicInt);

            // int
            wrapper = 3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicInt);
            wrapper = (int?)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicInt);

            // long
            wrapper = 3L;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicInt);
            wrapper = (long?)3L;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicInt);
            #endregion

            #region uint
            // sbyte
            wrapper = (sbyte)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicInt);
            wrapper = (sbyte?)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicInt);

            // short
            wrapper = (short)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicInt);
            wrapper = (short?)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicInt);

            // int
            wrapper = 3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicInt);
            wrapper = (int?)3;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicInt);

            // long
            wrapper = 3L;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicInt);
            wrapper = (long?)3L;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicInt);
            #endregion

            // real
            wrapper = 3.4;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicReal);
            wrapper = (double?)3.4;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicReal);

            // decimal
            wrapper = 3.4m;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicDecimal);
            wrapper = (decimal?)3.4m;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicDecimal);

            // bool
            wrapper = true;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicBool);
            wrapper = (bool?)true;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicBool);

            // string
            wrapper = "some value";
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicString);
            wrapper = (string)null;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicString);

            // date time
            wrapper = DateTimeOffset.Now;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicDate);
            wrapper = (DateTimeOffset?)null;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicDate);

            // time span
            wrapper = TimeSpan.FromSeconds(454);
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicTimeSpan);
            wrapper = (TimeSpan?)null;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicTimeSpan);

            // guid
            wrapper = Guid.NewGuid();
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicGuid);
            wrapper = (Guid?)null;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicGuid);

            // bytes
            wrapper = Array.Empty<byte>();
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicBytes);
            wrapper = (byte[])null;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicBytes);

            // list
            wrapper = Array.Empty<IBasicValue>();
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicList);
            wrapper = (IBasicValue[])null;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicList);

            // list
            wrapper = Array.Empty<BasicValueWrapper>();
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicList);
            wrapper = (BasicValueWrapper[])null;
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicList);

            // struct
            wrapper = default(BasicStruct);
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicStruct);

            // struct
            wrapper = new Initializer() { ["stuff"] = 3 };
            Assert.IsNotNull(wrapper.Value);
            Assert.IsTrue(wrapper.Value is BasicStruct);
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
            BasicStruct @struct = initializer;

            Assert.IsNotNull(initializer);
            Assert.AreNotEqual(default, @struct);
            Assert.AreEqual(initializer.Properties.Length, @struct.PropertyCount);
            Assert.AreEqual(initializer.Map["stuff"].Value, @struct["stuff"]);
            Assert.AreEqual(initializer.Map["stuff1"].Value, @struct["stuff1"]);
            Assert.AreEqual(initializer.Map["stuff2"].Value, @struct["stuff2"]);
            Assert.AreEqual(initializer.Map["stuff3"].Value, @struct["stuff3"]);
            Assert.AreEqual(initializer.Map["[meta]main-stuff"].Value, @struct["main-stuff"]);
            Assert.AreEqual(1, initializer.Metadata.Length);


            BasicStruct struct2 = initializer;
            Assert.AreEqual(@struct.GetHashCode(), struct2.GetHashCode());
        }
    }
}
