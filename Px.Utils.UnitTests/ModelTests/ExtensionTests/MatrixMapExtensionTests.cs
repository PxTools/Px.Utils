using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.ExtensionMethods;

namespace Px.Utils.UnitTests.ModelTests.ExtensionTests
{
    [TestClass]
    public class MatrixMapExtensionTests
    {
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

    }
}
