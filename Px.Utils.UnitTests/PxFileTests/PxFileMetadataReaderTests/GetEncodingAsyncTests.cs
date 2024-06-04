using PxFileTests.Fixtures;
using Px.Utils.PxFile.Metadata;
using System.Text;

namespace PxFileTests.PxFileMetadataReaderTests
{
    [TestClass]
    public class GetEncodingAsyncTests
    {
        [TestMethod]
        public async Task GetEncodingAsyncCalledForMinimalUTF8NReturnsUtf8()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);

            // Assert
            Assert.AreEqual(Encoding.UTF8, encoding);
        }

        [TestMethod]
        public async Task GetEncodingAsyncCalledForMinimalUTF8NUsingBOMReturnsUtf8()
        {
            // Arrange
            byte[] bom = [0xEF, 0xBB, 0xBF];
            byte[] bytes = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N.Replace("utf-8", "foo"));

            using Stream stream = new MemoryStream([.. bom, .. bytes]);

            // Act
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);

            // Assert
            Assert.AreEqual(Encoding.UTF8, encoding);
        }

        [TestMethod]
        public async Task GetEncodingAsyncCalledForMinimalUTF8NUsingUTF16BOMReturnsUtf16()
        {
            // Arrange
            byte[] bom = [0xFE, 0xFF];
            byte[] bytes = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N.Replace("utf-8", "foo"));

            using Stream stream = new MemoryStream([.. bom, .. bytes]);

            // Act
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);

            // Assert
            Assert.AreEqual(Encoding.Unicode, encoding);
        }

        [TestMethod]
        public async Task GetEncodingAsyncCalledForMinimalUTF8RNReturnsUtf8()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_RN);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);

            // Assert
            Assert.AreEqual(Encoding.UTF8, encoding);
        }

        [TestMethod]
        public async Task GetEncodingAsyncCalledForMinimalISO885915ReturnsIso885915()
        {
            // Arrange
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("iso-8859-15");
            byte[] data = encoding.GetBytes(MinimalPx.MINIMAL_ISO_8859_15_N);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding resutEncoding = await PxFileMetadataReader.GetEncodingAsync(stream);

            // Assert
            Assert.AreEqual(encoding, resutEncoding);
        }

        [TestMethod]
        public async Task GetEncodingAsyncCalledForMinimalISO88591ReturnsIso88591()
        {
            // Arrange
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            byte[] data = encoding.GetBytes(MinimalPx.MINIMAL_ISO_8859_1_RN);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding resutEncoding = await PxFileMetadataReader.GetEncodingAsync(stream);

            // Assert
            Assert.AreEqual(encoding, resutEncoding);
        }
    }
}
