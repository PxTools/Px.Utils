using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.ExtensionMethods;

namespace Px.Utils.UnitTests.ModelTests.ExtensionTests
{
    [TestClass]
    public class MatrixMapExtensionTests
    {
        #region IsSubmapOf

        [TestMethod]
        public void WhenCalledWithItselfShouldReturnTrue()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            // Act and Assert
            Assert.IsTrue(map.IsSubmapOf(map));
        }

        [TestMethod]
        public void WhenCalledWithSubmapShouldReturnTrue()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["aa11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsTrue(subMap.IsSubmapOf(map));
        }

        [TestMethod]
        public void WhenCalledWithSubmapWithoutfirstValuesShouldReturnTrue()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["bb11", "cc11"]),
                    new DimensionMap("22", ["bb22"]),
                    new DimensionMap("33", ["bb33", "cc33", "dd33"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsTrue(subMap.IsSubmapOf(map));
        }

        [TestMethod]
        public void WhenCalledWithSizeOneSubmapShouldReturnTrue()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["cc11"]),
                    new DimensionMap("22", ["bb22"]),
                    new DimensionMap("33", ["cc33"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsTrue(subMap.IsSubmapOf(map));
        }

        [TestMethod]
        public void WhenCalledWithDifferentDimensionOrderShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsFalse(subMap.IsSubmapOf(map));
        }

        [TestMethod]
        public void WhenCalledWithDifferentValuesShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33", "ee33"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsFalse(subMap.IsSubmapOf(map));
        }

        [TestMethod]
        public void WhenCalledWithDifferentNumberOfDimensionsShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsFalse(subMap.IsSubmapOf(map));
        }

        [TestMethod]
        public void WhenCalledWithDifferenValueOrderShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "cc33", "bb33"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsFalse(subMap.IsSubmapOf(map));
        }

        [TestMethod]
        public void WhenCalledWithNotMatchingValueCodesShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["foo", "bb22"]),
                    new DimensionMap("33", ["aa33", "bar"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsFalse(subMap.IsSubmapOf(map));
        }

        #endregion

        #region IsSupermapOf

        [TestMethod]
        public void IsSupermapOfWhenCalledWithItselfShouldReturnTrue()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            // Act and Assert
            Assert.IsTrue(map.IsSupermapOf(map));
        }

        [TestMethod]
        public void IsSupermapOfWhenCalledWithSubmapShouldReturnTrue()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["aa11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsTrue(map.IsSupermapOf(subMap));
        }

        [TestMethod]
        public void IsSupermapOfWhenCalledWithSubmapWithoutfirstValuesShouldReturnTrue()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["bb11", "cc11"]),
                    new DimensionMap("22", ["bb22"]),
                    new DimensionMap("33", ["bb33", "cc33", "dd33"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsTrue(map.IsSupermapOf(subMap));
        }

        [TestMethod]
        public void IsSupermapOfWhenCalledWithSizeOneSubmapShouldReturnTrue()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["cc11"]),
                    new DimensionMap("22", ["bb22"]),
                    new DimensionMap("33", ["cc33"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsTrue(map.IsSupermapOf(subMap));
        }

        [TestMethod]
        public void IsSupermapOfWhenCalledWithDifferentDimensionOrderShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsFalse(map.IsSupermapOf(subMap));
        }

        [TestMethod]
        public void IsSupermapOfWhenCalledWithDifferentValuesShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33", "ee33"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsFalse(map.IsSupermapOf(subMap));
        }

        [TestMethod]
        public void IsSupermapOfWhenCalledWithDifferentNumberOfDimensionsShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsFalse(map.IsSupermapOf(subMap));
        }

        [TestMethod]
        public void IsSupermapOfWhenCalledWithDifferenValueOrderShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "cc33", "bb33"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsFalse(map.IsSupermapOf(subMap));
        }

        [TestMethod]
        public void IsSupermapOfWhenCalledWithNotMatchingValueCodesShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];

            MatrixMap map = new(dimensions);

            List<IDimensionMap> subDimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["foo", "bb22"]),
                    new DimensionMap("33", ["aa33", "bar"]),
                ];

            MatrixMap subMap = new(subDimensions);

            // Act and Assert
            Assert.IsFalse(map.IsSupermapOf(subMap));
        }

        #endregion

        #region IsIdenticaMaplTo

        [TestMethod]
        public void IsIdenticalToWhenCalledWithItselfShouldReturnTrue()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];
            MatrixMap map = new(dimensions);
            // Act and Assert
            Assert.IsTrue(map.IsIdenticalMapTo(map));
        }

        [TestMethod]
        public void IsIdenticalToWhenCalledWithSameDimensionsAndValuesShouldReturnTrue()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];
            MatrixMap map1 = new(dimensions);
            MatrixMap map2 = new(dimensions);
            // Act and Assert
            Assert.IsTrue(map1.IsIdenticalMapTo(map2));
        }

        [TestMethod]
        public void IsIdenticalToWhenCalledWithDifferentDimensionValuesShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];
            List<IDimensionMap> dimensions2 = [
                    new DimensionMap("11", ["aa11", "bb11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33"]),
                ];
            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new(dimensions2);
            // Act and Assert
            Assert.IsFalse(map1.IsIdenticalMapTo(map2));
        }

        [TestMethod]
        public void IsIdenticalToWhenCalledWithDifferentDimensionCodesShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];
            List<IDimensionMap> dimensions2 = [
                    new DimensionMap("21", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];
            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new(dimensions2);
            // Act and Assert
            Assert.IsFalse(map1.IsIdenticalMapTo(map2));
        }

        [TestMethod]
        public void IsIdenticalToWhenCalledWithDifferentNumberOfDimensionsShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];
            List<IDimensionMap> dimensions2 = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                ];
            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new(dimensions2);
            // Act and Assert
            Assert.IsFalse(map1.IsIdenticalMapTo(map2));
        }

        [TestMethod]
        public void IsIdenticalToWhenCalledWithDifferentValueOrderShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];
            List<IDimensionMap> dimensions2 = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "cc33", "bb33"]),
                ];
            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new(dimensions2);
            // Act and Assert
            Assert.IsFalse(map1.IsIdenticalMapTo(map2));
        }

        [TestMethod]
        public void IsIdenticalToWhenCalledWithNotMatchingValueCodesShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];
            List<IDimensionMap> dimensions2 = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["foo", "bb22"]),
                    new DimensionMap("33", ["aa33", "bar"]),
                ];
            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new(dimensions2);
            // Act and Assert
            Assert.IsFalse(map1.IsIdenticalMapTo(map2));
        }

        [TestMethod]
        public void IsIdenticalToWhenCalledWithEmptyMapShouldReturnFalse()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];
            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new([]);
            // Act and Assert
            Assert.IsFalse(map1.IsIdenticalMapTo(map2));
        }

        [TestMethod]
        public void IsIdenticalToWhenCalledWithEmptyMapWithSameCodeShouldReturnTrue()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                    new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                    new DimensionMap("22", ["aa22", "bb22"]),
                    new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
                ];
            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new([new DimensionMap("11", []), new DimensionMap("22", []), new DimensionMap("33", [])]);
            // Act and Assert
            Assert.IsFalse(map1.IsIdenticalMapTo(map2));
        }

        #endregion

        #region GetSize

        [TestMethod]
        public void GetSizeWithValidMapReturnsCorrectSize()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                new DimensionMap("22", ["aa22", "bb22"]),
                new DimensionMap("33", ["aa33"])
            ];

            MatrixMap map = new(dimensions);

            // Act and Assert (3 * 2 * 1 = 6)
            Assert.AreEqual(6, map.GetSize());
        }

        [TestMethod]
        public void GetSizeWithZeroSizedDimensionReturnsZero()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                new DimensionMap("22", []),
                new DimensionMap("33", ["aa33"])
            ];

            MatrixMap map = new (dimensions);

            // Act and Assert (x * 0 = 0)
            Assert.AreEqual(0, map.GetSize());
        }

        #endregion

        #region CollapseDimension

                [TestMethod]
        public void CollapseDimensionWithValidMapReturnsCollapsedMap()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                new DimensionMap("22", ["aa22", "bb22"]),
                new DimensionMap("33", ["aa33", "bb33", "cc33"])
            ];

            MatrixMap map = new (dimensions);

            // Act
            IMatrixMap collapsedMap = map.CollapseDimension("22", "foo");

            // Assert
            Assert.AreEqual(9, collapsedMap.GetSize()); // 3 * 1 * 3 = 9
            Assert.AreEqual(1, collapsedMap.DimensionMaps[1].ValueCodes.Count);
            Assert.AreEqual("foo", collapsedMap.DimensionMaps[1].ValueCodes[0]);
        }

        #endregion
    }
}
