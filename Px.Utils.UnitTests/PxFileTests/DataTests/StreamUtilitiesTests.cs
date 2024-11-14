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
        public void FindKeywordTestDataKeywordAtStartOfStreamReturnsZero()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes("DATA=");
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, "DATA", PxFileConfiguration.Default);

            // Assert
            Assert.AreEqual(0, position);
        }

        [TestMethod]
        public void FindKeywordTestTwoDataKeywordsReturnsNegative1()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes("DATADATA=");
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, "DATA", PxFileConfiguration.Default);

            // Assert
            Assert.AreEqual(-1, position);
        }

        [TestMethod]
        public void FindKeywordTestDataKeywordInTheMiddleOfStreamReturnsIndex()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes("KEYWORD=\"foo\";\nDATA=123 345");
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, "DATA", PxFileConfiguration.Default);

            // Assert
            Assert.AreEqual(15, position);
        }

        [TestMethod]
        public void FindKeywordTestDataKeywordAtTheEndOfStreamReturnsIndex()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes("DADADADATA=");
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, "DATA", PxFileConfiguration.Default);

            // Assert
            Assert.AreEqual(-1, position);
        }

        [TestMethod]
        public void FindKeywordTestDKeywordInTheMiddleOfStreamReturnsIndex()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes("FFFFFF;D=AAAA");
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, "D", PxFileConfiguration.Default);

            // Assert
            Assert.AreEqual(7, position);
        }

        [TestMethod]
        public void FindKeywordTestDATAKeywordInTheMiddleOfUtfFixtureStreamReturnsIndex()
        {
            string keyword = "DATA";

            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, keyword, PxFileConfiguration.Default);
            string result = Encoding.ASCII.GetString(data, (int)position, keyword.Length);

            // Assert
            Assert.AreEqual(keyword, result);
        }

        [TestMethod]
        public void FindKeywordTestDATAKeywordInTheMiddleOfAsciiFixtureStreamReturnsIndex()
        {
            string keyword = "DATA";

            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_ISO_8859_15_N);
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, keyword, PxFileConfiguration.Default);
            string result = Encoding.ASCII.GetString(data, (int)position, keyword.Length);

            // Assert
            Assert.AreEqual(keyword, result);
        }

        [TestMethod]
        public void FindKeywordTestDATAKeywordInTheMiddleOfAsciiFixtureStreamShortBufferSplitsKeywordReturnsIndex()
        {
            string keyword = "DATA";

            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_ISO_8859_15_N);
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, keyword, PxFileConfiguration.Default, 3);
            string result = Encoding.ASCII.GetString(data, (int)position, keyword.Length);

            // Assert
            Assert.AreEqual(keyword, result);
        }

        [TestMethod]
        public void FindKeywordTestDATAKeywordInTheMiddleOfDataFinderFixtureStreamReturnsIndex()
        {
            string keyword = "DATA";

            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_FOR_DATA_FINDER);
            using Stream stream = new MemoryStream(data);

            // Act
            long position = StreamUtilities.FindKeywordPosition(stream, keyword, PxFileConfiguration.Default);
            string result = Encoding.ASCII.GetString(data, (int)position -1, keyword.Length+2);

            // Assert
            Assert.AreEqual('\n' + keyword + '=', result);
        }
    }
}
