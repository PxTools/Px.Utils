using Px.Utils.Models.Metadata.ExtensionMethods;

namespace Px.Utils.UnitTests.ModelTests.ExtensionTests.PropertyExtensionTests
{
    [TestClass]
    public class CleanDringDelimeterTests
    {
        [TestMethod]
        public void ValidInputStringReturnsString()
        {
            // Arrange
            string input = "\"aa\"";

            char stringDelimeter = '"';
            string expected = "aa";

            // Act
            string result = input.CleanStringDelimeters(stringDelimeter);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void EmptyInputStringReturnsEmptyString()
        {
            // Arrange
            string input = "";

            char stringDelimeter = '"';
            string expected = "";

            // Act
            string result = input.CleanStringDelimeters(stringDelimeter);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValidInputStringWithWhitespaceReturnsString()
        {
            // Arrange
            string input = "\" a a \"";

            char stringDelimeter = '"';
            string expected = " a a ";

            // Act
            string result = input.CleanStringDelimeters(stringDelimeter);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ValidInputStringWithTrimmableWhitespaceReturnsString()
        {
            // Arrange
            string input = " \"a a\" ";

            char stringDelimeter = '"';
            string expected = "a a";

            // Act
            string result = input.CleanStringDelimeters(stringDelimeter);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void MultilineStringReturnsString()
        {
            // Arrange
            string input = "\"foo\" \n\"bar\"";

            char stringDelimeter = '"';
            string expected = "foobar";

            // Act
            string result = input.CleanStringDelimeters(stringDelimeter);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}
