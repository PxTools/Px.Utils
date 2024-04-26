using Px.Utils.Models;
using Px.Utils.Models.Metadata;
using PxUtils.Models.Metadata;

namespace Px.Utils.UnitTests.ModelTests
{
    [TestClass]
    public class MatrixTransformationsTests
    {
        [TestMethod]
        public void MatrixMetadataTransformationTest_Subset()
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
    }
}
