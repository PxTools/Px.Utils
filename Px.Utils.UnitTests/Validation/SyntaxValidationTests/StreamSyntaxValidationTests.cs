using Px.Utils.Validation.SyntaxValidation;
using Px.Utils.UnitTests.Validation.Fixtures;
using System.Text;
using System.Reflection;
using Px.Utils.Validation;
using Px.Utils.PxFile;
using Px.Utils.PxFile.Metadata;
using Px.Utils.UnitTests.Validation.SyntaxValidationTests;

namespace Px.Utils.UnitTests.SyntaxValidationTests
{
    [TestClass]
    public class StreamSyntaxValidationTests
    {
        private readonly PxFileSyntaxConf syntaxConf = PxFileSyntaxConf.Default;
        private ValidationFeedback feedback = [];
        private MethodInfo? entryValidationMethod;
        private MethodInfo? kvpValidationMethod;
        private MethodInfo? structuredValidationMethod;
        private MethodInfo? getValueTypeFromStringMethod;

        [TestInitialize]
        public void Initialize()
        {
            entryValidationMethod = typeof(SyntaxValidator)
                .GetMethod("ValidateEntries", BindingFlags.NonPublic | BindingFlags.Static);
            kvpValidationMethod = typeof(SyntaxValidator)
                .GetMethod("ValidateKeyValuePairs", BindingFlags.NonPublic | BindingFlags.Static);
            structuredValidationMethod = typeof(SyntaxValidator)
                .GetMethod("ValidateStructs", BindingFlags.NonPublic | BindingFlags.Static);
            getValueTypeFromStringMethod = typeof(SyntaxValidationUtilityMethods)
                .GetMethod("GetValueTypeFromString", BindingFlags.NonPublic | BindingFlags.Static);
        }

        [TestMethod]
        public void ValidatePxFileSyntaxCalledWithMininalUtf8ReturnsValidResult()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();
            Encoding? encoding = reader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            SyntaxValidator validator = new();

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = validator.Validate(stream, "foo", encoding);
            Assert.AreEqual(8, result.Result.Count);
            Assert.AreEqual(0, feedback.Count);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithMultipleEntriesInSingleLineReturnsWithWarnings()
        {
            // Arrange
            List<ValidationEntry> entries = SyntaxValidationFixtures.MULTIPLE_ENTRIES_IN_SINGLE_LINE;
            List<EntryValidationFunction> functions = [SyntaxValidationFunctions.MultipleEntriesOnLine];

            // Act
            feedback = entryValidationMethod?.Invoke(null, [entries, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(2, feedback.First().Value.Count);
            Assert.AreEqual(ValidationFeedbackRule.MultipleEntriesOnOneLine, feedback.First().Key.Rule);
        }

        [TestMethod]
        public void ValidatePxFileSyntaxCalledWithWithSpecifiersSReturnsWithRightStructure()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_SPECIFIERS);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();
            Encoding? encoding = reader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            SyntaxValidator validator = new();

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = validator.Validate(stream, "foo", encoding);
            Assert.AreEqual(10, result.Result.Count);
            Assert.AreEqual("YES", result.Result[8].Value);
            Assert.AreEqual("NO", result.Result[9].Value);
            Assert.AreEqual("fi", result.Result[8].Key.Language);
            Assert.AreEqual("fi", result.Result[9].Key.Language);
            Assert.AreEqual("first_specifier", result.Result[8].Key.FirstSpecifier);
            Assert.AreEqual("first_specifier", result.Result[9].Key.FirstSpecifier);
            Assert.AreEqual("second_specifier", result.Result[8].Key.SecondSpecifier);
        }

