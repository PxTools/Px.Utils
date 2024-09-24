using Px.Utils.Language;
using Px.Utils.Models.Metadata.MetaProperties;

namespace Px.Utils.UnitTests.ModelTests
{
    [TestClass]
    public class PropertyConstructorTests
    {
        [TestMethod]
        public void StringProperty()
        {
            // Arrange
            string value = "value";

            // Act
            StringProperty property = new(value);

            // Assert
            Assert.AreEqual(value, property.Value);
        }

        [TestMethod]
        public void MultilanguageStringPropertyWithOneLanguage()
        {
            // Arrange
            string lang = "lang";
            string text = "text";
            MultilanguageString multilanguageString = new(lang, text);

            // Act
            MultilanguageStringProperty property = new(multilanguageString);

            // Assert
            Assert.AreEqual(multilanguageString, property.Value);
        }

        [TestMethod]
        public void MultilanguageStringWithMultipleLanguages()
        {
            // Arrange
            MultilanguageString multilanguageString = new([
                new KeyValuePair<string, string>("lang1", "text1"),
                new KeyValuePair<string, string>("lang2", "text2")
                ]);

            // Act
            MultilanguageStringProperty property = new(multilanguageString);

            // Assert
            Assert.AreEqual(multilanguageString, property.Value);
        }

        [TestMethod]
        public void BooleanProperty()
        {
            // Arrange
            bool value = true;

            // Act
            BooleanProperty property = new(value);

            // Assert
            Assert.AreEqual(value, property.Value);
        }

        [TestMethod]
        public void NumericProperty()
        {
            // Arrange
            double value = 1.0;

            // Act
            NumericProperty property = new(value);

            // Assert
            Assert.AreEqual(value, property.Value);
        }
    }
}
