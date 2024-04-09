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
        public void ReadEveryOtherDoubleDataValueFrom1stRow_ValidIntegers_ReturnsCorrectDoubleDataValues()
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

        [TestMethod]
        public void ReadDoubleDataValues_ValidDecimals_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DECIMALVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);
            reader.ReadDoubleDataValues(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(0.0, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(0.09, targetBuffer[9].UnsafeValue);
            Assert.AreEqual(0.10, targetBuffer[10].UnsafeValue);
            Assert.AreEqual(0.19, targetBuffer[19].UnsafeValue);
        }

        [TestMethod]
        public void ReadEveryOtherDoubleDataValueFrom1stRow_ValidDecimals_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DECIMALVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 2);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 2);
            reader.ReadDoubleDataValues(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(0.00, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(0.02, targetBuffer[1].UnsafeValue);
            Assert.AreEqual(0.04, targetBuffer[2].UnsafeValue);
            Assert.AreEqual(0.06, targetBuffer[3].UnsafeValue);
            Assert.AreEqual(0.08, targetBuffer[4].UnsafeValue);
        }

        [TestMethod]
        public void ReadEveryOtherDoubleDataValueFrom2ndRow_ValidDecimals_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DECIMALVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(1, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 2);
            reader.ReadDoubleDataValues(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(0.10, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(0.12, targetBuffer[1].UnsafeValue);
            Assert.AreEqual(0.14, targetBuffer[2].UnsafeValue);
            Assert.AreEqual(0.16, targetBuffer[3].UnsafeValue);
            Assert.AreEqual(0.18, targetBuffer[4].UnsafeValue);
        }

        [TestMethod]
        public void EveryValueOnSeparateRows_ValidDecimals_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20ROWS);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 20, 1);
            reader.ReadDoubleDataValues(targetBuffer, 0, rowRange, [0]);

            // Assert
            Assert.AreEqual(0.00, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(0.02, targetBuffer[2].UnsafeValue);
            Assert.AreEqual(0.04, targetBuffer[4].UnsafeValue);
            Assert.AreEqual(0.1, targetBuffer[10].UnsafeValue);
            Assert.AreEqual(0.19, targetBuffer[19].UnsafeValue);
        }

        [TestMethod]
        public void ReadAddDecimalDataValues_ValidIntegers_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DecimalDataValue[] targetBuffer = new DecimalDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);
            reader.ReadDecimalDataValues(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(0.0m, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(9.0m, targetBuffer[9].UnsafeValue);
            Assert.AreEqual(10.0m, targetBuffer[10].UnsafeValue);
            Assert.AreEqual(19.0m, targetBuffer[19].UnsafeValue);
        }

        [TestMethod]
        public void ReadDecimalDataValues_ValidDecimals_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DECIMALVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DecimalDataValue[] targetBuffer = new DecimalDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);
            reader.ReadDecimalDataValues(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(0.0m, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(0.09m, targetBuffer[9].UnsafeValue);
            Assert.AreEqual(0.10m, targetBuffer[10].UnsafeValue);
            Assert.AreEqual(0.19m, targetBuffer[19].UnsafeValue);
        }

        [TestMethod]
        public void ReadUnsafeDoubleValues_ValidIntegers_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            double[] targetBuffer = new double[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);
            reader.ReadUnsafeDoubles(targetBuffer, 0, rowRange, colRange, [0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6]);

            // Assert
            Assert.AreEqual(0.0, targetBuffer[0]);
            Assert.AreEqual(9.0, targetBuffer[9]);
            Assert.AreEqual(10.0, targetBuffer[10]); 
            Assert.AreEqual(19.0, targetBuffer[19]);
        }

        [TestMethod]
        public void ReadUnsafeDoubleValues_ValidDecimals_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DECIMALVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            double[] targetBuffer = new double[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);
            reader.ReadUnsafeDoubles(targetBuffer, 0, rowRange, colRange, [0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6]);

            // Assert
            Assert.AreEqual(0.0, targetBuffer[0]);
            Assert.AreEqual(0.09, targetBuffer[9]);
            Assert.AreEqual(0.10, targetBuffer[10]);
            Assert.AreEqual(0.19, targetBuffer[19]);
        }
    }
}
