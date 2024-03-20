using PxUtils.UnitTests.PxFileTests.Fixtures;
using System.Text;
using PxUtils.Exceptions;
using PxUtils.PxFile.Metadata;

namespace PxFileTests.PxFileMetadataReaderTests
{
    [TestClass]
    public class ReadMetadataAsyncTests
    {
        [TestMethod]
        public async Task ReadMetadataAsync_CalledWith_MINIMAL_UTF8_N_ReturnsMetadata()
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
            Assert.AreEqual(8, metadataList.Count);

            Assert.AreEqual("CHARSET", metadataList[0].Key);
            Assert.AreEqual("\"ANSI\"", metadataList[0].Value);

            Assert.AreEqual("AXIS-VERSION", metadataList[1].Key);
            Assert.AreEqual("\"2013\"", metadataList[1].Value);

            Assert.AreEqual("CODEPAGE", metadataList[2].Key);
            Assert.AreEqual("\"utf-8\"", metadataList[2].Value);

            Assert.AreEqual("LANGUAGES", metadataList[3].Key);
            Assert.AreEqual("\"aa\",\"åå\",\"öö\"", metadataList[3].Value);

            Assert.AreEqual("NEXT-UPDATE", metadataList[4].Key);
            Assert.AreEqual("\"20240131 08:00\"", metadataList[4].Value);

            Assert.AreEqual("SUBJECT-AREA", metadataList[5].Key);
            Assert.AreEqual("\"test\"", metadataList[5].Value);

            Assert.AreEqual("SUBJECT-AREA[åå]", metadataList[6].Key);
            Assert.AreEqual("\"test\"", metadataList[6].Value);

            Assert.AreEqual("COPYRIGHT", metadataList[7].Key);
            Assert.AreEqual("YES", metadataList[7].Value);
        }

        [TestMethod]
        public async Task ReadMetadataAsync_CalledWith_MINIMAL_UTF8_RN_ReturnsMetadata()
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
            Assert.AreEqual(8, metadataList.Count);

            Assert.AreEqual("CHARSET", metadataList[0].Key);
            Assert.AreEqual("\"ANSI\"", metadataList[0].Value);

            Assert.AreEqual("AXIS-VERSION", metadataList[1].Key);
            Assert.AreEqual("\"2013\"", metadataList[1].Value);

            Assert.AreEqual("CODEPAGE", metadataList[2].Key);
            Assert.AreEqual("\"utf-8\"", metadataList[2].Value);

            Assert.AreEqual("LANGUAGES", metadataList[3].Key);
            Assert.AreEqual("\"aa\",\"åå\",\"öö\"", metadataList[3].Value);

            Assert.AreEqual("NEXT-UPDATE", metadataList[4].Key);
            Assert.AreEqual("\"20240131 08:00\"", metadataList[4].Value);

            Assert.AreEqual("SUBJECT-AREA", metadataList[5].Key);
            Assert.AreEqual("\"test\"", metadataList[5].Value);

            Assert.AreEqual("SUBJECT-AREA[åå]", metadataList[6].Key);
            Assert.AreEqual("\"test\"", metadataList[6].Value);

