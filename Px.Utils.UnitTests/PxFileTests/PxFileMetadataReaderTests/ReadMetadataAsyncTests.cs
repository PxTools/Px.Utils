using System.Text;
using PxFileTests.Fixtures;
using Px.Utils.Exceptions;
using Px.Utils.PxFile.Metadata;

namespace Px.Utils.UnitTests.PxFileTests.PxFileMetadataReaderTests
{
    [TestClass]
    public class ReadMetadataAsyncTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task ReadMetadataAsyncCalledWithMinimalUTF8NReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act
            Encoding encoding = await reader.GetEncodingAsync(stream, TestContext.CancellationToken);
            stream.Seek(0, SeekOrigin.Begin);
            IAsyncEnumerable<KeyValuePair<string, string>> metadata = reader.ReadMetadataAsync(stream, encoding, cancellationToken: TestContext.CancellationToken);
            List<KeyValuePair<string, string>> metadataList = await metadata.ToListAsync(TestContext.CancellationToken);

            // Assert
            Assert.HasCount(8, metadataList);

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
        public async Task ReadMetadataAsyncCalledWithMinimalUTF8RNReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_RN);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act
            Encoding encoding = await reader.GetEncodingAsync(stream, TestContext.CancellationToken);
            stream.Seek(0, SeekOrigin.Begin);
            IAsyncEnumerable<KeyValuePair<string, string>> metadata = reader.ReadMetadataAsync(stream, encoding, cancellationToken: TestContext.CancellationToken);
            List<KeyValuePair<string, string>> metadataList = await metadata.ToListAsync(TestContext.CancellationToken);

            // Assert
            Assert.HasCount(8, metadataList);

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
        public async Task ReadMetadataAsyncCalledWithDelimetersInValueStringReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_WITH_DELIMETERS_IN_VALUESTRING);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act
            Encoding encoding = await reader.GetEncodingAsync(stream, TestContext.CancellationToken);
            stream.Seek(0, SeekOrigin.Begin);
            IAsyncEnumerable<KeyValuePair<string, string>> metadata = reader.ReadMetadataAsync(stream, encoding, cancellationToken: TestContext.CancellationToken);
            List<KeyValuePair<string, string>> metadataList = await metadata.ToListAsync(TestContext.CancellationToken);

            // Assert
            Assert.HasCount(8, metadataList);

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
        public async Task ReadMetadataAsyncCalledWithDelimetersInValueStringAndSmallBufferSizeReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_WITH_DELIMETERS_IN_VALUESTRING);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act
            Encoding encoding = await reader.GetEncodingAsync(stream, TestContext.CancellationToken);
            stream.Seek(0, SeekOrigin.Begin);
            IAsyncEnumerable<KeyValuePair<string, string>> metadata = reader.ReadMetadataAsync(stream, encoding, 28, cancellationToken: TestContext.CancellationToken);
            List<KeyValuePair<string, string>> metadataList = await metadata.ToListAsync(TestContext.CancellationToken);

            // Assert
            Assert.HasCount(8, metadataList);

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
        public async Task ReadMetadataCalledWithBrokenUTF8ThrowsInvalidPxFileMetadataException()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.BROKEN_UTF8_N);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act + Assert
            stream.Seek(0, SeekOrigin.Begin);
            await Assert.ThrowsExactlyAsync<InvalidPxFileMetadataException>(async () =>
            {
                await foreach (KeyValuePair<string, string> _ in reader.ReadMetadataAsync(stream, Encoding.UTF8, cancellationToken: TestContext.CancellationToken))
                {
                    // Iterate through to trigger exception
                }
            });
        }

        [TestMethod]
        public async Task ReadMetadataAsyncCalledWithMultilineValuesReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_WITH_MULTILINE_VALUES);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act
            Encoding encoding = await reader.GetEncodingAsync(stream, TestContext.CancellationToken);
            stream.Seek(0, SeekOrigin.Begin);
            IAsyncEnumerable<KeyValuePair<string, string>> metadata = reader.ReadMetadataAsync(stream, encoding, cancellationToken: TestContext.CancellationToken);
            List<KeyValuePair<string, string>> metadataList = await metadata.ToListAsync(TestContext.CancellationToken);

            // Assert
            Assert.HasCount(8, metadataList);

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
