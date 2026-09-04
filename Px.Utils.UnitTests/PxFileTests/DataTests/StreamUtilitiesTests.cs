using PxFileTests.Fixtures;
using Px.Utils.PxFile;
using Px.Utils.PxFile.Data;
using System.Text;

namespace PxFileTests.DataTests
{
    [TestClass]
    public class StreamUtilitiesTests
    {
        /*
         * THIS TEST SET ASSUMES THAT THE INPUT IS VALIDATED AND DOES NOT CONTAIN ANY ERRORS
         */

        [TestMethod]
        public void FindKeywordPositionKeywordSplitAcrossBuffersReturnsKeywordOffset()
        {
            // Arrange
            string content = "TITLE=\"DATA=79\";\r\n  VALUES=\"x\";\r\nDATA=1;";
            byte[] data = Encoding.UTF8.GetBytes(content);
            using Stream stream = new MemoryStream(data);
            long expectedPosition = Encoding.UTF8.GetByteCount(content[..content.LastIndexOf("DATA=", StringComparison.Ordinal)]);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, "DATA", PxFileConfiguration.Default, 3);

            // Assert
            Assert.AreEqual(expectedPosition, position);
            Assert.IsTrue(stream.Position > position);
        }

        [TestMethod]
        public async Task FindKeywordPositionAsyncKeywordSplitAcrossBuffersReturnsKeywordOffset()
        {
            // Arrange
            string content = "TITLE=\"DATA=79\";\r\n  VALUES=\"x\";\r\nDATA=1;";
            byte[] data = Encoding.UTF8.GetBytes(content);
            using Stream stream = new MemoryStream(data);
            long expectedPosition = Encoding.UTF8.GetByteCount(content[..content.LastIndexOf("DATA=", StringComparison.Ordinal)]);

            // Act
            long position = await StreamUtilities.FindKeywordPositionAsync(stream, "DATA", PxFileConfiguration.Default, CancellationToken.None, 3);

            // Assert
            Assert.AreEqual(expectedPosition, position);
            Assert.IsTrue(stream.Position > position);
        }

        [TestMethod]
        public void FindKeywordPositionAtStartOfMetadataReturnsFirstValueOffset()
        {
            // Arrange
            string content = "METADATA=\"FOO\";\r\nDATA=1;";
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            byte[] data = contentBytes;
            using Stream stream = new MemoryStream(data);
            long expectedPosition = Encoding.UTF8.GetByteCount(content[..content.LastIndexOf("METADATA=", StringComparison.Ordinal)]);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, "METADATA", PxFileConfiguration.Default, 2);

