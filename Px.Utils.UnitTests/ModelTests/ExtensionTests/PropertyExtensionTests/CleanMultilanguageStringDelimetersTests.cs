using Px.Utils.Language;
using Px.Utils.Models.Metadata.ExtensionMethods;

namespace Px.Utils.UnitTests.ModelTests.ExtensionTests.PropertyExtensionTests
{
    [TestClass]
    public class CleanMultilanguageStringDelimetersTests
    {
        [TestMethod]
        public void CleanMultilanguageStringDelimetersTestEmptyInputReturnsEmptyString()
        {
            MultilanguageString input = new("lang", "");
            MultilanguageString expected = new("lang", "");
            MultilanguageString actual = input.CleanStringDelimeters('"');

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CleanMultilanguageStringDelimetersTestOneLanguageStringReturnsCleanedString()
        {
            MultilanguageString input = new("lang", "\"foobar\"");
            MultilanguageString expected = new("lang", "foobar");
            MultilanguageString actual = input.CleanStringDelimeters('"');

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CleanMultilanguageStringDelimetersTestTwoLanguageStringReturnsCleanedStrings()
        {
            List<KeyValuePair<string, string>> inputTranslations =
            [
                new("lang1", "\"foobar1\""),
                new("lang2", "\"foobar2\"")
            ];

            List<KeyValuePair<string, string>> expectedTranslations =
            [
                new("lang1", "foobar1"),
                new("lang2", "foobar2")
            ];

            MultilanguageString input = new(inputTranslations);
            MultilanguageString expected = new(expectedTranslations);
            MultilanguageString actual = input.CleanStringDelimeters('"');

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CleanMultilanguageStringDelimetersTestTwoLanguageMultilineStringReturnsCleanedStrings()
        {
            List<KeyValuePair<string, string>> inputTranslations =
            [
                new("lang1", "\"foobar1\" \r\n \"barfoo1\""),
                new("lang2", "\"foobar2\" \r\n \"barfoo2\"")
            ];

            List<KeyValuePair<string, string>> expectedTranslations =
            [
                new("lang1", "foobar1barfoo1"),
                new("lang2", "foobar2barfoo2")
            ];

            MultilanguageString input = new(inputTranslations);
            MultilanguageString expected = new(expectedTranslations);
            MultilanguageString actual = input.CleanStringDelimeters('"');

            Assert.AreEqual(expected, actual);
        }
    }
}
