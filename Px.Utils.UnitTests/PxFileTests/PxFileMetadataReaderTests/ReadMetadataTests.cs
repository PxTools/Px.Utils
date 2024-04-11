using System.Text;
using PxFileTests.Fixtures;
using PxUtils.Exceptions;
using PxUtils.PxFile.Metadata;

namespace PxFileTests.PxFileMetadataReaderTests
{
    [TestClass]
    public class ReadMetadataTests
    {
        [TestMethod]
        public void ReadMetadata_CalledWith_MINIMAL_UTF8_N_ReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IList<KeyValuePair<string, string>> metadata = PxFileMetadataReader.ReadMetadata(stream, encoding).ToList();

            // Assert
            Assert.AreEqual(8, metadata.Count);

            Assert.AreEqual("CHARSET", metadata[0].Key);
            Assert.AreEqual("\"ANSI\"", metadata[0].Value);

            Assert.AreEqual("AXIS-VERSION", metadata[1].Key);
            Assert.AreEqual("\"2013\"", metadata[1].Value);

            Assert.AreEqual("CODEPAGE", metadata[2].Key);
            Assert.AreEqual("\"utf-8\"", metadata[2].Value);

            Assert.AreEqual("LANGUAGES", metadata[3].Key);
            Assert.AreEqual("\"aa\",\"åå\",\"öö\"", metadata[3].Value);

            Assert.AreEqual("NEXT-UPDATE", metadata[4].Key);
            Assert.AreEqual("\"20240131 08:00\"", metadata[4].Value);

            Assert.AreEqual("SUBJECT-AREA", metadata[5].Key);
            Assert.AreEqual("\"test\"", metadata[5].Value);

            Assert.AreEqual("SUBJECT-AREA[åå]", metadata[6].Key);
            Assert.AreEqual("\"test\"", metadata[6].Value);

            Assert.AreEqual("COPYRIGHT", metadata[7].Key);
            Assert.AreEqual("YES", metadata[7].Value);
        }

