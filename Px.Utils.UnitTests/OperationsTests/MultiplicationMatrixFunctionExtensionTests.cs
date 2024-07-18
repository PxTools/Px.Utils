using Px.Utils.Models;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Operations;

namespace Px.Utils.UnitTests.OperationsTests
{
    [TestClass]
    public class MultiplicationMatrixFunctionExtensionTests
    {

        [TestMethod]
        public void MultiplyToNewValueAddsNewValueAndComputesCorrectProducts()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([2, 3, 2]);
            int[] testData = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
            Matrix<int> matrix = new(meta, testData);

            DimensionMap multiplicationMap = new("var2", ["var2_val0", "var2_val1"]);
            DimensionValue productValue = new("var2_prodval0", new("fi", "var2_prodval0"), true);

            // Act
            Matrix<int> result = matrix.MultiplyToNewValue(productValue, multiplicationMap);

            // Assert
            int[] expectedData = [0, 1, 0, 2, 3, 6, 4, 5, 20, 6, 7, 42, 8, 9, 72, 10, 11, 110];
            CollectionAssert.AreEqual(expectedData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            string[] expectedVar0Vals = ["var0_val0", "var0_val1"];
            CollectionAssert.AreEqual(expectedVar0Vals, result.Metadata.Dimensions[0].Values.Select(v => v.Code).ToArray());

            string[] expectedVar1Vals = ["var1_val0", "var1_val1", "var1_val2"];
            CollectionAssert.AreEqual(expectedVar1Vals, result.Metadata.Dimensions[1].Values.Select(v => v.Code).ToArray());

            string[] expectedVar2Vals = ["var2_val0", "var2_val1", "var2_prodval0"];
            CollectionAssert.AreEqual(expectedVar2Vals, result.Metadata.Dimensions[2].Values.Select(v => v.Code).ToArray());
        }
        
        [TestMethod]
        public async Task MultiplyToNewValueAsyncAddsNewValueAndComputesCorrectProducts()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([2, 3, 2]);
            int[] testData = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
            Matrix<int> matrix = new(meta, testData);

            DimensionMap multiplicationMap = new("var2", ["var2_val0", "var2_val1"]);
            DimensionValue productValue = new("var2_prodval0", new("fi", "var2_prodval0"), true);

            // Act
            Matrix<int> result = await matrix.MultiplyToNewValueAsync(productValue, multiplicationMap);

            // Assert
            int[] expectedData = [0, 1, 0, 2, 3, 6, 4, 5, 20, 6, 7, 42, 8, 9, 72, 10, 11, 110];
            CollectionAssert.AreEqual(expectedData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            string[] expectedVar0Vals = ["var0_val0", "var0_val1"];
            CollectionAssert.AreEqual(expectedVar0Vals, result.Metadata.Dimensions[0].Values.Select(v => v.Code).ToArray());

            string[] expectedVar1Vals = ["var1_val0", "var1_val1", "var1_val2"];
            CollectionAssert.AreEqual(expectedVar1Vals, result.Metadata.Dimensions[1].Values.Select(v => v.Code).ToArray());

            string[] expectedVar2Vals = ["var2_val0", "var2_val1", "var2_prodval0"];
            CollectionAssert.AreEqual(expectedVar2Vals, result.Metadata.Dimensions[2].Values.Select(v => v.Code).ToArray());
        }
        
        [TestMethod]
        public async Task MultiplyToNewValueAsyncWithSourceTaskAddsNewValueAndComputesCorrectProducts()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([2, 3, 2]);
            int[] testData = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
            Task<Matrix<int>> matrixTask = Task.Factory.StartNew(() => new Matrix<int>(meta, testData));

            DimensionMap multiplicationMap = new("var2", ["var2_val0", "var2_val1"]);
            DimensionValue productValue = new("var2_prodval0", new("fi", "var2_prodval0"), true);

            // Act
            Matrix<int> result = await matrixTask.MultiplyToNewValueAsync(productValue, multiplicationMap);

            // Assert
            int[] expectedData = [0, 1, 0, 2, 3, 6, 4, 5, 20, 6, 7, 42, 8, 9, 72, 10, 11, 110];
            CollectionAssert.AreEqual(expectedData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            string[] expectedVar0Vals = ["var0_val0", "var0_val1"];
            CollectionAssert.AreEqual(expectedVar0Vals, result.Metadata.Dimensions[0].Values.Select(v => v.Code).ToArray());

            string[] expectedVar1Vals = ["var1_val0", "var1_val1", "var1_val2"];
            CollectionAssert.AreEqual(expectedVar1Vals, result.Metadata.Dimensions[1].Values.Select(v => v.Code).ToArray());

            string[] expectedVar2Vals = ["var2_val0", "var2_val1", "var2_prodval0"];
            CollectionAssert.AreEqual(expectedVar2Vals, result.Metadata.Dimensions[2].Values.Select(v => v.Code).ToArray());
        }

