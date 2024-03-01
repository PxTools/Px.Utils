using PxUtils.Exceptions;
using PxUtils.Language;

namespace LanguageTests
{
    [TestClass]
    public class MultilanguageStringTests
    {
        [TestMethod]
        public void ConstructorTest_SingleTranslation()
        {
            MultilanguageString multilanguageString = new("en", "test_value");
            Assert.AreEqual("test_value", multilanguageString["en"]);
            Assert.AreEqual(1, multilanguageString.Languages.Count());
        }

        [TestMethod]
        public void ConstructorTest_MultipleTranslations()
        {
            MultilanguageString multilanguageString = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            Assert.AreEqual("test_value_a", multilanguageString["a"]);
            Assert.AreEqual("test_value_b", multilanguageString["b"]);
            Assert.AreEqual(2, multilanguageString.Languages.Count());
        }

        [TestMethod]
        public void ConstructorTest_CopyConstructor()
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
        public void AddTest_AddingNewTranslation()
        {
            MultilanguageString multilanguageString = new("a", "test_value_a");
            multilanguageString.Add("b", "test_value_b");
            Assert.AreEqual("test_value_a", multilanguageString["a"]);
            Assert.AreEqual("test_value_b", multilanguageString["b"]);
            Assert.AreEqual(2, multilanguageString.Languages.Count());
        }

        [TestMethod]
        public void AddTest_AddingExistingTranslation_Throws()
        {
            MultilanguageString multilanguageString = new("a", "test_value_a");
            Assert.ThrowsException<TranslationAlreadyDefinedException>(() => multilanguageString.Add("a", "test_value_a"));
        }

        [TestMethod]
        public void AddTest_AddingExistingTranslationWithDifferentValue_Throws()
        {
            MultilanguageString multilanguageString = new("a", "test_value_a");
            Assert.ThrowsException<TranslationAlreadyDefinedException>(() => multilanguageString.Add("a", "test_value_b"));
        }

        [TestMethod]
        public void AddTest_AddingMultipleNewTranslations()
        {
            MultilanguageString multilanguageString = new("a", "test_value_a");
            multilanguageString.Add([new("b", "test_value_b"), new("c", "test_value_c")]);
            Assert.AreEqual("test_value_a", multilanguageString["a"]);
            Assert.AreEqual("test_value_b", multilanguageString["b"]);
            Assert.AreEqual("test_value_c", multilanguageString["c"]);
            Assert.AreEqual(3, multilanguageString.Languages.Count());
        }

        [TestMethod]
        public void AddTest_AddingMultipleTranslationsWithExistingTranslation_Throws()
        {
            MultilanguageString multilanguageString = new("a", "test_value_a");
            Assert.ThrowsException<TranslationAlreadyDefinedException>(() => multilanguageString.Add([new("a", "test_value_a"), new("b", "test_value_b")]));
        }

        [TestMethod]
        public void AddTest_AddingMultipleTranslationsWithExistingTranslationWithDifferentValue_Throws()
        {
            MultilanguageString multilanguageString = new("a", "test_value_a");
            Assert.ThrowsException<TranslationAlreadyDefinedException>(() => multilanguageString.Add([new("a", "test_value_b"), new("b", "test_value_b")]));
        }

        [TestMethod]
        public void EditTest_EditExistingLanguage()
        {
            MultilanguageString multilanguageString = new("a", "test_value_a");
            multilanguageString.Edit("a", "test_value_b");
            Assert.AreEqual("test_value_b", multilanguageString["a"]);
        }

        [TestMethod]
        public void EditTest_EditNonExistingLanguage_Throws()
        {
            MultilanguageString multilanguageString = new("a", "test_value_a");
            Assert.ThrowsException<TranslationNotFoundException>(() => multilanguageString.Edit("b", "test_value_b"));
        }

        [TestMethod]
        public void EqualsTest_SameStringsAreEqual()
        {
            MultilanguageString first = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            MultilanguageString second = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            Assert.AreEqual(first, second);
        }

        [TestMethod]
        public void EqualsTest_DifferentStringsAreNotEqual()
        {
            MultilanguageString first = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            MultilanguageString second = new([new("c", "test_value_c"), new("d", "test_value_d")]);
            Assert.AreNotEqual(first, second);
        }

        [TestMethod]
        public void EqualsTest_NullIsNotEqual()
        {
            MultilanguageString first = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            Assert.AreNotEqual(null, first);
        }

        [TestMethod]
        public void GetHashCodeTest_DifferentStringsHaveDifferentHashes()
        {
            MultilanguageString first = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            MultilanguageString second= new([new("c", "test_value_c"), new("d", "test_value_d")]);
            Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode());
        }

        [TestMethod]
        public void GetHashCodeTest_SameStringsHaveSameHashes()
        {
            MultilanguageString first = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            MultilanguageString second = new([new("a", "test_value_a"), new("b", "test_value_b")]);
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        }
    }
}
