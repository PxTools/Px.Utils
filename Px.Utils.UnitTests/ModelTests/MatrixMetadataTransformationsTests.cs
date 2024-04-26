using Px.Utils.Models.Metadata;
using PxUtils.Models.Metadata;

namespace Px.Utils.UnitTests.ModelTests
{
    [TestClass]
    public class MatrixMetadataTransformationsTests
    {
        [TestMethod]
        public void MatrixMetadataTransformationTest_Subset()
        {
            int[] dimensionSizes = [3, 3, 2];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);
            MatrixMap matrixMap = new(
                [
                    new DimensionMap("var0", ["var0_val1", "var0_val2"]),
                    new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                    new DimensionMap("var2", ["var2_val0", "var2_val1"])
                ]);
            MatrixMetadata newMeta = metadata.GetTransform(matrixMap);

            Assert.AreEqual(3, newMeta.Dimensions.Count);

            List<string> var0ExpectedVars = ["var0_val1", "var0_val2"];
            List<string> var1ExpectedVars = ["var1_val0", "var1_val1"];
            List<string> var2ExpectedVars = ["var2_val0", "var2_val1"];

            CollectionAssert.AreEqual(var0ExpectedVars, newMeta.Dimensions[0].Values.Select(v => v.Code).ToList());
            CollectionAssert.AreEqual(var1ExpectedVars, newMeta.Dimensions[1].Values.Select(v => v.Code).ToList());
            CollectionAssert.AreEqual(var2ExpectedVars, newMeta.Dimensions[2].Values.Select(v => v.Code).ToList());
        }

        [TestMethod]
        public void MatrixMetadataTransformationTest_Reorder()
        {
            int[] dimensionSizes = [3, 3, 2];
            MatrixMetadata metadata = TestModelBuilder.BuildTestMetadata(dimensionSizes);
            MatrixMap matrixMap = new(
                [
                    new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                    new DimensionMap("var0", ["var0_val0", "var0_val1"]),
                    new DimensionMap("var2", ["var2_val1", "var2_val0"])
                ]);
            MatrixMetadata newMeta = metadata.GetTransform(matrixMap);

            Assert.AreEqual(3, newMeta.Dimensions.Count);

            List<string> var0ExpectedVars = ["var1_val0", "var1_val1"];
            List<string> var1ExpectedVars = ["var0_val0", "var0_val1"];
            List<string> var2ExpectedVars = ["var2_val1", "var2_val0"];

            CollectionAssert.AreEqual(var0ExpectedVars, newMeta.Dimensions[0].Values.Select(v => v.Code).ToList());
            CollectionAssert.AreEqual(var1ExpectedVars, newMeta.Dimensions[1].Values.Select(v => v.Code).ToList());
            CollectionAssert.AreEqual(var2ExpectedVars, newMeta.Dimensions[2].Values.Select(v => v.Code).ToList());
        }
    }
}
