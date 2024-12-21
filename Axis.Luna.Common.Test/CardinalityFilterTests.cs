using Axis.Luna.Common.Cardinality;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Common.Test
{
    namespace CardinalityFilterTests
    {
        [TestClass]
        public class OneTests
        {
            [TestMethod]
            public void One_Default_IsDefault_ShouldBeTrue()
            {
                // Arrange
                var one = One<int>.Default;

                // Act
                var isDefault = one.IsDefault;

                // Assert
                Assert.IsTrue(isDefault);
            }

            [TestMethod]
            public void One_Constructor_ShouldSetItem()
            {
                // Arrange
                var item = 42;

                // Act
                var one = new One<int>(item);

                // Assert
                Assert.AreEqual(item, one.Item);
            }

            [TestMethod]
            public void One_Implicit_ShouldSetItem()
            {
                // Arrange
                var item = 42;

                // Act
                One<int> one = item;

                // Assert
                Assert.AreEqual(item, one.Item);
            }

            [TestMethod]
            public void One_Of_ShouldCreateInstance()
            {
                // Arrange
                var item = 42;

                // Act
                var one = One<int>.Of(item);

                // Assert
                Assert.AreEqual(item, one.Item);
            }

            [TestMethod]
            public void One_IsMatch_ShouldReturnTrueForMatchingItem()
            {
                // Arrange
                var item = 42;
                var one = One<int>.Of(item);

                // Act
                var result = one.IsMatch(item);

                // Assert
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void One_Equals_ShouldReturnTrueForSameItem()
            {
                // Arrange
                var item = 42;
                var one1 = One<int>.Of(item);
                var one2 = One<int>.Of(item);
                var one3 = One<int>.Of(765);

                // Act/Assert
                Assert.IsTrue(one1.Equals(one1));
                Assert.IsTrue(one1 == one2);
                Assert.IsTrue(one1 != one3);
            }

            [TestMethod]
            public void One_Equals_Object_ShouldReturnTrueForSameItem()
            {
                // Arrange
                var item = 42;
                var one = One<int>.Of(item);
                object other_ = One<int>.Of(33);
                object other = One<int>.Of(item);

                // Act
                var result = one.Equals(other);
                var result2 = one.Equals("42");
                var result3 = one.Equals(other_);

                // Assert
                Assert.IsTrue(result);
                Assert.IsFalse(result2);
                Assert.IsFalse(result3);
            }

            [TestMethod]
            public void One_GetHashCode_ShouldBeSameForSameItem()
            {
                // Arrange
                var item = 42;
                var one1 = One<int>.Of(item);
                var one2 = One<int>.Of(item);
                var one3 = One<string>.Default;

                // Act
                var hash1 = one1.GetHashCode();
                var hash2 = one2.GetHashCode();
                var hash3 = one3.GetHashCode();

                // Assert
                Assert.AreEqual(hash1, hash2);
                Assert.AreEqual(0, hash3);
            }

            [TestMethod]
            public void One_ToString_ShouldReturnFormattedString()
            {
                // Arrange
                var item = 42;
                var one = One<int>.Of(item);
                var one2 = One<int>.Default;
                var one3 = One<string>.Default;

                // Act
                var result = one.ToString();
                var result2 = one2.ToString();
                var result3 = one3.ToString();

                // Assert
                Assert.AreEqual("One(42)", result);
                Assert.AreEqual("One(0)", result2);
                Assert.AreEqual("One(null)", result3);
            }
        }

        [TestClass]
        public class AnyTests
        {
            [TestMethod]
            public void Any_Default_IsDefault_ShouldBeTrue()
            {
                // Arrange
                var any = Any<int>.Default;

                // Act
                var isDefault = any.IsDefault;

                // Assert
                Assert.IsTrue(isDefault);
            }

            [TestMethod]
            public void Any_Constructor_ShouldSetItems()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };

                // Act
                var any = new Any<int>(items);

                // Assert
                CollectionAssert.AreEquivalent(items, any.Items.ToList());
            }

            [TestMethod]
            public void Any_Implicit_ShouldSetItems()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };

                // Act
                Any<int> any = items;

                // Assert
                CollectionAssert.AreEquivalent(items, any.Items.ToList());
            }

            [TestMethod]
            public void Any_Of_ShouldCreateInstance()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };

                // Act
                var any = Any<int>.Of(items);
                var any2 = Any<int>.Of(items.AsEnumerable());
                var any3 = Any<int>.Of((IEnumerable<int>)null);

                // Assert
                CollectionAssert.AreEquivalent(items, any.Items.ToList());
                Assert.IsTrue(any3.IsDefault);
            }

            [TestMethod]
            public void Any_IsMatch_ShouldReturnTrueForContainedItem()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };
                var any = Any<int>.Of(items);
                var defaultAny = Any<int>.Default;

                // Assert
                Assert.IsTrue(any.IsMatch(2));
                Assert.IsFalse(any.IsMatch(9));
                Assert.IsFalse(defaultAny.IsMatch(3));
            }

            [TestMethod]
            public void Any_Equals_ShouldReturnTrueForSameItems()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };
                var any1 = Any<int>.Of(items);
                var any2 = Any<int>.Of(items);

                // Act
                var result = any1.Equals(any2);

                // Assert
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void Any_Equals_Object_ShouldReturnTrueForSameItems()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };
                var items2 = new[] { 1, 2, 3, 4 };
                var any = Any<int>.Of(items);
                var any2 = Any<int>.Of(items2);
                var defaultAny = Any<int>.Default;
                object other = Any<int>.Of(items);

                // Assert
                Assert.IsTrue(defaultAny.Equals(defaultAny));
                Assert.IsFalse(any.Equals(defaultAny));
                Assert.IsFalse(defaultAny.Equals(any));
                Assert.IsTrue(any.Equals(any));
                Assert.IsFalse(any.Equals(any2));
                Assert.IsTrue(any.Equals(other));
                Assert.IsFalse(any.Equals((object)any2));
                Assert.IsFalse(any.Equals("bleh"));
                Assert.IsFalse(any == any2);
                Assert.IsTrue(any != any2);
            }

            [TestMethod]
            public void Any_GetHashCode_ShouldBeSameForSameItems()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };
                var any1 = Any<int>.Of(items);
                var any2 = Any<int>.Of(items);

                // Act
                var hash1 = any1.GetHashCode();
                var hash2 = any2.GetHashCode();

                // Assert
                Assert.AreEqual(hash1, hash2);
                Assert.AreEqual(0, Any<string>.Default.GetHashCode());
            }

            [TestMethod]
            public void Any_ToString_ShouldReturnFormattedString()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };
                var any = Any<int>.Of(items);

                // Act
                var result = any.ToString();
                var result2 = Any<string>.Of("1", "2", null).ToString();

                // Assert
                Assert.AreEqual("Any(1, 2, 3)", result);
                Assert.AreEqual("Any(null, 1, 2)", result2);
                Assert.AreEqual("Any(null)", Any<string>.Default.ToString());
            }
        }

        [TestClass]
        public class ExclusionTests
        {
            [TestMethod]
            public void Exclusion_Default_IsDefault_ShouldBeTrue()
            {
                // Arrange
                var ex = Exclusion<int>.Default;

                // Act
                var isDefault = ex.IsDefault;

                // Assert
                Assert.IsTrue(isDefault);
            }

            [TestMethod]
            public void Exclusion_Constructor_ShouldSetItems()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };

                // Act
                var ex = new Exclusion<int>(items);

                // Assert
                CollectionAssert.AreEquivalent(items, ex.Items.ToList());
            }

            [TestMethod]
            public void Exclusion_Implicit_ShouldSetItems()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };

                // Act
                Exclusion<int> ex = items;

                // Assert
                CollectionAssert.AreEquivalent(items, ex.Items.ToList());
            }

            [TestMethod]
            public void Exclusion_Of_ShouldCreateInstance()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };

                // Act
                var ex = Exclusion<int>.Of(items);
                var ex2 = Exclusion<int>.Of(items.AsEnumerable());
                var ex3 = Exclusion<int>.Of((IEnumerable<int>)null);

                // Assert
                CollectionAssert.AreEquivalent(items, ex.Items.ToList());
                Assert.IsTrue(ex3.IsDefault);
            }

            [TestMethod]
            public void Exclusion_IsMatch_ShouldReturnTrueForContainedItem()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };
                var ex = Exclusion<int>.Of(items);
                var defaultExclusion = Exclusion<int>.Default;

                // Assert
                Assert.IsFalse(ex.IsMatch(2));
                Assert.IsTrue(ex.IsMatch(9));
                Assert.IsFalse(defaultExclusion.IsMatch(3));
            }

            [TestMethod]
            public void Exclusion_Equals_ShouldReturnTrueForSameItems()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };
                var ex1 = Exclusion<int>.Of(items);
                var ex2 = Exclusion<int>.Of(items);

                // Act
                var result = ex1.Equals(ex2);

                // Assert
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void Exclusion_Equals_Object_ShouldReturnTrueForSameItems()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };
                var items2 = new[] { 1, 2, 3, 4 };
                var ex = Exclusion<int>.Of(items);
                var ex2 = Exclusion<int>.Of(items2);
                var defaultExclusion = Exclusion<int>.Default;
                object other = Exclusion<int>.Of(items);

                // Assert
                Assert.IsTrue(defaultExclusion.Equals(defaultExclusion));
                Assert.IsFalse(ex.Equals(defaultExclusion));
                Assert.IsFalse(defaultExclusion.Equals(ex));
                Assert.IsTrue(ex.Equals(ex));
                Assert.IsFalse(ex.Equals(ex2));
                Assert.IsTrue(ex.Equals(other));
                Assert.IsFalse(ex.Equals((object)ex2));
                Assert.IsFalse(ex.Equals("bleh"));
                Assert.IsFalse(ex == ex2);
                Assert.IsTrue(ex != ex2);
            }

            [TestMethod]
            public void Exclusion_GetHashCode_ShouldBeSameForSameItems()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };
                var ex1 = Exclusion<int>.Of(items);
                var ex2 = Exclusion<int>.Of(items);

                // Act
                var hash1 = ex1.GetHashCode();
                var hash2 = ex2.GetHashCode();

                // Assert
                Assert.AreEqual(hash1, hash2);
                Assert.AreEqual(0, Exclusion<string>.Default.GetHashCode());
            }

            [TestMethod]
            public void Exclusion_ToString_ShouldReturnFormattedString()
            {
                // Arrange
                var items = new[] { 1, 2, 3 };
                var ex = Exclusion<int>.Of(items);

                // Act
                var result = ex.ToString();
                var result2 = Exclusion<string>.Of("1", "2", null).ToString();

                // Assert
                Assert.AreEqual("Exclusion(1, 2, 3)", result);
                Assert.AreEqual("Exclusion(null, 1, 2)", result2);
                Assert.AreEqual("Exclusion(null)", Exclusion<string>.Default.ToString());
            }
        }

        [TestClass]
        public class AllTests
        {
            [TestMethod]
            public void All_Constructor_ShouldSetItems()
            {
                // Arrange
                var all = All<int>.Default;

                // Assert
                Assert.IsTrue(all.IsDefault);
            }

            [TestMethod]
            public void All_TItem_IsMatch_ReturnsTrueForAnyItem()
            {
                var all = new All<int>();

                Assert.IsTrue(all.IsMatch(10));
            }

            [TestMethod]
            public void All_TItem_Equals_ReturnsTrueForAnyAllInstance()
            {
                var all1 = new All<int>();
                var all2 = new All<int>();

                Assert.IsTrue(all1.Equals(all2));
                Assert.IsTrue(all1.Equals((object)all2));
                Assert.IsFalse(all1.Equals("bleh"));
                Assert.IsTrue(all1 == all2);
                Assert.IsFalse(all1 != all2);
            }

            [TestMethod]
            public void All_TItem_ToString_FormatsCorrectly()
            {
                var all = new All<string>();

                Assert.AreEqual("All()", all.ToString());
            }

            [TestMethod]
            public void All_TItem_IsDefault_AlwaysReturnsTrue()
            {
                var all = new All<int>();

                Assert.IsTrue(all.IsDefault);
            }

            [TestMethod]
            public void All_TItem_GetHashCode_IsAlwaysZero()
            {
                var all = new All<int>();

                Assert.AreEqual(0, all.GetHashCode());
            }
        }

        [TestClass]
        public class NoneTests
        {
            [TestMethod]
            public void None_Constructor_ShouldSetItems()
            {
                // Arrange
                var none = None<int>.Default;

                // Assert
                Assert.IsTrue(none.IsDefault);
            }

            [TestMethod]
            public void None_TItem_IsMatch_ReturnsFalseForAnyItem()
            {
                var none = new None<int>();

                Assert.IsFalse(none.IsMatch(10));
            }

            [TestMethod]
            public void None_TItem_Equals_ReturnsTrueForAnyNoneInstance()
            {
                var none1 = new None<int>();
                var none2 = new None<int>();

                Assert.IsTrue(none1.Equals(none2));
                Assert.IsTrue(none1.Equals((object)none2));
                Assert.IsFalse(none1.Equals("bleh"));
                Assert.IsTrue(none1 == none2);
                Assert.IsFalse(none1 != none2);
            }

            [TestMethod]
            public void None_TItem_ToString_FormatsCorrectly()
            {
                var none = new None<string>();

                Assert.AreEqual("None()", none.ToString());
            }

            [TestMethod]
            public void None_TItem_IsDefault_AlwaysReturnsTrue()
            {
                var none = new None<int>();

                Assert.IsTrue(none.IsDefault);
            }

            [TestMethod]
            public void None_TItem_GetHashCode_IsAlwaysZero()
            {
                var none = new None<int>();

                Assert.AreEqual(0, none.GetHashCode());
            }
        }
    }

}
