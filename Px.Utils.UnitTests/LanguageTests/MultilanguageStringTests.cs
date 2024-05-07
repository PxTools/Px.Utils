using PxUtils.Exceptions;
using PxUtils.Language;

namespace LanguageTests
{
    [TestClass]
    public class MultilanguageStringTests
    {
        [TestMethod]
        public void ConstructorTestSingleTranslation()
        {
            MultilanguageString multilanguageString = new("en", "test_value");
            Assert.AreEqual("test_value", multilanguageString["en"]);
            Assert.AreEqual(1, multilanguageString.Languages.Count());
        }

        [TestMethod]
        public void ConstructorTestMultipleTranslations()
        {
            MultilanguageString multilanguageString = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            Assert.AreEqual("test_value_a", multilanguageString["a"]);
            Assert.AreEqual("test_value_b", multilanguageString["b"]);
            Assert.AreEqual(2, multilanguageString.Languages.Count());
        }

        [TestMethod]
        public void ConstructorTestCopyConstructor()
        {
            MultilanguageString original = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            MultilanguageString copy = new(original);
            Assert.AreEqual("test_value_a", copy["a"]);
            Assert.AreEqual("test_value_b", copy["b"]);
            Assert.AreEqual(2, copy.Languages.Count());
        }

        [TestMethod]
        public void LanguagesTest()
        {
            MultilanguageString multilanguageString = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            CollectionAssert.AreEquivalent(new List<string>() { "a", "b" }, multilanguageString.Languages.ToList());
        }

        [TestMethod]
        public void CopyAndAddTestAddingNewTranslation()
        {
            MultilanguageString multilanguageString = new("a", "test_value_a");
            MultilanguageString copy = multilanguageString.CopyAndAdd("b", "test_value_b");
            Assert.AreEqual("test_value_a", copy["a"]);
            Assert.AreEqual("test_value_b", copy["b"]);
            Assert.AreEqual(2, copy.Languages.Count());

            // Test that the original multilanguage string is not modified
            Assert.AreEqual("test_value_a", multilanguageString["a"]);
            Assert.AreEqual(1, multilanguageString.Languages.Count());
        }

        [TestMethod]
        public void CopyAndAddTestAddingExistingTranslationThrows()
        {
            MultilanguageString multilanguageString = new("a", "test_value_a");
            Assert.ThrowsException<TranslationAlreadyDefinedException>(() => multilanguageString.CopyAndAdd("a", "test_value_a"));

            // Test that the original multilanguage string is not modified
            Assert.AreEqual("test_value_a", multilanguageString["a"]);
            Assert.AreEqual(1, multilanguageString.Languages.Count());
        }

        [TestMethod]
        public void CopyAndAddTestAddingExistingTranslationWithDifferentValueThrows()
        {
            MultilanguageString multilanguageString = new("a", "test_value_a");
            Assert.ThrowsException<TranslationAlreadyDefinedException>(() => multilanguageString.CopyAndAdd("a", "test_value_b"));

            // Test that the original multilanguage string is not modified
            Assert.AreEqual("test_value_a", multilanguageString["a"]);
            Assert.AreEqual(1, multilanguageString.Languages.Count());
        }

        [TestMethod]
        public void CopyAndAddTestAddingMultipleNewTranslations()
        {
            MultilanguageString multilanguageString = new("a", "test_value_a");
            MultilanguageString copy = multilanguageString.CopyAndAdd([new("b", "test_value_b"), new("c", "test_value_c")]);
            Assert.AreEqual("test_value_a", copy["a"]);
            Assert.AreEqual("test_value_b", copy["b"]);
            Assert.AreEqual("test_value_c", copy["c"]);
            Assert.AreEqual(3, copy.Languages.Count());

            // Test that the original multilanguage string is not modified
            Assert.AreEqual("test_value_a", multilanguageString["a"]);
            Assert.AreEqual(1, multilanguageString.Languages.Count());
        }

