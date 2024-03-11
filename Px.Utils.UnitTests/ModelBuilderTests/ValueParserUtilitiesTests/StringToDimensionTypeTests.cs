using PxUtils.ModelBuilders;
using PxUtils.Models.Metadata.Enums;

namespace ModelBuilderTests.ValueParserUtilitiesTests
{
    [TestClass]
    public class StringToDimensionTypeTests
    {
        [TestMethod]
        public void TimeTest()
        {
            string input = "Time";
            DimensionType expected = DimensionType.Time;
            DimensionType actual = ValueParserUtilities.StringToDimensionType(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ContentTest()
        {
            string input = "Content";
            DimensionType expected = DimensionType.Content;
            DimensionType actual = ValueParserUtilities.StringToDimensionType(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DefaultValueTest()
        {
            string input = "abcd";
            DimensionType expected = DimensionType.Unknown;
            DimensionType actual = ValueParserUtilities.StringToDimensionType(input);
            Assert.AreEqual(expected, actual);
        }
    }
}
