using Px.Utils.ModelBuilders;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.PxFile;

namespace Px.Utils.UnitTests.ModelBuilderTests.ValueParserUtilitiesTests
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
        public void ContentAliasTest()
        {
            string input = "Contents";
            DimensionType expected = DimensionType.Content;
            DimensionType actual = ValueParserUtilities.StringToDimensionType(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CustomAliasTest()
        {
            string input = "Ranking";
            PxFileConfiguration conf = PxFileConfiguration.Default;
            conf.Tokens.VariableTypes.Ordinal = ["Ordinal", "Ranking"];

            DimensionType expected = DimensionType.Ordinal;
            DimensionType actual = ValueParserUtilities.StringToDimensionType(input, conf);
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
