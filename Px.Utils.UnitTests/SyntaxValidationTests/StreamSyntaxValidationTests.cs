using PxUtils.Validation.SyntaxValidation;
using PxUtils.UnitTests.SyntaxValidationTests.Fixtures;
using System.Text;
using System.Reflection;
using PxUtils.Validation;
using PxUtils.PxFile;
using PxUtils.PxFile.Meta;

namespace PxUtils.UnitTests.SyntaxValidationTests
{
    [TestClass]
    public class StreamSyntaxValidationTests
    {
        private readonly string filename = "foo";
        private readonly List<ValidationFeedbackItem> feedback = [];
        private readonly PxFileSyntaxConf syntaxConf = PxFileSyntaxConf.Default;
        private MethodInfo? entryValidationMethod;
        private MethodInfo? kvpValidationMethod;
        private MethodInfo? structuredValidationMethod;

        [TestInitialize]
        public void Initialize()
        {
            entryValidationMethod = typeof(SyntaxValidation)
                    .GetMethod("ValidateEntries", BindingFlags.NonPublic | BindingFlags.Static);
            kvpValidationMethod = typeof(SyntaxValidation)
                    .GetMethod("ValidateKeyValuePairs", BindingFlags.NonPublic | BindingFlags.Static);
            structuredValidationMethod = typeof(SyntaxValidation)
                    .GetMethod("ValidateStructs", BindingFlags.NonPublic | BindingFlags.Static);
        }

        [TestMethod]
        public void ValidatePxFileSyntax_CalledWith_MINIMAL_UTF8_Returns_Valid_Result()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            Encoding? encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileMetadataSyntax(stream, encoding, filename);
            Assert.AreEqual(8, result.Result.Count);
            Assert.AreEqual(0, feedback.Count);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_MULTIPLE_ENTRIES_IN_SINGLE_LINE_Returns_Result_With_Warnings()
        {
            // Arrange
            List<ValidationEntry> entries = SyntaxValidationFixtures.MULTIPLE_ENTRIES_IN_SINGLE_LINE;
            List<EntryValidationFunctionDelegate> functions = [SyntaxValidationFunctions.MultipleEntriesOnLine];

            // Act
            entryValidationMethod?.Invoke(null, new object[] { entries, functions, feedback, syntaxConf });

            Assert.AreEqual(2, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.MultipleEntriesOnOneLine, feedback[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.MultipleEntriesOnOneLine, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidatePxFileSyntax_CalledWith_UTF8_N_WITH_SPECIFIERS_Returns_Result_With_Right_Structure()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_SPECIFIERS);
            using Stream stream = new MemoryStream(data);
            Encoding? encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileMetadataSyntax(stream, encoding, filename);
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
        public void ValidatePxFileSyntax_CalledWith_UTF8_N_WITH_FEEDBACKS_Returns_Right_Line_And_Character_Indexes()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_FEEDBACKS);
            using Stream stream = new MemoryStream(data);
            Encoding? encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileMetadataSyntax(stream, encoding, filename);
            Assert.AreEqual(2, result.FeedbackItems.Count);
            Assert.AreEqual(9, result.FeedbackItems[0].Feedback.Line);
            Assert.AreEqual(18, result.FeedbackItems[0].Feedback.Character);
            Assert.AreEqual(12, result.FeedbackItems[1].Feedback.Line);
            Assert.AreEqual(40, result.FeedbackItems[1].Feedback.Character);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_WITH_MULTIPLE_LANGUAGE_PARAMETERS_Returns_Result_With_Error()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_MULTIPLE_LANGUAGE_PARAMETERS;
            List<KeyValuePairValidationFunctionDelegate> functions = [SyntaxValidationFunctions.MoreThanOneLanguageParameter];

            // Act
            kvpValidationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.MoreThanOneLanguageParameterSection, feedback[0].Feedback.Rule);
        }


        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIR_WITH_MULTIPLE_SPECIFIER_PARAMETER_SECTIONS_Returns_Result_With_Error()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_MULTIPLE_SPECIFIER_PARAMETER_SECTIONS;
            List<KeyValuePairValidationFunctionDelegate> functions = [SyntaxValidationFunctions.MoreThanOneSpecifierParameter];

            // Act
            kvpValidationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.MoreThanOneSpecifierParameterSection, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_IN_WRONG_ORDER_AND_MISSING_KEYWORD_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_IN_WRONG_ORDER_AND_MISSING_KEYWORD;
            List<KeyValuePairValidationFunctionDelegate> functions = [SyntaxValidationFunctions.WrongKeyOrderOrMissingKeyword];

