using Px.Utils.BinaryData;
using Px.Utils.BinaryData.ValueConverters;

namespace Px.Utils.UnitTests.BinaryData
{
    [TestClass]
    public class BinaryDataReaderCreateTests
    {
        [TestMethod]
        public void CreateCodecTypeUInt32CodecReturnsCorrectReaderType()
        {
            BinaryDataReader reader = BinaryDataReader.Create(BinaryValueCodecType.UInt32Codec);

            Assert.IsInstanceOfType<BinaryDataReader<UInt32Codec>>(reader);
        }

        [TestMethod]
        public void CreateUnsupportedCodecTypeThrowsNotSupported()
        {
            Assert.ThrowsExactly<NotSupportedException>(() =>
            {
                _ = BinaryDataReader.Create((BinaryValueCodecType)int.MaxValue);
            });
        }

        [TestMethod]
        public void CreateMaxWindowSizeBytesLessThanMinThrowsArgumentOutOfRange()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            {
                _ = BinaryDataReader.Create(BinaryValueCodecType.UInt32Codec, maxWindowSizeBytes: 0);
            });
        }

        [TestMethod]
        public void CreateMergeCapBytesLessThanMinThrowsArgumentOutOfRange()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            {
                _ = BinaryDataReader.Create(BinaryValueCodecType.UInt32Codec, mergeCapBytes: 0);
            });
        }

        [TestMethod]
        public void CreateHeaderLengthBytesLessThanMinThrowsArgumentOutOfRange()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            {
                _ = BinaryDataReader.Create(BinaryValueCodecType.UInt32Codec, headerLengthBytes: -1);
            });
        }

        [TestMethod]
        public void CreateValidCustomParametersDoesNotThrow()
        {
            BinaryDataReader reader = BinaryDataReader.Create(
                BinaryValueCodecType.UInt32Codec,
                maxWindowSizeBytes: 16,
                mergeCapBytes: 32,
                headerLengthBytes: 0);

            Assert.IsInstanceOfType<BinaryDataReader<UInt32Codec>>(reader);
        }
    }
}