        [TestMethod]
        public void ReadMetadata_CalledWith_MINIMAL_UTF8_RN_ReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_RN);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IList<KeyValuePair<string, string>> metadata = PxFileMetadataReader.ReadMetadata(stream, encoding).ToList();

            // Assert
            Assert.AreEqual(8, metadata.Count);

            Assert.AreEqual("CHARSET", metadata[0].Key);
            Assert.AreEqual("\"ANSI\"", metadata[0].Value);

            Assert.AreEqual("AXIS-VERSION", metadata[1].Key);
            Assert.AreEqual("\"2013\"", metadata[1].Value);

            Assert.AreEqual("CODEPAGE", metadata[2].Key);
            Assert.AreEqual("\"utf-8\"", metadata[2].Value);

            Assert.AreEqual("LANGUAGES", metadata[3].Key);
            Assert.AreEqual("\"aa\",\"åå\",\"öö\"", metadata[3].Value);

            Assert.AreEqual("NEXT-UPDATE", metadata[4].Key);
            Assert.AreEqual("\"20240131 08:00\"", metadata[4].Value);

            Assert.AreEqual("SUBJECT-AREA", metadata[5].Key);
            Assert.AreEqual("\"test\"", metadata[5].Value);

            Assert.AreEqual("SUBJECT-AREA[åå]", metadata[6].Key);
            Assert.AreEqual("\"test\"", metadata[6].Value);

            Assert.AreEqual("COPYRIGHT", metadata[7].Key);
            Assert.AreEqual("YES", metadata[7].Value);
        }


        [TestMethod]
        public void ReadMetadata_CalledWith_MINIMAL_UTF8_N_WITH_DELIMETERS_IN_VALUESTRING_ReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_WITH_DELIMETERS_IN_VALUESTRING);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IList<KeyValuePair<string, string>> metadata = PxFileMetadataReader.ReadMetadata(stream, encoding).ToList();

            // Assert
            Assert.AreEqual(8, metadata.Count);

            Assert.AreEqual("CHARSET", metadata[0].Key);
            Assert.AreEqual("\"ANSI\"", metadata[0].Value);

            Assert.AreEqual("AXIS-VERSION", metadata[1].Key);
            Assert.AreEqual("\"2013;2014\"", metadata[1].Value);

            Assert.AreEqual("CODEPAGE", metadata[2].Key);
            Assert.AreEqual("\"utf-8\"", metadata[2].Value);

            Assert.AreEqual("LANGUAGES", metadata[3].Key);
            Assert.AreEqual("\"aa\",\"åå\",\"öö\"", metadata[3].Value);

            Assert.AreEqual("NEXT-UPDATE", metadata[4].Key);
            Assert.AreEqual("\"20240131 08:00\"", metadata[4].Value);

            Assert.AreEqual("SUBJECT-AREA", metadata[5].Key);
            Assert.AreEqual("\"test\"", metadata[5].Value);

            Assert.AreEqual("SUBJECT-AREA[åå]", metadata[6].Key);
            Assert.AreEqual("\"test=a;\"", metadata[6].Value);

            Assert.AreEqual("COPYRIGHT", metadata[7].Key);
            Assert.AreEqual("YES", metadata[7].Value);
        }

        [TestMethod]
        public void ReadMetadata_CalledWith_MINIMAL_UTF8_N_WITH_DELIMETERS_IN_VALUESTRING_And_Small_Buffer_Size_ReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_WITH_DELIMETERS_IN_VALUESTRING);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IList<KeyValuePair<string, string>> metadata = PxFileMetadataReader.ReadMetadata(stream, encoding, null, 28).ToList();

            // Assert
            Assert.AreEqual(8, metadata.Count);

            Assert.AreEqual("CHARSET", metadata[0].Key);
            Assert.AreEqual("\"ANSI\"", metadata[0].Value);

            Assert.AreEqual("AXIS-VERSION", metadata[1].Key);
            Assert.AreEqual("\"2013;2014\"", metadata[1].Value);

            Assert.AreEqual("CODEPAGE", metadata[2].Key);
            Assert.AreEqual("\"utf-8\"", metadata[2].Value);

            Assert.AreEqual("LANGUAGES", metadata[3].Key);
            Assert.AreEqual("\"aa\",\"åå\",\"öö\"", metadata[3].Value);

            Assert.AreEqual("NEXT-UPDATE", metadata[4].Key);
            Assert.AreEqual("\"20240131 08:00\"", metadata[4].Value);

            Assert.AreEqual("SUBJECT-AREA", metadata[5].Key);
            Assert.AreEqual("\"test\"", metadata[5].Value);

            Assert.AreEqual("SUBJECT-AREA[åå]", metadata[6].Key);
            Assert.AreEqual("\"test=a;\"", metadata[6].Value);

            Assert.AreEqual("COPYRIGHT", metadata[7].Key);
            Assert.AreEqual("YES", metadata[7].Value);
        }

        [TestMethod]
        public void ReadMetadata_CalledWith_BROKEN_UTF8_N_Throws_InvalidPxFileMetadataException()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.BROKEN_UTF8_N);
            using Stream stream = new MemoryStream(data);

            // Act + Assert
            stream.Seek(0, SeekOrigin.Begin);
            Assert.ThrowsException<InvalidPxFileMetadataException>(() => PxFileMetadataReader.ReadMetadata(stream, Encoding.UTF8).ToList());
        }

        [TestMethod]
        public void ReadMetadata_CalledWith_MINIMAL_UTF8_N_WITH_MULTILINE_VALUES_ReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_WITH_MULTILINE_VALUES);
            using Stream stream = new MemoryStream(data);

            // Act
            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IList<KeyValuePair<string, string>> metadata = PxFileMetadataReader.ReadMetadata(stream, encoding).ToList();

            // Assert
            Assert.AreEqual(8, metadata.Count);

            Assert.AreEqual("CHARSET", metadata[0].Key);
            Assert.AreEqual("\"ANSI\"", metadata[0].Value);

            Assert.AreEqual("AXIS-VERSION", metadata[1].Key);
            Assert.AreEqual("\"2013\"", metadata[1].Value);

            Assert.AreEqual("CODEPAGE", metadata[2].Key);
            Assert.AreEqual("\"utf-8\"", metadata[2].Value);

            Assert.AreEqual("LANGUAGES", metadata[3].Key);
            Assert.AreEqual("\"aa\",\n\"åå\",\n\"öö\"", metadata[3].Value);

            Assert.AreEqual("NEXT-UPDATE", metadata[4].Key);
            Assert.AreEqual("\"20240131 08:00\"", metadata[4].Value);

            Assert.AreEqual("SUBJECT-AREA", metadata[5].Key);
            Assert.AreEqual("\"test\"", metadata[5].Value);

            Assert.AreEqual("SUBJECT-AREA[åå]", metadata[6].Key);
            Assert.AreEqual("\"test\"", metadata[6].Value);

            Assert.AreEqual("COPYRIGHT", metadata[7].Key);
            Assert.AreEqual("YES", metadata[7].Value);
        }
    }
}
