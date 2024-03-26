using PxFileTests.Fixtures;
using PxUtils.PxFile;
using PxUtils.PxFile.Data;
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
        public void FindKeywordTest_DataKeywordAtStartOfStream_ReturnsZero()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes("DATA=");
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, "DATA", PxFileSyntaxConf.Default);

            // Assert
            Assert.AreEqual(0, position);
        }

        [TestMethod]
        public void FindKeywordTest_TwoDataKeywords_ReturnsZero()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes("DATADATA=");
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, "DATA", PxFileSyntaxConf.Default);

            // Assert
            Assert.AreEqual(-1, position);
        }

        [TestMethod]
        public void FindKeywordTest_DataKeywordInTheMiddleOfStream_ReturnsIndex()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes("DATA=123");
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, "DATA", PxFileSyntaxConf.Default);

            // Assert
            Assert.AreEqual(0, position);
        }

        [TestMethod]
        public void FindKeywordTest_DataKeywordAtTheEndOfStream_ReturnsIndex()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes("DADADADATA=");
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, "DATA", PxFileSyntaxConf.Default);

            // Assert
            Assert.AreEqual(-1, position);
        }

        [TestMethod]
        public void FindKeywordTest_DKeywordInTheMiddleOfStream_ReturnsIndex()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes("FFFFFF;D=AAAA");
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, "D", PxFileSyntaxConf.Default);

            // Assert
            Assert.AreEqual(7, position);
        }

        [TestMethod]
        public void FindKeywordTest_DATAKeywordInTheMiddleOfUtfFixtureStream_ReturnsIndex()
        {
            string keyword = "DATA";

            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, keyword, PxFileSyntaxConf.Default);
            string result = Encoding.ASCII.GetString(data, (int)position, keyword.Length);

            // Assert
            Assert.AreEqual(keyword, result);
        }

        [TestMethod]
        public void FindKeywordTest_DATAKeywordInTheMiddleOfAsciiFixtureStream_ReturnsIndex()
        {
            string keyword = "DATA";

            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_ISO_8859_15_N);
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, keyword, PxFileSyntaxConf.Default);
            string result = Encoding.ASCII.GetString(data, (int)position, keyword.Length);

            // Assert
            Assert.AreEqual(keyword, result);
        }

        [TestMethod]
        public void FindKeywordTest_DATAKeywordInTheMiddleOfDataFinderFixtureStream_ReturnsIndex()
        {
            string keyword = "DATA";

            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_FOR_DATA_FINDER);
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, keyword, PxFileSyntaxConf.Default);
            string result = Encoding.ASCII.GetString(data, (int)position -1, keyword.Length+2);

            // Assert
            Assert.AreEqual('\n' + keyword + '=', result);
        }
    }
}