        [TestMethod]
        public void CopyAndAddTestAddingMultipleTranslationsWithExistingTranslationThrows()
        {
            MultilanguageString multilanguageString = new("a", "test_value_a");
            Assert.ThrowsException<TranslationAlreadyDefinedException>(() => multilanguageString.CopyAndAdd([new("a", "test_value_a"), new("b", "test_value_b")]));

            // Test that the original multilanguage string is not modified
            Assert.AreEqual("test_value_a", multilanguageString["a"]);
            Assert.AreEqual(1, multilanguageString.Languages.Count());
        }

        [TestMethod]
        public void CopyAndAddTestAddingMultipleTranslationsWithExistingTranslationWithDifferentValueThrows()
        {
            MultilanguageString multilanguageString = new("a", "test_value_a");
            Assert.ThrowsException<TranslationAlreadyDefinedException>(() => multilanguageString.CopyAndAdd([new("a", "test_value_b"), new("b", "test_value_b")]));

            // Test that the original multilanguage string is not modified
            Assert.AreEqual("test_value_a", multilanguageString["a"]);
            Assert.AreEqual(1, multilanguageString.Languages.Count());
        }

        [TestMethod]
        public void CopyAndReplaceEditExistingLanguage()
        {
            MultilanguageString multilanguageString = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            MultilanguageString copy = multilanguageString.CopyAndReplace("a", "test_value_aa");
            Assert.AreEqual("test_value_aa", copy["a"]);
            Assert.AreEqual("test_value_b", copy["b"]);
            Assert.AreEqual(2, copy.Languages.Count());

            // Test that the original multilanguage string is not modified
            Assert.AreEqual("test_value_a", multilanguageString["a"]);
            Assert.AreEqual("test_value_b", multilanguageString["b"]);
            Assert.AreEqual(2, multilanguageString.Languages.Count());

        }

        [TestMethod]
        public void CopyAndReplaceEditNonExistingLanguageThrows()
        {
            MultilanguageString multilanguageString = new("a", "test_value_a");
            Assert.ThrowsException<TranslationNotFoundException>(() => multilanguageString.CopyAndReplace("b", "test_value_aa"));

            // Test that the original multilanguage string is not modified
            Assert.AreEqual("test_value_a", multilanguageString["a"]);
            Assert.AreEqual(1, multilanguageString.Languages.Count());
        }

        [TestMethod]
        public void CopyAndEditAllReturnsEditedMultilanguageString()
        {
            MultilanguageString multilanguageString = new([new("a", "test_value_a"), new("b", "test_value_b"), new("c", "test_value_c")]);
            MultilanguageString copy = multilanguageString.CopyAndEditAll(s => s.Replace("test", "abcd"));
            Assert.AreEqual(3, copy.Languages.Count());
            Assert.AreEqual("abcd_value_a", copy["a"]);
            Assert.AreEqual("abcd_value_b", copy["b"]);
            Assert.AreEqual("abcd_value_c", copy["c"]);

            // Test that the original multilanguage string is not modified
            Assert.AreEqual("test_value_a", multilanguageString["a"]);
            Assert.AreEqual("test_value_b", multilanguageString["b"]);
            Assert.AreEqual("test_value_c", multilanguageString["c"]);
            Assert.AreEqual(3, multilanguageString.Languages.Count());
        }

        [TestMethod]
        public void EqualsTestSameStringsAreEqual()
        {
            MultilanguageString first = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            MultilanguageString second = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            Assert.AreEqual(first, second);
        }

        [TestMethod]
        public void EqualsTestDifferentStringsAreNotEqual()
        {
            MultilanguageString first = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            MultilanguageString second = new([new("c", "test_value_c"), new("d", "test_value_d")]);
            Assert.AreNotEqual(first, second);
        }

        [TestMethod]
        public void EqualsTestNullIsNotEqual()
        {
            MultilanguageString first = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            Assert.AreNotEqual(null, first);
        }

        [TestMethod]
        public void GetHashCodeTestDifferentStringsHaveDifferentHashes()
        {
            MultilanguageString first = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            MultilanguageString second = new([new("c", "test_value_c"), new("d", "test_value_d")]);
            Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode());
        }

        [TestMethod]
        public void GetHashCodeTestSameStringsHaveSameHashes()
        {
            MultilanguageString first = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            MultilanguageString second = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        }
    }
}