            // Assert   
            Assert.AreEqual(expectedPosition, position);
        }

        [TestMethod]
        public async Task FindKeywordPositionAtStreamOriginWithBomReturnsRawKeywordOffset()
        {
            // Arrange
            byte[] bom = Encoding.UTF8.GetPreamble();
            byte[] data = [.. bom, .. Encoding.UTF8.GetBytes("DATA=1;")];
            using Stream synchronousStream = new MemoryStream(data);
            using Stream asynchronousStream = new MemoryStream(data);

            // Act
            long synchronousPosition = StreamUtilities.FindKeywordPosition(synchronousStream, "DATA", PxFileConfiguration.Default, 2);
            long asynchronousPosition = await StreamUtilities.FindKeywordPositionAsync(asynchronousStream, "DATA", PxFileConfiguration.Default, TestContext.CancellationToken, 2);

            // Assert
            Assert.AreEqual(bom.Length, synchronousPosition);
            Assert.AreEqual(synchronousPosition, asynchronousPosition);
            Assert.IsGreaterThan(synchronousPosition, synchronousStream.Position);
            Assert.IsGreaterThan(asynchronousPosition, asynchronousStream.Position);
        }

        [TestMethod]
        public void FindKeywordPositionAtStartOfMetadataWithBomReturnsFirstValueOffset()
        {
            // Arrange
            string content = "METADATA=\"FOO\";\r\nDATA=1;";
            byte[] bom = Encoding.UTF8.GetPreamble();
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            byte[] data = [.. bom, .. contentBytes];
            using Stream stream = new MemoryStream(data);
            long expectedPosition = bom.Length + Encoding.UTF8.GetByteCount(content[..content.LastIndexOf("METADATA=", StringComparison.Ordinal)]);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, "METADATA", PxFileConfiguration.Default, 2);

            // Assert   
            Assert.AreEqual(expectedPosition, position);
        }

        [TestMethod]
        [DataRow("1")]
        [DataRow("-1")]
        [DataRow(".5")]
        [DataRow("0")]
        [DataRow("0.0")]
        [DataRow("0.5")]
        [DataRow("123.456")]
        [DataRow("\".\"")]
        [DataRow("\"..\"")]
        public void FindDataStartPositionDataWithBomMultibyteMetadataAndWhitespaceReturnsFirstValueOffset(string firstValue)
        {
            // Arrange
            string content = "TITLE=\"DATA=79_20180101;\";\r\nVALUES=\"Ää, Öö\";\r\nDATA=\t \r\n" + firstValue + " 2;";
            byte[] bom = Encoding.UTF8.GetPreamble();
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            byte[] data = [.. bom, .. contentBytes];
            using Stream stream = new MemoryStream(data);
            long expectedPosition = bom.Length + Encoding.UTF8.GetByteCount(content[..(content.LastIndexOf("DATA=", StringComparison.Ordinal) + 5 + "\t \r\n".Length)]);

            // Act
            long position = StreamUtilities.FindDataStartPosition(stream, PxFileConfiguration.Default, 3);

            // Assert
            Assert.AreEqual(expectedPosition, position);
            Assert.AreEqual(0, stream.Position);
            Assert.AreEqual(firstValue[0], (char)data[(int)position]);
        }

        [TestMethod]
        public async Task FindDataStartPositionAsyncDataSplitAcrossBuffersReturnsSameOffset()
        {
            // Arrange
            string content = "TITLE=\"Åland\";\nDATA=\r\n\t\".\" 2;";
            byte[] bom = Encoding.UTF8.GetPreamble();
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            byte[] data = [.. bom, .. contentBytes];
            using Stream stream = new MemoryStream(data);
            long expectedPosition = bom.Length + Encoding.UTF8.GetByteCount(content[..content.IndexOf('"', content.IndexOf("DATA=", StringComparison.Ordinal) + 5)]);

            // Act
            long synchronousPosition = StreamUtilities.FindDataStartPosition(stream, PxFileConfiguration.Default, 2);
            long asynchronousPosition = await StreamUtilities.FindDataStartPositionAsync(stream, PxFileConfiguration.Default, 2, System.Threading.CancellationToken.None);

            // Assert
            Assert.AreEqual(expectedPosition, synchronousPosition);
            Assert.AreEqual(synchronousPosition, asynchronousPosition);
            Assert.AreEqual(0, stream.Position);
        }

        [TestMethod]
        public async Task FindDataStartPositionAtStreamOriginWithBomReturnsFirstValueOffset()
        {
            // Arrange
            byte[] bom = Encoding.UTF8.GetPreamble();
            byte[] data = [.. bom, .. Encoding.UTF8.GetBytes("DATA=\r\n\t1;")];
            long expectedPosition = bom.Length + "DATA=\r\n\t".Length;
            using Stream stream = new MemoryStream(data);

            // Act
            long synchronousPosition = StreamUtilities.FindDataStartPosition(stream, PxFileConfiguration.Default, 2);
            long asynchronousPosition = await StreamUtilities.FindDataStartPositionAsync(stream, PxFileConfiguration.Default, 2, TestContext.CancellationToken);

            // Assert
            Assert.AreEqual(expectedPosition, synchronousPosition);
            Assert.AreEqual(synchronousPosition, asynchronousPosition);
            Assert.AreEqual(0, stream.Position);
        }

        [TestMethod]
        public async Task FindKeywordPositionQuotedKeywordWithBomAndMultibyteMetadataReturnsRawTopLevelOffset()
        {
            // Arrange
            string content = "TITLE=\"DATA=not-an-entry\";\nVALUES=\"Ää\";\nDATA=1;";
            byte[] data = [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(content)];
            long expectedPosition = Encoding.UTF8.GetPreamble().Length + Encoding.UTF8.GetByteCount(content[..content.LastIndexOf("DATA=", StringComparison.Ordinal)]);
            using Stream synchronousStream = new MemoryStream(data);
            using Stream asynchronousStream = new MemoryStream(data);

            // Act
            long synchronousPosition = StreamUtilities.FindKeywordPosition(synchronousStream, "DATA", PxFileConfiguration.Default, 2);
            long asynchronousPosition = await StreamUtilities.FindKeywordPositionAsync(asynchronousStream, "DATA", PxFileConfiguration.Default, CancellationToken.None, 2);

            // Assert
            Assert.AreEqual(expectedPosition, synchronousPosition);
            Assert.AreEqual(synchronousPosition, asynchronousPosition);
            Assert.IsGreaterThan(synchronousPosition, synchronousStream.Position);
            Assert.IsGreaterThan(asynchronousPosition, asynchronousStream.Position);
        }

        [TestMethod]
        public void FindDataStartPositionDataWithoutValueReturnsNegative1()
        {
            // Arrange
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes("TITLE=\"foo\";\nDATA=\r\n\t "));

            // Act
            long position = StreamUtilities.FindDataStartPosition(stream, PxFileConfiguration.Default, 2);

            // Assert
            Assert.AreEqual(-1, position);
        }

        [TestMethod]
        public void FindDataStartPositionWithMetadataKeywordReturnsFirstValueOffset()
        {
            // Arrange
            string content = "TITLE=\"foo\";\nMETADATA=\"some-metadata\";\nVALUES=\"Ää\";\nDATA=1;";
            byte[] data = Encoding.UTF8.GetBytes(content);
            int dataValueIndex = content.LastIndexOf("DATA=", StringComparison.Ordinal) + "DATA=".Length;
            int metadataValueIndex = content.IndexOf("METADATA=", StringComparison.Ordinal) + "METADATA=".Length;
            long expectedPosition = Encoding.UTF8.GetByteCount(content[..dataValueIndex]);
            long metadataValuePosition = Encoding.UTF8.GetByteCount(content[..metadataValueIndex]);
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindDataStartPosition(stream, PxFileConfiguration.Default, 2);

            // Assert
            Assert.AreEqual(expectedPosition, position);
            Assert.AreNotEqual(metadataValuePosition, position);
        }

        [TestMethod]
        public async Task FindDataStartPositionWithMetadataKeywordAsyncReturnsFirstValueOffset()
        {
            // Arrange
            string content = "TITLE=\"foo\";\nMETADATA=\"some-metadata\";\nVALUES=\"Ää\";\nDATA=1;";
            byte[] data = Encoding.UTF8.GetBytes(content);
            int dataValueIndex = content.LastIndexOf("DATA=", StringComparison.Ordinal) + "DATA=".Length;
            int metadataValueIndex = content.IndexOf("METADATA=", StringComparison.Ordinal) + "METADATA=".Length;
            long expectedPosition = Encoding.UTF8.GetByteCount(content[..dataValueIndex]);
            long metadataValuePosition = Encoding.UTF8.GetByteCount(content[..metadataValueIndex]);
            using Stream stream = new MemoryStream(data);

            // Act
            long position = await StreamUtilities.FindDataStartPositionAsync(stream, PxFileConfiguration.Default, 2, default);

            // Assert
            Assert.AreEqual(expectedPosition, position);
            Assert.AreNotEqual(metadataValuePosition, position);
        }

        [TestMethod]
        [DataRow("DATA=;")]
        [DataRow("DATA= ;")]
        [DataRow("DATA=\r\n\t;")]
        public async Task FindDataStartPositionEmptyDataEntryReturnsNegative1(string content)
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            long synchronousPosition = StreamUtilities.FindDataStartPosition(stream, PxFileConfiguration.Default, 1);
            long asynchronousPosition = await StreamUtilities.FindDataStartPositionAsync(stream, PxFileConfiguration.Default, 1);

            Assert.AreEqual(-1, synchronousPosition);
            Assert.AreEqual(synchronousPosition, asynchronousPosition);
            Assert.AreEqual(0, stream.Position);
        }

        public TestContext TestContext { get; set; }
    }
}
