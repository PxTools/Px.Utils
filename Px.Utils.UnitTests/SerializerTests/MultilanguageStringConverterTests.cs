using Px.Utils.Language;
using System.Text.Json;

namespace Px.Utils.UnitTests.SerializerTests
{
    [TestClass]
    public class MultilanguageStringConverterTests
    {
        [TestMethod]
        public void MultilanguageStringWithSingleLanguage()
        {
            // Arrange
            string lang = "lang";
            string text = "text";
            MultilanguageString multilanguageString = new(lang, text);

            // Act
            string serialized = JsonSerializer.Serialize(multilanguageString);
            string expected = $"{{\"{lang}\":\"{text}\"}}";

            // Assert
            Assert.AreEqual(expected, serialized);
        }

        [TestMethod]
        public void MultilanguageStringWithThreeLanguages()
        {
            // Arrange
            KeyValuePair<string, string>[] languages =
            [
                new KeyValuePair<string, string>("lang1", "text1"),
                new KeyValuePair<string, string>("lang2", "text2"),
                new KeyValuePair<string, string>("lang3", "text3")
            ];

            MultilanguageString multilanguageString = new(languages);

            // Act
            string serialized = JsonSerializer.Serialize(multilanguageString);
            string expected = "{\"lang1\":\"text1\",\"lang2\":\"text2\",\"lang3\":\"text3\"}";

            // Assert
            Assert.AreEqual(expected, serialized);
        }

        [TestMethod]
        public void MultilanguageStringWithNoLanguages()
        {
            // Arrange
            MultilanguageString multilanguageString = new([]);

            // Act
            string serialized = JsonSerializer.Serialize(multilanguageString);
            string expected = "{}";

            // Assert
            Assert.AreEqual(expected, serialized);
        }

        [TestMethod]
        public void DeserializeMultilanguageStringWithSingleLanguage()
        {
            // Arrange
            string lang = "lang";
            string text = "text";
            string serialized = $"{{\"{lang}\":\"{text}\"}}";

            // Act
            MultilanguageString? deserialized = JsonSerializer.Deserialize<MultilanguageString>(serialized);

            // Assert
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(lang, deserialized.Languages.First());
            Assert.AreEqual(text, deserialized[lang]);
        }

        [TestMethod]
        public void DeserializeMultilanguageStringWithThreeLanguages()
        {
            // Arrange
            string serialized = "{\"lang1\":\"text1\",\"lang2\":\"text2\",\"lang3\":\"text3\"}";

            // Act
            MultilanguageString? deserialized = JsonSerializer.Deserialize<MultilanguageString>(serialized);

            // Assert
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(3, deserialized.Languages.Count());
            Assert.IsTrue(deserialized.Languages.Contains("lang1"));
            Assert.AreEqual("text1", deserialized["lang1"]);
            Assert.IsTrue(deserialized.Languages.Contains("lang2"));
            Assert.AreEqual("text2", deserialized["lang2"]);
            Assert.IsTrue(deserialized.Languages.Contains("lang3"));
            Assert.AreEqual("text3", deserialized["lang3"]);
        }

        [TestMethod]
        public void DeserializeMultilanguageStringWithNoLanguages()
        {
            // Arrange
            string serialized = "{}";

            // Act
            MultilanguageString? deserialized = JsonSerializer.Deserialize<MultilanguageString>(serialized);

            // Assert
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(0, deserialized.Languages.Count());
        }
    }
}
