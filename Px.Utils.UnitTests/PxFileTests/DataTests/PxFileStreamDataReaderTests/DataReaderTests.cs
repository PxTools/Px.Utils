using Px.Utils.Models.Metadata;
using Px.Utils.PxFile.Data;
using Px.Utils.UnitTests;
using PxFileTests.Fixtures;
using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;
using System.Text;

namespace PxFileTests.DataTests.PxFileStreamDataReaderTests
{
    [TestClass]
    public class DataReaderTests
    {
        private readonly double[] missingMarkers = [0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6];

        [TestMethod]
        public void ReadDoubleDataValuesValidIntegersReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[21];
            double canary = 123.456;
            targetBuffer[^1] = new DoubleDataValue(canary, DataValueType.Exists);

            // Act
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([2, 2, 5]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val0", "var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            reader.ReadDoubleDataValues(targetBuffer, 0, testMeta, matrixMap);

            // Assert
            double[] expexted =
            [
                0.00, 1.00, 2.00, 3.00, 4.00, 5.00, 6.00, 7.00, 8.00, 9.00,
                10.00, 11.00, 12.00, 13.00, 14.00, 15.00, 16.00, 17.00, 18.00, 19.00, canary
            ];
            // The canary in the expected checks against overwrites

            CollectionAssert.AreEqual(expexted, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public void ReadDoubleDataValuesValidIntegersWithSmallBufferReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream, null, 16); // Small buffer
            DoubleDataValue[] targetBuffer = new DoubleDataValue[21];
            double canary = 123.456;
            targetBuffer[^1] = new DoubleDataValue(canary, DataValueType.Exists);

            // Act
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([2, 2, 5]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val0", "var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            reader.ReadDoubleDataValues(targetBuffer, 0, testMeta, matrixMap);

            // Assert
            double[] expexted =
            [
                0.00, 1.00, 2.00, 3.00, 4.00, 5.00, 6.00, 7.00, 8.00, 9.00,
                10.00, 11.00, 12.00, 13.00, 14.00, 15.00, 16.00, 17.00, 18.00, 19.00, canary
            ];
            // The canary in the expected checks against overwrites

            CollectionAssert.AreEqual(expexted, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public void ReadDoubleDataValuesValidIntegersAndMissingReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES_WITH_MISSING);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[21];
            DoubleDataValue canary = new(123.456, DataValueType.Exists);
            targetBuffer[^1] = canary;

            // Act
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([2, 2, 5]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val0", "var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            reader.ReadDoubleDataValues(targetBuffer, 0, testMeta, matrixMap);

            // Assert
            DoubleDataValue[] expexted =
            [
                new(0.0, DataValueType.Missing), new(1.00, DataValueType.Exists), new(0.0, DataValueType.Missing), new(3.00, DataValueType.Exists), new(0.0, DataValueType.Missing),
                new(5.00, DataValueType.Exists), new(0.0, DataValueType.Missing), new(7.00, DataValueType.Exists), new(0.0, DataValueType.Missing), new(9.00, DataValueType.Exists),
                new(0.0, DataValueType.Confidential), new(11.00, DataValueType.Exists), new(0.0, DataValueType.Confidential), new(13.00, DataValueType.Exists), new(0.0, DataValueType.Confidential),
                new(15.00, DataValueType.Exists), new(0.0, DataValueType.Confidential), new(17.00, DataValueType.Exists), new(0.0, DataValueType.Confidential), new(19.00, DataValueType.Exists),
                canary
            ];
            // The canary in the expected checks against overwrites

            CollectionAssert.AreEqual(expexted, targetBuffer);
        }

        [TestMethod]
        public void ReadEveryOtherDoubleDataValueFrom1stRowValidIntegersReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[6];
            double canary = 123.456;
            targetBuffer[^1] = new DoubleDataValue(canary, DataValueType.Exists);

            // Act
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([2, 5, 2]);
            MatrixMap targetMap = new(
            [
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2", "var1_val3", "var1_val4"]),
                new DimensionMap("var2", ["var2_val0"])
            ]);
            reader.ReadDoubleDataValues(targetBuffer, 0, targetMap, testMeta);

            // Assert
            double[] expexted = [0.00, 2.00, 4.00, 6.00, 8.00, canary];
            // The canary in the expected checks against overwrites

            CollectionAssert.AreEqual(expexted, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public void ReadEveryOtherDoubleDataValueFrom2ndRowValidIntegersReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[5];

            // Act
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([2, 5, 2]);
            MatrixMap targetMap = new(
            [
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2", "var1_val3", "var1_val4"]),
                new DimensionMap("var2", ["var2_val0"])
            ]);
            reader.ReadDoubleDataValues(targetBuffer, 0, targetMap, testMeta);

            // Assert
            double[] expexted = [10.00, 12.00, 14.00, 16.00, 18.00];
            CollectionAssert.AreEqual(expexted, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public void ReadDoubleDataValuesValidDecimalsReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DECIMALVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([2, 2, 5]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val0", "var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            reader.ReadDoubleDataValues(targetBuffer, 0, testMeta, matrixMap);

            // Assert
            double[] expexted =
            [
                0.00, 0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.07, 0.08, 0.09,
                0.10, 0.11, 0.12, 0.13, 0.14, 0.15, 0.16, 0.17, 0.18, 0.19
            ];

            CollectionAssert.AreEqual(expexted, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public void ReadEveryOtherDoubleDataValueFrom1stRowValidDecimalsReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DECIMALVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[5];

            // Act
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([2, 5, 2]);
            MatrixMap targetMap = new(
            [
                new DimensionMap("var0", ["var0_val0"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2", "var1_val3", "var1_val4"]),
                new DimensionMap("var2", ["var2_val0"])
            ]);
            reader.ReadDoubleDataValues(targetBuffer, 0, targetMap, testMeta);

            // Assert
            double[] expexted = [0.00, 0.02, 0.04, 0.06, 0.08];
            CollectionAssert.AreEqual(expexted, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public void ReadEveryOtherDoubleDataValueFrom2ndRowValidDecimalsReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DECIMALVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[5];

            // Act
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([2, 5, 2]);
            MatrixMap targetMap = new(
            [
                new DimensionMap("var0", ["var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1", "var1_val2", "var1_val3", "var1_val4"]),
                new DimensionMap("var2", ["var2_val0"])
            ]);
            reader.ReadDoubleDataValues(targetBuffer, 0, targetMap, testMeta);

            // Assert
            double[] expexted = [0.10, 0.12, 0.14, 0.16, 0.18];
            CollectionAssert.AreEqual(expexted, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public void EveryValueOnSeparateRowsValidDecimalsReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20ROWS);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([2, 2, 5]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val0", "var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            reader.ReadDoubleDataValues(targetBuffer, 0, testMeta, matrixMap);

            // Assert
            double[] expexted =
            [
                0.00, 0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.07, 0.08, 0.09,
                0.10, 0.11, 0.12, 0.13, 0.14, 0.15, 0.16, 0.17, 0.18, 0.19
            ];

            CollectionAssert.AreEqual( expexted, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public void ReadDecimalDataValuesValidIntegersReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DecimalDataValue[] targetBuffer = new DecimalDataValue[20];

            // Act
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([2, 2, 5]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val0", "var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            reader.ReadDecimalDataValues(targetBuffer, 0, testMeta, matrixMap);

            // Assert
            decimal[] expexted =
            [
                0.00m, 1.00m, 2.00m, 3.00m, 4.00m, 5.00m, 6.00m, 7.00m, 8.00m, 9.00m,
                10.00m, 11.00m, 12.00m, 13.00m, 14.00m, 15.00m, 16.00m, 17.00m, 18.00m, 19.00m
            ];

            CollectionAssert.AreEqual(expexted, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public void ReadDecimalDataValuesValidDecimalsReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DECIMALVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DecimalDataValue[] targetBuffer = new DecimalDataValue[20];

            // Act
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([2, 2, 5]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val0", "var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            reader.ReadDecimalDataValues(targetBuffer, 0, testMeta, matrixMap);

            // Assert
            decimal[] expexted =
            [
                0.00m, 0.01m, 0.02m, 0.03m, 0.04m, 0.05m, 0.06m, 0.07m, 0.08m, 0.09m,
                0.10m, 0.11m, 0.12m, 0.13m, 0.14m, 0.15m, 0.16m, 0.17m, 0.18m, 0.19m
            ];

            CollectionAssert.AreEqual(expexted, targetBuffer.Select(v => v.UnsafeValue).ToArray());
        }

        [TestMethod]
        public void ReadUnsafeDoubleValuesValidIntegersReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            double[] targetBuffer = new double[20];

            // Act
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([2, 2, 5]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val0", "var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            reader.ReadUnsafeDoubles(targetBuffer, 0, testMeta, matrixMap, missingMarkers);

            // Assert
            double[] expexted =
            [
                0.00, 1.00, 2.00, 3.00, 4.00, 5.00, 6.00, 7.00, 8.00, 9.00,
                10.00, 11.00, 12.00, 13.00, 14.00, 15.00, 16.00, 17.00, 18.00, 19.00
            ];

            CollectionAssert.AreEqual(expexted, targetBuffer);
        }

        [TestMethod]
        public void ReadUnsafeDoubleValuesValidDecimalsReturnsCorrectDoubleDataValues()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DECIMALVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            double[] targetBuffer = new double[20];

            // Act
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([2, 2, 5]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val0", "var0_val1"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);
            reader.ReadUnsafeDoubles(targetBuffer, 0, testMeta, matrixMap, missingMarkers);

            // Assert
            double[] expexted =
            [
                0.00, 0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.07, 0.08, 0.09,
                0.10, 0.11, 0.12, 0.13, 0.14, 0.15, 0.16, 0.17, 0.18, 0.19
            ];

            CollectionAssert.AreEqual(expexted, targetBuffer);
        }

        [TestMethod]
        public void ReadDoubleDataValuesInWrongOrderThrowsArgumentException()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([2, 2, 5]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val1", "var0_val0"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"])
            ]);

            // Assert
            Assert.ThrowsException<ArgumentException>(() => reader.ReadDoubleDataValues(targetBuffer, 0, testMeta, matrixMap));
        }

        [TestMethod]
        public void ReadDoubleDataDimensionsInWrongOrderThrowsArgumentException()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(DataReaderFixtures.MINIMAL_UTF8_20DATAVALUES);
            using Stream stream = new MemoryStream(data);
            using PxFileStreamDataReader reader = new(stream);
            DoubleDataValue[] targetBuffer = new DoubleDataValue[20];

            // Act
            MatrixMetadata testMeta = TestModelBuilder.BuildTestMetadata([2, 2, 5]);
            MatrixMap matrixMap = new(
            [
                new DimensionMap("var0", ["var0_val0", "var0_val1"]),
                new DimensionMap("var2", ["var2_val0", "var2_val1", "var2_val2", "var2_val3", "var2_val4"]),
                new DimensionMap("var1", ["var1_val0", "var1_val1"])
            ]);

            // Assert
            Assert.ThrowsException<ArgumentException>(() => reader.ReadDoubleDataValues(targetBuffer, 0, testMeta, matrixMap));
        }
    }
}
