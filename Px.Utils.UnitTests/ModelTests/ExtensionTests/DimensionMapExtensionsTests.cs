using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.ExtensionMethods;

namespace Px.Utils.UnitTests.ModelTests.ExtensionTests
{
    [TestClass]
    public class DimensionMapExtensionsTests
    {
        #region IsSubmapOf

        [TestMethod]
        public void IsSubmapOfIdenticalMapsReturnsTrue()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val1", "val2"]);
            IDimensionMap map2 = new DimensionMap("dim1", ["val1", "val2"]);
            Assert.IsTrue(map1.IsSubmapOf(map2));
        }

        [TestMethod]
        public void IsSubmapOfDifferentCodesReturnsFalse()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val1", "val2"]);
            IDimensionMap map2 = new DimensionMap("dim2", ["val1", "val2"]);
            Assert.IsFalse(map1.IsSubmapOf(map2));
        }

        [TestMethod]
        public void IsSubmapOfSameCodeDifferentValuesReturnsFalse()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val1", "val2"]);
            IDimensionMap map2 = new DimensionMap("dim1", ["val1", "val3"]);
            Assert.IsFalse(map1.IsSubmapOf(map2));
        }

        [TestMethod]
        public void IsSubmapOfContainsAllValuesInOrderReturnsTrue()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val1", "val2"]);
            IDimensionMap map2 = new DimensionMap("dim1", ["val0", "val1", "val2", "val3"]);
            Assert.IsTrue(map1.IsSubmapOf(map2));
        }

        [TestMethod]
        public void IsSubmapOfContainsValuesOutOfOrderReturnsFalse()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val1", "val2"]);
            IDimensionMap map2 = new DimensionMap("dim1", ["val2", "val1"]);
            Assert.IsFalse(map1.IsSubmapOf(map2));
        }

        [TestMethod]
        public void IsSubmapOfEmptyMapReturnsTrue()
        {
            IDimensionMap map1 = new DimensionMap("dim1", []);
            IDimensionMap map2 = new DimensionMap("dim1", ["val1", "val2"]);
            Assert.IsTrue(map1.IsSubmapOf(map2));
        }

        [TestMethod]
        public void IsSubmapOfEmptyMapWithSameCodeReturnsTrue()
        {
            IDimensionMap map1 = new DimensionMap("dim1", []);
            IDimensionMap map2 = new DimensionMap("dim1", []);
            Assert.IsTrue(map1.IsSubmapOf(map2));
        }

        #endregion

        #region IsSupermapOf

        [TestMethod]
        public void IsSupermapOfIdenticalMapsReturnsTrue()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val1", "val2"]);
            IDimensionMap map2 = new DimensionMap("dim1", ["val1", "val2"]);
            Assert.IsTrue(map1.IsSupermapOf(map2));
        }

        [TestMethod]
        public void IsSupermapOfDifferentCodesReturnsFalse()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val1", "val2"]);
            IDimensionMap map2 = new DimensionMap("dim2", ["val1", "val2"]);
            Assert.IsFalse(map1.IsSupermapOf(map2));
        }

        [TestMethod]
        public void IsSupermapOfSameCodeDifferentValuesReturnsFalse()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val1", "val2"]);
            IDimensionMap map2 = new DimensionMap("dim1", ["val1", "val3"]);
            Assert.IsFalse(map1.IsSupermapOf(map2));
        }

        [TestMethod]
        public void IsSupermapOfContainsAllValuesInOrderReturnsTrue()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val0", "val1", "val2", "val3"]);
            IDimensionMap map2 = new DimensionMap("dim1", ["val1", "val2"]);
            Assert.IsTrue(map1.IsSupermapOf(map2));
        }

        [TestMethod]
        public void IsSupermapOfContainsValuesOutOfOrderReturnsFalse()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val2", "val1"]);
            IDimensionMap map2 = new DimensionMap("dim1", ["val1", "val2"]);
            Assert.IsFalse(map1.IsSupermapOf(map2));
        }

        [TestMethod]
        public void IsSupermapOfEmptyMapReturnsTrue()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val1", "val2"]);
            IDimensionMap map2 = new DimensionMap("dim1", []);
            Assert.IsTrue(map1.IsSupermapOf(map2));
        }

        [TestMethod]
        public void IsSupermapOfEmptyMapWithSameCodeReturnsTrue()
        {
            IDimensionMap map1 = new DimensionMap("dim1", []);
            IDimensionMap map2 = new DimensionMap("dim1", []);
            Assert.IsTrue(map1.IsSupermapOf(map2));
        }

        #endregion

        #region IsIdenticalMapTo

        [TestMethod]
        public void IsIdenticalMapToIdenticalMapsReturnsTrue()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val1", "val2"]);
            IDimensionMap map2 = new DimensionMap("dim1", ["val1", "val2"]);
            Assert.IsTrue(map1.IsIdenticalMapTo(map2));
        }

        [TestMethod]
        public void IsIdenticalMapToDifferentCodeReturnsFalse()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val1", "val2"]);
            IDimensionMap map2 = new DimensionMap("dim2", ["val1", "val2"]);
            Assert.IsFalse(map1.IsIdenticalMapTo(map2));
        }

        [TestMethod]
        public void IsIdenticalMapToValuesInDifferentOrderReturnsFalse()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val1", "val2"]);
            IDimensionMap map2 = new DimensionMap("dim1", ["val2", "val1"]);
            Assert.IsFalse(map1.IsIdenticalMapTo(map2));
        }

        [TestMethod]
        public void IsIdenticalMapToDifferentValuesReturnsFalse()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val1", "val2"]);
            IDimensionMap map2 = new DimensionMap("dim1", ["val3", "val4"]);
            Assert.IsFalse(map1.IsIdenticalMapTo(map2));
        }

        [TestMethod]
        public void IsIdenticalMapToSubmapReturnsFalse()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val1", "val2", "val3", "val4"]);
            IDimensionMap map2 = new DimensionMap("dim1", ["val1", "val2"]);
            Assert.IsFalse(map1.IsIdenticalMapTo(map2));
        }

        [TestMethod]
        public void IsIdenticalMapToSuperReturnsFalse()
        {
            IDimensionMap map1 = new DimensionMap("dim1", ["val1", "val2"]);
            IDimensionMap map2 = new DimensionMap("dim1", ["val1", "val2", "val3", "val4"]);
            Assert.IsFalse(map1.IsIdenticalMapTo(map2));
        }

        #endregion
    }
}