            Assert.AreEqual("COPYRIGHT", metadataList[7].Key);
            Assert.AreEqual("YES", metadataList[7].Value);
        }


        [TestMethod]
        public async Task ReadMetadataAsync_CalledWith_MINIMAL_UTF8_N_WITH_DELIMETERS_IN_VALUESTRING_ReturnsMetadata()
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
            Assert.AreEqual(8, metadataList.Count);

            Assert.AreEqual("CHARSET", metadataList[0].Key);
            Assert.AreEqual("\"ANSI\"", metadataList[0].Value);

            Assert.AreEqual("AXIS-VERSION", metadataList[1].Key);
            Assert.AreEqual("\"2013;2014\"", metadataList[1].Value);

            Assert.AreEqual("CODEPAGE", metadataList[2].Key);
            Assert.AreEqual("\"utf-8\"", metadataList[2].Value);

            Assert.AreEqual("LANGUAGES", metadataList[3].Key);
            Assert.AreEqual("\"aa\",\"åå\",\"öö\"", metadataList[3].Value);

            Assert.AreEqual("NEXT-UPDATE", metadataList[4].Key);
            Assert.AreEqual("\"20240131 08:00\"", metadataList[4].Value);

            Assert.AreEqual("SUBJECT-AREA", metadataList[5].Key);
            Assert.AreEqual("\"test\"", metadataList[5].Value);

            Assert.AreEqual("SUBJECT-AREA[åå]", metadataList[6].Key);
            Assert.AreEqual("\"test=a;\"", metadataList[6].Value);

            Assert.AreEqual("COPYRIGHT", metadataList[7].Key);
            Assert.AreEqual("YES", metadataList[7].Value);
        }

        [TestMethod]
        public async Task ReadMetadataAsync_CalledWith_MINIMAL_UTF8_N_WITH_DELIMETERS_IN_VALUESTRING_And_Small_Buffer_Size_ReturnsMetadata()
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
            Assert.AreEqual(8, metadataList.Count);

            Assert.AreEqual("CHARSET", metadataList[0].Key);
            Assert.AreEqual("\"ANSI\"", metadataList[0].Value);

            Assert.AreEqual("AXIS-VERSION", metadataList[1].Key);
            Assert.AreEqual("\"2013;2014\"", metadataList[1].Value);

            Assert.AreEqual("CODEPAGE", metadataList[2].Key);
            Assert.AreEqual("\"utf-8\"", metadataList[2].Value);

            Assert.AreEqual("LANGUAGES", metadataList[3].Key);
            Assert.AreEqual("\"aa\",\"åå\",\"öö\"", metadataList[3].Value);

            Assert.AreEqual("NEXT-UPDATE", metadataList[4].Key);
            Assert.AreEqual("\"20240131 08:00\"", metadataList[4].Value);

            Assert.AreEqual("SUBJECT-AREA", metadataList[5].Key);
            Assert.AreEqual("\"test\"", metadataList[5].Value);

            Assert.AreEqual("SUBJECT-AREA[åå]", metadataList[6].Key);
            Assert.AreEqual("\"test=a;\"", metadataList[6].Value);

            Assert.AreEqual("COPYRIGHT", metadataList[7].Key);
            Assert.AreEqual("YES", metadataList[7].Value);
        }

        [TestMethod]
        public async Task ReadMetadata_CalledWith_BROKEN_UTF8_N_Throws_InvalidPxFileMetadataException()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.BROKEN_UTF8_N);
            using Stream stream = new MemoryStream(data);

            // Act + Assert
            stream.Seek(0, SeekOrigin.Begin);
            await Assert.ThrowsExceptionAsync<InvalidPxFileMetadataException>(async () =>
            {
                IAsyncEnumerable<KeyValuePair<string, string>> iterator = PxFileMetadataReader.ReadMetadataAsync(stream, Encoding.UTF8);
                await iterator.ForEachAsync(_ => { });
            });
        }

        [TestMethod]
        public async Task ReadMetadataAsync_CalledWith_MINIMAL_UTF8_N_WITH_MULTILINE_VALUES_ReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_WITH_MULTILINE_VALUES);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IAsyncEnumerable<KeyValuePair<string, string>> metadata = PxFileMetadataReader.ReadMetadataAsync(stream, encoding);
            IList<KeyValuePair<string, string>> metadataList = await metadata.ToListAsync();

            // Assert
            Assert.AreEqual(8, metadataList.Count);

            Assert.AreEqual("CHARSET", metadataList[0].Key);
            Assert.AreEqual("\"ANSI\"", metadataList[0].Value);

            Assert.AreEqual("AXIS-VERSION", metadataList[1].Key);
            Assert.AreEqual("\"2013\"", metadataList[1].Value);

            Assert.AreEqual("CODEPAGE", metadataList[2].Key);
            Assert.AreEqual("\"utf-8\"", metadataList[2].Value);

            Assert.AreEqual("LANGUAGES", metadataList[3].Key);
            Assert.AreEqual("\"aa\",\n\"åå\",\n\"öö\"", metadataList[3].Value);

            Assert.AreEqual("NEXT-UPDATE", metadataList[4].Key);
            Assert.AreEqual("\"20240131 08:00\"", metadataList[4].Value);

            Assert.AreEqual("SUBJECT-AREA", metadataList[5].Key);
            Assert.AreEqual("\"test\"", metadataList[5].Value);

            Assert.AreEqual("SUBJECT-AREA[åå]", metadataList[6].Key);
            Assert.AreEqual("\"test\"", metadataList[6].Value);

            Assert.AreEqual("COPYRIGHT", metadataList[7].Key);
            Assert.AreEqual("YES", metadataList[7].Value);
        }
    }
}
