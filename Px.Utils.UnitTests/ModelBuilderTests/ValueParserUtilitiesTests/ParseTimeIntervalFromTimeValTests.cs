using PxUtils.ModelBuilders;
using PxUtils.Models.Metadata.Enums;

namespace ModelBuilderTests.ValueParserUtilitiesTests
{
    [TestClass]
    public class ParseTimeIntervalFromTimeValTests
    {
        [TestMethod]
        public void ParseTimeIntervalFromTimeValTest_A1Input_ReturnsYear()
        {
            string input = "TLIST(A1)";
            TimeDimensionInterval expected = TimeDimensionInterval.Year;
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTimeIntervalFromTimeValTest_H1Input_ReturnsHalfYear()
        {
            string input = "TLIST(H1)";
            TimeDimensionInterval expected = TimeDimensionInterval.HalfYear;
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTimeIntervalFromTimeValTest_Q1Input_ReturnQuarter()
        {
            string input = "TLIST(Q1)";
            TimeDimensionInterval expected = TimeDimensionInterval.Quarter;
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void ParseTimeIntervalFromTimeValTest_M1Input_ReturnsMonth()
        {
            string input = "TLIST(M1)";
            TimeDimensionInterval expected = TimeDimensionInterval.Month;
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTimeIntervalFromTimeValTest_W1Input_ReturnsWeek()
        {
            string input = "TLIST(W1)";
            TimeDimensionInterval expected = TimeDimensionInterval.Week;
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTimeIntervalFromTimeValTest_UnknownInteralValidInput_ReturnsOther()
        {
            string input = "TLIST(F1)";
            TimeDimensionInterval expected = TimeDimensionInterval.Other;
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTimeIntervalFromTimeValTest_InvalidInput_ThrowsArgumentException()
        {
            string input = "FOO";
            void act() => ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            Assert.ThrowsException<ArgumentException>(act);
        }
    }
}
