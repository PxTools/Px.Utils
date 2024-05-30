using Px.Utils.Models;
using Px.Utils.Models.Metadata;
using Px.Utils.Operations;

namespace Px.Utils.UnitTests.OperationsTests
{
    [TestClass]
    public class TransformationMatrixFunctionTests
    {
        [TestMethod]
        public void ApplyWhenCalledWithIdentityMapReturnsIdenticalMatrix()
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
        public void ApplyWhenCalledWithTransformationMapReturnsReorderedSubset()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([5, 5, 10]);
            int[] testData = new int[250];
            for (int i = 0; i < 250; i++) testData[i] = i;
            var matrix = new Matrix<int>(meta, testData);

            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val3", "var0_val0", "var0_val4"]),
                new DimensionMap("var2", ["var2_val1", "var2_val7"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2", "var1_val4"])
            ]);

            var function = new TransformationMatrixFunction<int>(matrixMap);

            // Act
            var result = function.Apply(matrix);

            // Assert
            int[] expectedData = [
                151, 161, 171, 191, 157, 167,
                177, 197, 1, 11, 21, 41,
                7, 17, 27, 47, 201, 211,
                221, 241, 207, 217, 227, 247];
            CollectionAssert.AreEqual(expectedData, result.Data);
            CollectionAssert.AreEqual(matrixMap.DimensionMaps.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            for (int i = 0; i < meta.Dimensions.Count; i++)
            {
                CollectionAssert.AreEqual(matrixMap.DimensionMaps[i].ValueCodes.ToArray(), result.Metadata.Dimensions[i].Values.Select(v => v.Code).ToArray());
            }
        }
    }
}
