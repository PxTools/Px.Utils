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
    }
}
