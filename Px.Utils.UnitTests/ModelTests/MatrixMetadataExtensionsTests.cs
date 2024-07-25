using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;

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
    }
}
