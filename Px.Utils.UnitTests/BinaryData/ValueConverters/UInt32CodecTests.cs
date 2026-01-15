using Px.Utils.BinaryData.ValueConverters;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Data;

namespace Px.Utils.UnitTests.BinaryData.ValueConverters
{
    [TestClass]
    public class UInt32CodecTests
    {
        [TestMethod]
        public void WriteReadExistsRoundtrip()
        {
            UInt32Codec codec = new();
            DoubleDataValue[] input = [
                new DoubleDataValue(0, DataValueType.Exists),
                new DoubleDataValue(123456789, DataValueType.Exists),
                new DoubleDataValue(uint.MaxValue - 8, DataValueType.Exists) // Adjusted to avoid sentinel values
            ];
            using MemoryStream ms = new();
            codec.Write(input, ms);
            ReadOnlySpan<byte> bytes = ms.ToArray();
            DoubleDataValue[] output = new DoubleDataValue[input.Length];
            codec.Read(bytes, output);
            Assert.AreEqual(input.Length, output.Length);
            for (int i = 0; i < input.Length; i++)
            {
                Assert.AreEqual(DataValueType.Exists, output[i].Type);
                Assert.AreEqual(input[i].UnsafeValue, output[i].UnsafeValue);
            }
        }

        [TestMethod]
        public void ReadSentinelMapped()
        {
            UInt32Codec codec = new();
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
            UInt32Codec codec = new();
            DecimalDataValue[] output = new DecimalDataValue[3];
            using MemoryStream ms = new();
            DoubleDataValue[] input = [
                new DoubleDataValue(0, DataValueType.Exists),
                new DoubleDataValue(123456789, DataValueType.Exists),
                new DoubleDataValue(uint.MaxValue - 9, DataValueType.Exists)
            ];
            codec.Write(input, ms);
            ReadOnlySpan<byte> bytes = ms.ToArray();
            codec.Read(bytes, output);
            Assert.AreEqual(3, output.Length);
            Assert.AreEqual(DataValueType.Exists, output[0].Type);
            Assert.AreEqual(0m, output[0].UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, output[1].Type);
            Assert.AreEqual(123456789m, output[1].UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, output[2].Type);
            Assert.AreEqual((decimal)(uint.MaxValue - 9), output[2].UnsafeValue);
        }

        [TestMethod]
        public void BoundaryValuesAndSentinels()
        {
            UInt32Codec codec = new();
            using MemoryStream ms = new();
            DoubleDataValue[] input = [
                new DoubleDataValue(UInt32Codec.SentinelStart - 1, DataValueType.Exists), // just below sentinel start
                new DoubleDataValue(UInt32Codec.SentinelStart, DataValueType.Exists), // sentinel value
                new DoubleDataValue(UInt32Codec.SentinelStart + 1, DataValueType.Exists) // just above sentinel start
            ];
            codec.Write(input, ms);
            ReadOnlySpan<byte> bytes = ms.ToArray();
            DoubleDataValue[] output = new DoubleDataValue[input.Length];
            codec.Read(bytes, output);
            Assert.AreEqual(DataValueType.Exists, output[0].Type);
            Assert.AreEqual(UInt32Codec.SentinelStart - 1, output[0].UnsafeValue);
            Assert.AreNotEqual(DataValueType.Exists, output[1].Type); // mapped to a sentinel type
            Assert.AreEqual(DataValueType.CanNotRepresent, output[2].Type);
        }

        [TestMethod]
        public void ReadDecimalSentinelMapped()
        {
            UInt32Codec codec = new();
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
    }
}
