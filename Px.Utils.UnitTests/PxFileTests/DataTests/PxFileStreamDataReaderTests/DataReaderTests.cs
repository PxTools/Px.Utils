using PxFileTests.Fixtures;
using PxUtils.Models.Data.DataValue;
using PxUtils.PxFile.Data;
using System.Text;

namespace PxFileTests.DataTests.PxFileStreamDataReaderTests
{
    [TestClass]
    public class DataReaderTests
    {
        [TestMethod]
        public void ReadAddDoubleDataValues_ValidIntegers_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);
            reader.ReadDoubleDataValues(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(0.0, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(9.0, targetBuffer[9].UnsafeValue);
            Assert.AreEqual(10.0, targetBuffer[10].UnsafeValue);
            Assert.AreEqual(19.0, targetBuffer[19].UnsafeValue);
        }

        [TestMethod]
        public void ReadEveryOtherDoubleDataValue_ValidIntegers_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 2);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 2);
            reader.ReadDoubleDataValues(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(0.0, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(2.0, targetBuffer[1].UnsafeValue);
            Assert.AreEqual(4.0, targetBuffer[2].UnsafeValue);
            Assert.AreEqual(6.0, targetBuffer[3].UnsafeValue);
            Assert.AreEqual(8.0, targetBuffer[4].UnsafeValue);
        }

        [TestMethod]
        public void ReadEveryOtherDoubleDataValueFrom2ndRow_ValidIntegers_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(1, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 2);
            reader.ReadDoubleDataValues(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(10.0, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(12.0, targetBuffer[1].UnsafeValue);
            Assert.AreEqual(14.0, targetBuffer[2].UnsafeValue);
            Assert.AreEqual(16.0, targetBuffer[3].UnsafeValue);
            Assert.AreEqual(18.0, targetBuffer[4].UnsafeValue);
        }
    }
}
