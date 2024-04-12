using PxFileTests.Fixtures;
using PxUtils.Models.Data.DataValue;
using PxUtils.PxFile.Data;
using System.Text;

namespace PxFileTests.DataTests.PxFileStreamDataReaderTests
{
    [TestClass]
    public class AsyncDataReaderTests
    {
        private readonly double[] missingMarkers = [0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6];

        [TestMethod]
        public async Task ReadAddDoubleDataValuesAsync_ValidIntegers_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(0.0, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(9.0, targetBuffer[9].UnsafeValue);
            Assert.AreEqual(10.0, targetBuffer[10].UnsafeValue);
            Assert.AreEqual(19.0, targetBuffer[19].UnsafeValue);
        }

        [TestMethod]
        public async Task ReadAddDoubleDataValuesAsyncWithCancellationToken_ValidIntegers_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            using CancellationTokenSource cts = new();
            CancellationToken cToken = cts.Token;

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, rowRange, colRange, cToken);

            // Assert
            Assert.AreEqual(0.0, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(9.0, targetBuffer[9].UnsafeValue);
            Assert.AreEqual(10.0, targetBuffer[10].UnsafeValue);
            Assert.AreEqual(19.0, targetBuffer[19].UnsafeValue);
        }

        [TestMethod]
        public async Task CancelReadAddDoubleDataValuesAsync_ValidIntegers_IsCancelled()
        {
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);

            using CancellationTokenSource cts = new();
            cts.Cancel();
            CancellationToken cToken = cts.Token;

