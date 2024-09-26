using Px.Utils.Language;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Models.Metadata.MetaProperties;
using System.Text.Json;

namespace Px.Utils.UnitTests.SerializerTests
{
    [TestClass]
    public class MetaPropertyConverterTests
    {

        #region MultilanguageStringListProperty

        [TestMethod]
        public void SerializeMultilanguageStringListProperty()
        {
            // Arrange
            KeyValuePair<string, string>[] languages =
            [
                new("en", "Hello"),
                new("fr", "Bonjour"),
                new("es", "Hola")
            ];
            List<MultilanguageString> values =
            [
                new(languages),
                new("de", "Guten Tag")
            ];
            MultilanguageStringListProperty property = new(values);

            // Act
            string json = JsonSerializer.Serialize(property);

            // Assert
            Assert.AreEqual("{\"Type\":\"MultilanguageTextArray\",\"Value\":[{\"en\":\"Hello\",\"fr\":\"Bonjour\",\"es\":\"Hola\"},{\"de\":\"Guten Tag\"}]}", json);
        }

        [TestMethod]
        public void DeserializeMultilanguageStringListProperty()
        {
            // Arrange
            string json = "{\"Type\":\"MultilanguageTextArray\",\"Value\":[{\"en\":\"Hello\",\"fr\":\"Bonjour\",\"es\":\"Hola\"},{\"de\":\"Guten Tag\"}]}";

            // Act
            MultilanguageStringListProperty? property = JsonSerializer.Deserialize<MultilanguageStringListProperty>(json);

            // Assert
            Assert.IsNotNull(property);
            Assert.AreEqual(MetaPropertyType.MultilanguageTextArray, property.Type);
            Assert.AreEqual(2, property.Value.Count);
            Assert.AreEqual("Hello", property.Value[0]["en"]);
            Assert.AreEqual("Bonjour", property.Value[0]["fr"]);
            Assert.AreEqual("Hola", property.Value[0]["es"]);
            Assert.AreEqual("Guten Tag", property.Value[1]["de"]);
        }

        [TestMethod]
        public void SerializeAndDeserializeMultilanguageStringListWithCustomOptions()
        {
            // Arrange
            KeyValuePair<string, string>[] languages =
            [
                new("en", "Hello"),
                new("fr", "Bonjour"),
                new("es", "Hola")
            ];
            List<MultilanguageString> values =
            [
                new(languages),
                new("de", "Guten Tag")
            ];
            MultilanguageStringListProperty property = new(values);
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            JsonSerializerOptions options = jsonSerializerOptions;

            // Act
            string json = JsonSerializer.Serialize(property, options);
            MultilanguageStringListProperty? deserializedProperty = JsonSerializer.Deserialize<MultilanguageStringListProperty>(json, options);

            // Assert
            Assert.IsNotNull(deserializedProperty);
            Assert.AreEqual(MetaPropertyType.MultilanguageTextArray, deserializedProperty.Type);
            Assert.AreEqual(2, deserializedProperty.Value.Count);
            Assert.AreEqual("Hello", deserializedProperty.Value[0]["en"]);
            Assert.AreEqual("Bonjour", deserializedProperty.Value[0]["fr"]);
            Assert.AreEqual("Hola", deserializedProperty.Value[0]["es"]);
            Assert.AreEqual("Guten Tag", deserializedProperty.Value[1]["de"]);
        }

        #endregion

        #region MultilanguageStringProperty

        [TestMethod]
        public void SerializeMultilanguageStringProperty()
        {
            // Arrange
            MultilanguageString value = new("en", "Hello");
            MultilanguageStringProperty property = new(value);

            // Act
            string json = JsonSerializer.Serialize(property);

            // Assert
            Assert.AreEqual("{\"Type\":\"MultilanguageText\",\"Value\":{\"en\":\"Hello\"}}", json);
        }

        [TestMethod]
        public void DeserializeMultilanguageStringProperty()
        {
            // Arrange
            string json = "{\"Type\":\"MultilanguageText\",\"Value\":{\"en\":\"Hello\"}}";

            // Act
            MultilanguageStringProperty? property = JsonSerializer.Deserialize<MultilanguageStringProperty>(json);

            // Assert
            Assert.IsNotNull(property);
            Assert.AreEqual(MetaPropertyType.MultilanguageText, property.Type);
            Assert.AreEqual("Hello", property.Value["en"]);
        }

