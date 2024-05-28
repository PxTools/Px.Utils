using Px.Utils.Language;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.ExtensionMethods;

namespace Px.Utils.UnitTests.ModelTests.ExtensionTests.PropertyExtensionTests
{
    [TestClass]
    public class ValueAsListOfMultilanguageStringsTests
    {
        [TestMethod]
        public void ParseListOfMultilanguageStringsValidInputReturnsList()
        {
            // Arrange
            string a_list_string = "\"a_0\", \"a_1\", \"a_2\"";
            string b_list_string = "\"b_0\", \"b_1\", \"b_2\"";
            string c_list_string = "\"c_0\", \"c_1\", \"c_2\"";
            MultilanguageString mls = new([new("a", a_list_string), new("b", b_list_string), new("c", c_list_string)]);
            MetaProperty property = new("test_property", mls);

            List<MultilanguageString> expected =
            [
                new([new("a", "a_0"), new("b", "b_0"), new("c", "c_0")]),
                new([new("a", "a_1"), new("b", "b_1"), new("c", "c_1")]),
                new([new("a", "a_2"), new("b", "b_2"), new("c", "c_2")])
            ];

            // Act
            List<MultilanguageString> result = property.ValueAsListOfMultilanguageStrings( "none", ',', '"');

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ParseListOfMultilanguageStringsValidInputWithLineBreaksReturnsList()
        {
            // Arrange
            string a_list_string = "\"a_0\", \r\n \"a_1\", \r\n \"a_2\"";
            string b_list_string = "\"b_0\", \r\n \"b_1\", \r\n \"b_2\"";
            string c_list_string = "\"c_0\", \r\n \"c_1\", \r\n \"c_2\"";
            MultilanguageString mls = new([new("a", a_list_string), new("b", b_list_string), new("c", c_list_string)]);
            MetaProperty property = new("test_property", mls);

            List<MultilanguageString> expected =
            [
                new([new("a", "a_0"), new("b", "b_0"), new("c", "c_0")]),
                new([new("a", "a_1"), new("b", "b_1"), new("c", "c_1")]),
                new([new("a", "a_2"), new("b", "b_2"), new("c", "c_2")])
            ];

            // Act
            List<MultilanguageString> result = property.ValueAsListOfMultilanguageStrings( "none", ',', '"');

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ParseListOfMultilanguageStringsNoLanguagePropertyThrows()
        {
            // Arrange
            MetaProperty property = new("test_key", "test_value");

            // Act and assert
            Assert.ThrowsException<ArgumentException>(() => property.ValueAsListOfMultilanguageStrings( "none", ',', '"'));
        }

        [TestMethod]
        public void ParseListOfMultilanguageStringsOneLanguageMLS()
        {
            // Arrange
            MultilanguageString mls = new("lang_a", "\"a_0\"");
            MetaProperty property = new("test_property", mls);

            List<MultilanguageString> expected = [new("lang_a", "a_0")];

            // Act
            List<MultilanguageString> result = property.ValueAsListOfMultilanguageStrings( "none", ',', '"');

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }
    }
}
