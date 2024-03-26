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
        public void ReadDoubleDataValues_ValidIntegers_ReturnsCorrectDoubleDataValues()
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
    }
}
