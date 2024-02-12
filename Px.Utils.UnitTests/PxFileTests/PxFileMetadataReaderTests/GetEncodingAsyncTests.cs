using PxUtils.UnitTests.PxFileTests.Fixtures;
using PxUtils.PxFile.Meta;
using System.Text;

namespace PxFileTests.PxFileMetadataReaderTests
{
    public static class GetEncodingAsyncTests
    {
        [Fact]
        public static async Task GetEncodingAsync_CalledFor_MINIMAL_UTF8_N_ReturnsUtf8()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);

            // Assert
            Assert.Equal(Encoding.UTF8, encoding);
        }

        [Fact]
        public static async Task GetEncodingAsync_CalledFor_MINIMAL_N_WithUtf8BOM_ReturnsUtf8()
        {
            // Arrange
            byte[] bom = [0xEF, 0xBB, 0xBF];
            byte[] bytes = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N.Replace("utf-8", "foo"));

            using Stream stream = new MemoryStream([.. bom, .. bytes]);

            // Act
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);

            // Assert
            Assert.Equal(Encoding.UTF8, encoding);
        }

        [Fact]
        public static async Task GetEncodingAsync_CalledFor_MINIMAL_N_WithUtf16BOM_ReturnsUtf16()
        {
            // Arrange
            byte[] bom = [0xFE, 0xFF];
            byte[] bytes = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N.Replace("utf-8", "foo"));

            using Stream stream = new MemoryStream([.. bom, .. bytes]);

            // Act
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);

            // Assert
            Assert.Equal(Encoding.Unicode, encoding);
        }

        [Fact]
        public static async Task GetEncodingAsync_CalledFor_MINIMAL_UTF8_RN_ReturnsUtf8()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_RN);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);

            // Assert
            Assert.Equal(Encoding.UTF8, encoding);
        }

        [Fact]
        public static async Task GetEncodingAsync_CalledFor_MINIMAL_ISO_8859_15_N_ReturnsIso885915()
        {
            // Arrange
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("iso-8859-15");
            byte[] data = encoding.GetBytes(MinimalPx.MINIMAL_ISO_8859_15_N);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding resutEncoding = await PxFileMetadataReader.GetEncodingAsync(stream);

            // Assert
            Assert.Equal(encoding, resutEncoding);
        }

        [Fact]
        public static async Task GetEncodingAsync_CalledFor_MINIMAL_ISO_8859_1_RN_ReturnsIso88591()
        {
            // Arrange
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            byte[] data = encoding.GetBytes(MinimalPx.MINIMAL_ISO_8859_1_RN);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding resutEncoding = await PxFileMetadataReader.GetEncodingAsync(stream);

            // Assert
            Assert.Equal(encoding, resutEncoding);
        }
    }
}
