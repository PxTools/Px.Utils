using Px.Utils.ModelBuilders;
using Px.Utils.Models.Metadata.Enums;

namespace ModelBuilderTests.ValueParserUtilitiesTests
{
    [TestClass]
    public class ParseTimeIntervalFromTimeValTests
    {
        [TestMethod]
        public void ParseTimeIntervalFromTimeValTestA1InputReturnsYear()
        {
            string input = "TLIST(A1)";
            TimeDimensionInterval expected = TimeDimensionInterval.Year;
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void ParseTimeIntervalFromTimeValTestA1WithRangeInputReturnsYear()
        {
            string input = "TLIST(A1, \"2000-2005\")";
            TimeDimensionInterval expected = TimeDimensionInterval.Year;
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTimeIntervalFromTimeValTestH1InputReturnsHalfYear()
        {
            string input = "TLIST(H1)";
            TimeDimensionInterval expected = TimeDimensionInterval.HalfYear;
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTimeIntervalFromTimeValTestQ1InputReturnQuarter()
        {
            string input = "TLIST(Q1)";
            TimeDimensionInterval expected = TimeDimensionInterval.Quarter;
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void ParseTimeIntervalFromTimeValTestM1InputReturnsMonth()
        {
            string input = "TLIST(M1)";
            TimeDimensionInterval expected = TimeDimensionInterval.Month;
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTimeIntervalFromTimeValTestW1InputReturnsWeek()
        {
            string input = "TLIST(W1)";
            TimeDimensionInterval expected = TimeDimensionInterval.Week;
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTimeIntervalFromTimeValTestUnknownInteralValidInputReturnsOther()
        {
            string input = "TLIST(F1)";
            TimeDimensionInterval expected = TimeDimensionInterval.Other;
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTimeIntervalFromTimeValTestInvalidInputThrowsArgumentException()
        {
            string input = "FOO";
            void act() => ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            Assert.ThrowsException<ArgumentException>(act);
        }
    }
}
