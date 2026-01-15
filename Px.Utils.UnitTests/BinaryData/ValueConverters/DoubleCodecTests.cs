using Px.Utils.BinaryData.ValueConverters;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Data;
using System.Buffers.Binary;

namespace Px.Utils.UnitTests.BinaryData.ValueConverters
{
    [TestClass]
    public class DoubleCodecTests
    {
        private static DoubleCodec CreateCodec()
        {
            return new DoubleCodec();
        }

        [TestMethod]
        public void WriteReadExistsRoundtrip()
        {
            DoubleCodec codec = CreateCodec();
            DoubleDataValue[] input = [ new DoubleDataValue(0.0, DataValueType.Exists), new DoubleDataValue(1.5, DataValueType.Exists), new DoubleDataValue(-2.25, DataValueType.Exists) ];
            using MemoryStream ms = new();
            codec.Write(input, ms);
            ReadOnlySpan<byte> bytes = ms.ToArray();
            DoubleDataValue[] output = new DoubleDataValue[input.Length];
            codec.Read(bytes, output);
            Assert.AreEqual(input.Length, output.Length);
            for (int i = 0; i < input.Length; i++)
            {
                Assert.AreEqual(DataValueType.Exists, output[i].Type);
                Assert.AreEqual(input[i].UnsafeValue, output[i].UnsafeValue, 1e-9);
            }
        }

        [TestMethod]
        public void ReadSentinelMapped()
        {
            DoubleCodec codec = new();
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
            DoubleCodec codec = CreateCodec();
            DoubleDataValue[] input = [ new DoubleDataValue(0.0, DataValueType.Exists), new DoubleDataValue(1.5, DataValueType.Exists), new DoubleDataValue(-2.25, DataValueType.Exists) ];
            using MemoryStream ms = new();
            codec.Write(input, ms);
            ReadOnlySpan<byte> bytes = ms.ToArray();
            DecimalDataValue[] output = new DecimalDataValue[input.Length];
            codec.Read(bytes, output);
            Assert.AreEqual(input.Length, output.Length);
            for (int i = 0; i < input.Length; i++)
            {
                Assert.AreEqual(DataValueType.Exists, output[i].Type);
                Assert.AreEqual((decimal)input[i].UnsafeValue, output[i].UnsafeValue);
            }
        }

        [TestMethod]
        public void ReadDecimalSentinelMapped()
        {
            DoubleCodec codec = new();
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
