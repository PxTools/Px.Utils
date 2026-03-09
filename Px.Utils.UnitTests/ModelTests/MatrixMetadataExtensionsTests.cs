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
            Assert.ThrowsExactly<InvalidOperationException>(() => metadata.GetContentDimension());
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
            Assert.ThrowsExactly<InvalidOperationException>(() => metadata.GetTimeDimension());
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

        [TestMethod]
        public void MatrixMetadataExtensionsGetLastUpdatedReturnsLatestLastUpdatedTimeStamp()
        {
            // Arrange
            int[] dimensionSizes = [2, 1, 1, 1];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);
            ContentDimension contDim = metadata.GetContentDimension();
            ContentDimensionValue value0 = new(
                contDim.Values[0],
                contDim.Values[0].Unit,
                new(2020, 3, 9, 13, 0, 0, DateTimeKind.Unspecified),
                contDim.Values[0].Precision
            );
            ContentDimensionValue value1 = new(
                contDim.Values[1],
                contDim.Values[1].Unit,
                new(2026, 3, 9, 13, 0, 0, DateTimeKind.Unspecified),
                contDim.Values[1].Precision
            );
            ContentDimension newContDim = new(
                contDim.Code,
                contDim.Name,
                contDim.AdditionalProperties,
                new ContentValueList([value0, value1])
            );
            metadata.Dimensions[0] = newContDim;

            // Act
            DateTime lastUpdated = metadata.GetLastUpdated();

            // Assert
            Assert.AreEqual(lastUpdated, value1.LastUpdated);
        }

        [TestMethod]
        public void MatrixMetadataExtensionsGetLastUpdatedTimestampReturnsLatestDefaultFormat()
        {
            // Arrange
            int[] dimensionSizes = [2, 1, 1, 1];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);
            ContentDimension contDim = metadata.GetContentDimension();
            DateTime older = new(2020, 3, 9, 13, 0, 0, DateTimeKind.Unspecified);
            DateTime newer = new (2026, 3, 9, 13, 0, 0, DateTimeKind.Unspecified);
            ContentDimensionValue value0 = new(
                contDim.Values[0],
                contDim.Values[0].Unit,
                older,
                contDim.Values[0].Precision
            );
            ContentDimensionValue value1 = new(
                contDim.Values[1],
                contDim.Values[1].Unit,
                newer,
                contDim.Values[1].Precision
            );
            ContentDimension newContDim = new(
                contDim.Code,
                contDim.Name,
                contDim.AdditionalProperties,
                new ContentValueList([value0, value1])
            );
            metadata.Dimensions[0] = newContDim;
            string expected = "202603091300";

            // Act
            string timestamp = metadata.GetLastUpdatedTimestamp();

            // Assert
            Assert.AreEqual(expected, timestamp);
        }

        [TestMethod]
        public void MatrixMetadataExtensionsGetLastUpdatedTimestampReturnsLatestWithCustomFormat()
        {
            // Arrange
            int[] dimensionSizes = [2, 1, 1, 1];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);
            ContentDimension contDim = metadata.GetContentDimension();
            DateTime older = new(2020, 3, 9, 13, 0, 0, DateTimeKind.Unspecified);
            DateTime newer = new(2026, 3, 9, 13, 0, 0, DateTimeKind.Unspecified);
            ContentDimensionValue value0 = new(
                contDim.Values[0],
                contDim.Values[0].Unit,
                older,
                contDim.Values[0].Precision
            );
            ContentDimensionValue value1 = new(
                contDim.Values[1],
                contDim.Values[1].Unit,
                newer,
                contDim.Values[1].Precision
            );
            ContentDimension newContDim = new(
                contDim.Code,
                contDim.Name,
                contDim.AdditionalProperties,
                new ContentValueList([value0, value1])
            );
            metadata.Dimensions[0] = newContDim;
            const string format = "yyyy-MM-dd";
            string expected = "2026-03-09";

            // Act
            string timestamp = metadata.GetLastUpdatedTimestamp(format);

            // Assert
            Assert.AreEqual(expected, timestamp);
        }

        [TestMethod]
        public void MatrixMetadataExtensionsGetContentDimensionValueCodesReturnsCodes()
        {
            // Arrange
            int[] dimensionSizes = [3, 1, 1, 1];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);
            string[] expected = ["var0_val0", "var0_val1", "var0_val2"];

            // Act
            string[] result = metadata.GetContentDimensionValueCodes();

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }
    }
}
