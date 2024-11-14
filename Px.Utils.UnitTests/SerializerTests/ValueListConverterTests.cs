using Px.Utils.Language;
using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Models.Metadata.MetaProperties;
using System.Text.Json;

namespace Px.Utils.UnitTests.SerializerTests
{
    [TestClass]
    public class ValueListConverterTests
    {
        [TestMethod]
        public void SerializationTestWithNoVariables()
        {
            // Arrange
            ValueList valueList = new([]);
            string expected = "[]";

            // Act
            string actual = JsonSerializer.Serialize(valueList);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SerializationTestWithOneVariable()
        {
            // Arrange
            Dictionary<string, MetaProperty> additionalProperties = new()
            {
                { "key", new StringProperty("foobar")}
            };
            ValueList valueList = new([new("code", new MultilanguageString("lang", "name"), false, additionalProperties)]);
            string expected = "[{\"Code\":\"code\",\"Name\":{\"lang\":\"name\"},\"AdditionalProperties\":{\"key\":{\"Type\":\"Text\",\"Value\":\"foobar\"}},\"IsVirtual\":false}]";

            // Act
            string actual = JsonSerializer.Serialize(valueList);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SerializationTestWithTwoVariables()
        {
            // Arrange
            ValueList valueList = new(
            [
                new("code1", new MultilanguageString("lang", "name1")),
                new("code2", new MultilanguageString("lang", "name2"))
            ]);
            string expected = "[{\"Code\":\"code1\",\"Name\":{\"lang\":\"name1\"},\"AdditionalProperties\":{},\"IsVirtual\":false},{\"Code\":\"code2\",\"Name\":{\"lang\":\"name2\"},\"AdditionalProperties\":{},\"IsVirtual\":false}]";

            // Act
            string actual = JsonSerializer.Serialize(valueList);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeserializationTestWithNoVariables()
        {
            // Arrange
            string json = "[]";

            // Act
            ValueList? actual = JsonSerializer.Deserialize<ValueList>(json);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        public void DeserializationTestWithOneVariable()
        {
            // Arrange
            string json = "[{\"Code\":\"code\",\"Name\":{\"lang\":\"name\"},\"AdditionalProperties\":{},\"IsVirtual\":false}]";

            // Act
            ValueList? actual = JsonSerializer.Deserialize<ValueList>(json);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual("code", actual[0].Code);
            Assert.AreEqual("name", actual[0].Name["lang"]);
            Assert.IsFalse(actual[0].IsVirtual);
            Assert.AreEqual(0, actual[0].AdditionalProperties.Count);
        }

        [TestMethod]
        public void DeserializationTestWithTwoVariables()
        {
            // Arrange
            string json = "[{\"Code\":\"code1\",\"Name\":{\"lang\":\"name1\"},\"AdditionalProperties\":{},\"IsVirtual\":false},{\"Code\":\"code2\",\"Name\":{\"lang\":\"name2\"},\"AdditionalProperties\":{},\"IsVirtual\":false}]";

            // Act
            ValueList? actual = JsonSerializer.Deserialize<ValueList>(json);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual("code1", actual[0].Code);
            Assert.AreEqual("name1", actual[0].Name["lang"]);
            Assert.IsFalse(actual[0].IsVirtual);
            Assert.AreEqual(0, actual[0].AdditionalProperties.Count);

            Assert.AreEqual("code2", actual[1].Code);
            Assert.AreEqual("name2", actual[1].Name["lang"]);
            Assert.IsFalse(actual[1].IsVirtual);
            Assert.AreEqual(0, actual[1].AdditionalProperties.Count);
        }

        [TestMethod]
        public void SerializationDeserializationTestWithNoVariables()
        {
            // Arrange
            ValueList valueList = new([]);
            string json = JsonSerializer.Serialize(valueList);

            // Act
            ValueList? actual = JsonSerializer.Deserialize<ValueList>(json);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(valueList.Count, actual.Count);
        }

        [TestMethod]
        public void SerializationDeserializationTestWithOneVariable()
        {
            // Arrange
            ValueList valueList = new([new("code", new MultilanguageString("lang", "name"))]);
            string json = JsonSerializer.Serialize(valueList);

            // Act
            ValueList? actual = JsonSerializer.Deserialize<ValueList>(json);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(valueList.Count, actual.Count);
            Assert.AreEqual(valueList[0].Code, actual[0].Code);
            Assert.AreEqual(valueList[0].Name["lang"], actual[0].Name["lang"]);
            Assert.AreEqual(valueList[0].IsVirtual, actual[0].IsVirtual);
            Assert.AreEqual(valueList[0].AdditionalProperties.Count, actual[0].AdditionalProperties.Count);
        }

        [TestMethod]
        public void SerializationDeserializationTestWithTwoVariables()
        {
            // Arrange
            ValueList valueList = new(
            [
                new("code1", new MultilanguageString("lang", "name1")),
                new("code2", new MultilanguageString("lang", "name2"))
            ]);
            string json = JsonSerializer.Serialize(valueList);

            // Act
            ValueList? actual = JsonSerializer.Deserialize<ValueList>(json);

            // Assert
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual);
            Assert.AreEqual(valueList.Count, actual.Count);
            Assert.AreEqual(valueList[0].Code, actual[0].Code);
            Assert.AreEqual(valueList[0].Name["lang"], actual[0].Name["lang"]);
            Assert.AreEqual(valueList[0].IsVirtual, actual[0].IsVirtual);
            Assert.AreEqual(valueList[0].AdditionalProperties.Count, actual[0].AdditionalProperties.Count);
            Assert.AreEqual(valueList[1].Code, actual[1].Code);
            Assert.AreEqual(valueList[1].Name["lang"], actual[1].Name["lang"]);
            Assert.AreEqual(valueList[1].IsVirtual, actual[1].IsVirtual);
            Assert.AreEqual(valueList[1].AdditionalProperties.Count, actual[1].AdditionalProperties.Count);
        }
    }
}
