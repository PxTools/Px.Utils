using PxFileTests.Fixtures;
using Px.Utils.PxFile.Metadata;
using System.Text;

namespace PxFileTests.reader.ests
{
    [TestClass]
    public class GetEncodingTests
    {
        [TestMethod]
        public void GetEncodingCalledForMinimalUTF8NReturnsUtf8()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act
            Encoding encoding = reader.GetEncoding(stream);

            // Assert
            Assert.AreEqual(Encoding.UTF8, encoding);
        }

        [TestMethod]
        public void GetEncodingCalledForMinimalUTF8BUsingBOMReturnsUtf8()
        {
            // Arrange
            byte[] bom = [0xEF, 0xBB, 0xBF];
            byte[] bytes = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N.Replace("utf-8", "foo"));

            using Stream stream = new MemoryStream([.. bom, .. bytes]);
            PxFileMetadataReader reader = new();

            // Act
            Encoding encoding = reader.GetEncoding(stream);

            // Assert
            Assert.AreEqual(Encoding.UTF8, encoding);
        }

        [TestMethod]
        public void GetEncodingCalledForMinimalUTF8NUsingUTF16BOMReturnsUtf16()
        {
            // Arrange
            byte[] bom = [0xFE, 0xFF];
            byte[] bytes = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N.Replace("utf-8", "foo"));

            using Stream stream = new MemoryStream([.. bom, .. bytes]);
            PxFileMetadataReader reader = new();

            // Act
            Encoding encoding = reader.GetEncoding(stream);

            // Assert
            Assert.AreEqual(Encoding.Unicode, encoding);
        }

        [TestMethod]
        public void GetEncodingCalledForMinimalUTF8RNReturnsUtf8()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_RN);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act
            Encoding encoding = reader.GetEncoding(stream);

            // Assert
            Assert.AreEqual(Encoding.UTF8, encoding);
        }

        [TestMethod]
        public void GetEncodingCalledForMinimalISO885915ReturnsIso885915()
        {
            // Arrange
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("iso-8859-15");
            byte[] data = encoding.GetBytes(MinimalPx.MINIMAL_ISO_8859_15_N);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act
            Encoding resutEncoding = reader.GetEncoding(stream);

            // Assert
            Assert.AreEqual(encoding, resutEncoding);
        }

        [TestMethod]
        public void GetEncodingCalledForMinimalISO88591ReturnsIso88591()
        {
            // Arrange
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            byte[] data = encoding.GetBytes(MinimalPx.MINIMAL_ISO_8859_1_RN);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act
            Encoding resutEncoding = reader.GetEncoding(stream);

            // Assert
            Assert.AreEqual(encoding, resutEncoding);
        }
    }
}
