using Px.Utils.ModelBuilders;

namespace Px.Utils.UnitTests.ModelBuilderTests.ValueParserUtilitiesTests
{
    [TestClass]
    public class GetTimeValValueStringTests
    {
        [TestMethod]
        public void GetTimeValValueStringTestEmptyInputReturnsEmptyString()
        {
            string input = "TLIST(A1)";
            string expected = "";
            string actual = ValueParserUtilities.GetTimeValValueString(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetTimeValValueStringTestListInputReturnsFirstEnclosedString()
        {
            string input = "TLIST(A1), \"9000\", \"9001\", \"9002\", \"9003\", \"9004\"";
            string expected = "9000";
            string actual = ValueParserUtilities.GetTimeValValueString(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetTimeValValueStringTestSingularInputReturnsString()
        {
            string input = "TLIST(A1, \"9001\")";
            string expected = "9001";
            string actual = ValueParserUtilities.GetTimeValValueString(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetTimeValueStringTestValidInputReturnsString()
        {
            string input = "TLIST(A1, \"9000-9001\")";
            string expected = "9000-9001";
            string actual = ValueParserUtilities.GetTimeValValueString(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetTimeValueStringTestInputWithTwoRangesReturnsFirstEnclosedString()
        {
            string input = "TLIST(A1, \"9000-9001\", \"9002-9003\")";
            string expected = "9000-9001";
            string actual = ValueParserUtilities.GetTimeValValueString(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetTimeValueStringTestInputWithRangeOfThreeValuesReturnsString()
        {
            string input = "TLIST(A1, \"9000-9001-9002\")";
            string expected = "9000-9001-9002";
            string actual = ValueParserUtilities.GetTimeValValueString(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetTimeValueStringTestInputRangeWithoutStringDelimetersReturnsEmptyString()
        {
            string input = "TLIST(A1, 9000-9001)";
            string expected = "";
            string actual = ValueParserUtilities.GetTimeValValueString(input);

            Assert.AreEqual(expected, actual);
        }
    }
}
