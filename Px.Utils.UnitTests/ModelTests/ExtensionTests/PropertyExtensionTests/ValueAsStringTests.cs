using PxUtils.Language;
using PxUtils.Models.Metadata;
using PxUtils.Models.Metadata.ExtensionMethods;

namespace ModelTests.ExtensionTests.PropertyExtensionTests
{
    [TestClass]
    public class ValueAsStringTests
    {
        [TestMethod]
        public void ValueToString_ValidValue_ReturnsString()
        {
            // Arrange
            string input = "\"aa\"";
            Property property = new("key", input);

            char stringDelimeter = '"';
            string expected = "aa";

            // Act
            string result = property.ValueAsString(stringDelimeter);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToString_ValidValueWithWhitespace_ReturnsString()
        {
            // Arrange
            string input = "\" a a \"";
            Property property = new("key", input);

            char stringDelimeter = '"';
            string expected = " a a ";

            // Act
            string result = property.ValueAsString(stringDelimeter);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToString_ValidValueWithTrimmableWhitespace_ReturnsString()
        {
            // Arrange
            string input = " \"a a\" ";
            Property property = new("key", input);

            char stringDelimeter = '"';
            string expected = "a a";

            // Act
            string result = property.ValueAsString(stringDelimeter);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringListt_MissingListSeparator_ThrowsArgumentException()
        {
            // Arrange
            string a_list_string = "\"a_0\", \"a_1\", \"a_2\"";
            string b_list_string = "\"b_0\", \"b_1\", \"b_2\"";
            string c_list_string = "\"c_0\", \"c_1\", \"c_2\"";
            MultilanguageString mls = new([new("a", a_list_string), new("b", b_list_string), new("c", c_list_string)]);
            Property property = new("test_property", mls);

            char stringDelimeter = '"';

            // Act
            Action func = new(() => property.ValueAsString(stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }
    }
}