        [TestMethod]
        public void MultiplyToNewValueAddsNewValueAndInsertsCorrectProductsInSecondPosition()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([2, 3, 2]);
            int[] testData = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
            var matrix = new Matrix<int>(meta, testData);

            DimensionMap multiplicationMap = new("var2", ["var2_val0", "var2_val1"]);
            DimensionValue productValue = new("var2_productval0", new("fi", "var2_productval0"), true);

            Matrix<int> result = matrix.MultiplyToNewValue(productValue, multiplicationMap, 1);

            // Assert
            int[] expectedData = [0, 0, 1, 2, 6, 3, 4, 20, 5, 6, 42, 7, 8, 72, 9, 10, 110, 11];
            CollectionAssert.AreEqual(expectedData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            string[] expectedVar0Vals = ["var0_val0", "var0_val1"];
            CollectionAssert.AreEqual(expectedVar0Vals, result.Metadata.Dimensions[0].Values.Select(v => v.Code).ToArray());

            string[] expectedVar1Vals = ["var1_val0", "var1_val1", "var1_val2"];
            CollectionAssert.AreEqual(expectedVar1Vals, result.Metadata.Dimensions[1].Values.Select(v => v.Code).ToArray());

            string[] expectedVar2Vals = ["var2_val0", "var2_productval0", "var2_val1"];
            CollectionAssert.AreEqual(expectedVar2Vals, result.Metadata.Dimensions[2].Values.Select(v => v.Code).ToArray());
        }

        [TestMethod]
        public void MultiplyToNewValueAddsNewContentValueAndComputesCorrectProducts()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([2, 3, 2]);
            int[] testData = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
            var matrix = new Matrix<int>(meta, testData);

            DimensionMap multiplicationMap = new("var0", ["var0_val0", "var0_val1"]);
            ContentDimensionValue productValue = new("var0_productval0", new("fi", "var0_productval0"), new("fi", "var0_unit"), new(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc), 0, true);

            // Act
            Matrix<int> result = matrix.MultiplyToNewValue(productValue, multiplicationMap);

            // Assert
            int[] expectedData = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 0, 7, 16, 27, 40, 55];
            CollectionAssert.AreEqual(expectedData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            string[] expectedVar0Vals = ["var0_val0", "var0_val1", "var0_productval0"];
            CollectionAssert.AreEqual(expectedVar0Vals, result.Metadata.Dimensions[0].Values.Select(v => v.Code).ToArray());
            CollectionAssert.AllItemsAreInstancesOfType(result.Metadata.Dimensions[0].Values.ToList(), typeof(ContentDimensionValue));

            string[] expectedVar1Vals = ["var1_val0", "var1_val1", "var1_val2"];
            CollectionAssert.AreEqual(expectedVar1Vals, result.Metadata.Dimensions[1].Values.Select(v => v.Code).ToArray());

            string[] expectedVar2Vals = ["var2_val0", "var2_val1"];
            CollectionAssert.AreEqual(expectedVar2Vals, result.Metadata.Dimensions[2].Values.Select(v => v.Code).ToArray());
        }

        [TestMethod]
        public void MultiplyToNewValueIsCalledWithOneValueMap()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([2, 2, 1]);
            int[] testData = [0, 1, 2, 3];
            var matrix = new Matrix<int>(meta, testData);

            DimensionMap multiplicationMap = new("var2", ["var2_val0"]);
            DimensionValue productValue = new("var2_productval0", new("fi", "var2_productval0"), true);

            // Act
            Matrix<int> result = matrix.MultiplyToNewValue(productValue, multiplicationMap);

            // Assert
            int[] expectedData = [0, 0, 1, 1, 2, 2, 3, 3];
            CollectionAssert.AreEqual(expectedData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            string[] expectedVar0Vals = ["var0_val0", "var0_val1"];
            CollectionAssert.AreEqual(expectedVar0Vals, result.Metadata.Dimensions[0].Values.Select(v => v.Code).ToArray());

            string[] expectedVar1Vals = ["var1_val0", "var1_val1"];
            CollectionAssert.AreEqual(expectedVar1Vals, result.Metadata.Dimensions[1].Values.Select(v => v.Code).ToArray());

            string[] expectedVar2Vals = ["var2_val0", "var2_productval0"];
            CollectionAssert.AreEqual(expectedVar2Vals, result.Metadata.Dimensions[2].Values.Select(v => v.Code).ToArray());
        }

        [TestMethod]
        public void MultiplySubmapWithConstantProducesCorrectOutputValues()
        {
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([3,3,3]);
            int[] testData = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1];
            Matrix<int> matrix = new(meta, testData);

            IMatrixMap targetMap = new MatrixMap([
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2"]),
                new DimensionMap("var2", ["var2_val0"])
                ]);

            // Act
            Matrix<int> result = matrix.MultiplySubsetByConstant(targetMap, 2);

