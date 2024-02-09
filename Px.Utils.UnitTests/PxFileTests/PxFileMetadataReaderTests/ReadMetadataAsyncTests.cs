using Px.Utils.PxFile.Exceptions;
using Px.Utils.UnitTests.PxFileTests.Fixtures;
using PxUtils.PxFile.Meta;
using System.Text;

namespace PxFileTests.PxFileMetadataReaderTests
{
    public static class ReadMetadataAsyncTests
    {
        [Fact]
        public static async Task ReadMetadataAsync_CalledWith_MINIMAL_UTF8_N_ReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IAsyncEnumerable<KeyValuePair<string, string>> metadata = PxFileMetadataReader.ReadMetadataAsync(stream, encoding);
            IList<KeyValuePair<string, string>> metadataList = await metadata.ToListAsync();

            // Assert
            Assert.Equal(8, metadataList.Count);

            Assert.Equal("CHARSET", metadataList[0].Key);
            Assert.Equal("\"ANSI\"", metadataList[0].Value);

            Assert.Equal("AXIS-VERSION", metadataList[1].Key);
            Assert.Equal("\"2013\"", metadataList[1].Value);

            Assert.Equal("CODEPAGE", metadataList[2].Key);
            Assert.Equal("\"utf-8\"", metadataList[2].Value);

            Assert.Equal("LANGUAGES", metadataList[3].Key);
            Assert.Equal("\"aa\",\"åå\",\"öö\"", metadataList[3].Value);

            Assert.Equal("NEXT-UPDATE", metadataList[4].Key);
            Assert.Equal("\"20240131 08:00\"", metadataList[4].Value);

            Assert.Equal("SUBJECT-AREA", metadataList[5].Key);
            Assert.Equal("\"test\"", metadataList[5].Value);

            Assert.Equal("SUBJECT-AREA[åå]", metadataList[6].Key);
            Assert.Equal("\"test\"", metadataList[6].Value);

            Assert.Equal("COPYRIGHT", metadataList[7].Key);
            Assert.Equal("YES", metadataList[7].Value);
        }