            async Task call() => await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, rowRange, colRange, cToken);
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(call);
        }

        [TestMethod]
        public async Task ReadDecimalDataValuesAsyncWithCancellationToken_ValidIntegers_ReturnsCorrectDecimalValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DecimalDataValue[] targetBuffer = new DecimalDataValue[20];

            using CancellationTokenSource cts = new();
            CancellationToken cToken = cts.Token;

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);
            await reader.ReadDecimalDataValuesAsync(targetBuffer, 0, rowRange, colRange, cToken);

            // Assert
            Assert.AreEqual(0.0m, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(9.0m, targetBuffer[9].UnsafeValue);
            Assert.AreEqual(10.0m, targetBuffer[10].UnsafeValue);
            Assert.AreEqual(19.0m, targetBuffer[19].UnsafeValue);
        }

        [TestMethod]
        public async Task CancelReadAddDecimalDataValuesAsync_ValidIntegers_IsCancelled()
        {
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DecimalDataValue[] targetBuffer = new DecimalDataValue[20];

            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);

            using CancellationTokenSource cts = new();
            cts.Cancel();
            CancellationToken cToken = cts.Token;

            async Task call() => await reader.ReadDecimalDataValuesAsync(targetBuffer, 0, rowRange, colRange, cToken);
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(call);
        }

        [TestMethod]
        public async Task ReadUnsafeDoubleValuesAsyncWithCancellationToken_ValidIntegers_ReturnsCorrectUnsafeDoubleValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            double[] targetBuffer = new double[20];

            using CancellationTokenSource cts = new();
            CancellationToken cToken = cts.Token;

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);
            await reader.ReadUnsafeDoublesAsync(targetBuffer, 0, rowRange, colRange, missingMarkers, cToken);

            // Assert
            Assert.AreEqual(0.0, targetBuffer[0]);
            Assert.AreEqual(9.0, targetBuffer[9]);
            Assert.AreEqual(10.0, targetBuffer[10]);
            Assert.AreEqual(19.0, targetBuffer[19]);
        }

        [TestMethod]
        public async Task CancelReadUnsafeDoubleValuesAsync_ValidIntegers_IsCancelled()
        {
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            double[] targetBuffer = new double[20];

            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);

            using CancellationTokenSource cts = new();
            cts.Cancel();
            CancellationToken cToken = cts.Token;

            async Task call() => await reader.ReadUnsafeDoublesAsync(targetBuffer, 0, rowRange, colRange, missingMarkers, cToken);
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(call);
        }

        [TestMethod]
        public async Task ReadEveryOtherDoubleDataValueFrom1stRowAsync_ValidIntegers_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 2);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 2);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(0.0, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(2.0, targetBuffer[1].UnsafeValue);
            Assert.AreEqual(4.0, targetBuffer[2].UnsafeValue);
            Assert.AreEqual(6.0, targetBuffer[3].UnsafeValue);
            Assert.AreEqual(8.0, targetBuffer[4].UnsafeValue);
        }

        [TestMethod]
        public async Task ReadEveryOtherDoubleDataValueFrom2ndRowAsync_ValidIntegers_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(1, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 2);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(10.0, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(12.0, targetBuffer[1].UnsafeValue);
            Assert.AreEqual(14.0, targetBuffer[2].UnsafeValue);
            Assert.AreEqual(16.0, targetBuffer[3].UnsafeValue);
            Assert.AreEqual(18.0, targetBuffer[4].UnsafeValue);
        }

        [TestMethod]
        public async Task ReadDoubleDataValuesAsync_ValidDecimals_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DECIMALVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(0.0, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(0.09, targetBuffer[9].UnsafeValue);
            Assert.AreEqual(0.10, targetBuffer[10].UnsafeValue);
            Assert.AreEqual(0.19, targetBuffer[19].UnsafeValue);
        }

        [TestMethod]
        public async Task ReadEveryOtherDoubleDataValueFrom1stRowAsync_ValidDecimals_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DECIMALVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 2);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 2);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(0.00, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(0.02, targetBuffer[1].UnsafeValue);
            Assert.AreEqual(0.04, targetBuffer[2].UnsafeValue);
            Assert.AreEqual(0.06, targetBuffer[3].UnsafeValue);
            Assert.AreEqual(0.08, targetBuffer[4].UnsafeValue);
        }

        [TestMethod]
        public async Task ReadEveryOtherDoubleDataValueFrom2ndRowAsync_ValidDecimals_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DECIMALVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(1, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 2);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(0.10, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(0.12, targetBuffer[1].UnsafeValue);
            Assert.AreEqual(0.14, targetBuffer[2].UnsafeValue);
            Assert.AreEqual(0.16, targetBuffer[3].UnsafeValue);
            Assert.AreEqual(0.18, targetBuffer[4].UnsafeValue);
        }

        [TestMethod]
        public async Task EveryValueOnSeparateRowsAsync_ValidDecimals_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20ROWS);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 20, 1);
            await reader.ReadDoubleDataValuesAsync(targetBuffer, 0, rowRange, [0]);

            // Assert
            Assert.AreEqual(0.00, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(0.02, targetBuffer[2].UnsafeValue);
            Assert.AreEqual(0.04, targetBuffer[4].UnsafeValue);
            Assert.AreEqual(0.1, targetBuffer[10].UnsafeValue);
            Assert.AreEqual(0.19, targetBuffer[19].UnsafeValue);
        }

        [TestMethod]
        public async Task ReadAddDecimalDataValuesAsync_ValidIntegers_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DecimalDataValue[] targetBuffer = new DecimalDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);
            await reader.ReadDecimalDataValuesAsync(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(0.0m, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(9.0m, targetBuffer[9].UnsafeValue);
            Assert.AreEqual(10.0m, targetBuffer[10].UnsafeValue);
            Assert.AreEqual(19.0m, targetBuffer[19].UnsafeValue);
        }

        [TestMethod]
        public async Task ReadDecimalDataValuesAsync_ValidDecimals_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DECIMALVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DecimalDataValue[] targetBuffer = new DecimalDataValue[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);
            await reader.ReadDecimalDataValuesAsync(targetBuffer, 0, rowRange, colRange);

            // Assert
            Assert.AreEqual(0.0m, targetBuffer[0].UnsafeValue);
            Assert.AreEqual(0.09m, targetBuffer[9].UnsafeValue);
            Assert.AreEqual(0.10m, targetBuffer[10].UnsafeValue);
            Assert.AreEqual(0.19m, targetBuffer[19].UnsafeValue);
        }

        [TestMethod]
        public async Task ReadUnsafeDoubleValuesAsync_ValidIntegers_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            double[] targetBuffer = new double[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);
            await reader.ReadUnsafeDoublesAsync(targetBuffer, 0, rowRange, colRange, missingMarkers);

            // Assert
            Assert.AreEqual(0.0, targetBuffer[0]);
            Assert.AreEqual(9.0, targetBuffer[9]);
            Assert.AreEqual(10.0, targetBuffer[10]);
            Assert.AreEqual(19.0, targetBuffer[19]);
        }

        [TestMethod]
        public async Task ReadUnsafeDoubleValuesAsync_ValidDecimals_ReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DECIMALVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            double[] targetBuffer = new double[20];

            // Act
            int[] rowRange = DataTestHelpers.BuildRanges(0, 2, 1);
            int[] colRange = DataTestHelpers.BuildRanges(0, 10, 1);
            await reader.ReadUnsafeDoublesAsync(targetBuffer, 0, rowRange, colRange, missingMarkers);

            // Assert
            Assert.AreEqual(0.0, targetBuffer[0]);
            Assert.AreEqual(0.09, targetBuffer[9]);
            Assert.AreEqual(0.10, targetBuffer[10]);
            Assert.AreEqual(0.19, targetBuffer[19]);
        }
    }
}
