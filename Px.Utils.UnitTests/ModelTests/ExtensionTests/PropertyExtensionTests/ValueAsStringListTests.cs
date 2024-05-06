using PxUtils.Models.Metadata;
using PxUtils.Models.Metadata.ExtensionMethods;

namespace Px.Utils.UnitTests.ModelTests.ExtensionTests.PropertyExtensionTests
{
    [TestClass]
    public class ValueAsStringListTests
    {
        #region Valid input tests

        [TestMethod]
        public void ValueToStringList_ValidValue_ReturnsList()
        {
            // Arrange
            string input = "\"a\", \"b\", \"c\"";
            MetaProperty property = new("key", input);

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a", "b", "c"];

            // Act
            List<string> result = property.ValueAsListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringList_ValidValueWithLineBreaks_ReturnsList()
        {
            // Arrange
            string input = "\"a\", \r\n \"b\", \r\n \"c\"";
            MetaProperty property = new("key", input);

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a", "b", "c"];

            // Act
            List<string> result = property.ValueAsListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringList_ValidValueWithWhiteSpaces_ReturnsList()
        {
            // Arrange
            string input = " \" a \",   \" b \",   \" c \"  ";
            MetaProperty property = new("key", input);

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = [" a ", " b ", " c "];

            // Act
            List<string> result = property.ValueAsListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringList_ValidValueWithSeparators_ReturnsList()
        {
            // Arrange
            string input = "\"a,a\", \"b,b\", \"c,c\"";
            MetaProperty property = new("key", input);

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a,a", "b,b", "c,c"];

            // Act
            List<string> result = property.ValueAsListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringList_ValidValueWithOneElement_ReturnsList()
        {
            // Arrange
            string input = "\"a\"";
            MetaProperty property = new("key", input);

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a"];

            // Act
            List<string> result = property.ValueAsListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringList_EmptyString_ReturnsEmptyList()
        {
            // Arrange
            string input = "";
            MetaProperty property = new("key", input);

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = [];

            // Act
            List<string> result = property.ValueAsListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringList_EmptySubstring_ReturnsList()
        {
            // Arrange
            string input = "\"a\",,\"b\"";
            MetaProperty property = new("key", input);

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a", "", "b"];

            // Act
            List<string> result = property.ValueAsListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringList_EmptySubstringWithDelimeters_ReturnsList()
        {
            // Arrange
            string input = "\"a\",\"\",\"b\"";
            MetaProperty property = new("key", input);

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a", "", "b"];

            // Act
            List<string> result = property.ValueAsListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        #endregion

        #region Invalid input tests

        [TestMethod]
        public void ValueToStringListt_MissingListSeparator_ThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\" \"b\", \"c\"";
            MetaProperty property = new("key", input);

            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => property.ValueAsListOfStrings(listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ValueToStringList_MissingOpeningDelimeterInSubstring_ThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", b\", \"c\"";
            MetaProperty property = new("key", input);

            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => property.ValueAsListOfStrings(listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ValueToStringList_MissingClosingDelimeterInSubstring_ThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", \"b, \"c\"";
            MetaProperty property = new("key", input);

            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => property.ValueAsListOfStrings(listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ValueToStringList_SingleDelimeterAsSubstring_ThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", \", \"c\"";
            MetaProperty property = new("key", input);

            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => property.ValueAsListOfStrings(listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }


        [TestMethod]
        public void ValueToStringList_ThreeDelimetersAsSubstring_ThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", \"\"\", \"c\"";
            MetaProperty property = new("key", input);

            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => property.ValueAsListOfStrings(listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ValueToStringList_MissingClosingDelimeter_ThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", \"b\", \"c";
            MetaProperty property = new("key", input);

            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => property.ValueAsListOfStrings(listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ValueToStringList_EndsWithListSeparator_ThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", \"b\", \"c\",";
            MetaProperty property = new("key", input);

            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => property.ValueAsListOfStrings(listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        #endregion
    }
}
