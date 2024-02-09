using Px.Utils.UnitTests.PxFileTests.Fixtures;
using PxUtils.PxFile.Meta;
using System.Text;

namespace PxFileTests.PxFileMetadataReaderTests
{
    public static class GetEncodingTests
    {
        [Fact]
        public static void GetEncoding_CalledFor_MINIMAL1_UTF8_N_ReturnsUtf8()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL1_UTF8_N);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);

            // Assert
            Assert.Equal(Encoding.UTF8, encoding);
        }
    }
}
