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
        public void ReadAddDoubleDataValues_OneRowAtTime_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[10];

            // Act and Assert
            int[] rowRange1 = [0];
            int[] rowRange2 = [1];

            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);


            double[] expected1 = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, rowRange1, colRange);
            CollectionAssert.AreEqual(expected1, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            double[] expected2 = [10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 17.0, 18.0, 19.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, rowRange2, colRange);
            CollectionAssert.AreEqual(expected2, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public void ReadAddDoubleDataValues_HalfRowAtTime_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[5];

            // Act and Assert
            int[] colRange1 = DataTestHelpers.BuildRanges(0, 5, 1);
            int[] colRange2 = DataTestHelpers.BuildRanges(5, 10, 1);

            double[] expected1 = [0.0, 1.0, 2.0, 3.0, 4.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, [0], colRange1);
            CollectionAssert.AreEqual(expected1, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            double[] expected2 = [5.0, 6.0, 7.0, 8.0, 9.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, [0], colRange2);
            CollectionAssert.AreEqual(expected2, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            double[] expected3 = [10.0, 11.0, 12.0, 13.0, 14.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, [1], colRange1);
            CollectionAssert.AreEqual(expected3, targetBuffer.Select(v => v.UnsafeValue).ToArray());

            double[] expected4 = [15.0, 16.0, 17.0, 18.0, 19.0];
            reader.ReadDoubleDataValues(targetBuffer, 0, [1], colRange2);
            CollectionAssert.AreEqual(expected4, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }


        [TestMethod]
        public void ReadAddDoubleDataValues_OneColAtTime_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[1];

            // Act and Assert
            reader.ReadDoubleDataValues(targetBuffer, 0, [0], [0]);
            Assert.AreEqual(0.0, targetBuffer[0].UnsafeValue);

            reader.ReadDoubleDataValues(targetBuffer, 0, [0], [1]);
            Assert.AreEqual(1.0, targetBuffer[0].UnsafeValue);

            reader.ReadDoubleDataValues(targetBuffer, 0, [0], [9]);
            Assert.AreEqual(9.0, targetBuffer[0].UnsafeValue);

            reader.ReadDoubleDataValues(targetBuffer, 0, [1], [0]);
            Assert.AreEqual(10.0, targetBuffer[0].UnsafeValue);

            reader.ReadDoubleDataValues(targetBuffer, 0, [1], [1]);
            Assert.AreEqual(11.0, targetBuffer[0].UnsafeValue);

            reader.ReadDoubleDataValues(targetBuffer, 0, [1], [19]);
            Assert.AreEqual(19.0, targetBuffer[0].UnsafeValue);
        }
    }
}
