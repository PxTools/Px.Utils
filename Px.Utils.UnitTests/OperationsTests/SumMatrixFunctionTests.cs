using Px.Utils.Models;
using Px.Utils.Models.Metadata;
using Px.Utils.Operations;
using PxUtils.Models.Metadata;
using PxUtils.Models.Metadata.Dimensions;

namespace Px.Utils.UnitTests.OperationsTests
{
    [TestClass]
    public class SumMatrixFunctionTests
    {
        [TestMethod]
        public void Apply_WhenCalledWithNonContentSumMap_ReturnsNewMatrixWithSummedValues()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([2, 3, 2]);
            int[] testData = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
            var matrix = new Matrix<int>(meta, testData);

            DimensionMap sumMap = new("var2", ["var2_val0", "var2_val1"]);
            DimensionValue sumValue = new("var2_sumval0", new("fi", "var2_sumval0"), true);

            SumMatrixFunction<int> function = new(sumValue, sumMap);

            // Act
            Matrix<int> result = function.Apply(matrix);

            // Assert
            int[] expectedData = [0, 1, 1, 2, 3, 5, 4, 5, 9, 6, 7, 13, 8, 9, 17, 10, 11, 21];
            CollectionAssert.AreEqual(expectedData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            string[] expectedVar0Vals = ["var0_val0", "var0_val1"];
            CollectionAssert.AreEqual(expectedVar0Vals, result.Metadata.Dimensions[0].Values.Select(v => v.Code).ToArray());

            string[] expectedVar1Vals = ["var1_val0", "var1_val1", "var1_val2"];
            CollectionAssert.AreEqual(expectedVar1Vals, result.Metadata.Dimensions[1].Values.Select(v => v.Code).ToArray());

            string[] expectedVar2Vals = ["var2_val0", "var2_val1", "var2_sumval0"];
            CollectionAssert.AreEqual(expectedVar2Vals, result.Metadata.Dimensions[2].Values.Select(v => v.Code).ToArray());
        }

        [TestMethod]
        public void Apply_WhenCalledWithNonContentSumMapWithValueInsertion_ReturnsNewMatrixWithSummedValuesInserted()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([2, 3, 2]);
            int[] testData = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
            var matrix = new Matrix<int>(meta, testData);

            DimensionMap sumMap = new("var2", ["var2_val0", "var2_val1"]);
            DimensionValue sumValue = new("var2_sumval0", new("fi", "var2_sumval0"), true);

            SumMatrixFunction<int> function = new(sumValue, sumMap, 1); // Insert at index 1

            // Act
            Matrix<int> result = function.Apply(matrix);

            // Assert
            int[] expectedData = [0, 1, 1, 2, 5, 3, 4, 9, 5, 6, 13, 7, 8, 17, 9, 10, 21, 11];
            CollectionAssert.AreEqual(expectedData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            string[] expectedVar0Vals = ["var0_val0", "var0_val1"];
            CollectionAssert.AreEqual(expectedVar0Vals, result.Metadata.Dimensions[0].Values.Select(v => v.Code).ToArray());

            string[] expectedVar1Vals = ["var1_val0", "var1_val1", "var1_val2"];
            CollectionAssert.AreEqual(expectedVar1Vals, result.Metadata.Dimensions[1].Values.Select(v => v.Code).ToArray());

            string[] expectedVar2Vals = ["var2_val0", "var2_sumval0", "var2_val1"];
            CollectionAssert.AreEqual(expectedVar2Vals, result.Metadata.Dimensions[2].Values.Select(v => v.Code).ToArray());
        }

        [TestMethod]
        public void Apply_WhenCalledWithContentSumMap_ReturnsNewMatrixWithSummedValues()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([2, 3, 2]);
            int[] testData = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
            var matrix = new Matrix<int>(meta, testData);

            DimensionMap sumMap = new("var0", ["var0_val0", "var0_val1"]);
            ContentDimensionValue sumValue = new("var0_sumval0", new("fi", "var0_sumval0"), new("fi", "var0_unit"), new(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc), 0, true);

            SumMatrixFunction<int> function = new(sumValue, sumMap);

            // Act
            Matrix<int> result = function.Apply(matrix);

            // Assert
            int[] expectedData = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 6, 8, 10, 12, 14, 16];
            CollectionAssert.AreEqual(expectedData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            string[] expectedVar0Vals = ["var0_val0", "var0_val1", "var0_sumval0"];
            CollectionAssert.AreEqual(expectedVar0Vals, result.Metadata.Dimensions[0].Values.Select(v => v.Code).ToArray());
            CollectionAssert.AllItemsAreInstancesOfType(result.Metadata.Dimensions[0].Values.ToList(), typeof(ContentDimensionValue));

            string[] expectedVar1Vals = ["var1_val0", "var1_val1", "var1_val2"];
            CollectionAssert.AreEqual(expectedVar1Vals, result.Metadata.Dimensions[1].Values.Select(v => v.Code).ToArray());

            string[] expectedVar2Vals = ["var2_val0", "var2_val1"];
            CollectionAssert.AreEqual(expectedVar2Vals, result.Metadata.Dimensions[2].Values.Select(v => v.Code).ToArray());
        }

        [TestMethod]
        public void Apply_WhenCalledWithOneValueSumMap_ReturnsNewMatrixWithSummedValues()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([2, 2, 1]);
            int[] testData = [0, 1, 2, 3];
            var matrix = new Matrix<int>(meta, testData);

            DimensionMap sumMap = new("var2", ["var2_val0"]);
            DimensionValue sumValue = new("var2_sumval0", new("fi", "var2_sumval0"), true);

            SumMatrixFunction<int> function = new(sumValue, sumMap);

            // Act
            Matrix<int> result = function.Apply(matrix);

            // Assert
            int[] expectedData = [0, 0, 1, 1, 2, 2, 3, 3];
            CollectionAssert.AreEqual(expectedData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            string[] expectedVar0Vals = ["var0_val0", "var0_val1"];
            CollectionAssert.AreEqual(expectedVar0Vals, result.Metadata.Dimensions[0].Values.Select(v => v.Code).ToArray());

            string[] expectedVar1Vals = ["var1_val0", "var1_val1"];
            CollectionAssert.AreEqual(expectedVar1Vals, result.Metadata.Dimensions[1].Values.Select(v => v.Code).ToArray());

            string[] expectedVar2Vals = ["var2_val0", "var2_sumval0"];
            CollectionAssert.AreEqual(expectedVar2Vals, result.Metadata.Dimensions[2].Values.Select(v => v.Code).ToArray());
        }
    }
}
