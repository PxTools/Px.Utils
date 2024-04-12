using PxFileTests.Fixtures;
using PxUtils.PxFile.Metadata;
using System.Text;

namespace PxFileTests.PxFileMetadataReaderTests
{
    [TestClass]
    public class GetEncodingTests
    {
        [TestMethod]
        public void GetEncoding_CalledFor_MINIMAL_UTF8_N_ReturnsUtf8()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);

            // Assert
            Assert.AreEqual(Encoding.UTF8, encoding);
        }

        [TestMethod]
        public void GetEncoding_CalledFor_MINIMAL_N_WithUtf8BOM_ReturnsUtf8()
        {
            // Arrange
            byte[] bom = [0xEF, 0xBB, 0xBF];
            byte[] bytes = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N.Replace("utf-8", "foo"));

            using Stream stream = new MemoryStream([.. bom, .. bytes]);

            // Act
            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);

            // Assert
            Assert.AreEqual(Encoding.UTF8, encoding);
        }

        [TestMethod]
        public void GetEncoding_CalledFor_MINIMAL_N_WithUtf16BOM_ReturnsUtf16()
        {
            // Arrange
            byte[] bom = [0xFE, 0xFF];
            byte[] bytes = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N.Replace("utf-8", "foo"));

            using Stream stream = new MemoryStream([.. bom, .. bytes]);

            // Act
            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);

            // Assert
            Assert.AreEqual(Encoding.Unicode, encoding);
        }

        [TestMethod]
        public void GetEncoding_CalledFor_MINIMAL_UTF8_RN_ReturnsUtf8()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_RN);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);

            // Assert
            Assert.AreEqual(Encoding.UTF8, encoding);
        }

        [TestMethod]
        public void GetEncoding_CalledFor_MINIMAL_ISO_8859_15_N_ReturnsIso885915()
        {
            // Arrange
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("iso-8859-15");
            byte[] data = encoding.GetBytes(MinimalPx.MINIMAL_ISO_8859_15_N);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding resutEncoding = PxFileMetadataReader.GetEncoding(stream);

            // Assert
            Assert.AreEqual(encoding, resutEncoding);
        }

        [TestMethod]
        public void GetEncoding_CalledFor_MINIMAL_ISO_8859_1_RN_ReturnsIso88591()
        {
            // Arrange
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            byte[] data = encoding.GetBytes(MinimalPx.MINIMAL_ISO_8859_1_RN);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding resutEncoding = PxFileMetadataReader.GetEncoding(stream);

            // Assert
            Assert.AreEqual(encoding, resutEncoding);
        }
    }
}
