﻿using Px.Utils.Language;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.ExtensionMethods;

namespace ModelTests.ExtensionTests.PropertyExtensionTests
{
    [TestClass]
    public class ValueAsStringTests
    {
        [TestMethod]
        public void ValueAsStringValidValueReturnsString()
        {
            // Arrange
            string input = "\"aa\"";
            MetaProperty property = new("key", input);

            char stringDelimeter = '"';
            string expected = "aa";

            // Act
            string result = property.ValueAsString(stringDelimeter);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueAsStringValidValueWithWhitespaceReturnsString()
        {
            // Arrange
            string input = "\" a a \"";
            MetaProperty property = new("key", input);

            char stringDelimeter = '"';
            string expected = " a a ";

            // Act
            string result = property.ValueAsString(stringDelimeter);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueAsStringValidValueWithTrimmableWhitespaceReturnsString()
        {
            // Arrange
            string input = " \"a a\" ";
            MetaProperty property = new("key", input);

            char stringDelimeter = '"';
            string expected = "a a";

            // Act
            string result = property.ValueAsString(stringDelimeter);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueAsStringMultilineStringReturnsString()
        {
            // Arrange
            string input = "\"foo\" \n\"bar\"";
            MetaProperty property = new("key", input);

            char stringDelimeter = '"';
            string expected = "foobar";

            // Act
            string result = property.ValueAsString(stringDelimeter);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringListMissingListSeparatorThrowsArgumentException()
        {
            // Arrange
            string a_list_string = "\"a_0\", \"a_1\", \"a_2\"";
            string b_list_string = "\"b_0\", \"b_1\", \"b_2\"";
            string c_list_string = "\"c_0\", \"c_1\", \"c_2\"";
            MultilanguageString mls = new([new("a", a_list_string), new("b", b_list_string), new("c", c_list_string)]);
            MetaProperty property = new("test_property", mls);

            char stringDelimeter = '"';

            // Act
            Action func = new(() => property.ValueAsString(stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }
    }
}
