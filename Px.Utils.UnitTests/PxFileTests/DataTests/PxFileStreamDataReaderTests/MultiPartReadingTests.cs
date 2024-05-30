using Px.Utils.Models.Metadata;
using Px.Utils.PxFile.Data;
using Px.Utils.UnitTests;
using PxFileTests.Fixtures;
using Px.Utils.Models.Data.DataValue;
using System.Text;

namespace PxFileTests.DataTests.PxFileStreamDataReaderTests
{
    [TestClass]
    public class MultiPartReadingTests
    {
        [TestMethod]
        public void ReadDoubleDataValuesOneRowAtTimeReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[10];

            // Act and Assert
            int[] testDimLengths_2_5_2 = [2, 5, 2];

            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata(testDimLengths_2_5_2);
            MatrixMap matrixMap0 = new(
            [
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2", "var1_val3", "var1_val4"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1"])
            ]);
            DataIndexer indexer0 = new(testMeta, matrixMap0);

            double[] expected0 = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer0);
            CollectionAssert.AreEqual(expected0, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            MatrixMap matrixMap1 = new(
            [
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2", "var1_val3", "var1_val4"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1"])
            ]);
            DataIndexer indexer1 = new(testMeta, matrixMap1);

            double[] expected1 = [10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 17.0, 18.0, 19.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer1);
            CollectionAssert.AreEqual(expected1, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public void ReadDoubleDataValuesOneRowAtTimeWithSmallBufferReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream, null, 8); // Buffer size of 8
            DoubleDataValue[] targetBuffer = new DoubleDataValue[10];

            // Act and Assert
            int[] testDimLengths_2_5_2 = [2, 5, 2];
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata(testDimLengths_2_5_2);
            MatrixMap matrixMap0 = new(
            [
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2", "var1_val3", "var1_val4"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1"])
            ]);
            DataIndexer indexer0 = new(testMeta, matrixMap0);

            double[] expected0 = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer0);
            CollectionAssert.AreEqual(expected0, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            MatrixMap matrixMap1 = new(
            [
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2", "var1_val3", "var1_val4"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1"])
            ]);
            DataIndexer indexer1 = new(testMeta, matrixMap1);

            double[] expected1 = [10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 17.0, 18.0, 19.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer1);
            CollectionAssert.AreEqual(expected1, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public void ReadDoubleDataValuesHalfRowAtTimeReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[5];

            // Act and Assert
            int[] testDimLengths_2_2_5 = [2, 2, 5];
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata(testDimLengths_2_2_5);

            MatrixMap matrixMap0 = new(
            [
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val0"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            DataIndexer indexer0 = new(testMeta, matrixMap0);
            double[] expected0 = [0.0, 1.0, 2.0, 3.0, 4.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer0);
            CollectionAssert.AreEqual(expected0, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            MatrixMap matrixMap1 = new(
            [
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            DataIndexer indexer1 = new(testMeta, matrixMap1);
            double[] expected1 = [5.0, 6.0, 7.0, 8.0, 9.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer1);
            CollectionAssert.AreEqual(expected1, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            MatrixMap matrixMap2 = new(
            [
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val0"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            DataIndexer indexer2 = new(testMeta, matrixMap2);
            double[] expected2 = [10.0, 11.0, 12.0, 13.0, 14.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer2);
            CollectionAssert.AreEqual(expected2, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            MatrixMap matrixMap3 = new(
            [
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            DataIndexer indexer3 = new(testMeta, matrixMap3);
            double[] expected3 = [15.0, 16.0, 17.0, 18.0, 19.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer3);
            CollectionAssert.AreEqual(expected3, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }


        [TestMethod]
        public void ReadDoubleDataValuesOneColAtTimeReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[1];

            // Act and Assert
            int[] testDimLengths_2_2_5 = [2, 2, 5];
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata(testDimLengths_2_2_5);
            MatrixMap matrixMap0 = new(
            [
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val0"]),
                new DimensionMap("var2", ["var2_val0"])
            ]);
            DataIndexer indexer0 = new(testMeta, matrixMap0);
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer0);
            Assert.AreEqual(0.0, targetBuffer[0].UnsafeValue);

            MatrixMap matrixMap1 = new(
            [
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val0"]),
                new DimensionMap("var2", ["var2_val1"])
            ]);
            DataIndexer indexer1 = new(testMeta, matrixMap1);
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer1);
            Assert.AreEqual(1.0, targetBuffer[0].UnsafeValue);

            MatrixMap matrixMap2 = new(
            [
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val1"]),
                new DimensionMap("var2", ["var2_val4"])
            ]);
            DataIndexer indexer2 = new(testMeta, matrixMap2);
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer2);
            Assert.AreEqual(9.0, targetBuffer[0].UnsafeValue);

            MatrixMap matrixMap3 = new(
            [
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val0"]),
                new DimensionMap("var2", ["var2_val0"])
            ]);
            DataIndexer indexer3 = new(testMeta, matrixMap3);
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer3);
            Assert.AreEqual(10.0, targetBuffer[0].UnsafeValue);

            MatrixMap matrixMap4 = new(
            [
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val0"]),
                new DimensionMap("var2", ["var2_val1"])
            ]);
            DataIndexer indexer4 = new(testMeta, matrixMap4);
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer4);
            Assert.AreEqual(11.0, targetBuffer[0].UnsafeValue);

            MatrixMap matrixMap5 = new(
            [
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val1"]),
                new DimensionMap("var2", ["var2_val4"])
            ]);
            DataIndexer indexer5 = new(testMeta, matrixMap5);
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer5);
            Assert.AreEqual(19.0, targetBuffer[0].UnsafeValue);
        }

        [TestMethod]
        public async Task ReadDoubleDataValuesAsycOneRowAtTimeReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[10];

            // Act and Assert
            int[] testDimLengths_2_5_2 = [2, 5, 2];
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata(testDimLengths_2_5_2);

            MatrixMap matrixMap0 = new(
            [
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2", "var1_val3", "var1_val4"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1"])
            ]);
            DataIndexer indexer0 = new(testMeta, matrixMap0);

            double[] expected0 = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0];
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer0);
            CollectionAssert.AreEqual(expected0, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            MatrixMap matrixMap1 = new(
            [
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2", "var1_val3", "var1_val4"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1"])
            ]);
            DataIndexer indexer1 = new(testMeta, matrixMap1);

            double[] expected1 = [10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 17.0, 18.0, 19.0];
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer1);
            CollectionAssert.AreEqual(expected1, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public async Task ReadDoubleDataValuesAsnycHalfRowAtTimeReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[5];

            // Act and Assert
            int[] testDimLengths_2_2_5 = [2, 2, 5];
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata(testDimLengths_2_2_5);

            MatrixMap matrixMap0 = new(
            [
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val0"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            DataIndexer indexer0 = new(testMeta, matrixMap0);
            double[] expected0 = [0.0, 1.0, 2.0, 3.0, 4.0];
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer0);
            CollectionAssert.AreEqual(expected0, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            MatrixMap matrixMap1 = new(
            [
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            DataIndexer indexer1 = new(testMeta, matrixMap1);

            double[] expected1 = [5.0, 6.0, 7.0, 8.0, 9.0];
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer1);
            CollectionAssert.AreEqual(expected1, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            MatrixMap matrixMap2 = new(
            [
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val0"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            DataIndexer indexer2 = new(testMeta, matrixMap2);

            double[] expected2 = [10.0, 11.0, 12.0, 13.0, 14.0];
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer2);
            CollectionAssert.AreEqual(expected2, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            MatrixMap matrixMap3 = new(
            [
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            DataIndexer indexer3 = new(testMeta, matrixMap3);

            double[] expected3 = [15.0, 16.0, 17.0, 18.0, 19.0];
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer3);
            CollectionAssert.AreEqual(expected3, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public async Task ReadDoubleDataValuesAsnycOneColAtTimeReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[1];

            // Act and Assert
            int[] testDimLengths_2_2_5 = [2, 2, 5];
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata(testDimLengths_2_2_5);

            MatrixMap matrixMap0 = new(
            [
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val0"]),
                new DimensionMap("var2", ["var2_val0"])
            ]);
            DataIndexer indexer0 = new(testMeta, matrixMap0);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer0);
            Assert.AreEqual(0.0, targetBuffer[0].UnsafeValue);

            MatrixMap matrixMap1 = new(
            [
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val0"]),
                new DimensionMap("var2", ["var2_val1"])
            ]);
            DataIndexer indexer1 = new(testMeta, matrixMap1);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer1);
            Assert.AreEqual(1.0, targetBuffer[0].UnsafeValue);

            MatrixMap matrixMap2 = new(
            [
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val1"]),
                new DimensionMap("var2", ["var2_val4"])
            ]);
            DataIndexer indexer2 = new(testMeta, matrixMap2);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer2);
            Assert.AreEqual(9.0, targetBuffer[0].UnsafeValue);

            MatrixMap matrixMap3 = new(
            [
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val0"]),
                new DimensionMap("var2", ["var2_val0"])
            ]);
            DataIndexer indexer3 = new(testMeta, matrixMap3);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer3);
            Assert.AreEqual(10.0, targetBuffer[0].UnsafeValue);

            MatrixMap matrixMap4 = new(
            [
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val0"]),
                new DimensionMap("var2", ["var2_val1"])
            ]);
            DataIndexer indexer4 = new(testMeta, matrixMap4);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer4);
            Assert.AreEqual(11.0, targetBuffer[0].UnsafeValue);

            MatrixMap matrixMap5 = new(
            [
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val1"]),
                new DimensionMap("var2", ["var2_val4"])
            ]);
            DataIndexer indexer5 = new(testMeta, matrixMap5);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer5);
            Assert.AreEqual(19.0, targetBuffer[0].UnsafeValue);
        }
    }
}