        [TestMethod]
        public void SerializeMultilanguageStringPropertyZeroLanguages()
        {
            // Arrange
            MultilanguageString value = new([]);
            MultilanguageStringProperty property = new(value);

            // Act
            string json = JsonSerializer.Serialize(property);

            // Assert
            Assert.AreEqual("{\"Type\":\"MultilanguageText\",\"Value\":{}}", json);
        }

        [TestMethod]
        public void SerializeMultilanguageStringPropertyThreeLanguages()
        {
            // Arrange
            KeyValuePair<string, string>[] languages =
            [
                new("en", "Hello"),
                new("fr", "Bonjour"),
                new("es", "Hola")
            ];
            MultilanguageString value = new(languages);
            MultilanguageStringProperty property = new(value);

            // Act
            string json = JsonSerializer.Serialize(property);

            // Assert
            Assert.AreEqual("{\"Type\":\"MultilanguageText\",\"Value\":{\"en\":\"Hello\",\"fr\":\"Bonjour\",\"es\":\"Hola\"}}", json);
        }

        [TestMethod]
        public void SerializeAndDeserializeMultilanguageStringWithCustomOptions()
        {
            // Arrange
            MultilanguageString value = new("en", "Hello");
            MultilanguageStringProperty property = new(value);
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            JsonSerializerOptions options = jsonSerializerOptions;

            // Act
            string json = JsonSerializer.Serialize(property, options);
            MultilanguageStringProperty? deserializedProperty = JsonSerializer.Deserialize<MultilanguageStringProperty>(json, options);

            // Assert
            Assert.IsNotNull(deserializedProperty);
            Assert.AreEqual(MetaPropertyType.MultilanguageText, deserializedProperty.Type);
            Assert.AreEqual("Hello", deserializedProperty.Value["en"]);
        }

        #endregion

        #region StringListProperty

        [TestMethod]
        public void SerializeStringListProperty()
        {
            // Arrange
            List<string> values = ["Hello", "World", "!"];
            StringListProperty property = new(values);

            // Act
            string json = JsonSerializer.Serialize(property);

            // Assert
            Assert.AreEqual("{\"Type\":\"TextArray\",\"Value\":[\"Hello\",\"World\",\"!\"]}", json);
        }

        [TestMethod]
        public void DeserializeStringListProperty()
        {
            // Arrange
            string json = "{\"Value\":[\"Hello\",\"World\",\"!\"],\"Type\":\"TextArray\"}";

            // Act
            StringListProperty? property = JsonSerializer.Deserialize<StringListProperty>(json);

            // Assert
            Assert.IsNotNull(property);
            Assert.AreEqual(MetaPropertyType.TextArray, property.Type);
            Assert.AreEqual(3, property.Value.Count);
            Assert.AreEqual("Hello", property.Value[0]);
            Assert.AreEqual("World", property.Value[1]);
            Assert.AreEqual("!", property.Value[2]);
        }

        [TestMethod]
        public void SerializeAndDeserializeStringListWithCustomOptions()
        {
            // Arrange
            List<string> values = ["Hello", "World", "!"];
            StringListProperty property = new(values);
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            JsonSerializerOptions options = jsonSerializerOptions;

            // Act
            string json = JsonSerializer.Serialize(property, options);
            StringListProperty? deserializedProperty = JsonSerializer.Deserialize<StringListProperty>(json, options);

            // Assert
            Assert.IsNotNull(deserializedProperty);
            Assert.AreEqual(MetaPropertyType.TextArray, deserializedProperty.Type);
            Assert.AreEqual(3, deserializedProperty.Value.Count);
            Assert.AreEqual("Hello", deserializedProperty.Value[0]);
            Assert.AreEqual("World", deserializedProperty.Value[1]);
            Assert.AreEqual("!", deserializedProperty.Value[2]);
        }

        #endregion

        #region StringProperty

        [TestMethod]
        public void SerializeStringProperty()
        {
            // Arrange
            string value = "Hello, World!";
            StringProperty property = new StringProperty(value);

            // Act
            string json = JsonSerializer.Serialize(property);

            // Assert
            Assert.AreEqual("{\"Type\":\"Text\",\"Value\":\"Hello, World!\"}", json);
        }

        [TestMethod]
        public void DeserializeStringProperty()
        {
            // Arrange
            string json = "{\"Type\":\"Text\",\"Value\":\"Hello, World!\"}";

            // Act
            StringProperty? property = JsonSerializer.Deserialize<StringProperty>(json);

            // Assert
            Assert.IsNotNull(property);
            Assert.AreEqual(MetaPropertyType.Text, property.Type);
            Assert.AreEqual("Hello, World!", property.Value);
        }

