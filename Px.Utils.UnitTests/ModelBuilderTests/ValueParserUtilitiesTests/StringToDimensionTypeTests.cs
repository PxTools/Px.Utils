using Px.Utils.ModelBuilders;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.PxFile;
using System.Globalization;

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
        public void ContentsTest()
        {
            string input = "Contents";
            DimensionType expected = DimensionType.Content;
            DimensionType actual = ValueParserUtilities.StringToDimensionType(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("RANKING", DimensionType.Ordinal)]
        [DataRow("region", DimensionType.Geographical)]
        public void CustomAliasTest(string alias, DimensionType expected)
        {
            string input = alias.ToUpper(CultureInfo.InvariantCulture); // Testing case-insensitivity
            PxFileConfiguration conf = PxFileConfiguration.Default;
            conf.Tokens.VariableTypes.Mappings[alias] = expected; 

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
