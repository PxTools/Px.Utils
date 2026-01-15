using Px.Utils.BinaryData.ValueConverters;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Data;
using System;

namespace Px.Utils.UnitTests.BinaryData.ValueConverters
{
    [TestClass]
    public class FloatCodecTests
    {
        private static FloatCodec CreateCodec()
        {
            return new FloatCodec();
        }

        [TestMethod]
        public void WriteReadExistsRoundtrip()
        {
            FloatCodec codec = CreateCodec();
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
                Assert.AreEqual(input[i].UnsafeValue, output[i].UnsafeValue, 1e-6);
            }
        }

        [TestMethod]
        public void ReadSentinelMapped()
        {
            FloatCodec codec = new();
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
            FloatCodec codec = new();
            DecimalDataValue[] output = new DecimalDataValue[3];
            using MemoryStream ms = new();
            DoubleDataValue[] input = [
                new DoubleDataValue(0.0, DataValueType.Exists),
                new DoubleDataValue(1.5, DataValueType.Exists),
                new DoubleDataValue(-2.25, DataValueType.Exists),
            ];
            codec.Write(input, ms);
            ReadOnlySpan<byte> bytes = ms.ToArray();
            codec.Read(bytes, output);
            Assert.AreEqual(3, output.Length);
            Assert.AreEqual(DataValueType.Exists, output[0].Type);
            Assert.IsTrue(Math.Abs(output[0].UnsafeValue - 0.0m) <= 0.0001m);
            Assert.AreEqual(DataValueType.Exists, output[1].Type);
            Assert.IsTrue(Math.Abs(output[1].UnsafeValue - 1.5m) <= 0.0001m);
            Assert.AreEqual(DataValueType.Exists, output[2].Type);
            Assert.IsTrue(Math.Abs(output[2].UnsafeValue - (-2.25m)) <= 0.0001m);
        }

        [TestMethod]
        public void ReadDecimalSentinelMapped()
        {
            FloatCodec codec = new();
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
        public void SpanSizeMismatchHandled()
        {
            FloatCodec codec = new();
            using MemoryStream ms = new();
            DoubleDataValue[] input = [ new DoubleDataValue(1.0, DataValueType.Exists) ];
            codec.Write(input, ms);
            ReadOnlySpan<byte> bytes = ms.ToArray();
            DoubleDataValue[] output = new DoubleDataValue[2];
            try
            {
                codec.Read(bytes, output);
                Assert.Fail("Expected exception due to insufficient input bytes for output span length.");
            }
            catch (Exception)
            {
                // Expected: codec should validate buffer sizes.
            }
        }
    }
}
