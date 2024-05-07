using Px.Utils.PxFile.Data;
using PxFileTests.Fixtures;
using PxUtils.Models.Data.DataValue;
using PxUtils.PxFile.Data;
using System.Text;

namespace PxFileTests.DataTests.PxFileStreamDataReaderTests
{
    [TestClass]
    public class MultiPartReadingTests
    {
        [TestMethod]
        public void ReadAddDoubleDataValuesOneRowAtTimeReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[10];

            // Act and Assert
            int[] testDimLengths_2_5_2 = [2, 5, 2];
            
            int[][] testCoordinates0 =
            [
                [0],
                [0, 1, 2, 3, 4],
                [0, 1]
            ];
            DataIndexer indexer0 = new(testCoordinates0, testDimLengths_2_5_2);

            double[] expected0 = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer0);
            CollectionAssert.AreEqual(expected0, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            int[][] testCoordinates1 =
            [
                [1],
                [0, 1, 2, 3, 4],
                [0, 1]
            ];
            DataIndexer indexer1 = new(testCoordinates1, testDimLengths_2_5_2);

            double[] expected1 = [10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 17.0, 18.0, 19.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer1);
            CollectionAssert.AreEqual(expected1, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public void ReadAddDoubleDataValuesOneRowAtTimeWithSmallBufferReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream, null, 8); // Buffer size of 8
            DoubleDataValue[] targetBuffer = new DoubleDataValue[10];

            // Act and Assert
            int[] testDimLengths_2_5_2 = [2, 5, 2];

            int[][] testCoordinates0 =
            [
                [0],
                [0, 1, 2, 3, 4],
                [0, 1]
            ];
            DataIndexer indexer0 = new(testCoordinates0, testDimLengths_2_5_2);

            double[] expected0 = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer0);
            CollectionAssert.AreEqual(expected0, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            int[][] testCoordinates1 =
            [
                [1],
                [0, 1, 2, 3, 4],
                [0, 1]
            ];
            DataIndexer indexer1 = new(testCoordinates1, testDimLengths_2_5_2);

            double[] expected1 = [10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 17.0, 18.0, 19.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer1);
            CollectionAssert.AreEqual(expected1, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public void ReadAddDoubleDataValuesHalfRowAtTimeReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[5];

            // Act and Assert
            int[] testDimLengths_2_2_5 = [2, 2, 5];

            int[][] testCoordinates0 =
            [
                [0],
                [0],
                [0, 1, 2, 3, 4]
            ];
            DataIndexer indexer0 = new(testCoordinates0, testDimLengths_2_2_5);
            double[] expected0 = [0.0, 1.0, 2.0, 3.0, 4.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer0);
            CollectionAssert.AreEqual(expected0, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            int[][] testCoordinates1 =
            [
                [0],
                [1],
                [0, 1, 2, 3, 4]
            ];
            DataIndexer indexer1 = new(testCoordinates1, testDimLengths_2_2_5);
            double[] expected1 = [5.0, 6.0, 7.0, 8.0, 9.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer1);
            CollectionAssert.AreEqual(expected1, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            int[][] testCoordinates2 =
            [
                [1],
                [0],
                [0, 1, 2, 3, 4]
            ];
            DataIndexer indexer2 = new(testCoordinates2, testDimLengths_2_2_5);
            double[] expected2 = [10.0, 11.0, 12.0, 13.0, 14.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer2);
            CollectionAssert.AreEqual(expected2, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            int[][] testCoordinates3 =
            [
                [1],
                [1],
                [0, 1, 2, 3, 4]
            ];
            DataIndexer indexer3 = new(testCoordinates3, testDimLengths_2_2_5);
            double[] expected3 = [15.0, 16.0, 17.0, 18.0, 19.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer3);
            CollectionAssert.AreEqual(expected3, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }


        [TestMethod]
        public void ReadAddDoubleDataValuesOneColAtTimeReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[1];

            // Act and Assert
            int[] testDimLengths_2_2_5 = [2, 2, 5];
            int[][] testCoordinates0 = [[0], [0], [0]];
            DataIndexer indexer0 = new(testCoordinates0, testDimLengths_2_2_5);
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer0);
            Assert.AreEqual(0.0, targetBuffer[0].UnsafeValue);

            int[][] testCoordinates1 = [[0], [0], [1]];
            DataIndexer indexer1 = new(testCoordinates1, testDimLengths_2_2_5);
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer1);
            Assert.AreEqual(1.0, targetBuffer[0].UnsafeValue);

            int[][] testCoordinates2 = [[0], [1], [4]];
            DataIndexer indexer2 = new(testCoordinates2, testDimLengths_2_2_5);
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer2);
            Assert.AreEqual(9.0, targetBuffer[0].UnsafeValue);

            int[][] testCoordinates3 = [[1], [0], [0]];
            DataIndexer indexer3 = new(testCoordinates3, testDimLengths_2_2_5);
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer3);
            Assert.AreEqual(10.0, targetBuffer[0].UnsafeValue);

            int[][] testCoordinates4 = [[1], [0], [1]];
            DataIndexer indexer4 = new(testCoordinates4, testDimLengths_2_2_5);
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer4);
            Assert.AreEqual(11.0, targetBuffer[0].UnsafeValue);

