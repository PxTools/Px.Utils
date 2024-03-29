﻿using PxUtils.ModelBuilders;

namespace ModelBuilderTests
{
    [TestClass]
    public class MetadataEntryKeyBuilderTests
    {
        private readonly MetadataEntryKeyBuilder builder = new();

        #region Valid key tests

        [TestMethod]
        public void ParseMetadataEntryKeyTest_KeyWithLangAndTwoSpecifiers_ReturnsEntryKey()
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
        public void ParseMetadataEntryKeyTest_KeyWithLangAndTwoSpecifiersWithSpace_ReturnsEntryKey()
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
        public void ParseMetadataEntryKeyTest_KeyWithLang_ReturnsEntryKey()
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
        public void ParseMetadataEntryKeyTest_KeyWithTwoSpesifiers_ReturnsEntryKey()
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
        public void ParseMetadataEntryKeyTest_KeyWithOneSpesifier_ReturnsEntryKey()
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
        public void ParseMetadataEntryKeyTest_OnlyKeyword_ReturnsEntryKey()
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
        public void ParseMetadataEntryKeyTest_OnlyKeywordWithMinusSign_ReturnsEntryKey()
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
        public void ParseMetadataEntryKeyTest_KeyWithOneSpecifierWithBrackets_ReturnsEntryKey()
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
        public void ParseMetadataEntryKeyTest_SymbolBetweenLangAndSpecifierBlocks_ThrowsArgumentException()
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
        public void ParseMetadataEntryKeyTest_SymbolBetweenSpecifiers_ThrowsArgumentException()
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
        public void ParseMetadataEntryKeyTest_SymbolAfterSpecifiers_ThrowsArgumentException()
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
        public void ParseMetadataEntryKeyTest_SymbolAfterKey_ThrowsArgumentException()
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
        public void ParseMetadataEntryKeyTest_TwoLangBlocks_ThrowsArgumentException()
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
        public void ParseMetadataEntryKeyTest_TwoSpecifierBlocks_ThrowsArgumentException()
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
        public void ParseMetadataEntryKeyTest_MissingSeparatorBetweenSpecifier_ThrowsArgumentException()
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
        public void ParseMetadataEntryKeyTest_ThreeSpecifiers_ThrowsArgumentException()
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
        public void ParseMetadataEntryKeyTest_ExtraLangBracket_ThrowsArgumentException()
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
        public void ParseMetadataEntryKeyTest_QuotesInSpecifier_ThrowsArgumentException()
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
        public void ParseMetadataEntryKeyTest_EmptySpecifierBlock_ThrowsArgumentException()
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
        public void ParseMetadataEntryKeyTest_EmptyLangBlock_ThrowsArgumentException()
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
        public void ParseMetadataEntryKeyTest_KeyWordHasWhiteSpace_ThrowsArgumentException()
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
        public void ParseMetadataEntryKeyTest_LangBlockHasTwoClosingBrackets_ThrowsArgumentException()
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
