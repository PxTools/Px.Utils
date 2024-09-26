using Px.Utils.Models.Metadata.ExtensionMethods;

namespace Px.Utils.UnitTests.ModelTests.ExtensionTests.PropertyExtensionTests
{
    [TestClass]
    public class ValueAsStringListTests
    {
        #region Valid input tests

        [TestMethod]
        public void ValueToStringListValidValueReturnsList()
        {
            // Arrange
            string input = "\"a\", \"b\", \"c\"";

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a", "b", "c"];

            // Act
            List<string> result = input.SplitToListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringListValidValueWithLineBreaksReturnsList()
        {
            // Arrange
            string input = "\"a\", \r\n \"b\", \r\n \"c\"";

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a", "b", "c"];

            // Act
            List<string> result = input.SplitToListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringListValidValueWithWhiteSpacesReturnsList()
        {
            // Arrange
            string input = " \" a \",   \" b \",   \" c \"  ";

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = [" a ", " b ", " c "];

            // Act
            List<string> result = input.SplitToListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringListValidValueWithSeparatorsReturnsList()
        {
            // Arrange
            string input = "\"a,a\", \"b,b\", \"c,c\"";

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a,a", "b,b", "c,c"];

            // Act
            List<string> result = input.SplitToListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringListValidValueWithOneElementReturnsList()
        {
            // Arrange
            string input = "\"a\"";

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a"];

            // Act
            List<string> result = input.SplitToListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringListEmptyStringReturnsEmptyList()
        {
            // Arrange
            string input = "";

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = [];

            // Act
            List<string> result = input.SplitToListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringListEmptySubstringReturnsList()
        {
            // Arrange
            string input = "\"a\",,\"b\"";

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a", "", "b"];

            // Act
            List<string> result = input.SplitToListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValueToStringListEmptySubstringWithDelimetersReturnsList()
        {
            // Arrange
            string input = "\"a\",\"\",\"b\"";

            char listSeparator = ',';
            char stringDelimeter = '"';
            List<string> expected = ["a", "", "b"];

            // Act
            List<string> result = input.SplitToListOfStrings(listSeparator, stringDelimeter);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        #endregion

        #region Invalid input tests

        [TestMethod]
        public void ValueToStringListMissingListSeparatorThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\" \"b\", \"c\"";

            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => input.SplitToListOfStrings(listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ValueToStringListMissingOpeningDelimeterInSubstringThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", b\", \"c\"";

            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => input.SplitToListOfStrings(listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ValueToStringListMissingClosingDelimeterInSubstringThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", \"b, \"c\"";

            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => input.SplitToListOfStrings(listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ValueToStringListSingleDelimeterAsSubstringThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", \", \"c\"";

            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => input.SplitToListOfStrings(listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }


        [TestMethod]
        public void ValueToStringListThreeDelimetersAsSubstringThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", \"\"\", \"c\"";

            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => input.SplitToListOfStrings(listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ValueToStringListMissingClosingDelimeterThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", \"b\", \"c";

            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => input.SplitToListOfStrings(listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ValueToStringListEndsWithListSeparatorThrowsArgumentException()
        {
            // Arrange
            string input = "\"a\", \"b\", \"c\",";

            char listSeparator = ',';
            char stringDelimeter = '"';

            // Act
            Action func = new(() => input.SplitToListOfStrings(listSeparator, stringDelimeter));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        #endregion
    }
}
