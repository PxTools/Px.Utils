using Px.Utils.Language;
using Px.Utils.Models.Metadata;

namespace ModelTests
{
    [TestClass]
    public class PropertyTests
    {
        [TestMethod]
        public void ConstructorTestWithSingleTranslationCreatesPropertyWithSingleTranslation()
        {
            // Arrange
            string keyWord = "keyWord";
            string lang = "lang";
            string text = "text";
            MultilanguageString multilanguageString = new(lang, text);

            // Act
            MetaProperty property = new(keyWord, multilanguageString);

            // Assert
            Assert.AreEqual(keyWord, property.KeyWord);
            Assert.IsTrue(property.CanGetStringValue);
            Assert.IsTrue(property.CanGetMultilanguageValue);
            Assert.AreEqual(text, property.GetRawValueString());
            Assert.AreEqual(multilanguageString, property.GetRawValueMultiLanguageString());
        }

        [TestMethod]
        public void ConstructorTestWithMultipleTranslationsCreatesPropertyWithMultipleTranslations()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text1"),
                new KeyValuePair<string, string>("lang2", "text2")
                ]);

            // Act
            MetaProperty property = new(keyWord, multilanguageString);

            // Assert
            Assert.AreEqual(keyWord, property.KeyWord);
            Assert.IsFalse(property.CanGetStringValue);
            Assert.IsTrue(property.CanGetMultilanguageValue);
            Assert.AreEqual(multilanguageString, property.GetRawValueMultiLanguageString());
        }

        [TestMethod]
        public void ConstructorTestWithMultipleTranslationsWithSameValueCreatesPropertyWithMultipleTranslations()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text"),
                new KeyValuePair<string, string>("lang2", "text")
                ]);

            // Act
            MetaProperty property = new(keyWord, multilanguageString);

            // Assert
            Assert.AreEqual(keyWord, property.KeyWord);
            Assert.IsTrue(property.CanGetStringValue);
            Assert.IsTrue(property.CanGetMultilanguageValue);
            Assert.AreEqual(multilanguageString, property.GetRawValueMultiLanguageString());
        }

        [TestMethod]
        public void ConstructorTestNoLanguageCreatesPropertyWithNoLanguage()
        {
            // Arrange
            string keyWord = "keyWord";
            string value = "test_value";

            // Act
            MetaProperty property = new(keyWord, value);

            // Assert
            Assert.AreEqual(keyWord, property.KeyWord);
            Assert.IsTrue(property.CanGetStringValue);
            Assert.IsFalse(property.CanGetMultilanguageValue);
        }

        [TestMethod]
        public void TryGetStringTestWithSingleTranslationReturnsTrueAndValue()
        {
            // Arrange
            string keyWord = "keyWord";
            string lang = "lang";
            string text = "text";
            MultilanguageString multilanguageString = new(lang, text);
            MetaProperty property = new(keyWord, multilanguageString);

            // Act
            bool result = property.TryGetRawValueString(out string? value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(text, value);
        }

        [TestMethod]
        public void TryGetStringTestWithMultipleTranslationsReturnsFalseAndNull()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text1"),
                new KeyValuePair<string, string>("lang2", "text2")
                ]);
            MetaProperty property = new(keyWord, multilanguageString);

            // Act
            bool result = property.TryGetRawValueString(out string? value);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(value);
        }

        [TestMethod]
        public void TryGetStringTestTranslationswithSameValuesReturnsTrueAndValue()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text"),
                new KeyValuePair<string, string>("lang2", "text")
                ]);
            MetaProperty property = new(keyWord, multilanguageString);

            // Act
            bool result = property.TryGetRawValueString(out string? value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("text", value);
        }

        [TestMethod]
        public void GetStringTestNoLanguageReturnValue()
        {
            // Arrange
            string keyWord = "keyWord";
            string value = "test_value";
            MetaProperty property = new(keyWord, value);

            // Act and assert
            Assert.AreEqual(value, property.GetRawValueString());
        }

        [TestMethod]
        public void GetStringTestWithSingleTranslationReturnValue()
        {
            // Arrange
            string keyWord = "keyWord";
            string lang = "lang";
            string text = "text";
            MultilanguageString multilanguageString = new(lang, text);
            MetaProperty property = new(keyWord, multilanguageString);

            // Act and assert
            Assert.AreEqual(text, property.GetRawValueString());
        }

        [TestMethod]
        public void GetStringTestWithMultipleTranslationsReturnsInvalidOperationException()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text1"),
                new KeyValuePair<string, string>("lang2", "text2")
                ]);
            MetaProperty property = new(keyWord, multilanguageString);

            // Act and assert
            Assert.ThrowsException<InvalidOperationException>(() => property.GetRawValueString());
        }

        [TestMethod]
        public void TryGetMultilanguageStringTestWithSingleTranslationReturnsTrueAndValue()
        {
            // Arrange
            string keyWord = "keyWord";
            string lang = "lang";
            string text = "text";
            MultilanguageString multilanguageString = new(lang, text);
            MetaProperty property = new(keyWord, multilanguageString);

            // Act
            bool result = property.TryGetRawValueMultilanguageString(out MultilanguageString? value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(multilanguageString, value);
        }

        [TestMethod]
        public void TryGetMultilanguageStringTestWithMultipleTranslationsReturnsTrueAndValue()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text1"),
                new KeyValuePair<string, string>("lang2", "text2")
                ]);
            MetaProperty property = new(keyWord, multilanguageString);

            // Act
            bool result = property.TryGetRawValueMultilanguageString(out MultilanguageString? value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(multilanguageString, value);
        }


        [TestMethod]
        public void TryGetMultilanguageStringTestTranslationsWithSameValuesReturnsTrueAndValue()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text"),
                new KeyValuePair<string, string>("lang2", "text")
                ]);
            MetaProperty property = new(keyWord, multilanguageString);

            // Act
            bool result = property.TryGetRawValueMultilanguageString(out MultilanguageString? value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(multilanguageString, value);
        }

        [TestMethod]
        public void TryGetMultilanguageStringTestNoLanguageReturnsFalseAndNull()
        {
            // Arrange
            string keyWord = "keyWord";
            string value = "test_value";
            MetaProperty property = new(keyWord, value);

            // Act
            bool result = property.TryGetRawValueMultilanguageString(out MultilanguageString? mls);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(mls);
        }

        [TestMethod]
        public void GetMultiLanguageStringTestWithSingleTranslationReturnsValue()
        {
            // Arrange
            string keyWord = "keyWord";
            string lang = "lang";
            string text = "text";
            MultilanguageString multilanguageString = new(lang, text);
            MetaProperty property = new(keyWord, multilanguageString);

            // Act and assert
            Assert.AreEqual(multilanguageString, property.GetRawValueMultiLanguageString());
        }

        [TestMethod]
        public void GetMultiLanguageStringTestWithMultipleTranslationsReturnsValue()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text1"),
                new KeyValuePair<string, string>("lang2", "text2")
                ]);
            MetaProperty property = new(keyWord, multilanguageString);

            // Act and assert
            Assert.AreEqual(multilanguageString, property.GetRawValueMultiLanguageString());
        }

        [TestMethod]
        public void GetMultiLanguageStringTestWithSingleTranslationThrowsInvalidOperationException()
        {
            // Arrange
            string keyWord = "keyWord";
            string text = "text";
            MetaProperty property = new(keyWord, text);

            // Act and assert
            Assert.ThrowsException<InvalidOperationException>(() => property.GetRawValueMultiLanguageString());
        }
    }
}
