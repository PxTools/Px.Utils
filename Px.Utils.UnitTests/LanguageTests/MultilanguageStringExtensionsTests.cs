using Px.Utils.Language;

namespace Px.Utils.UnitTests.LanguageTests
{
    [TestClass]
    public class MultilanguageStringExtensionsTests
    {
        [TestMethod]
        public void TestWithOneLanguageReturnsValue()
        {
            string testvalue = "test_value";
            MultilanguageString multilanguageString = new("test_lang", testvalue);
            string result = multilanguageString.UniformValue();
            Assert.AreEqual(testvalue, result);
        }

        [TestMethod]
        public void TestWithTwoLanguageWithSameValueReturnsValue()
        {
            string testvalue = "test_value";
            MultilanguageString multilanguageString = new([new("a", testvalue), new("b", testvalue)]);
            string result = multilanguageString.UniformValue();
            Assert.AreEqual(testvalue, result);
        }

        [TestMethod]
        public void TestWithOneEmptyValueReturnsValue()
        {
            string testvalue = "";
            MultilanguageString multilanguageString = new("test_lang", testvalue);
            string result = multilanguageString.UniformValue();
            Assert.AreEqual(testvalue, result);
        }

        [TestMethod]
        public void TestWithTwoDifferentLanguagesThrowsException()
        {
            MultilanguageString multilanguageString = new([new("a", "test_value"), new("b", "different_value")]);
            Assert.ThrowsExactly<InvalidOperationException>(() => multilanguageString.UniformValue());
        }
    }
}