        [Fact]
        public static async Task ReadMetadataAsync_CalledWith_MINIMAL_UTF8_RN_ReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_RN);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IAsyncEnumerable<KeyValuePair<string, string>> metadata = PxFileMetadataReader.ReadMetadataAsync(stream, encoding);
            IList<KeyValuePair<string, string>> metadataList = await metadata.ToListAsync();

            // Assert
            Assert.Equal(8, metadataList.Count);

            Assert.Equal("CHARSET", metadataList[0].Key);
            Assert.Equal("\"ANSI\"", metadataList[0].Value);

            Assert.Equal("AXIS-VERSION", metadataList[1].Key);
            Assert.Equal("\"2013\"", metadataList[1].Value);

            Assert.Equal("CODEPAGE", metadataList[2].Key);
            Assert.Equal("\"utf-8\"", metadataList[2].Value);

            Assert.Equal("LANGUAGES", metadataList[3].Key);
            Assert.Equal("\"aa\",\"åå\",\"öö\"", metadataList[3].Value);

            Assert.Equal("NEXT-UPDATE", metadataList[4].Key);
            Assert.Equal("\"20240131 08:00\"", metadataList[4].Value);

            Assert.Equal("SUBJECT-AREA", metadataList[5].Key);
            Assert.Equal("\"test\"", metadataList[5].Value);

            Assert.Equal("SUBJECT-AREA[åå]", metadataList[6].Key);
            Assert.Equal("\"test\"", metadataList[6].Value);

            Assert.Equal("COPYRIGHT", metadataList[7].Key);
            Assert.Equal("YES", metadataList[7].Value);
        }


        [Fact]
        public static async Task ReadMetadataAsync_CalledWith_MINIMAL_UTF8_N_WITH_DELIMETERS_IN_VALUESTRING_ReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_WITH_DELIMETERS_IN_VALUESTRING);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IAsyncEnumerable<KeyValuePair<string, string>> metadata = PxFileMetadataReader.ReadMetadataAsync(stream, encoding);
            IList<KeyValuePair<string, string>> metadataList = await metadata.ToListAsync();

            // Assert
            Assert.Equal(8, metadataList.Count);

            Assert.Equal("CHARSET", metadataList[0].Key);
            Assert.Equal("\"ANSI\"", metadataList[0].Value);

            Assert.Equal("AXIS-VERSION", metadataList[1].Key);
            Assert.Equal("\"2013;2014\"", metadataList[1].Value);

            Assert.Equal("CODEPAGE", metadataList[2].Key);
            Assert.Equal("\"utf-8\"", metadataList[2].Value);

            Assert.Equal("LANGUAGES", metadataList[3].Key);
            Assert.Equal("\"aa\",\"åå\",\"öö\"", metadataList[3].Value);

            Assert.Equal("NEXT-UPDATE", metadataList[4].Key);
            Assert.Equal("\"20240131 08:00\"", metadataList[4].Value);

            Assert.Equal("SUBJECT-AREA", metadataList[5].Key);
            Assert.Equal("\"test\"", metadataList[5].Value);

            Assert.Equal("SUBJECT-AREA[åå]", metadataList[6].Key);
            Assert.Equal("\"test=a;\"", metadataList[6].Value);

            Assert.Equal("COPYRIGHT", metadataList[7].Key);
            Assert.Equal("YES", metadataList[7].Value);
        }

        [Fact]
        public static async Task ReadMetadataAsync_CalledWith_MINIMAL_UTF8_N_WITH_DELIMETERS_IN_VALUESTRING_And_Small_Buffer_Size_ReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_WITH_DELIMETERS_IN_VALUESTRING);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IAsyncEnumerable<KeyValuePair<string, string>> metadata = PxFileMetadataReader.ReadMetadataAsync(stream, encoding, null, 28);
            IList<KeyValuePair<string, string>> metadataList = await metadata.ToListAsync();

            // Assert
            Assert.Equal(8, metadataList.Count);

            Assert.Equal("CHARSET", metadataList[0].Key);
            Assert.Equal("\"ANSI\"", metadataList[0].Value);

            Assert.Equal("AXIS-VERSION", metadataList[1].Key);
            Assert.Equal("\"2013;2014\"", metadataList[1].Value);

            Assert.Equal("CODEPAGE", metadataList[2].Key);
            Assert.Equal("\"utf-8\"", metadataList[2].Value);

            Assert.Equal("LANGUAGES", metadataList[3].Key);
            Assert.Equal("\"aa\",\"åå\",\"öö\"", metadataList[3].Value);

            Assert.Equal("NEXT-UPDATE", metadataList[4].Key);
            Assert.Equal("\"20240131 08:00\"", metadataList[4].Value);

            Assert.Equal("SUBJECT-AREA", metadataList[5].Key);
            Assert.Equal("\"test\"", metadataList[5].Value);

            Assert.Equal("SUBJECT-AREA[åå]", metadataList[6].Key);
            Assert.Equal("\"test=a;\"", metadataList[6].Value);

            Assert.Equal("COPYRIGHT", metadataList[7].Key);
            Assert.Equal("YES", metadataList[7].Value);
        }

        [Fact]
        public static async Task ReadMetadata_CalledWith_BROKEN_UTF8_N_Throws_InvalidPxFileMetadataException()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.BROKEN_UTF8_N);
            using Stream stream = new MemoryStream(data);

            // Act + Assert
            stream.Seek(0, SeekOrigin.Begin);
            await Assert.ThrowsAsync<InvalidPxFileMetadataException>(async () =>
            {
                IAsyncEnumerable<KeyValuePair<string, string>> iterator = PxFileMetadataReader.ReadMetadataAsync(stream, Encoding.UTF8);
                await iterator.ForEachAsync(_ => { });
            });
        }
    }
}