            int[][] testCoordinates5 = [[1], [1], [4]];
            DataIndexer indexer5 = new(testCoordinates5, testDimLengths_2_2_5);
            reader.ReadDoubleDataValues(targetBuffer, 0, indexer5);
            Assert.AreEqual(19.0, targetBuffer[0].UnsafeValue);
        }

        [TestMethod]
        public async Task ReadAddDoubleDataValuesAsnycOneRowAtTimeReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[10];

            // Act and Assert
            int[] testDimLengths_2_5_2 = [2, 5, 2];

            int[][] testCoordinates0 =
            [
                [0],
                [0, 1, 2, 3, 4],
                [0, 1]
            ];
            DataIndexer indexer0 = new(testCoordinates0, testDimLengths_2_5_2);

            double[] expected0 = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0];
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer0);
            CollectionAssert.AreEqual(expected0, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            int[][] testCoordinates1 =
            [
                [1],
                [0, 1, 2, 3, 4],
                [0, 1]
            ];
            DataIndexer indexer1 = new(testCoordinates1, testDimLengths_2_5_2);

            double[] expected1 = [10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 17.0, 18.0, 19.0];
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer1);
            CollectionAssert.AreEqual(expected1, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public async Task ReadAddDoubleDataValuesAsnycHalfRowAtTimeReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[5];

            // Act and Assert
            int[] testDimLengths_2_2_5 = [2, 2, 5];

            int[][] testCoordinates0 =
            [
                [0],
                [0],
                [0, 1, 2, 3, 4]
            ];
            DataIndexer indexer0 = new(testCoordinates0, testDimLengths_2_2_5);
            double[] expected0 = [0.0, 1.0, 2.0, 3.0, 4.0];
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer0);
            CollectionAssert.AreEqual(expected0, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            int[][] testCoordinates1 =
            [
                [0],
                [1],
                [0, 1, 2, 3, 4]
            ];
            DataIndexer indexer1 = new(testCoordinates1, testDimLengths_2_2_5);
            double[] expected1 = [5.0, 6.0, 7.0, 8.0, 9.0];
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer1);
            CollectionAssert.AreEqual(expected1, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            int[][] testCoordinates2 =
            [
                [1],
                [0],
                [0, 1, 2, 3, 4]
            ];
            DataIndexer indexer2 = new(testCoordinates2, testDimLengths_2_2_5);
            double[] expected2 = [10.0, 11.0, 12.0, 13.0, 14.0];
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer2);
            CollectionAssert.AreEqual(expected2, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            int[][] testCoordinates3 =
            [
                [1],
                [1],
                [0, 1, 2, 3, 4]
            ];
            DataIndexer indexer3 = new(testCoordinates3, testDimLengths_2_2_5);
            double[] expected3 = [15.0, 16.0, 17.0, 18.0, 19.0];
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer3);
            CollectionAssert.AreEqual(expected3, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }


        [TestMethod]
        public async Task ReadAddDoubleDataValuesAsnycOneColAtTimeReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[1];

            // Act and Assert
            int[] testDimLengths_2_2_5 = [2, 2, 5];
            int[][] testCoordinates0 = [[0], [0], [0]];
            DataIndexer indexer0 = new(testCoordinates0, testDimLengths_2_2_5);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer0);
            Assert.AreEqual(0.0, targetBuffer[0].UnsafeValue);

            int[][] testCoordinates1 = [[0], [0], [1]];
            DataIndexer indexer1 = new(testCoordinates1, testDimLengths_2_2_5);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer1);
            Assert.AreEqual(1.0, targetBuffer[0].UnsafeValue);

            int[][] testCoordinates2 = [[0], [1], [4]];
            DataIndexer indexer2 = new(testCoordinates2, testDimLengths_2_2_5);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer2);
            Assert.AreEqual(9.0, targetBuffer[0].UnsafeValue);

            int[][] testCoordinates3 = [[1], [0], [0]];
            DataIndexer indexer3 = new(testCoordinates3, testDimLengths_2_2_5);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer3);
            Assert.AreEqual(10.0, targetBuffer[0].UnsafeValue);

            int[][] testCoordinates4 = [[1], [0], [1]];
            DataIndexer indexer4 = new(testCoordinates4, testDimLengths_2_2_5);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer4);
            Assert.AreEqual(11.0, targetBuffer[0].UnsafeValue);

            int[][] testCoordinates5 = [[1], [1], [4]];
            DataIndexer indexer5 = new(testCoordinates5, testDimLengths_2_2_5);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, indexer5);
            Assert.AreEqual(19.0, targetBuffer[0].UnsafeValue);
        }
    }
}
