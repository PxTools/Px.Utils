using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Models.Metadata.ExtensionMethods;

namespace Px.Utils.UnitTests.ModelTests
{
    [TestClass]
    public class MatrixMetadataExtensionsTests
    {
        [TestMethod]
        public void MatrixMetadataExtensionsGetContentDimensionReturnsContentDimension()
        {
            // Arrange
            int[] dimensionSizes = [4, 3, 2, 1];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);

            // Act
            ContentDimension contentDimension = metadata.GetContentDimension();

            // Assert
            Assert.IsNotNull(contentDimension);
            Assert.AreEqual(4, contentDimension.Values.Count);
        }

        [TestMethod]
        public void MatrixMetadataExtensionsGetContentDimensionThrowsWhenNoContent()
        {
            // Arrange
            int[] dimensionSizes = [4, 3, 2, 1];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);
            metadata.Dimensions.RemoveAt(0);

            // Assert
            Assert.ThrowsException<InvalidOperationException>(metadata.GetContentDimension);
        }

        [TestMethod]
        public void MatrixMetadataExtensionsTryGetContentDimensionReturnsTrue()
        {
            // Arrange
            int[] dimensionSizes = [4, 3, 2, 1];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);

            // Act
            bool found = metadata.TryGetContentDimension(out ContentDimension? dim);

            // Assert
            Assert.IsTrue(found);
            Assert.IsNotNull(dim);
            Assert.AreEqual(4, dim.Values.Count);
        }

        [TestMethod]
        public void MatrixMetadataExtensionsTryGetContentDimensionReturnsFalse()
        {
            // Arrange
            int[] dimensionSizes = [4, 3, 2, 1];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);
            metadata.Dimensions.RemoveAt(0);

            // Act
            bool found = metadata.TryGetContentDimension(out ContentDimension? dim);

            // Assert
            Assert.IsFalse(found);
            Assert.IsNull(dim);
        }

        [TestMethod]
        public void MatrixMetadataExtensionsGetTimeDimensionReturnsTimeDimension()
        {
            // Arrange
            int[] dimensionSizes = [4, 3, 2, 1];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);

            // Act
            TimeDimension timeDimension = metadata.GetTimeDimension();

            // Assert
            Assert.IsNotNull(timeDimension);
            Assert.AreEqual(3, timeDimension.Values.Count);
        }

        [TestMethod]
        public void MatrixMetadataExtensionsGetTimeDimensionThrowsWhenNoTimeDimension()
        {
            // Arrange
            int[] dimensionSizes = [4, 3, 2, 1];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);
            metadata.Dimensions.RemoveAll(dimension => dimension.Type == Models.Metadata.Enums.DimensionType.Time);

            // Assert
            Assert.ThrowsException<InvalidOperationException>(metadata.GetTimeDimension);
        }

        [TestMethod]
        public void MatrixMetadataExtensionsTryGetTimeDimensionReturnsTrue()
        {
            // Arrange
            int[] dimensionSizes = [4, 3, 2, 1];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);

            // Act
            bool found = metadata.TryGetTimeDimension(out TimeDimension? dim);

            // Assert
            Assert.IsTrue(found);
            Assert.IsNotNull(dim);
            Assert.AreEqual(3, dim.Values.Count);
        }

        [TestMethod]
        public void MatrixMetadataExtensionsTryGetTimeDimensionReturnsFalse()
        {
            // Arrange
            int[] dimensionSizes = [4, 3, 2, 1];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);
            metadata.Dimensions.RemoveAll(dimension => dimension.Type == Models.Metadata.Enums.DimensionType.Time);

            // Act
            bool found = metadata.TryGetTimeDimension(out TimeDimension? dim);

            // Assert
            Assert.IsFalse(found);
            Assert.IsNull(dim);
        }
    }
}
