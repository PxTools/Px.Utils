﻿using System.Text;
using PxFileTests.Fixtures;
using Px.Utils.Exceptions;
using Px.Utils.PxFile.Metadata;

namespace Px.Utils.UnitTests.PxFileTests.PxFileMetadataReaderTests
{
    [TestClass]
    public class ReadMetadataTests
    {
        [TestMethod]
        public void ReadMetadataCalledWithMinimalUTF8NReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act
            Encoding encoding = reader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IList<KeyValuePair<string, string>> metadata = reader.ReadMetadata(stream, encoding).ToList();

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
        public void ReadMetadataCalledWithMinimalUTF8RNReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_RN);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act
            Encoding encoding = reader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IList<KeyValuePair<string, string>> metadata = reader.ReadMetadata(stream, encoding).ToList();

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
        public void ReadMetadataCalledWithDelimetersInValueStringReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_WITH_DELIMETERS_IN_VALUESTRING);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act
            Encoding encoding = reader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IList<KeyValuePair<string, string>> metadata = reader.ReadMetadata(stream, encoding).ToList();

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
        public void ReadMetadataCalledWithDelimetersInValueStringAndSmallBufferSizeReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_WITH_DELIMETERS_IN_VALUESTRING);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act
            Encoding encoding = reader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IList<KeyValuePair<string, string>> metadata = reader.ReadMetadata(stream, encoding, 28).ToList();

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
        public void ReadMetadataCalledWithBrokenUTF8ThrowsInvalidPxFileMetadataException()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.BROKEN_UTF8_N);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act + Assert
            stream.Seek(0, SeekOrigin.Begin);
            Assert.ThrowsException<InvalidPxFileMetadataException>(() => reader.ReadMetadata(stream, Encoding.UTF8).ToList());
        }

        [TestMethod]
        public void ReadMetadataCalledWithMultilineValuesReturnsMetadata()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_WITH_MULTILINE_VALUES);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();

            // Act
            Encoding encoding = reader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            IList<KeyValuePair<string, string>> metadata = reader.ReadMetadata(stream, encoding).ToList();

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
