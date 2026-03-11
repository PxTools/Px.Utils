using Px.Utils.BinaryData.ValueConverters;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Data;

namespace Px.Utils.UnitTests.BinaryData.ValueConverters
{
    [TestClass]
    public class Int16CodecTests
    {
        [TestMethod]
        public void WriteReadExistsRoundtrip()
        {
            Int16Codec codec = new();
            DoubleDataValue[] input = [
                new DoubleDataValue(short.MinValue, DataValueType.Exists),
                new DoubleDataValue(0, DataValueType.Exists),
                new DoubleDataValue(short.MaxValue - 8, DataValueType.Exists) // Adjusted to avoid sentinel overlap
            ];
            using MemoryStream ms = new();
            codec.Write(input, ms);
            ReadOnlySpan<byte> bytes = ms.ToArray();
            DoubleDataValue[] output = new DoubleDataValue[input.Length];
            codec.Read(bytes, output);
            Assert.HasCount(input.Length, output);
            for (int i = 0; i < input.Length; i++)
            {
                Assert.AreEqual(DataValueType.Exists, output[i].Type);
                Assert.AreEqual(input[i].UnsafeValue, output[i].UnsafeValue);
            }
        }

        [TestMethod]
        public void ReadSentinelMapped()
        {
            Int16Codec codec = new();
            // Create bytes by writing sentinel-typed values using the codec
            DoubleDataValue[] sentinels =
            [
                new DoubleDataValue(0, DataValueType.Missing),
                new DoubleDataValue(0, DataValueType.CanNotRepresent),
                new DoubleDataValue(0, DataValueType.Confidential),
                new DoubleDataValue(0, DataValueType.NotAcquired),
                new DoubleDataValue(0, DataValueType.NotAsked),
                new DoubleDataValue(0, DataValueType.Empty),
                new DoubleDataValue(0, DataValueType.Nill)
            ];
            using MemoryStream ms = new();
            codec.Write(sentinels, ms);
            ReadOnlySpan<byte> bytes = ms.ToArray();

            DoubleDataValue[] output = new DoubleDataValue[7];
            codec.Read(bytes, output);
            Assert.AreEqual(DataValueType.Missing, output[0].Type);
            Assert.AreEqual(DataValueType.CanNotRepresent, output[1].Type);
            Assert.AreEqual(DataValueType.Confidential, output[2].Type);
            Assert.AreEqual(DataValueType.NotAcquired, output[3].Type);
            Assert.AreEqual(DataValueType.NotAsked, output[4].Type);
            Assert.AreEqual(DataValueType.Empty, output[5].Type);
            Assert.AreEqual(DataValueType.Nill, output[6].Type);
        }

        [TestMethod]
        public void ReadDecimalExistsRoundtrip()
        {
            Int16Codec codec = new();
            DecimalDataValue[] output = new DecimalDataValue[3];
            using MemoryStream ms = new();
            DoubleDataValue[] input = [
                new DoubleDataValue(-10, DataValueType.Exists),
                new DoubleDataValue(0, DataValueType.Exists),
                new DoubleDataValue(100, DataValueType.Exists)
            ];
            codec.Write(input, ms);
            ReadOnlySpan<byte> bytes = ms.ToArray();
            codec.Read(bytes, output);
            Assert.HasCount(3, output);
            Assert.AreEqual(DataValueType.Exists, output[0].Type);
            Assert.AreEqual(-10m, output[0].UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, output[1].Type);
            Assert.AreEqual(0m, output[1].UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, output[2].Type);
            Assert.AreEqual(100m, output[2].UnsafeValue);
        }

        [TestMethod]
        public void ReadDecimalSentinelMapped()
        {
            Int16Codec codec = new();
            DoubleDataValue[] sentinels =
            [
                new DoubleDataValue(0, DataValueType.Missing),
                new DoubleDataValue(0, DataValueType.CanNotRepresent),
                new DoubleDataValue(0, DataValueType.Confidential),
                new DoubleDataValue(0, DataValueType.NotAcquired),
                new DoubleDataValue(0, DataValueType.NotAsked),
                new DoubleDataValue(0, DataValueType.Empty),
                new DoubleDataValue(0, DataValueType.Nill)
            ];
            using MemoryStream ms = new();
            codec.Write(sentinels, ms);
            ReadOnlySpan<byte> bytes = ms.ToArray();

            DecimalDataValue[] output = new DecimalDataValue[7];
            codec.Read(bytes, output);
            Assert.AreEqual(DataValueType.Missing, output[0].Type);
            Assert.AreEqual(DataValueType.CanNotRepresent, output[1].Type);
            Assert.AreEqual(DataValueType.Confidential, output[2].Type);
            Assert.AreEqual(DataValueType.NotAcquired, output[3].Type);
            Assert.AreEqual(DataValueType.NotAsked, output[4].Type);
            Assert.AreEqual(DataValueType.Empty, output[5].Type);
            Assert.AreEqual(DataValueType.Nill, output[6].Type);
        }

        [TestMethod]
        public void BoundaryValuesAndSentinels()
        {
            Int16Codec codec = new();
            using MemoryStream ms = new();
            DoubleDataValue[] input = [
                new DoubleDataValue(Int16Codec.SentinelStart - 1, DataValueType.Exists), // just below sentinel start
                new DoubleDataValue(Int16Codec.SentinelStart, DataValueType.Exists), // sentinel value
                new DoubleDataValue(Int16Codec.SentinelStart + 1, DataValueType.Exists) // just above sentinel start
            ];
            codec.Write(input, ms);
            ReadOnlySpan<byte> bytes = ms.ToArray();
            DoubleDataValue[] output = new DoubleDataValue[input.Length];
            codec.Read(bytes, output);
            Assert.AreEqual(DataValueType.Exists, output[0].Type);
            Assert.AreEqual(Int16Codec.SentinelStart - 1, output[0].UnsafeValue);
            Assert.AreNotEqual(DataValueType.Exists, output[1].Type); // mapped to a sentinel type
            Assert.AreEqual(DataValueType.CanNotRepresent, output[2].Type);
        }
    }
}
