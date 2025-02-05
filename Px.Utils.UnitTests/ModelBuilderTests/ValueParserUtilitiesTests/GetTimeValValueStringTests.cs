using Px.Utils.ModelBuilders;

namespace Px.Utils.UnitTests.ModelBuilderTests.ValueParserUtilitiesTests
{
    [TestClass]
    public class GetTimeValValueRangeStringTests
    {
        [TestMethod]
        public void GetTimeValValueRangeStringTestEmptyInputThrowsException()
        {
            string input = "TLIST(A1)";
            Assert.ThrowsException<ArgumentException>(() => ValueParserUtilities.GetTimeValValueRangeString(input));
        }

        [TestMethod]
        public void GetTimeValValueRangeStringTestListInputThrowsException()
        {
            string input = "TLIST(A1), \"9000\", \"9001\", \"9002\", \"9003\", \"9004\"";
            Assert.ThrowsException<ArgumentException>(() => ValueParserUtilities.GetTimeValValueRangeString(input));
        }

        [TestMethod]
        public void GetTimeValValueRangeStringTestInvalidRangeFormatThrowsException()
        {
            string input = "TLIST(A1, \"9001\")";
            Assert.ThrowsException<ArgumentException>(() => ValueParserUtilities.GetTimeValValueRangeString(input));
        }

        [TestMethod]
        public void GetTimeValueStringTestValidInputReturnsString()
        {
            string input = "TLIST(A1, \"9000-9001\")";
            string expected = "9000-9001";
            string actual = ValueParserUtilities.GetTimeValValueRangeString(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetTimeValueStringTestInputWithTwoRangesThrowsException()
        {
            string input = "TLIST(A1, \"9000-9001\", \"9002-9003\")";
            Assert.ThrowsException<ArgumentException>(() => ValueParserUtilities.GetTimeValValueRangeString(input));
        }

        [TestMethod]
        public void GetTimeValueStringTestInputWithRangeOfThreePartRangeThrowsException()
        {
            string input = "TLIST(A1, \"9000-9001-9002\")";
            Assert.ThrowsException<ArgumentException>(() => ValueParserUtilities.GetTimeValValueRangeString(input));
        }

        [TestMethod]
        public void GetTimeValueStringTestInputRangeWithoutStringDelimetersThrowsException()
        {
            string input = "TLIST(A1, 9000-9001)";
            Assert.ThrowsException<ArgumentException>(() => ValueParserUtilities.GetTimeValValueRangeString(input));
        }

        [TestMethod]
        public void GetTimeValueStringTestInputRangeWithExtraWhitespaceReturnsString()
        {
            string input = "TLIST(A1,  \"9000-9001\" )";
            string expected = "9000-9001";
            string actual = ValueParserUtilities.GetTimeValValueRangeString(input);

            Assert.AreEqual(expected, actual);
        }
    }
}
