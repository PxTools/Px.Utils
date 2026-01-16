using Px.Utils.BinaryData.ValueConverters;
using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;

namespace Px.Utils.UnitTests.BinaryData.ValueConverters
{
    [TestClass]
    public class BinaryValueCodecSelectorTests
    {
        [TestMethod]
        public void CreateBestCodecNoInputReturnsUInt16()
        {
            BinaryValueCodecSelector selector = new();
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<UInt16Codec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecIntegersNonNegativeWithinUInt16ReturnsUInt16()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(0, DataValueType.Exists),
                new DoubleDataValue(123, DataValueType.Exists),
                new DoubleDataValue(10000, DataValueType.Exists),
                new DoubleDataValue(65527, DataValueType.Exists) // strictly below UInt16 Missing sentinel (65529)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<UInt16Codec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecNonNegativeAtUInt16MissingReturnsUInt24()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(UInt16Codec.SentinelStart, DataValueType.Exists)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<UInt24Codec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecNonNegativeAtUInt24MissingReturnsUInt32()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(UInt24Codec.SentinelStart, DataValueType.Exists)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<UInt32Codec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecIntegersWithNegativesWithinInt16ReturnsInt16()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(-10, DataValueType.Exists),
                new DoubleDataValue(0, DataValueType.Exists),
                new DoubleDataValue(100, DataValueType.Exists)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<Int16Codec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecNegativeWithMaxAtInt16MissingReturnsInt24()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(-1, DataValueType.Exists),
                new DoubleDataValue(Int16Codec.SentinelStart, DataValueType.Exists)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<Int24Codec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecIntegersNonNegativeWithinUInt24ReturnsUInt24()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(10000000, DataValueType.Exists), // 10,000,000 < 16,777,209 (Missing sentinel)
                new DoubleDataValue(1, DataValueType.Exists)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<UInt24Codec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecIntegersWithNegativesWithinInt24ReturnsInt24()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(-100, DataValueType.Exists),
                new DoubleDataValue(32766, DataValueType.Exists)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<Int24Codec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecIntegersLargeNonNegativeReturnsUInt32()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(20000000, DataValueType.Exists), // > UInt24 Missing sentinel
                new DoubleDataValue(0, DataValueType.Exists)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<UInt32Codec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecIntegersWithLargeNegativeReturnsInt32()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(-9000000, DataValueType.Exists), // < -8,388,608
                new DoubleDataValue(123, DataValueType.Exists)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<Int32Codec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecNonIntegersFloatExactReturnsFloat()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(1.5, DataValueType.Exists),
                new DoubleDataValue(2.25, DataValueType.Exists)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<FloatCodec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecNonIntegersNotFloatExactReturnsDouble()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(Math.PI, DataValueType.Exists)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<DoubleCodec>(codec);
        }

        [TestMethod]
        public void ProcessTinyNonIntegerIsConsideredInteger()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(5.0 * float.Epsilon, DataValueType.Exists)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<UInt16Codec>(codec); // non-negative tiny is treated as integer and falls to UInt16 by default
        }

        [TestMethod]
        public void CreateBestCodecMultipleSpansAccumulateReturnsFloat()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk1 =
            [
                new DoubleDataValue(1, DataValueType.Exists),
                new DoubleDataValue(2, DataValueType.Exists)
            ];
            DoubleDataValue[] chunk2 =
            [
                new DoubleDataValue(3.5, DataValueType.Exists)
            ];
            selector.Process(chunk1);
            selector.Process(chunk2);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<FloatCodec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecNaNForcesDouble()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(double.NaN, DataValueType.Exists)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<DoubleCodec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecInfinityForcesDouble()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(double.PositiveInfinity, DataValueType.Exists)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<DoubleCodec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecNegativeAtInt24MissingReturnsInt32()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(-1, DataValueType.Exists),
                new DoubleDataValue(Int24Codec.SentinelStart, DataValueType.Exists)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<Int32Codec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecOnlySentinelsReturnsDefaultUInt16()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk =
            [
                new DoubleDataValue(0, DataValueType.Missing),
                new DoubleDataValue(0, DataValueType.CanNotRepresent),
                new DoubleDataValue(0, DataValueType.Confidential)
            ];
            selector.Process(chunk);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<UInt16Codec>(codec);
        }

        [TestMethod]
        public void CreateBestCodecDoubleSelectionIsStickyAcrossFurtherProcessCalls()
        {
            BinaryValueCodecSelector selector = new();
            DoubleDataValue[] chunk1 =
            [
                new DoubleDataValue(Math.PI, DataValueType.Exists)
            ];
            DoubleDataValue[] chunk2 =
            [
                new DoubleDataValue(1, DataValueType.Exists),
                new DoubleDataValue(2, DataValueType.Exists)
            ];
            selector.Process(chunk1);
            selector.Process(chunk2);
            IBinaryValueCodec codec = selector.CreateCodec();
            Assert.IsInstanceOfType<DoubleCodec>(codec);
        }
    }
}
