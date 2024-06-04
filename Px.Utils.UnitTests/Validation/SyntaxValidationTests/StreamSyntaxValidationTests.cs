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
        private readonly string filename = "foo";
        private readonly PxFileSyntaxConf syntaxConf = PxFileSyntaxConf.Default;
        private List<ValidationFeedbackItem> feedback = [];
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
            Encoding? encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            SyntaxValidator validator = new(stream, encoding, filename);

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = validator.Validate();
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
            feedback = entryValidationMethod?.Invoke(null, [entries, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(2, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.MultipleEntriesOnOneLine, feedback[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.MultipleEntriesOnOneLine, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidatePxFileSyntaxCalledWithWithSpecifiersSReturnsWithRightStructure()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_SPECIFIERS);
            using Stream stream = new MemoryStream(data);
            Encoding? encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            SyntaxValidator validator = new(stream, encoding, filename);

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = validator.Validate();
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
            Encoding? encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            SyntaxValidator validator = new(stream, encoding, filename);

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = validator.Validate();
            Assert.AreEqual(2, result.FeedbackItems.Length);
            Assert.AreEqual(9, result.FeedbackItems[0].Feedback.Line);
            Assert.AreEqual(18, result.FeedbackItems[0].Feedback.Character);
            Assert.AreEqual(12, result.FeedbackItems[1].Feedback.Line);
            Assert.AreEqual(40, result.FeedbackItems[1].Feedback.Character);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithMultipleLangParamsReturnsWithError()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_MULTIPLE_LANGUAGE_PARAMETERS;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.MoreThanOneLanguageParameter];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.MoreThanOneLanguageParameterSection, feedback[0].Feedback.Rule);
        }


        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithMultipleSpecifierParamsSReturnsWithError()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_MULTIPLE_SPECIFIER_PARAMETER_SECTIONS;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.MoreThanOneSpecifierParameter];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.MoreThanOneSpecifierParameterSection, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithWrongOrderAndMissingKeywordReturnsWithErrors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_IN_WRONG_ORDER_AND_MISSING_KEYWORD;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.WrongKeyOrderOrMissingKeyword];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(3, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeyHasWrongOrder, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.KeyHasWrongOrder, feedback[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.MissingKeyword, feedback[2].Feedback.Rule);
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

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(4, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.TooManySpecifiers, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.SpecifierPartNotEnclosed, feedback[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.SpecifierPartNotEnclosed, feedback[2].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.SpecifierDelimiterMissing, feedback[3].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithIllegalSymbolsInLanguageParamReturnsWithErrors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_ILLEGAL_SYMBOLS_IN_LANGUAGE_SECTIONS;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.IllegalSymbolsInLanguageParamSection];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(3, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInLanguageParameter, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInLanguageParameter, feedback[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInLanguageParameter, feedback[2].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithIllegalSymbolsInSpecifierParamReturnsWithErrors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_ILLEGAL_SYMBOLS_IN_SPECIFIER_SECTIONS;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.IllegalSymbolsInSpecifierParamSection];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInSpecifierParameter, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithBadValuesReturnsErrors()
        {

            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_BAD_VALUES;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.InvalidValueFormat];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(4, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, feedback[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, feedback[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithExcessListValueWhitespaceReturnsWithWarning()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_EXCESS_LIST_VALUE_WHITESPACE;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.ExcessWhitespaceInValue];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.ExcessWhitespaceInValue, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithExcessKeyWhitespaceReturnsWithWarning()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_EXCESS_KEY_WHITESPACE;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.KeyContainsExcessWhiteSpace];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeyContainsExcessWhiteSpace, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithKvpWithShortMultilineValueReturnsWithWarnings()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_SHORT_MULTILINE_VALUES;
            List<KeyValuePairValidationFunction> functions = [SyntaxValidationFunctions.ExcessNewLinesInValue];

            // Act
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(2, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.ExcessNewLinesInValue, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.ExcessNewLinesInValue, feedback[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithStructuredEntriesWithInvalidKeywordsReturnsWithErrors()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTURED_ENTRIES_WITH_INVALID_KEYWORDS;
            List<StructuredValidationFunction> functions = [SyntaxValidationFunctions.KeywordDoesntStartWithALetter, SyntaxValidationFunctions.KeywordContainsIllegalCharacters];

            // Act
            feedback = structuredValidationMethod?.Invoke(null, [structuredEntries, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(3, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeywordDoesntStartWithALetter, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInKeyword, feedback[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInKeyword, feedback[2].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithStructuredEntriesWithValidLanguagesReturnsWithNoFeedback()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTURED_ENTRIES_WITH_VALID_LANGUAGES;
            List<StructuredValidationFunction> functions = [SyntaxValidationFunctions.IllegalCharactersInLanguageParameter];

            // Act
            feedback = structuredValidationMethod?.Invoke(null, [structuredEntries, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(0, feedback.Count);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithStructuredEntriesWithInvalidLanguagesReturnsWithErrors()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTURED_ENTRIES_WITH_INVALID_LANGUAGES;
            List<StructuredValidationFunction> functions = [SyntaxValidationFunctions.IllegalCharactersInLanguageParameter];

            // Act
            feedback = structuredValidationMethod?.Invoke(null, [structuredEntries, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInLanguageParameter, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithStructuredEntriesWithIllegalCharactersInSpecifiersReturnsWithErrors()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTIRED_ENTRIES_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIERS;
            List<StructuredValidationFunction> functions = [SyntaxValidationFunctions.IllegalCharactersInSpecifierParts];

            // Act
            feedback = structuredValidationMethod?.Invoke(null, [structuredEntries, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInSpecifierPart, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithEntryWithoutValueReturnsWithError()
        {
            // Arrange
            List<ValidationEntry> entries = SyntaxValidationFixtures.ENTRY_WITHOUT_VALUE;
            List<EntryValidationFunction> functions = [SyntaxValidationFunctions.EntryWithoutValue];

            // Act
            feedback = entryValidationMethod?.Invoke(null, [entries, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.EntryWithoutValue, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithStructuredEntriesWithIncompliantLanguagesReturnsWithWarnings()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTURED_ENTRIES_WITH_INCOMPLIANT_LANGUAGES;
            List<StructuredValidationFunction> functions = [SyntaxValidationFunctions.IncompliantLanguage];

            // Act
            feedback = structuredValidationMethod?.Invoke(null, [structuredEntries, functions, syntaxConf] ) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(2, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IncompliantLanguage, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IncompliantLanguage, feedback[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithStructuredEntriesWithUnrecommendedKeywordNamingReturnsWithWarnings()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTURED_ENTRIES_WITH_UNRECOMMENDED_KEYWORD_NAMING;
            List<StructuredValidationFunction> functions = [SyntaxValidationFunctions.KeywordContainsUnderscore, SyntaxValidationFunctions.KeywordIsNotInUpperCase];

            // Act
            feedback = structuredValidationMethod?.Invoke(null, [structuredEntries, functions, syntaxConf] ) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(2, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeywordIsNotInUpperCase, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.KeywordContainsUnderscore, feedback[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjectsCalledWithLongKeywordReturnsWithWarnings()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTS_WITH_LONG_KEYWORD;
            List<StructuredValidationFunction> functions = [SyntaxValidationFunctions.KeywordIsExcessivelyLong];

            // Act
            feedback = structuredValidationMethod?.Invoke(null, [structuredEntries, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeywordExcessivelyLong, feedback[0].Feedback.Rule);
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
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];
            Utils.Validation.ValueType? valueType = getValueTypeFromStringMethod?.Invoke(null, [keyValuePairs[0].KeyValuePair.Value, PxFileSyntaxConf.Default]) as Utils.Validation.ValueType?;

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
            feedback = kvpValidationMethod?.Invoke(null, [keyValuePairs, functions, syntaxConf]) as List<ValidationFeedbackItem> ?? [];
            Utils.Validation.ValueType? valueType = getValueTypeFromStringMethod?.Invoke(null, [keyValuePairs[0].KeyValuePair.Value, PxFileSyntaxConf.Default]) as Utils.Validation.ValueType?;

            // Assert
            if (type is null)
            {
                Assert.AreEqual(1, feedback.Count);
                Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, feedback[0].Feedback.Rule);
            }
            Assert.AreEqual(type, valueType);
        }

        [TestMethod]
        public void ValidatePxFileSyntaxCalledWithTimevalsSRetunrsWithValidResult()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_TIMEVALS);
            using Stream stream = new MemoryStream(data);
            Encoding? encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            SyntaxValidator validator = new(stream, encoding, filename);

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = validator.Validate();
            Assert.AreEqual(0, result.FeedbackItems.Length);
        }

        [TestMethod]
        public void ValidatePxFileSyntaxCalledWithBadTimevalsReturnsErrors()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_BAD_TIMEVALS);
            using Stream stream = new MemoryStream(data);
            Encoding? encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            SyntaxValidator validator = new(stream, encoding, filename);

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = validator.Validate();
            Assert.AreEqual(2, result.FeedbackItems.Length);
            Assert.AreEqual(9, result.FeedbackItems[0].Feedback.Line);
            Assert.AreEqual(16, result.FeedbackItems[0].Feedback.Character);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, result.FeedbackItems[0].Feedback.Rule);
            Assert.AreEqual(10, result.FeedbackItems[1].Feedback.Line);
            Assert.AreEqual(16, result.FeedbackItems[1].Feedback.Character);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, result.FeedbackItems[1].Feedback.Rule);
        }

        [TestMethod]
        public void TestWithCustomSyntaxValidationFunctions()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            Encoding? encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);

            // Act
            SyntaxValidator validator = new(stream, encoding, filename, PxFileSyntaxConf.Default, new MockCustomSyntaxValidationFunctions());
            validator.Validate();

            // Assert
            Assert.AreEqual(0, feedback.Count);
        }

        [TestMethod]
        public async Task TestWithCustomSyntaxValidationFunctionsAsync()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            Encoding? encoding = await PxFileMetadataReader.GetEncodingAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);

            // Act
            SyntaxValidator validator = new(stream, encoding, filename, PxFileSyntaxConf.Default, new MockCustomSyntaxValidationFunctions());
            await validator.ValidateAsync();

            // Assert
            Assert.AreEqual(0, feedback.Count);
        }
    }
}
