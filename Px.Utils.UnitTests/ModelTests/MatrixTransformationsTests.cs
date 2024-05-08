using Px.Utils.Models;
using Px.Utils.Models.Metadata;
using PxUtils.Models.Metadata;

namespace Px.Utils.UnitTests.ModelTests
{
    [TestClass]
    public class MatrixTransformationsTests
    {
        [TestMethod]
        public void Subset()
        {
            int[] dimensionSizes = [3, 3, 2];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);


            double[] data = new double[18];
            for(int i = 0; i < 18; i++) data[i] = i;

            Matrix<double> matrix = new(metadata, data);

            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val1", "var0_val2"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1"])
            ]);

            Matrix<double> newMatrix = matrix.GetTransform(matrixMap);
            double[] expected = [ 6.0, 7.0, 8.0, 9.0, 12.0, 13.0, 14.0, 15.0 ];

            CollectionAssert.AreEqual(expected, newMatrix.Data);
        }

        [TestMethod]
        public void SingleValue()
        {
            int[] dimensionSizes = [3, 3, 2];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);


            double[] data = new double[18];
            for (int i = 0; i < 18; i++) data[i] = i;

            Matrix<double> matrix = new(metadata, data);

            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val2"]),
                new DimensionMap("var1", ["var1_val1"]),
                new DimensionMap("var2", ["var2_val1"])
            ]);

            Matrix<double> newMatrix = matrix.GetTransform(matrixMap);
            double[] expected = [15.0];

            CollectionAssert.AreEqual(expected, newMatrix.Data);
        }

        [TestMethod]
        public void Reorder()
        {
            int[] dimensionSizes = [3, 3, 2];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);


            double[] data = new double[18];
            for (int i = 0; i < 18; i++) data[i] = i;

            Matrix<double> matrix = new(metadata, data);

            MatrixMap matrixMap = new(
            [
                new DimensionMap("var1", ["var1_val1", "var1_val2", "var1_val0"]),
                new DimensionMap("var0", ["var0_val1", "var0_val2", "var0_val0",]),
                new DimensionMap("var2", ["var2_val0", "var2_val1"])
            ]);

            Matrix<double> newMatrix = matrix.GetTransform(matrixMap);
            double[] expected = [8.0, 9.0, 14.0, 15.0, 2.0, 3.0, 10.0, 11.0, 16.0, 17.0, 4.0, 5.0, 6.0, 7.0, 12.0, 13.0, 0.0, 1.0];

            CollectionAssert.AreEqual(
                matrixMap.DimensionMaps.Select(dm => dm.Code).ToList(),
                newMatrix.Metadata.DimensionMaps.Select(dm => dm.Code).ToList());
            CollectionAssert.AreEqual(expected, newMatrix.Data);
        }

        [TestMethod]
        public void SubsetAndReorder()
        {
            int[] dimensionSizes = [3, 3, 2];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);


            double[] data = new double[18];
            for (int i = 0; i < 18; i++) data[i] = i;

            Matrix<double> matrix = new(metadata, data);

            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val1", "var0_val2"]),
                new DimensionMap("var2", ["var2_val1", "var2_val0"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1"])
            ]);

            Matrix<double> newMatrix = matrix.GetTransform(matrixMap);
            double[] expected = [7.0, 9.0, 6.0, 8.0, 13.0, 15.0, 12.0, 14.0];

            CollectionAssert.AreEqual(expected, newMatrix.Data);
        }

        [TestMethod]
        public void SubsetAndReorderNoUniformDimensions()
        {
            int[] dimensionSizes = [2, 3, 4];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);


            double[] data = new double[24];
            for (int i = 0; i < 24; i++) data[i] = i;

            Matrix<double> matrix = new(metadata, data);

            MatrixMap matrixMap = new(
            [
                new DimensionMap("var2", ["var2_val1", "var2_val0", "var2_val3"]),
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val2", "var1_val1"])
            ]);

            Matrix<double> newMatrix = matrix.GetTransform(matrixMap);
            double[] expected = [21.0, 17.0, 20.0, 16.0, 23.0, 19.0];

            CollectionAssert.AreEqual(expected, newMatrix.Data);
        }

        [TestMethod]
        public void Identity()
        {
            int[] dimensionSizes = [3, 3, 2];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);


            double[] data = new double[18];
            for (int i = 0; i < 18; i++) data[i] = i;

            Matrix<double> matrix = new(metadata, data);

            Matrix<double> newMatrix = matrix.GetTransform(matrix.Metadata);
            double[] expected = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 17.0];

            CollectionAssert.AreEqual(
                matrix.Metadata.DimensionMaps.Select(dm => dm.Code).ToList(),
                newMatrix.Metadata.DimensionMaps.Select(dm => dm.Code).ToList());
            CollectionAssert.AreEqual(expected, newMatrix.Data);
        }
    }
}