        [TestMethod]
        public void ValidatePxFileSyntaxCalledWithWithFeedbacksReturnsRightLineAndCharacterIndexes()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_FEEDBACKS);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();
            Encoding? encoding = reader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            SyntaxValidator validator = new();
            ValidationFeedbackKey keyWhiteSpaceFeedbackKey = new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeyContainsExcessWhiteSpace);
            ValidationFeedbackKey valueWhiteSpaceFeedbackKey = new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.ExcessWhitespaceInValue);
            ValidationFeedbackKey entryWithoutValueFeedbackKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.EntryWithoutValue);
            ValidationFeedbackKey invalidValue = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.InvalidValueFormat);

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = validator.Validate(stream, "foo", encoding);
            Assert.AreEqual(4, result.FeedbackItems.Count);
            Assert.AreEqual(9, result.FeedbackItems[keyWhiteSpaceFeedbackKey][0].Line);
            Assert.AreEqual(12, result.FeedbackItems[valueWhiteSpaceFeedbackKey][0].Line);
            Assert.AreEqual(14, result.FeedbackItems[entryWithoutValueFeedbackKey][0].Line);
            Assert.AreEqual(13, result.FeedbackItems[invalidValue][0].Line);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithMultipleLangParamsReturnsWithError()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_MULTIPLE_LANGUAGE_PARAMETERS;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.MoreThanOneLanguageParameter];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.MoreThanOneLanguageParameterSection, feedback.First().Key.Rule);
        }


        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithMultipleSpecifierParamsSReturnsWithError()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_MULTIPLE_SPECIFIER_PARAMETER_SECTIONS;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.MoreThanOneSpecifierParameter];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.MoreThanOneSpecifierParameterSection, feedback.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithWrongOrderAndMissingKeywordReturnsWithErrors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_IN_WRONG_ORDER_AND_MISSING_KEYWORD;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.WrongKeyOrderOrMissingKeyword];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(2, feedback.Count);
            Assert.IsTrue(feedback.ContainsKey(new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.KeyHasWrongOrder)));
            Assert.IsTrue(feedback.ContainsKey(new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingKeyword)));
            Assert.AreEqual(2, feedback[new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.KeyHasWrongOrder)].Count);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithInvalidSpecifiersReturnsWithErrors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_INVALID_SPECIFIERS;
            List<KeyValuePairValidationFunction> functions = [
                SyntaxValidationFunctions.MoreThanTwoSpecifierParts,
                SyntaxValidationFunctions.SpecifierPartNotEnclosed, 
                SyntaxValidationFunctions.NoDelimiterBetweenSpecifierParts];
            ValidationFeedbackKey missingDelimeterFeedbackKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.SpecifierDelimiterMissing);
            ValidationFeedbackKey tooManySpecifiersFeedbackKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.TooManySpecifiers);
            ValidationFeedbackKey notEnclosedFeedbackKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.SpecifierPartNotEnclosed);

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(3, feedback.Count);
            Assert.IsTrue(feedback.ContainsKey(missingDelimeterFeedbackKey));
            Assert.IsTrue(feedback.ContainsKey(tooManySpecifiersFeedbackKey));
            Assert.IsTrue(feedback.ContainsKey(notEnclosedFeedbackKey));
            Assert.AreEqual(2, feedback[notEnclosedFeedbackKey].Count);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithIllegalSymbolsInLanguageParamReturnsWithErrors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_ILLEGAL_SYMBOLS_IN_LANGUAGE_SECTIONS;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.IllegalSymbolsInLanguageParamSection];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(3, feedback.First().Value.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInLanguageSection, feedback.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithIllegalSymbolsInSpecifierParamReturnsWithErrors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_ILLEGAL_SYMBOLS_IN_SPECIFIER_SECTIONS;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.IllegalCharactersInSpecifierSection];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(3, feedback.First().Value.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInSpecifierSection, feedback.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithBadValuesReturnsErrors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_BAD_VALUES;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.InvalidValueFormat];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(4, feedback.First().Value.Count);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, feedback.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithExcessListValueWhitespaceReturnsWithWarning()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_EXCESS_LIST_VALUE_WHITESPACE;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.ExcessWhitespaceInValue];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.ExcessWhitespaceInValue, feedback.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithExcessKeyWhitespaceReturnsWithWarning()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_EXCESS_KEY_WHITESPACE;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.KeyContainsExcessWhiteSpace];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeyContainsExcessWhiteSpace, feedback.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithShortMultilineValueReturnsWithWarnings()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_SHORT_MULTILINE_VALUES;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.ExcessNewLinesInValue];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(2, feedback.First().Value.Count);
            Assert.AreEqual(ValidationFeedbackRule.ExcessNewLinesInValue, feedback.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithStructuredEntriesWithInvalidKeywordsReturnsWithErrors()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTURED_ENTRIES_WITH_INVALID_KEYWORDS;
            List<StructuredValidationFunction> functions = [SyntaxValidationFunctions.KeywordDoesntStartWithALetter, SyntaxValidationFunctions.KeywordContainsIllegalCharacters];
            ValidationFeedbackKey startWithletterFeedbackKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.KeywordDoesntStartWithALetter);
            ValidationFeedbackKey illegalCharactersFeedbackKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInKeyword);

            // Act
            feedback = structuredValidationMethod?.Invoke(null, [structuredEntries, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(2, feedback.Count);
            Assert.IsTrue(feedback.ContainsKey(startWithletterFeedbackKey));
            Assert.IsTrue(feedback.ContainsKey(illegalCharactersFeedbackKey));
            Assert.AreEqual(2, feedback[illegalCharactersFeedbackKey].Count);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithStructuredEntriesWithValidLanguagesReturnsWithNoFeedback()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTURED_ENTRIES_WITH_VALID_LANGUAGES;
            List<StructuredValidationFunction> functions = [SyntaxValidationFunctions.IllegalCharactersInLanguageParameter];

            // Act
            feedback = structuredValidationMethod?.Invoke(null, [structuredEntries, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(0, feedback.Count);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithStructuredEntriesWithInvalidLanguagesReturnsWithErrors()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTURED_ENTRIES_WITH_INVALID_LANGUAGES;
            List<StructuredValidationFunction> functions = [SyntaxValidationFunctions.IllegalCharactersInLanguageParameter];

            // Act
            feedback = structuredValidationMethod?.Invoke(null, [structuredEntries, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInLanguageSection, feedback.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithStructuredEntriesWithIllegalCharactersInSpecifiersReturnsWithErrors()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTIRED_ENTRIES_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIER_PARTS;
            List<StructuredValidationFunction> functions = [SyntaxValidationFunctions.IllegalCharactersInSpecifierParts];

            // Act
            feedback = structuredValidationMethod?.Invoke(null, [structuredEntries, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInSpecifierPart, feedback.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithEntryWithoutValueReturnsWithError()
        {
            // Arrange
            List<ValidationEntry> entries = SyntaxValidationFixtures.ENTRY_WITHOUT_VALUE;
            List<EntryValidationFunction> functions = [SyntaxValidationFunctions.EntryWithoutValue];

            // Act
            feedback = entryValidationMethod?.Invoke(null, [entries, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.EntryWithoutValue, feedback.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithStructuredEntriesWithIncompliantLanguagesReturnsWithWarnings()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTURED_ENTRIES_WITH_INCOMPLIANT_LANGUAGES;
            List<StructuredValidationFunction> functions = [SyntaxValidationFunctions.IncompliantLanguage];

            // Act
            feedback = structuredValidationMethod?.Invoke(null, [structuredEntries, functions, syntaxConf] ) as ValidationFeedback ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(2, feedback.First().Value.Count);
            Assert.AreEqual(ValidationFeedbackRule.IncompliantLanguage, feedback.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithStructuredEntriesWithUnrecommendedKeywordNamingReturnsWithWarnings()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTURED_ENTRIES_WITH_UNRECOMMENDED_KEYWORD_NAMING;
            List<StructuredValidationFunction> functions = [SyntaxValidationFunctions.KeywordContainsUnderscore, SyntaxValidationFunctions.KeywordIsNotInUpperCase];

            // Act
            feedback = structuredValidationMethod?.Invoke(null, [structuredEntries, functions, syntaxConf] ) as ValidationFeedback ?? [];

            Assert.AreEqual(2, feedback.Count);
            Assert.IsTrue(feedback.ContainsKey(new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeywordIsNotInUpperCase)));
            Assert.IsTrue(feedback.ContainsKey(new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeywordContainsUnderscore)));
        }

        [TestMethod]
        public void ValidateObjectsCalledWithLongKeywordReturnsWithWarnings()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTS_WITH_LONG_KEYWORD;
            List<StructuredValidationFunction> functions = [SyntaxValidationFunctions.KeywordIsExcessivelyLong];

            // Act
            feedback = structuredValidationMethod?.Invoke(null, [structuredEntries, functions, syntaxConf]) as ValidationFeedback ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeywordExcessivelyLong, feedback.First().Key.Rule);
        }

        [TestMethod]
        [DataRow("\"Item1\", \"Item2\", \"Item3\"")]
        [DataRow("\"Item 1\", \"Item 2\", \"Item 3\"")]
        [DataRow("\"Item1\",\"Item2\",\"Item3\"")]
        [DataRow("\"Item, 1\", \"Item, 2\", \"Item, 3\"")]
        [DataRow("\"Item1\",\"Item2\"")]
        public void GetValueTypeFromStringCalledWithValidListsValuesCorrectValueType(string list)
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = [new("foo", new KeyValuePair<string, string>("foo-key", list), 0, [], 0)];
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.InvalidValueFormat];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as ValidationFeedback ?? [];
            Utils.Validation.ValueType? valueType = getValueTypeFromStringMethod?.Invoke(null, [keyValuePairs.First().KeyValuePair.Value, PxFileSyntaxConf.Default]) as Utils.Validation.ValueType?;

            // Assert
            Assert.AreEqual(Utils.Validation.ValueType.ListOfStrings, valueType);
            Assert.AreEqual(0, feedback.Count);
        }

        [TestMethod]
        [DataRow("TLIST(A1),\"2000\",\"2001\",\"2003\"", Utils.Validation.ValueType.TimeValSeries)]
        [DataRow("TLIST(H1),\"20001\", \"20002\", \"20011\", \"20012\"", Utils.Validation.ValueType.TimeValSeries)] // Has spaces between values
        [DataRow("TLIST(T1),\"20001\",\"20002\",\"20003\"", Utils.Validation.ValueType.TimeValSeries)]
        [DataRow("TLIST(Q1),\"20001\",\"20002\",\"20003\",\"20004\"", Utils.Validation.ValueType.TimeValSeries)]
        [DataRow("TLIST(M1),\"200001\",\"200002\",\n\"200003\"", Utils.Validation.ValueType.TimeValSeries)] // Has newline between values
        [DataRow("TLIST(W1),\"200050\",\"200051\",\"200052\"", Utils.Validation.ValueType.TimeValSeries)]
        [DataRow("TLIST(D1),\"20001010\",\"20001011\",\"20001012\"", Utils.Validation.ValueType.TimeValSeries)]
        [DataRow("TLIST(A1, \"2000-2003\")", Utils.Validation.ValueType.TimeValRange)]
        [DataRow("TLIST(H1, \"20001-20012\")", Utils.Validation.ValueType.TimeValRange)]
        [DataRow("TLIST(T1, \"20001-20003\")", Utils.Validation.ValueType.TimeValRange)]
        [DataRow("TLIST(Q1, \"20001-20004\")", Utils.Validation.ValueType.TimeValRange)]
        [DataRow("TLIST(M1, \"200001-200003\")", Utils.Validation.ValueType.TimeValRange)]
        [DataRow("TLIST(W1, \"200050-200052\")", Utils.Validation.ValueType.TimeValRange)]
        [DataRow("TLIST(D1, \"20001010-20001012\")", Utils.Validation.ValueType.TimeValRange)]
        public void CorrectlyDefinedRangeAndSeriesTimeValuesReturnCorrectValueType(string timeval, Utils.Validation.ValueType type)
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = [ new("foo", new KeyValuePair<string, string>("foo-key", timeval), 0, [], 0) ];
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.InvalidValueFormat];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as ValidationFeedback ?? [];
            Utils.Validation.ValueType? valueType = getValueTypeFromStringMethod?.Invoke(null, [keyValuePairs.First().KeyValuePair.Value, PxFileSyntaxConf.Default]) as Utils.Validation.ValueType?;

            // Assert
            Assert.AreEqual(0, feedback.Count);
            Assert.AreEqual(type, valueType);
        }

        [TestMethod]
        [DataRow("TLIST(A1),2000,2001,2003", null)]
        [DataRow("\"TLIST(H1)\",\"20001\", \"20002\", \"20011\", \"20012\"", Utils.Validation.ValueType.ListOfStrings)]
        [DataRow("TLIST(T1)=\"20001\",\"20002\",\"20003\"", null)]
        [DataRow("TLIST(Q1):\"20001\",\"20002\",\"20003\",\"20004\"", null)]
        [DataRow("TLIST(M1),\"20001\",\"20002\",\"20003\"", null)]
        [DataRow("TLIST(W1),\"2000-50\",\"2000-51\",\"2000-52\"", null)]
        [DataRow("TLIST(D1),\"2000/1010\",\"2000/1011\",\"2000/1012\"", null)]
        [DataRow("TLIST(A1, 2000-2003)", null)]
        [DataRow("TLIST(H1, \"20001,20012\")", null)]
        [DataRow("TLIST(T1, \"20001/20003\"", null)]
        [DataRow("TLIST(Q1, \"2001-2004\")", null)]
        [DataRow("TLIST(M1, \"2000/01-2000/03\")", null)]
        [DataRow("TLIST(W1, \"2000.50-2000.52\")", null)]
        [DataRow("TLIST(D1, \"10/10/2000-10/11/2000\")", null)]
        public void IncorrectlyDefinedRangeAndSeriesTimeValuesReturnWithErrors(string timeval, Utils.Validation.ValueType? type)
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = [new("foo", new KeyValuePair<string, string>("foo-key", timeval), 0, [], 0)];
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.InvalidValueFormat];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as ValidationFeedback ?? [];
            Utils.Validation.ValueType? valueType = getValueTypeFromStringMethod?.Invoke(null, [keyValuePairs.First().KeyValuePair.Value, PxFileSyntaxConf.Default]) as Utils.Validation.ValueType?;

            // Assert
            if (type is null)
            {
                Assert.AreEqual(1, feedback.Count);
                Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, feedback.First().Key.Rule);
            }
            Assert.AreEqual(type, valueType);
        }

        [TestMethod]
        public void ValidatePxFileSyntaxCalledWithTimevalsSRetunrsWithValidResult()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_TIMEVALS);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();
            Encoding? encoding = reader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            SyntaxValidator validator = new();

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = validator.Validate(stream, "foo", encoding);
            Assert.AreEqual(0, result.FeedbackItems.Count);
        }

        [TestMethod]
        public void ValidatePxFileSyntaxCalledWithBadTimevalsReturnsErrors()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_BAD_TIMEVALS);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();
            Encoding? encoding = reader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            SyntaxValidator validator = new();

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = validator.Validate(stream, "foo", encoding);
            Assert.AreEqual(1, result.FeedbackItems.Count);
            Assert.AreEqual(9, result.FeedbackItems.First().Value[0].Line);
            Assert.AreEqual(16, result.FeedbackItems.First().Value[0].Character);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, result.FeedbackItems.First().Key.Rule);
            Assert.AreEqual(10, result.FeedbackItems.First().Value[1].Line);
            Assert.AreEqual(16, result.FeedbackItems.First().Value[1].Character);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, result.FeedbackItems.First().Key.Rule);
        }

        [TestMethod]
        public void TestWithCustomSyntaxValidationFunctions()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();
            Encoding? encoding = reader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);

            // Act
            SyntaxValidator validator = new(customValidationFunctions: new MockCustomSyntaxValidationFunctions());
            SyntaxValidationResult result = validator.Validate(stream, "foo", encoding);

            // Assert
            Assert.AreEqual(0, result.FeedbackItems.Count);
        }

        [TestMethod]
        public async Task TestWithCustomSyntaxValidationFunctionsAsync()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            PxFileMetadataReader reader = new();
            Encoding? encoding = await reader.GetEncodingAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);

            // Act
            SyntaxValidator validator = new(customValidationFunctions: new MockCustomSyntaxValidationFunctions());
            SyntaxValidationResult result = await validator.ValidateAsync(stream, "foo", encoding);

            // Assert
            Assert.AreEqual(0, result.FeedbackItems.Count);
        }
    }
}
