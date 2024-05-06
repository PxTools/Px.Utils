using Px.Utils.Models;
using Px.Utils.Operations;
using PxUtils.Models.Metadata;

namespace Px.Utils.UnitTests.OperationsTests
{
    [TestClass]
    public class TransformationMatrixFunctionTests
    {
        [TestMethod]
        public void Apply_WhenCalledWithIdentityMap_ReturnsIdenticalMatrix()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([2, 3, 4]);
            int[] testData = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23];
            var matrix = new Matrix<int>(meta, testData);
            var function = new TransformationMatrixFunction<int>(meta);

            // Act
            var result = function.Apply(matrix);

            // Assert
            CollectionAssert.AreEqual(testData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            for (int i = 0; i < meta.Dimensions.Count; i++)
            {
                CollectionAssert.AreEqual(meta.Dimensions[i].Values.Select(v => v.Code).ToArray(), result.Metadata.Dimensions[i].Values.Select(v => v.Code).ToArray());
            }
        }

        [TestMethod]
        public void Apply_WhenCalledWithIdentityMap_ReturnsIdenticalMatrix()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([2, 3, 4]);
            int[] testData = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23];
            var matrix = new Matrix<int>(meta, testData);
            var function = new TransformationMatrixFunction<int>(meta);

            // Act
            var result = function.Apply(matrix);

            // Assert
            CollectionAssert.AreEqual(testData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            for (int i = 0; i < meta.Dimensions.Count; i++)
            {
                CollectionAssert.AreEqual(meta.Dimensions[i].Values.Select(v => v.Code).ToArray(), result.Metadata.Dimensions[i].Values.Select(v => v.Code).ToArray());
            }
        }
    }
}
