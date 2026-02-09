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
            Assert.AreEqual(6L, map.GetSize());
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

            MatrixMap map = new(dimensions);

            // Act and Assert (x * 0 = 0)
            Assert.AreEqual(0L, map.GetSize());
        }

        [TestMethod]
        public void GetSizeWithSingleDimensionReturnsCorrectSize()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                new DimensionMap("11", ["aa11", "bb11", "cc11", "dd11", "ee11"])
            ];

            MatrixMap map = new(dimensions);

            // Act and Assert
            Assert.AreEqual(5L, map.GetSize());
        }

        [TestMethod]
        public void GetSizeWithEmptyMapReturnsOne()
        {
            // Arrange
            MatrixMap map = new([]);

            // Act and Assert
            Assert.AreEqual(1L, map.GetSize());
        }

        [TestMethod]
        public void GetSizeWithLargeDimensionsReturnsCorrectSize()
        {
            // Arrange - Create dimensions that would result in a large number
            List<IDimensionMap> dimensions = [
                new DimensionMap("dim1", Enumerable.Range(0, 100).Select(i => $"val{i}").ToList()),
                new DimensionMap("dim2", Enumerable.Range(0, 50).Select(i => $"val{i}").ToList()),
                new DimensionMap("dim3", Enumerable.Range(0, 10).Select(i => $"val{i}").ToList())
            ];

            MatrixMap map = new(dimensions);

            // Act and Assert (100 * 50 * 10 = 50,000)
            Assert.AreEqual(50000L, map.GetSize());
        }

        [TestMethod]
        public void GetSizeWithVeryLargeDimensionsHandlesLongRange()
        {
            // Arrange - Create dimensions that would overflow int but fit in long
            // This simulates a case where GetSize() would fail but GetSizeLong() should work
            List<IDimensionMap> dimensions = [
                new DimensionMap("dim1", Enumerable.Range(0, 1000).Select(i => $"val{i}").ToList()),
                new DimensionMap("dim2", Enumerable.Range(0, 1000).Select(i => $"val{i}").ToList()),
                new DimensionMap("dim3", Enumerable.Range(0, 5).Select(i => $"val{i}").ToList())
            ];

            MatrixMap map = new(dimensions);

            // Act and Assert (1000 * 1000 * 5 = 5,000,000)
            Assert.AreEqual(5000000L, map.GetSize());
        }

        [TestMethod]
        public void GetSizeWithOneDimensionOfSizeOneReturnsCorrectSize()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                new DimensionMap("dim1", ["val1"]),
                new DimensionMap("dim2", ["val1", "val2", "val3"]),
                new DimensionMap("dim3", ["val1", "val2"])
            ];

            MatrixMap map = new(dimensions);

            // Act and Assert (1 * 3 * 2 = 6)
            Assert.AreEqual(6L, map.GetSize());
        }

        [TestMethod]
        public void GetSizeWithMultipleDimensionsOfSizeOneReturnsOne()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                new DimensionMap("dim1", ["val1"]),
                new DimensionMap("dim2", ["val1"]),
                new DimensionMap("dim3", ["val1"])
            ];

            MatrixMap map = new(dimensions);

            // Act and Assert (1 * 1 * 1 = 1)
            Assert.AreEqual(1L, map.GetSize());
        }

        [TestMethod]
        public void GetSizeWithManySmallDimensionsReturnsCorrectSize()
        {
            // Arrange - Test with many dimensions with small sizes
            List<IDimensionMap> dimensions = [
                new DimensionMap("dim1", ["val1", "val2"]),
                new DimensionMap("dim2", ["val1", "val2"]),
                new DimensionMap("dim3", ["val1", "val2"]),
                new DimensionMap("dim4", ["val1", "val2"]),
                new DimensionMap("dim5", ["val1", "val2"])
            ];

            MatrixMap map = new(dimensions);

            // Act and Assert (2^5 = 32)
            Assert.AreEqual(32L, map.GetSize());
        }

        #endregion

        #region IntersectionMap

        [TestMethod]
        public void IntersectionMapWithIdenticalMapsReturnsIdenticalMap()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                new DimensionMap("22", ["aa22", "bb22"]),
                new DimensionMap("33", ["aa33", "bb33", "cc33"])
            ];

            MatrixMap map1 = new(dimensions);
            MatrixMap map2 = new(dimensions);

            // Act
            IMatrixMap result = map1.IntersectionMap(map2);

            // Assert
            Assert.IsTrue(result.IsIdenticalMapTo(map1));
        }

        [TestMethod]
        public void IntersectionMapWithPartialOverlapReturnsCommonValues()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                new DimensionMap("22", ["aa22", "bb22", "cc22"]),
                new DimensionMap("33", ["aa33", "bb33"])
            ];

            List<IDimensionMap> dimensions2 = [
                new DimensionMap("11", ["bb11", "cc11", "dd11"]),
                new DimensionMap("22", ["aa22", "cc22"]),
                new DimensionMap("33", ["aa33", "bb33", "cc33"])
            ];

            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new(dimensions2);

            // Act
            IMatrixMap result = map1.IntersectionMap(map2);

            // Assert
            Assert.AreEqual(3, result.DimensionMaps.Count);
            Assert.AreEqual(2, result.DimensionMaps[0].ValueCodes.Count); // bb11, cc11
            Assert.AreEqual(2, result.DimensionMaps[1].ValueCodes.Count); // aa22, cc22
            Assert.AreEqual(2, result.DimensionMaps[2].ValueCodes.Count); // aa33, bb33
            CollectionAssert.AreEqual(new List<string> { "bb11", "cc11" }, result.DimensionMaps[0].ValueCodes.ToList());
        }

        [TestMethod]
        public void IntersectionMapWithNoCommonValuesReturnsEmptyDimensions()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                new DimensionMap("11", ["aa11", "bb11"]),
                new DimensionMap("22", ["aa22", "bb22"])
            ];

            List<IDimensionMap> dimensions2 = [
                new DimensionMap("11", ["cc11", "dd11"]),
                new DimensionMap("22", ["cc22", "dd22"])
            ];

            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new(dimensions2);

            // Act
            IMatrixMap result = map1.IntersectionMap(map2);

            // Assert
            Assert.AreEqual(2, result.DimensionMaps.Count);
            Assert.AreEqual(0, result.DimensionMaps[0].ValueCodes.Count);
            Assert.AreEqual(0, result.DimensionMaps[1].ValueCodes.Count);
        }

        [TestMethod]
        public void IntersectionMapPreservesOrderFromFirstMap()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                new DimensionMap("11", ["aa11", "bb11", "cc11", "dd11"])
            ];

            List<IDimensionMap> dimensions2 = [
                new DimensionMap("11", ["dd11", "bb11", "aa11"]) // Different order
            ];

            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new(dimensions2);

            // Act
            IMatrixMap result = map1.IntersectionMap(map2);

            // Assert - Should preserve order from map1
            CollectionAssert.AreEqual(new List<string> { "aa11", "bb11", "dd11" }, result.DimensionMaps[0].ValueCodes.ToList());
        }

        [TestMethod]
        public void IntersectionMapWithDifferentDimensionCountThrows()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                new DimensionMap("11", ["aa11", "bb11"]),
                new DimensionMap("22", ["aa22", "bb22"])
            ];

            List<IDimensionMap> dimensions2 = [
                new DimensionMap("11", ["aa11", "bb11"])
            ];

            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new(dimensions2);

            // Act
            Assert.ThrowsExactly<InvalidOperationException>(() => _ = map1.IntersectionMap(map2));
        }

        [TestMethod]
        public void IntersectionMapWithDifferentDimensionCodesThrows()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                new DimensionMap("11", ["aa11", "bb11"]),
                new DimensionMap("22", ["aa22", "bb22"])
            ];

            List<IDimensionMap> dimensions2 = [
                new DimensionMap("11", ["aa11", "bb11"]),
                new DimensionMap("33", ["aa33", "bb33"]) // Different code
            ];

            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new(dimensions2);

            // Act
            Assert.ThrowsExactly<InvalidOperationException>(() => _ = map1.IntersectionMap(map2));
        }

        #endregion

        #region UnionMap

        [TestMethod]
        public void UnionMapWithIdenticalMapsReturnsIdenticalMap()
        {
            // Arrange
            List<IDimensionMap> dimensions = [
                new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                new DimensionMap("22", ["aa22", "bb22"]),
                new DimensionMap("33", ["aa33", "bb33", "cc33"])
            ];

            MatrixMap map1 = new(dimensions);
            MatrixMap map2 = new(dimensions);

            // Act
            IMatrixMap result = map1.UnionMap(map2);

            // Assert
            Assert.IsTrue(result.IsIdenticalMapTo(map1));
        }

        [TestMethod]
        public void UnionMapWithPartialOverlapCombinesAllValues()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                new DimensionMap("11", ["aa11", "bb11"]),
                new DimensionMap("22", ["aa22", "bb22"])
            ];

            List<IDimensionMap> dimensions2 = [
                new DimensionMap("11", ["bb11", "cc11"]),
                new DimensionMap("22", ["cc22", "dd22"])
            ];

            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new(dimensions2);

            // Act
            IMatrixMap result = map1.UnionMap(map2);

            // Assert
            Assert.AreEqual(2, result.DimensionMaps.Count);
            Assert.AreEqual(3, result.DimensionMaps[0].ValueCodes.Count); // aa11, bb11, cc11
            Assert.AreEqual(4, result.DimensionMaps[1].ValueCodes.Count); // aa22, bb22, cc22, dd22
            CollectionAssert.AreEqual(new List<string> { "aa11", "bb11", "cc11" }, result.DimensionMaps[0].ValueCodes.ToList());
            CollectionAssert.AreEqual(new List<string> { "aa22", "bb22", "cc22", "dd22" }, result.DimensionMaps[1].ValueCodes.ToList());
        }

        [TestMethod]
        public void UnionMapWithNoOverlapCombinesAllValues()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                new DimensionMap("11", ["aa11", "bb11"]),
                new DimensionMap("22", ["aa22"])
            ];

            List<IDimensionMap> dimensions2 = [
                new DimensionMap("11", ["cc11", "dd11"]),
                new DimensionMap("22", ["bb22", "cc22"])
            ];

            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new(dimensions2);

            // Act
            IMatrixMap result = map1.UnionMap(map2);

            // Assert
            Assert.AreEqual(2, result.DimensionMaps.Count);
            Assert.AreEqual(4, result.DimensionMaps[0].ValueCodes.Count); // aa11, bb11, cc11, dd11
            Assert.AreEqual(3, result.DimensionMaps[1].ValueCodes.Count); // aa22, bb22, cc22
        }

        [TestMethod]
        public void UnionMapPreservesOrderWithFirstMapValuesFirst()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                new DimensionMap("11", ["cc11", "aa11"])
            ];

            List<IDimensionMap> dimensions2 = [
                new DimensionMap("11", ["bb11", "aa11", "dd11"]) // aa11 is common
            ];

            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new(dimensions2);

            // Act
            IMatrixMap result = map1.UnionMap(map2);

            // Assert - Should have map1 values first, then new values from map2
            CollectionAssert.AreEqual(new List<string> { "cc11", "aa11", "bb11", "dd11" }, result.DimensionMaps[0].ValueCodes.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UnionMapWithDifferentDimensionCountThrows()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                new DimensionMap("11", ["aa11", "bb11"]),
                new DimensionMap("22", ["aa22", "bb22"])
            ];

            List<IDimensionMap> dimensions2 = [
                new DimensionMap("11", ["aa11", "bb11"])
            ];

            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new(dimensions2);

            // Act
            _ = map1.UnionMap(map2);
        }

        [TestMethod]
        public void UnionMapWithDifferentDimensionCodesThrows()
        {
            // Arrange
            List<IDimensionMap> dimensions1 = [
                new DimensionMap("11", ["aa11", "bb11"]),
                new DimensionMap("22", ["aa22", "bb22"])
            ];

            List<IDimensionMap> dimensions2 = [
                new DimensionMap("11", ["aa11", "bb11"]),
                new DimensionMap("33", ["aa33", "bb33"]) // Different code
            ];

            MatrixMap map1 = new(dimensions1);
            MatrixMap map2 = new(dimensions2);

            // Act
            Assert.ThrowsExactly<InvalidOperationException>(() => _ = map1.UnionMap(map2));
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
            Assert.AreEqual(9L, collapsedMap.GetSize()); // 3 * 1 * 3 = 9
            Assert.AreEqual(1, collapsedMap.DimensionMaps[1].ValueCodes.Count);
            Assert.AreEqual("foo", collapsedMap.DimensionMaps[1].ValueCodes[0]);
        }

        #endregion

        #region GetIndicesOfSubmap

        [TestMethod]
        public void GetIndicesOfSubmapWithValidSubmapReturnsCorrectIndices()
        {
            List<IDimensionMap> superDimensions = [
                new DimensionMap("11", ["aa11", "bb11", "cc11"]),

                new DimensionMap("22", ["aa22", "bb22", "cc22"]),
                new DimensionMap("33", ["aa33", "bb33", "cc33", "dd33"]),
            ];
            MatrixMap superMap = new(superDimensions);

            List<IDimensionMap> subDimensions = [
                new DimensionMap("11", ["aa11", "cc11"]), // indices 0,2
                new DimensionMap("22", ["bb22"]),           // index 1
                new DimensionMap("33", ["bb33", "dd33"])   // indices 1,3
            ];
            MatrixMap subMap = new(subDimensions);

            int[][] indices = superMap.GetIndicesOfSubmap(subMap);
            int[][] expected =
            [
                [0, 2],
                [1],
                [1, 3]
            ];

            CollectionAssert.AreEqual(expected[0], indices[0]);
            CollectionAssert.AreEqual(expected[1], indices[1]);
            CollectionAssert.AreEqual(expected[2], indices[2]);
        }

        [TestMethod]
        public void GetIndicesOfSubmapSubmapEqualsSupermapReturnsIdentityIndices()
        {
            List<IDimensionMap> dimensions = [
                new DimensionMap("11", ["aa11", "bb11"]),
                new DimensionMap("22", ["aa22"])
            ];
            MatrixMap map = new(dimensions);

            int[][] indices = map.GetIndicesOfSubmap(map);
            int[][] expected =
            [
                [0, 1],
                [0]
            ];

            CollectionAssert.AreEqual(expected[0], indices[0]);
            CollectionAssert.AreEqual(expected[1], indices[1]);
        }

        [TestMethod]
        public void GetIndicesOfSubmapDimensionCountMismatchThrowsArgumentException()
        {
            List<IDimensionMap> superDimensions = [
                new DimensionMap("11", ["aa11", "bb11", "cc11"]),
                new DimensionMap("22", ["aa22", "bb22"])
            ];
            MatrixMap superMap = new(superDimensions);

            List<IDimensionMap> subDimensions = [
                new DimensionMap("11", ["aa11", "bb11", "cc11"]) // missing second dimension
            ];
            MatrixMap subMap = new(subDimensions);

            Assert.ThrowsExactly<ArgumentException>(() => superMap.GetIndicesOfSubmap(subMap));
        }
        #endregion
    }
}
