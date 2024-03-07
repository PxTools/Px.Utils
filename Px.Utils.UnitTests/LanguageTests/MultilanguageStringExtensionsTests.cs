using PxUtils.Language;

namespace LanguageTests
{
    [TestClass]
    public class MultilanguageStringExtensionsTests
    {
        [TestMethod]
        public void TestWithOneLanguage_ReturnsValue()
        {
            string testvalue = "test_value";
            var multilanguageString = new MultilanguageString("test_lang", testvalue);
            var result = multilanguageString.UniformValue();
            Assert.AreEqual(testvalue, result);
        }

        [TestMethod]
        public void TestWithTwoLanguageWithSameValue_ReturnsValue()
        {
            string testvalue = "test_value";
            var multilanguageString = new MultilanguageString([new("a", testvalue), new("b", testvalue)]);
            var result = multilanguageString.UniformValue();
            Assert.AreEqual(testvalue, result);
        }

        [TestMethod]
        public void TestWithOneEmptyValue_ReturnsValue()
        {
            string testvalue = "";
            var multilanguageString = new MultilanguageString("test_lang", testvalue);
            var result = multilanguageString.UniformValue();
            Assert.AreEqual(testvalue, result);
        }

        [TestMethod]
        public void TestWithTwoDifferentLanguages_ThrowsException()
        {
            var multilanguageString = new MultilanguageString([new("a", "test_value"), new("b", "different_value")]);
            Assert.ThrowsException<InvalidOperationException>(() => multilanguageString.UniformValue());
        }
    }
}
