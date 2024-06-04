using Px.Utils.Models.Metadata;
using Px.Utils.PxFile.Data;
using Px.Utils.UnitTests;

namespace PxFileTests.DataTests
{
    [TestClass]
    public class DataIndexerTests
    {
        [TestMethod]
        public void OneDimension1ValueNextTest()
        {
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([1]);
            MatrixMap matrixMap = new([new DimensionMap("var0", ["var0_val0"])]);

            DataIndexer generator = new(testMeta, matrixMap);
            long[] expected = [0];
            int i = 0;

            Assert.AreEqual(1, generator.DataLength);

            do
            {
                Assert.AreEqual(expected[i++], generator.CurrentIndex);
            }
            while (generator.Next());
        }

        [TestMethod]
        public void OneDimension5ValuesNextTest()
        {
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([5]);
            MatrixMap matrixMap = new([new DimensionMap("var0", ["var0_val2", "var0_val3", "var0_val4"])]);
            DataIndexer generator = new(testMeta, matrixMap);
            long[] expected = [2, 3, 4];
            int i = 0;

            Assert.AreEqual(3, generator.DataLength);

            do
            {
                Assert.AreEqual(expected[i++], generator.CurrentIndex);
            }
            while (generator.Next());
        }

        [TestMethod]
        public void OneDimension5ValuesLastTest()
        {
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([5]);
            MatrixMap matrixMap = new([new DimensionMap("var0", ["var0_val4"])]);
            DataIndexer generator = new(testMeta, matrixMap);
            long[] expected = [4];
            int i = 0;

            do
            {
                Assert.AreEqual(expected[i++], generator.CurrentIndex);
            }
            while (generator.Next());
        }

        [TestMethod]
        public void TwoDimensions3val5valNextTest()
        {
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([3, 5]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val1", "var0_val2"]),
                new DimensionMap("var1", ["var1_val2", "var1_val3", "var1_val4"])
            ]);
            DataIndexer generator = new(testMeta, matrixMap);
            long[] expected = [7, 8, 9, 12, 13, 14];
            int i = 0;

            Assert.AreEqual(6, generator.DataLength);

            do
            {
                Assert.AreEqual(expected[i++], generator.CurrentIndex);
            } 
            while (generator.Next());
        }

        [TestMethod]
        public void ThreeDimensions3val2val5valReorderingTest()
        {
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([3, 2, 5]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val2", "var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                new DimensionMap("var2", ["var2_val2", "var2_val3", "var2_val4"])
            ]);
            DataIndexer generator = new(testMeta, matrixMap);

            long[] expected = [22, 23, 24, 27, 28, 29, 12, 13, 14, 17, 18, 19];
            int i = 0;

            Assert.AreEqual(12, generator.DataLength);

            do
            {
                Assert.AreEqual(expected[i++], generator.CurrentIndex);
            }
            while (generator.Next());
        }

        [TestMethod]
        public void ThreeDimensions3val2val5valMultidimensionalReorderingTest()
        {
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([3, 2, 5]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val2", "var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                new DimensionMap("var2", ["var2_val4", "var2_val3", "var2_val2"])
            ]);
            DataIndexer generator = new(testMeta, matrixMap);

            long[] expected = [24, 23, 22, 29, 28, 27, 14, 13, 12, 19, 18, 17];
            int i = 0;

            Assert.AreEqual(12, generator.DataLength);

            do
            {
                Assert.AreEqual(expected[i++], generator.CurrentIndex);
            }
            while (generator.Next());
        }

        [TestMethod]
        public void TwoDimensionsWithSingleValueDimOrderChange()
        {
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([1, 1]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var1", ["var1_val0"]),
                new DimensionMap("var0", ["var0_val0"])
            ]);
            DataIndexer generator = new(testMeta, matrixMap);

            long[] expected = [0];
            int i = 0;

            Assert.AreEqual(1, generator.DataLength);

            do
            {
                Assert.AreEqual(expected[i++], generator.CurrentIndex);
            }
            while (generator.Next());
        }

        [TestMethod]
        public void ThreeDimensions3val2val5valMultidimensionalReorderingTestWithDimOrderChange()
        {
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([3, 2, 5]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                new DimensionMap("var0", ["var0_val2", "var0_val1"]),
                new DimensionMap("var2", ["var2_val4", "var2_val3", "var2_val2"])
            ]);
            DataIndexer generator = new(testMeta, matrixMap);

            long[] expected = [24, 23, 22, 14, 13, 12, 29, 28, 27, 19, 18, 17];
            int i = 0;

            do
            {
                Assert.AreEqual(expected[i++], generator.CurrentIndex);
            }
            while (generator.Next());
        }
    }
}
