using PxUtils.Language;
using PxUtils.Models.Metadata;

namespace ModelTests
{
    [TestClass]
    public class PropertyTests
    {
        [TestMethod]
        public void ConstructorTest_WithSingleTranslation_CreatesPropertyWithSingleTranslation()
        {
            // Arrange
            string keyWord = "keyWord";
            string lang = "lang";
            string text = "text";
            MultilanguageString multilanguageString = new(lang, text);

            // Act
            Property property = new(keyWord, multilanguageString);

            // Assert
            Assert.AreEqual(keyWord, property.KeyWord);
            Assert.IsTrue(property.CanGetStringValue);
            Assert.IsTrue(property.CanGetMultilanguageValue);
            Assert.AreEqual(text, property.GetRawValueString());
            Assert.AreEqual(multilanguageString, property.GetRawValueMultiLanguageString());
        }

        [TestMethod]
        public void ConstructorTest_WithMultipleTranslations_CreatesPropertyWithMultipleTranslations()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text1"),
                new KeyValuePair<string, string>("lang2", "text2")
                ]);

            // Act
            Property property = new(keyWord, multilanguageString);

            // Assert
            Assert.AreEqual(keyWord, property.KeyWord);
            Assert.IsFalse(property.CanGetStringValue);
            Assert.IsTrue(property.CanGetMultilanguageValue);
            Assert.AreEqual(multilanguageString, property.GetRawValueMultiLanguageString());
        }

        [TestMethod]
        public void ConstructorTest_WithMultipleTranslationsWithSameValue_CreatesPropertyWithMultipleTranslations()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text"),
                new KeyValuePair<string, string>("lang2", "text")
                ]);

            // Act
            Property property = new(keyWord, multilanguageString);

            // Assert
            Assert.AreEqual(keyWord, property.KeyWord);
            Assert.IsTrue(property.CanGetStringValue);
            Assert.IsTrue(property.CanGetMultilanguageValue);
            Assert.AreEqual(multilanguageString, property.GetRawValueMultiLanguageString());
        }

        [TestMethod]
        public void ConstructorTest_NoLanguage_CreatesPropertyWithNoLanguage()
        {
            // Arrange
            string keyWord = "keyWord";
            string value = "test_value";

            // Act
            Property property = new(keyWord, value);

            // Assert
            Assert.AreEqual(keyWord, property.KeyWord);
            Assert.IsTrue(property.CanGetStringValue);
            Assert.IsFalse(property.CanGetMultilanguageValue);
        }

        [TestMethod]
        public void TryGetStringTest_WithSingleTranslation_ReturnsTrueAndValue()
        {
            // Arrange
            string keyWord = "keyWord";
            string lang = "lang";
            string text = "text";
            MultilanguageString multilanguageString = new(lang, text);
            Property property = new(keyWord, multilanguageString);

            // Act
            bool result = property.TryGetRawValueString(out string? value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(text, value);
        }

        [TestMethod]
        public void TryGetStringTest_WithMultipleTranslations_ReturnsFalseAndNull()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text1"),
                new KeyValuePair<string, string>("lang2", "text2")
                ]);
            Property property = new(keyWord, multilanguageString);

            // Act
            bool result = property.TryGetRawValueString(out string? value);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(value);
        }

        [TestMethod]
        public void TryGetStringTest_TranslationswithSameValues_ReturnsTrueAndValue()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text"),
                new KeyValuePair<string, string>("lang2", "text")
                ]);
            Property property = new(keyWord, multilanguageString);

            // Act
            bool result = property.TryGetRawValueString(out string? value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("text", value);
        }

        [TestMethod]
        public void GetStringTest_NoLanguage_ReturnValue()
        {
            // Arrange
            string keyWord = "keyWord";
            string value = "test_value";
            Property property = new(keyWord, value);

            // Act and assert
            Assert.AreEqual(value, property.GetRawValueString());
        }

        [TestMethod]
        public void GetStringTest_WithSingleTranslation_ReturnValue()
        {
            // Arrange
            string keyWord = "keyWord";
            string lang = "lang";
            string text = "text";
            MultilanguageString multilanguageString = new(lang, text);
            Property property = new(keyWord, multilanguageString);

            // Act and assert
            Assert.AreEqual(text, property.GetRawValueString());
        }

        [TestMethod]
        public void GetStringTest_WithMultipleTranslations_ReturnsInvalidOperationException()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text1"),
                new KeyValuePair<string, string>("lang2", "text2")
                ]);
            Property property = new(keyWord, multilanguageString);

            // Act and assert
            Assert.ThrowsException<InvalidOperationException>(() => property.GetRawValueString());
        }

        [TestMethod]
        public void TryGetMultilanguageStringTest_WithSingleTranslation_ReturnsTrueAndValue()
        {
            // Arrange
            string keyWord = "keyWord";
            string lang = "lang";
            string text = "text";
            MultilanguageString multilanguageString = new(lang, text);
            Property property = new(keyWord, multilanguageString);

            // Act
            bool result = property.TryGetRawValueMultilanguageString(out MultilanguageString? value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(multilanguageString, value);
        }

        [TestMethod]
        public void TryGetMultilanguageStringTest_WithMultipleTranslations_ReturnsTrueAndValue()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text1"),
                new KeyValuePair<string, string>("lang2", "text2")
                ]);
            Property property = new(keyWord, multilanguageString);

            // Act
            bool result = property.TryGetRawValueMultilanguageString(out MultilanguageString? value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(multilanguageString, value);
        }


        [TestMethod]
        public void TryGetMultilanguageStringTest_TranslationsWithSameValues_ReturnsTrueAndValue()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text"),
                new KeyValuePair<string, string>("lang2", "text")
                ]);
            Property property = new(keyWord, multilanguageString);

            // Act
            bool result = property.TryGetRawValueMultilanguageString(out MultilanguageString? value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(multilanguageString, value);
        }

        [TestMethod]
        public void TryGetMultilanguageStringTest_NoLanguage_ReturnsFalseAndNull()
        {
            // Arrange
            string keyWord = "keyWord";
            string value = "test_value";
            Property property = new(keyWord, value);

            // Act
            bool result = property.TryGetRawValueMultilanguageString(out MultilanguageString? mls);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(mls);
        }

        [TestMethod]
        public void GetMultiLanguageStringTest_WithSingleTranslation_ReturnsValue()
        {
            // Arrange
            string keyWord = "keyWord";
            string lang = "lang";
            string text = "text";
            MultilanguageString multilanguageString = new(lang, text);
            Property property = new(keyWord, multilanguageString);

            // Act and assert
            Assert.AreEqual(multilanguageString, property.GetRawValueMultiLanguageString());
        }

        [TestMethod]
        public void GetMultiLanguageStringTest_WithMultipleTranslations_ReturnsValue()
        {
            // Arrange
            string keyWord = "keyWord";
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text1"),
                new KeyValuePair<string, string>("lang2", "text2")
                ]);
            Property property = new(keyWord, multilanguageString);

            // Act and assert
            Assert.AreEqual(multilanguageString, property.GetRawValueMultiLanguageString());
        }

        [TestMethod]
        public void GetMultiLanguageStringTest_WithSingleTranslation_ThrwosInvalidOperationException()
        {
            // Arrange
            string keyWord = "keyWord";
            string text = "text";
            Property property = new(keyWord, text);

            // Act and assert
            Assert.ThrowsException<InvalidOperationException>(() => property.GetRawValueMultiLanguageString());
        }
    }
}
