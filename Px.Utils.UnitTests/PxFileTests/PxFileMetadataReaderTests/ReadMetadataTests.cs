using Moq;
using Px.Utils.UnitTests.PxFileTests.Fixtures;
using PxUtils.PxFile.Meta;
using System.Text;

namespace PxFileTests.PxFileMetadataReaderTests
{
    public static class ReadMetadataTests
    {
        [Fact]
        public static void ReadMetadata_WhenCalled_ReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL1_UTF8_N);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IList<KeyValuePair<string, string>> metadata = PxFileMetadataReader.ReadMetadata(stream, encoding).ToList();

            // Assert
            Assert.Equal(8, metadata.Count);

            Assert.Equal("CHARSET", metadata[0].Key);
            Assert.Equal("\"ANSI\"", metadata[0].Value);

            Assert.Equal("AXIS-VERSION", metadata[1].Key);
            Assert.Equal("\"2013\"", metadata[1].Value);

            Assert.Equal("CODEPAGE", metadata[2].Key);
            Assert.Equal("\"utf-8\"", metadata[2].Value);

            Assert.Equal("LANGUAGES", metadata[3].Key);
            Assert.Equal("\"aa\",\"åå\",\"öö\"", metadata[3].Value);

            Assert.Equal("NEXT-UPDATE", metadata[4].Key);
            Assert.Equal("\"20240131 08:00\"", metadata[4].Value);

            Assert.Equal("SUBJECT-AREA", metadata[5].Key);
            Assert.Equal("\"test\"", metadata[5].Value);

            Assert.Equal("SUBJECT-AREA[åå]", metadata[6].Key);
            Assert.Equal("\"test\"", metadata[6].Value);

            Assert.Equal("COPYRIGHT", metadata[7].Key);
            Assert.Equal("YES", metadata[7].Value);
        }
    }
}
