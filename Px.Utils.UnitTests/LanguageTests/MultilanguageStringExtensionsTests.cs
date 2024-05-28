using Px.Utils.Language;

namespace LanguageTests
{
    [TestClass]
    public class MultilanguageStringExtensionsTests
    {
        [TestMethod]
        public void TestWithOneLanguageReturnsValue()
        {
            string testvalue = "test_value";
            var multilanguageString = new MultilanguageString("test_lang", testvalue);
            var result = multilanguageString.UniformValue();
            Assert.AreEqual(testvalue, result);
        }

        [TestMethod]
        public void TestWithTwoLanguageWithSameValueReturnsValue()
        {
            string testvalue = "test_value";
            var multilanguageString = new MultilanguageString([new("a", testvalue), new("b", testvalue)]);
            var result = multilanguageString.UniformValue();
            Assert.AreEqual(testvalue, result);
        }

        [TestMethod]
        public void TestWithOneEmptyValueReturnsValue()
        {
            string testvalue = "";
            var multilanguageString = new MultilanguageString("test_lang", testvalue);
            var result = multilanguageString.UniformValue();
            Assert.AreEqual(testvalue, result);
        }

        [TestMethod]
        public void TestWithTwoDifferentLanguagesThrowsException()
        {
            var multilanguageString = new MultilanguageString([new("a", "test_value"), new("b", "different_value")]);
            Assert.ThrowsException<InvalidOperationException>(() => multilanguageString.UniformValue());
        }
    }
}