            // Assert
            int[] expectedData = [2, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1];
            CollectionAssert.AreEqual(expectedData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            string[] expectedVar0Vals = ["var0_val0", "var0_val1", "var0_val2"];
            CollectionAssert.AreEqual(expectedVar0Vals, result.Metadata.Dimensions[0].Values.Select(v => v.Code).ToArray());

            string[] expectedVar1Vals = ["var1_val0", "var1_val1", "var1_val2"];
            CollectionAssert.AreEqual(expectedVar1Vals, result.Metadata.Dimensions[1].Values.Select(v => v.Code).ToArray());

            string[] expectedVar2Vals = ["var2_val0", "var2_val1", "var2_val2"];
            CollectionAssert.AreEqual(expectedVar2Vals, result.Metadata.Dimensions[2].Values.Select(v => v.Code).ToArray());
        }

        [TestMethod]
        public async Task MultiplySubmapAsyncWithConstantProducesCorrectOutputValues()
        {
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([3,3,3]);
            int[] testData = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1];
            Matrix<int> matrix = new(meta, testData);

            IMatrixMap targetMap = new MatrixMap([
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2"]),
                new DimensionMap("var2", ["var2_val0"])
                ]);

            // Act
            Matrix<int> result = await matrix.MultiplySubsetByConstantAsync(targetMap, 2);

            // Assert
            int[] expectedData = [2, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1];
            CollectionAssert.AreEqual(expectedData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            string[] expectedVar0Vals = ["var0_val0", "var0_val1", "var0_val2"];
            CollectionAssert.AreEqual(expectedVar0Vals, result.Metadata.Dimensions[0].Values.Select(v => v.Code).ToArray());

            string[] expectedVar1Vals = ["var1_val0", "var1_val1", "var1_val2"];
            CollectionAssert.AreEqual(expectedVar1Vals, result.Metadata.Dimensions[1].Values.Select(v => v.Code).ToArray());

            string[] expectedVar2Vals = ["var2_val0", "var2_val1", "var2_val2"];
            CollectionAssert.AreEqual(expectedVar2Vals, result.Metadata.Dimensions[2].Values.Select(v => v.Code).ToArray());
        }

        [TestMethod]
        public async Task MultiplySubmapAsyncWithSourceTaskWithConstantProducesCorrectOutputValues()
        {
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([3,3,3]);
            int[] testData = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1];
            Task<Matrix<int>> matrixTask = Task.Factory.StartNew(() => new Matrix<int>(meta, testData));

            IMatrixMap targetMap = new MatrixMap([
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2"]),
                new DimensionMap("var2", ["var2_val0"])
                ]);

            // Act
            Matrix<int> result = await matrixTask.MultiplySubsetByConstantAsync(targetMap, 2);

            // Assert
            int[] expectedData = [2, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1];
            CollectionAssert.AreEqual(expectedData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            string[] expectedVar0Vals = ["var0_val0", "var0_val1", "var0_val2"];
            CollectionAssert.AreEqual(expectedVar0Vals, result.Metadata.Dimensions[0].Values.Select(v => v.Code).ToArray());

            string[] expectedVar1Vals = ["var1_val0", "var1_val1", "var1_val2"];
            CollectionAssert.AreEqual(expectedVar1Vals, result.Metadata.Dimensions[1].Values.Select(v => v.Code).ToArray());

            string[] expectedVar2Vals = ["var2_val0", "var2_val1", "var2_val2"];
            CollectionAssert.AreEqual(expectedVar2Vals, result.Metadata.Dimensions[2].Values.Select(v => v.Code).ToArray());
        }

        [TestMethod]
        public void MultiplyWholeMatrixWithConstantProducesCorrectOutputValues()
        {
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([3,3,3]);
            int[] testData = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26];
            Matrix<int> matrix = new(meta, testData);

            IMatrixMap targetMap = new MatrixMap([
                new DimensionMap("var0", ["var0_val0", "var0_val1", "var0_val2"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2"])
                ]);

            // Act
            Matrix<int> result = matrix.MultiplySubsetByConstant(targetMap, 2);

            // Assert
            int[] expectedData = [0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52];
            CollectionAssert.AreEqual(expectedData, result.Data);
            CollectionAssert.AreEqual(meta.Dimensions.Select(c => c.Code).ToArray(), result.Metadata.Dimensions.Select(c => c.Code).ToArray());

            string[] expectedVar0Vals = ["var0_val0", "var0_val1", "var0_val2"];
            CollectionAssert.AreEqual(expectedVar0Vals, result.Metadata.Dimensions[0].Values.Select(v => v.Code).ToArray());

            string[] expectedVar1Vals = ["var1_val0", "var1_val1", "var1_val2"];
            CollectionAssert.AreEqual(expectedVar1Vals, result.Metadata.Dimensions[1].Values.Select(v => v.Code).ToArray());

            string[] expectedVar2Vals = ["var2_val0", "var2_val1", "var2_val2"];
            CollectionAssert.AreEqual(expectedVar2Vals, result.Metadata.Dimensions[2].Values.Select(v => v.Code).ToArray());
        }
    }
}
