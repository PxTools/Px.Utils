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
            // Arrange
            string input = "TLIST(A1)";
            TimeDimensionInterval expected = TimeDimensionInterval.Year;
            
            // Act
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);
            
            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTimeIntervalFromTimeValTest_H1Input_ReturnsHalfYear()
        {
            // Arrange
            string input = "TLIST(H1)";
            TimeDimensionInterval expected = TimeDimensionInterval.HalfYear;
            
            // Act
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);
            
            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTimeIntervalFromTimeValTest_Q1Input_ReturnQuarter()
        {
            // Arrange
            string input = "TLIST(Q1)";
            TimeDimensionInterval expected = TimeDimensionInterval.Quarter;

            // Act
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            // Assert
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void ParseTimeIntervalFromTimeValTest_M1Input_ReturnsMonth()
        {
            // Arrange
            string input = "TLIST(M1)";
            TimeDimensionInterval expected = TimeDimensionInterval.Month;

            // Act
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTimeIntervalFromTimeValTest_W1Input_ReturnsWeek()
        {
            // Arrange
            string input = "TLIST(W1)";
            TimeDimensionInterval expected = TimeDimensionInterval.Week;

            // Act
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTimeIntervalFromTimeValTest_UnknownInteralValidInput_ReturnsOther()
        {
            // Arrange
            string input = "TLIST(F1)";
            TimeDimensionInterval expected = TimeDimensionInterval.Other;

            // Act
            TimeDimensionInterval actual = ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTimeIntervalFromTimeValTest_InvalidInput_ThrowsArgumentException()
        {
            // Arrange
            string input = "FOO";

            // Act
            void act() => ValueParserUtilities.ParseTimeIntervalFromTimeVal(input);

            // Assert
            Assert.ThrowsException<ArgumentException>(act);
        }
    }
}
