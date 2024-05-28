using Px.Utils.ModelBuilders;

namespace ModelBuilderTests
{
    [TestClass]
    public class MetadataEntryKeyBuilderTests
    {
        private readonly MetadataEntryKeyBuilder builder = new();

        #region Valid key tests

        [TestMethod]
        public void ParseMetadataEntryKeyTestKeyWithLangAndTwoSpecifiersReturnsEntryKey()
        {
            // Arrange
            string key = "FOOBAR";
            string lang = "aa";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar";
            string input = $"{key}[{lang}](\"{firstIdentifier}\",\"{secondIdentifier}\")";

            // Act
            var result = builder.Parse(input);

            // Assert
            Assert.AreEqual(key, result.KeyWord);
            Assert.AreEqual(lang, result.Language);
            Assert.AreEqual(firstIdentifier, result.FirstIdentifier);
            Assert.AreEqual(secondIdentifier, result.SecondIdentifier);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestKeyWithLangAndTwoSpecifiersWithSpaceReturnsEntryKey()
        {
            // Arrange
            string key = "FOOBAR";
            string lang = "aa";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar";
            string input = $"{key}[{lang}](\"{firstIdentifier}\", \"{secondIdentifier}\")";

            // Act
            var result = builder.Parse(input);

            // Assert
            Assert.AreEqual(key, result.KeyWord);
            Assert.AreEqual(lang, result.Language);
            Assert.AreEqual(firstIdentifier, result.FirstIdentifier);
            Assert.AreEqual(secondIdentifier, result.SecondIdentifier);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestKeyWithLangReturnsEntryKey()
        {
            // Arrange
            string key = "FOOBAR";
            string lang = "aa";
            string input = $"{key}[{lang}]";

            // Act
            var result = builder.Parse(input);

            // Assert
            Assert.AreEqual(key, result.KeyWord);
            Assert.AreEqual(lang, result.Language);
            Assert.IsNull(result.FirstIdentifier);
            Assert.IsNull(result.SecondIdentifier);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestKeyWithTwoSpesifiersReturnsEntryKey()
        {
            // Arrange
            string key = "FOOBAR";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar";
            string input = $"{key}(\"{firstIdentifier}\",\"{secondIdentifier}\")";

            // Act
            var result = builder.Parse(input);

            // Assert
            Assert.AreEqual(key, result.KeyWord);
            Assert.IsNull(result.Language);
            Assert.AreEqual(firstIdentifier, result.FirstIdentifier);
            Assert.AreEqual(secondIdentifier, result.SecondIdentifier);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestKeyWithOneSpesifierReturnsEntryKey()
        {
            // Arrange
            string key = "FOOBAR";
            string firstIdentifier = "foo";
            string input = $"{key}(\"{firstIdentifier}\")";

            // Act
            var result = builder.Parse(input);

            // Assert
            Assert.AreEqual(key, result.KeyWord);
            Assert.IsNull(result.Language);
            Assert.AreEqual(firstIdentifier, result.FirstIdentifier);
            Assert.IsNull(result.SecondIdentifier);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestOnlyKeywordReturnsEntryKey()
        {
            // Arrange
            string key = "FOOBAR";

            // Act
            var result = builder.Parse(key);

            // Assert
            Assert.AreEqual(key, result.KeyWord);
            Assert.IsNull(result.Language);
            Assert.IsNull(result.FirstIdentifier);
            Assert.IsNull(result.SecondIdentifier);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestOnlyKeywordWithMinusSignReturnsEntryKey()
        {
            // Arrange
            string key = "FOO-BAR";

            // Act
            var result = builder.Parse(key);

            // Assert
            Assert.AreEqual(key, result.KeyWord);
            Assert.IsNull(result.Language);
            Assert.IsNull(result.FirstIdentifier);
            Assert.IsNull(result.SecondIdentifier);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestKeyWithOneSpecifierWithBracketsReturnsEntryKey()
        {
            // Arrange
            string key = "FOOBAR";
            string firstIdentifier = "foo(bar)";
            string input = $"{key}(\"{firstIdentifier}\")";

            // Act
            var result = builder.Parse(input);

            // Assert
            Assert.AreEqual(key, result.KeyWord);
            Assert.IsNull(result.Language);
            Assert.AreEqual(firstIdentifier, result.FirstIdentifier);
            Assert.IsNull(result.SecondIdentifier);
        }

        #endregion

        #region Invalid key tests

        [TestMethod]
        public void ParseMetadataEntryKeyTestSymbolBetweenLangAndSpecifierBlocksThrowsArgumentException()
        {
            // Arrange
            string key = "FOOBAR";
            string lang = "aa";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar";
            string input = $"{key}[{lang}]X(\"{firstIdentifier}\",\"{secondIdentifier}\")";

            Action func = new(() => builder.Parse(input));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestSymbolBetweenSpecifiersThrowsArgumentException()
        {
            // Arrange
            string key = "FOOBAR";
            string lang = "aa";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar";
            string input = $"{key}[{lang}](\"{firstIdentifier}\"X,\"{secondIdentifier}\")";

            Action func = new(() => builder.Parse(input));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestSymbolAfterSpecifiersThrowsArgumentException()
        {
            // Arrange
            string key = "FOOBAR";
            string lang = "aa";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar";
            string input = $"{key}[{lang}](\"{firstIdentifier}\",\"{secondIdentifier}\"X)";

            Action func = new(() => builder.Parse(input));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestSymbolAfterKeyThrowsArgumentException()
        {
            // Arrange
            string key = "FOOBAR";
            string lang = "aa";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar";
            string input = $"{key}[{lang}](\"{firstIdentifier}\",\"{secondIdentifier}\")X";

            Action func = new(() => builder.Parse(input));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestTwoLangBlocksThrowsArgumentException()
        {
            // Arrange
            string key = "FOOBAR";
            string lang = "aa";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar";
            string input = $"{key}[{lang}][{lang}](\"{firstIdentifier}\",\"{secondIdentifier}\")";

            Action func = new(() => builder.Parse(input));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestTwoSpecifierBlocksThrowsArgumentException()
        {
            // Arrange
            string key = "FOOBAR";
            string lang = "aa";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar";
            string input = $"{key}[{lang}](\"{firstIdentifier}\")(\"{secondIdentifier}\")";

            Action func = new(() => builder.Parse(input));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestMissingSeparatorBetweenSpecifierThrowsArgumentException()
        {
            // Arrange
            string key = "FOOBAR";
            string lang = "aa";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar";
            string input = $"{key}[{lang}](\"{firstIdentifier}\" \"{secondIdentifier}\")";

            // Act
            Action func = new(() => builder.Parse(input));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestThreeSpecifiersThrowsArgumentException()
        {
            // Arrange
            string key = "FOOBAR";
            string lang = "aa";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar";
            string input = $"{key}[{lang}](\"{firstIdentifier}\",\"{secondIdentifier}\",\"fyy\")";

            // Act
            Action func = new(() => builder.Parse(input));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestExtraLangBracketThrowsArgumentException()
        {
            // Arrange
            string key = "FOOBAR";
            string lang = "aa";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar";
            string input = $"{key}[[{lang}]](\"{firstIdentifier}\",\"{secondIdentifier}\")";

            Action func = new(() => builder.Parse(input));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestQuotesInSpecifierThrowsArgumentException()
        {
            // Arrange
            string key = "FOOBAR";
            string lang = "aa";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar(\"baz\")";
            string input = $"{key}[{lang}](\"{firstIdentifier}\",\"{secondIdentifier}\")";

            Action func = new(() => builder.Parse(input));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestEmptySpecifierBlockThrowsArgumentException()
        {
            // Arrange
            string key = "FOOBAR";
            string lang = "aa";
            string input = $"{key}[{lang}]()";

            Action func = new(() => builder.Parse(input));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestEmptyLangBlockThrowsArgumentException()
        {
            // Arrange
            string key = "FOOBAR";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar";
            string input = $"{key}[](\"{firstIdentifier}\",\"{secondIdentifier}\")";

            // Act
            Action func = new(() => builder.Parse(input));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestKeyWordHasWhiteSpaceThrowsArgumentException()
        {
            // Arrange
            string key = "FOO BAR";
            string lang = "aa";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar";
            string input = $"{key}[{lang}](\"{firstIdentifier}\",\"{secondIdentifier}\")";

            // Act
            Action func = new(() => builder.Parse(input));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        [TestMethod]
        public void ParseMetadataEntryKeyTestLangBlockHasTwoClosingBracketsThrowsArgumentException()
        {
            // Arrange
            string key = "FOOBAR";
            string lang = "aa]";
            string firstIdentifier = "foo";
            string secondIdentifier = "bar";
            string input = $"{key}[{lang}](\"{firstIdentifier}\",\"{secondIdentifier}\")";

            // Act
            Action func = new(() => builder.Parse(input));

            // Assert
            Assert.ThrowsException<ArgumentException>(func);
        }

        #endregion
    }
}