        [TestMethod]
        public void SerializeAndDeserializeStringWithCustomOptions()
        {
            // Arrange
            string value = "Hello, World!";
            StringProperty property = new StringProperty(value);
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            JsonSerializerOptions options = jsonSerializerOptions;

            // Act
            string json = JsonSerializer.Serialize(property, options);
            StringProperty? deserializedProperty = JsonSerializer.Deserialize<StringProperty>(json, options);

            // Assert
            Assert.IsNotNull(deserializedProperty);
            Assert.AreEqual(MetaPropertyType.Text, deserializedProperty.Type);
            Assert.AreEqual("Hello, World!", deserializedProperty.Value);
        }

        #endregion

        #region NumericProperty

        [TestMethod]
        public void SerializeNumericProperty()
        {
            // Arrange
            double value = 42.0;
            NumericProperty property = new(value);

            // Act
            string json = JsonSerializer.Serialize(property);

            // Assert
            Assert.AreEqual("{\"Type\":\"Numeric\",\"Value\":42}", json);
        }

        [TestMethod]
        public void SerializeNumericPropertyWithDecimals()
        {
            // Arrange
            double value = 42.123;
            NumericProperty property = new(value);

            // Act
            string json = JsonSerializer.Serialize(property);

            // Assert
            Assert.AreEqual("{\"Type\":\"Numeric\",\"Value\":42.123}", json);
        }

        [TestMethod]
        public void DeserializeNumericProperty()
        {
            // Arrange
            string json = "{\"Type\":\"Numeric\",\"Value\":42}";

            // Act
            NumericProperty? property = JsonSerializer.Deserialize<NumericProperty>(json);

            // Assert
            Assert.IsNotNull(property);
            Assert.AreEqual(MetaPropertyType.Numeric, property.Type);
            Assert.AreEqual(42.0, property.Value);
        }

        [TestMethod]
        public void DeserializeNumericPropertyWithDecimals()
        {
            // Arrange
            string json = "{\"Type\":\"Numeric\",\"Value\":42.123}";

            // Act
            NumericProperty? property = JsonSerializer.Deserialize<NumericProperty>(json);

            // Assert
            Assert.IsNotNull(property);
            Assert.AreEqual(MetaPropertyType.Numeric, property.Type);
            Assert.AreEqual(42.123, property.Value);
        }

        [TestMethod]
        public void SerializeAndDeserializeNumericWithCustomOptions()
        {
            // Arrange
            double value = 42.0;
            NumericProperty property = new(value);
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            JsonSerializerOptions options = jsonSerializerOptions;

            // Act
            string json = JsonSerializer.Serialize(property, options);
            NumericProperty? deserializedProperty = JsonSerializer.Deserialize<NumericProperty>(json, options);

            // Assert
            Assert.IsNotNull(deserializedProperty);
            Assert.AreEqual(MetaPropertyType.Numeric, deserializedProperty.Type);
            Assert.AreEqual(42.0, deserializedProperty.Value);
        }

        #endregion

        #region BooleanProperty

        [TestMethod]
        public void SerializeBooleanProperty()
        {
            // Arrange
            bool value = true;
            BooleanProperty property = new(value);

            // Act
            string json = JsonSerializer.Serialize(property);

            // Assert
            Assert.AreEqual("{\"Type\":\"Boolean\",\"Value\":true}", json);
        }

        [TestMethod]
        public void DeserializeBooleanProperty()
        {
            // Arrange
            string json = "{\"Type\":\"Boolean\",\"Value\":true}";

            // Act
            BooleanProperty? property = JsonSerializer.Deserialize<BooleanProperty>(json);

            // Assert
            Assert.IsNotNull(property);
            Assert.AreEqual(MetaPropertyType.Boolean, property.Type);
            Assert.IsTrue(property.Value);
        }

        [TestMethod]
        public void SerializeAndDeserializeBooleanWithCustomOptions()
        {
            // Arrange
            bool value = true;
            BooleanProperty property = new(value);
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            JsonSerializerOptions options = jsonSerializerOptions;

            // Act
            string json = JsonSerializer.Serialize(property, options);
            BooleanProperty? deserializedProperty = JsonSerializer.Deserialize<BooleanProperty>(json, options);

            // Assert
            Assert.IsNotNull(deserializedProperty);
            Assert.AreEqual(MetaPropertyType.Boolean, deserializedProperty.Type);
            Assert.IsTrue(deserializedProperty.Value);
        }

        #endregion
    }
}
