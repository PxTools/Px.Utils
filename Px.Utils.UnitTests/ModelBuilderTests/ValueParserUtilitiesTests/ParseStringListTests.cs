using PxUtils.ModelBuilders;

namespace ModelBuilderTests.ValueParserUtilitiesTests
{
    [TestClass]
    public class ParseStringListTests
    {
        [TestMethod]
        public void ParseStringList_ValidInput_ReturnsList()
        {
            // Arrange
            string input = "\"a\", \"b\", \"c\"";
            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a", "b", "c"];

            // Act
            List<string> result = ValueParserUtilities.ParseStringList(input, listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ParseStringList_ValidInputWithLineBreaks_ReturnsList()
        {
            // Arrange
            string input = "\"a\", \r\n \"b\", \r\n \"c\"";
            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a", "b", "c"];

            // Act
            List<string> result = ValueParserUtilities.ParseStringList(input, listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ParseStringList_ValidInputWithWhiteSpaces_ReturnsList()
        {
            // Arrange
            string input = " \" a \",   \" b \",   \" c \"  ";
            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = [" a ", " b ", " c "];

            // Act
            List<string> result = ValueParserUtilities.ParseStringList(input, listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ParseStringList_ValidInputWithSeparators_ReturnsList()
        {
            // Arrange
            string input = "\"a,a\", \"b,b\", \"c,c\"";
            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a,a", "b,b", "c,c"];

            // Act
            List<string> result = ValueParserUtilities.ParseStringList(input, listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ParseStringList_ValidInputWithOneElement_ReturnsList()
        {
            // Arrange
            string input = "\"a\"";
            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a"];

            // Act
            List<string> result = ValueParserUtilities.ParseStringList(input, listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ParseStringList_EmptyString_ReturnsEmptyList()
        {
            // Arrange
            string input = "";
            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = [];

            // Act
            List<string> result = ValueParserUtilities.ParseStringList(input, listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ParseStringList_MissingListSeparator_ThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\" \"b\", \"c\"";
            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => ValueParserUtilities.ParseStringList(input, listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseStringList_MissingOpeningDelimeterInSubstring_ThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", b\", \"c\"";
            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => ValueParserUtilities.ParseStringList(input, listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseStringList_MissingClosingDelimeterInSubstring_ThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", \"b, \"c\"";
            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => ValueParserUtilities.ParseStringList(input, listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseStringList_SingleDelimeterAsSubstring_ThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", \", \"c\"";
            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => ValueParserUtilities.ParseStringList(input, listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }


        [TestMethod]
        public void ParseStringList_ThreeDelimetersAsSubstring_ThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", \"\"\", \"c\"";
            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => ValueParserUtilities.ParseStringList(input, listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseStringList_MissingClosingDelimeter_ThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", \"b\", \"c";
            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => ValueParserUtilities.ParseStringList(input, listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseStringList_EndsWithListSeparator_ThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", \"b\", \"c\",";
            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => ValueParserUtilities.ParseStringList(input, listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseStringList_EmptySubstring_ReturnsList()
        {
            // Arrange
            string input = "\"a\",,\"b\"";
            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a", "", "b"];

            // Act
            List<string> result = ValueParserUtilities.ParseStringList(input, listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ParseStringList_EmptySubstringWithDelimeters_ReturnsList()
        {
            // Arrange
            string input = "\"a\",\"\",\"b\"";
            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a", "", "b"];

            // Act
            List<string> result = ValueParserUtilities.ParseStringList(input, listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }
    }
}