            // Act
            kvpValidationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(3, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeyHasWrongOrder, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.KeyHasWrongOrder, feedback[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.MissingKeyword, feedback[2].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_WITH_INVALID_SPECIFIERS_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_INVALID_SPECIFIERS;
            List<KeyValuePairValidationFunctionDelegate> functions = [
                SyntaxValidationFunctions.MoreThanTwoSpecifierParts,
                SyntaxValidationFunctions.SpecifierPartNotEnclosed, 
                SyntaxValidationFunctions.NoDelimiterBetweenSpecifierParts];

            // Act
            kvpValidationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(4, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.TooManySpecifiers, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.SpecifierPartNotEnclosed, feedback[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.SpecifierPartNotEnclosed, feedback[2].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.SpecifierDelimiterMissing, feedback[3].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_WITH_ILLEGAL_SYMBOLS_IN_LANGUAGE_SECTION_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_ILLEGAL_SYMBOLS_IN_LANGUAGE_SECTIONS;
            List<KeyValuePairValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IllegalSymbolsInLanguageParamSection];

            // Act
            kvpValidationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(3, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInLanguageParameter, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInLanguageParameter, feedback[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInLanguageParameter, feedback[2].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIR_WITH_ILLEGAL_SYMBOLS_IN_SPECIFIER_SECTIONS_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_ILLEGAL_SYMBOLS_IN_SPECIFIER_SECTIONS;
            List<KeyValuePairValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IllegalSymbolsInSpecifierParamSection];

            // Act
            kvpValidationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInSpecifierParameter, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_WITH_BAD_VALUES_Returns_Errors()
        {

            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_BAD_VALUES;
            List<KeyValuePairValidationFunctionDelegate> functions = [SyntaxValidationFunctions.InvalidValueFormat];

            // Act
            kvpValidationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(4, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, feedback[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, feedback[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIR_WITH_EXCESS_LIST_VALUE_WHITESPACE_Returns_Result_With_Warning()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_EXCESS_LIST_VALUE_WHITESPACE;
            List<KeyValuePairValidationFunctionDelegate> functions = [SyntaxValidationFunctions.ExcessWhitespaceInValue];

            // Act
            kvpValidationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.ExcessWhitespaceInValue, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIR_WITH_EXCESS_KEY_WHITESPACE_Returns_Result_With_Warning()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_EXCESS_KEY_WHITESPACE;
            List<KeyValuePairValidationFunctionDelegate> functions = [SyntaxValidationFunctions.KeyContainsExcessWhiteSpace];

            // Act
            kvpValidationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeyContainsExcessWhiteSpace, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_WITH_SHORT_MULTILINE_VALUES_Returns_Result_With_Warnings()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_SHORT_MULTILINE_VALUES;
            List<KeyValuePairValidationFunctionDelegate> functions = [SyntaxValidationFunctions.ExcessNewLinesInValue];

            // Act
            kvpValidationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(2, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.ExcessNewLinesInValue, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.ExcessNewLinesInValue, feedback[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTURED_ENTRIES_WITH_INVALID_KEYWORDS_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTURED_ENTRIES_WITH_INVALID_KEYWORDS;
            List<StructuredValidationFunctionDelegate> functions = [SyntaxValidationFunctions.KeywordDoesntStartWithALetter, SyntaxValidationFunctions.KeywordContainsIllegalCharacters];

            // Act
            structuredValidationMethod?.Invoke(null, new object[] { structuredEntries, functions, feedback, syntaxConf });

            Assert.AreEqual(3, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeywordDoesntStartWithALetter, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInKeyword, feedback[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInKeyword, feedback[2].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTURED_ENTRIES_WITH_VALID_LANGUAGES_Returns_With_No_Feedback()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTURED_ENTRIES_WITH_VALID_LANGUAGES;
            List<StructuredValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IllegalCharactersInLanguageParameter];

            // Act
            structuredValidationMethod?.Invoke(null, new object[] { structuredEntries, functions, feedback, syntaxConf });

            Assert.AreEqual(0, feedback.Count);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTURED_ENTRIES_WITH_INVALID_LANGUAGES_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTURED_ENTRIES_WITH_INVALID_LANGUAGES;
            List<StructuredValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IllegalCharactersInLanguageParameter];

            // Act
            structuredValidationMethod?.Invoke(null, new object[] { structuredEntries, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInLanguageParameter, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTURED_ENTRIES_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIERS_Returns_With_Errors()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTIRED_ENTRIES_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIERS;
            List<StructuredValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IllegalCharactersInSpecifierParts];

            // Act
            structuredValidationMethod?.Invoke(null, new object[] { structuredEntries, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInSpecifierPart, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_ENTRY_WITHOUT_VALUE_Returns_With_Errors()
        {
            // Arrange
            List<ValidationEntry> entries = SyntaxValidationFixtures.ENTRY_WITHOUT_VALUE;
            List<EntryValidationFunctionDelegate> functions = [SyntaxValidationFunctions.EntryWithoutValue];

            // Act
            entryValidationMethod?.Invoke(null, new object[] { entries, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.EntryWithoutValue, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTURED_ENTRIES_WITH_INCOMPLIANT_LANGUAGES_Returns_With_Warnings()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTURED_ENTRIES_WITH_INCOMPLIANT_LANGUAGES;
            List<StructuredValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IncompliantLanguage];

            // Act
            structuredValidationMethod?.Invoke(null, new object[] { structuredEntries, functions, feedback, syntaxConf });

            Assert.AreEqual(2, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IncompliantLanguage, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IncompliantLanguage, feedback[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTURED_ENTRIES_WITH_UNRECOMMENDED_KEYWORD_NAMING_Returns_With_Warnings()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTURED_ENTRIES_WITH_UNRECOMMENDED_KEYWORD_NAMING;
            List<StructuredValidationFunctionDelegate> functions = [SyntaxValidationFunctions.KeywordContainsUnderscore, SyntaxValidationFunctions.KeywordIsNotInUpperCase];

            // Act
            structuredValidationMethod?.Invoke(null, new object[] { structuredEntries, functions, feedback, syntaxConf });

            Assert.AreEqual(2, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeywordIsNotInUpperCase, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.KeywordContainsUnderscore, feedback[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTS_WITH_LONG_KEYWORD_Returns_With_Warnings()
        {
            // Arrange
            List<ValidationStructuredEntry> structuredEntries = SyntaxValidationFixtures.STRUCTS_WITH_LONG_KEYWORD;
            List<StructuredValidationFunctionDelegate> functions = [SyntaxValidationFunctions.KeywordIsExcessivelyLong];

            // Act
            structuredValidationMethod?.Invoke(null, new object[] { structuredEntries, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeywordExcessivelyLong, feedback[0].Feedback.Rule);
        }
    }
}
