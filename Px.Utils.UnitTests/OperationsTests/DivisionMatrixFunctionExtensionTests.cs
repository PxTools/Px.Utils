using Px.Utils.Models;
using Px.Utils.Models.Metadata;
using Px.Utils.Operations;

namespace Px.Utils.UnitTests.OperationsTests
{
    [TestClass]
    public class DivisionMatrixFunctionExtensionTests
    {
        [TestMethod]
        public void DivideBySelectedValueProducesCorrectOutputMatrix()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([2, 3, 2]);
            double[] testData = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
            Matrix<double> matrix = new(meta, testData);

            string divisionCode = "var1_val0";
            DimensionMap targetMap = new("var1", ["var1_val1", "var1_val2"]);

            Matrix<double> result = matrix.DivideSubsetBySelectedValue(targetMap, divisionCode);

            // Assert with delta
            double[] expectedData = [1, 2, 3, 2, 5, 3, 7, 8, 1.285, 1.25, 1.571, 1.5];
            for(int i = 0; i < result.Data.Length; i++)
            {
                    Assert.AreEqual(expectedData[i], result.Data[i], 0.001);
            }
        }
        
        [TestMethod]
        public async Task DivideBySelectedValueAsyncProducesCorrectOutputMatrix()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([2, 3, 2]);
            double[] testData = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
            Matrix<double> matrix = new(meta, testData);

            string divisionCode = "var1_val0";
            DimensionMap targetMap = new("var1", ["var1_val1", "var1_val2"]);

            Matrix<double> result = await matrix.DivideSubsetBySelectedValueAsync(targetMap, divisionCode);

            // Assert with delta
            double[] expectedData = [1, 2, 3, 2, 5, 3, 7, 8, 1.285, 1.25, 1.571, 1.5];
            for(int i = 0; i < result.Data.Length; i++)
            {
                    Assert.AreEqual(expectedData[i], result.Data[i], 0.001);
            }
        }
        
        [TestMethod]
        public async Task DivideBySelectedValueAsyncWithTaskSourceProducesCorrectOutputMatrix()
        {
            // Arrange
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([2, 3, 2]);
            double[] testData = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
            Task<Matrix<double>> matrix = Task.Factory.StartNew(() => new Matrix<double>(meta, testData));

            string divisionCode = "var1_val0";
            DimensionMap targetMap = new("var1", ["var1_val1", "var1_val2"]);

            Matrix<double> result = await matrix.DivideSubsetBySelectedValueAsync(targetMap, divisionCode);

            // Assert with delta
            double[] expectedData = [1, 2, 3, 2, 5, 3, 7, 8, 1.285, 1.25, 1.571, 1.5];
            for(int i = 0; i < result.Data.Length; i++)
            {
                    Assert.AreEqual(expectedData[i], result.Data[i], 0.001);
            }
        }
        
        [TestMethod]
        public void DivideSubmapWithConstantProducesCorrectOutputValues()
        {
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([3,3,3]);
            double[] testData = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1];
            Matrix<double> matrixTask = new (meta, testData);

            IMatrixMap targetMap = new MatrixMap([
                new DimensionMap("var0", ["var0_val0", "var0_val2"]),
                new DimensionMap("var1", ["var1_val0", "var1_val2"]),
                new DimensionMap("var2", ["var2_val0", "var2_val2"])
                ]);

            // Act
            Matrix<double> result = matrixTask.DivideSubsetByConstant(targetMap, 4);

            // Assert
            double[] expectedData = [0.25, 1, 0.25, 1, 1, 1, 0.25, 1, 0.25, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0.25, 1, 0.25, 1, 1, 1, 0.25, 1, 0.25];
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
        public async Task DivideSubmapWithConstantAsyncProducesCorrectOutputValues()
        {
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([3,3,3]);
            double[] testData = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1];
            Matrix<double> matrixTask = new Matrix<double>(meta, testData);

            IMatrixMap targetMap = new MatrixMap([
                new DimensionMap("var0", ["var0_val0", "var0_val2"]),
                new DimensionMap("var1", ["var1_val0", "var1_val2"]),
                new DimensionMap("var2", ["var2_val0", "var2_val2"])
                ]);

            // Act
            Matrix<double> result = await matrixTask.DivideSubsetByConstantAsync(targetMap, 4);

            // Assert
            double[] expectedData = [0.25, 1, 0.25, 1, 1, 1, 0.25, 1, 0.25, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0.25, 1, 0.25, 1, 1, 1, 0.25, 1, 0.25];
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
        public async Task DivideSubmapWithConstantAsyncWithSourceTaskProducesCorrectOutputValues()
        {
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([3,3,3]);
            double[] testData = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1];
            Task<Matrix<double>> matrixTask = Task.Factory.StartNew(() => new Matrix<double>(meta, testData));

            IMatrixMap targetMap = new MatrixMap([
                new DimensionMap("var0", ["var0_val0", "var0_val2"]),
                new DimensionMap("var1", ["var1_val0", "var1_val2"]),
                new DimensionMap("var2", ["var2_val0", "var2_val2"])
                ]);

            // Act
            Matrix<double> result = await matrixTask.DivideSubsetByConstantAsync(targetMap, 4);

            // Assert
            double[] expectedData = [0.25, 1, 0.25, 1, 1, 1, 0.25, 1, 0.25, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0.25, 1, 0.25, 1, 1, 1, 0.25, 1, 0.25];
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
        public void DivideWholeMatrixWithConstantProducesCorrectOutputValues()
        {
            MatrixMetadata meta = TestModelBuilder.BuildTestMetadata([3,3,3]);
            double[] testData = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26];
            Matrix<double> matrix = new(meta, testData);

            IMatrixMap targetMap = new MatrixMap([
                new DimensionMap("var0", ["var0_val0", "var0_val1", "var0_val2"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2"])
                ]);

            // Act
            Matrix<double> result = matrix.DivideSubsetByConstant(targetMap, 2);

            // Assert
            double[] expectedData = [0, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5, 10, 10.5, 11, 11.5, 12, 12.5, 13];
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
