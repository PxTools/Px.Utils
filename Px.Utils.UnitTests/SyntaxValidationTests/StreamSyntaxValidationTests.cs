using PxUtils.Validation.SyntaxValidation;
using PxUtils.UnitTests.SyntaxValidationTests.Fixtures;
using System.Text;
using PxUtils.Validation;
using PxUtils.PxFile;

namespace PxUtils.UnitTests.SyntaxValidationTests
{
    [TestClass]
    public class StreamSyntaxValidationTests
    {
        private readonly string filename = "foo";
        private readonly ValidationReport report = new();
        private readonly PxFileSyntaxConf syntaxConf = PxFileSyntaxConf.Default;

        [TestMethod]
        public void ValidatePxFileSyntax_CalledWith_MINIMAL_UTF8_Returns_Valid_Result()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            Encoding? encoding = SyntaxValidation.GetEncoding(stream, PxFileSyntaxConf.Default, report, filename);
            stream.Seek(0, SeekOrigin.Begin);

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileMetadataSyntax(stream, encoding, filename, report);
            Assert.AreEqual(8, result.Result.Count);
            Assert.AreEqual(0, result.Report.FeedbackItems?.Count);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_MULTIPLE_ENTRIES_IN_SINGLE_LINE_Returns_Result_With_Warnings()
        {
            // Arrange
            List<ValidationEntry> entries = SyntaxValidationFixtures.MULTIPLE_ENTRIES_IN_SINGLE_LINE;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.MultipleEntriesOnLine];

            // Act
            SyntaxValidation.ValidateObjects(entries, functions, report, syntaxConf);

            Assert.AreEqual(2, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.MultipleEntriesOnOneLine, report.FeedbackItems?[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.MultipleEntriesOnOneLine, report.FeedbackItems?[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidatePxFileSyntax_CalledWith_UTF8_N_WITH_SPECIFIERS_Returns_Result_With_Right_Structure()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_SPECIFIERS);
            using Stream stream = new MemoryStream(data);
            Encoding? encoding = SyntaxValidation.GetEncoding(stream, PxFileSyntaxConf.Default, report, filename);
            stream.Seek(0, SeekOrigin.Begin);

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileMetadataSyntax(stream, encoding, filename, report);
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
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_WITH_MULTIPLE_LANGUAGE_PARAMETERS_Returns_Result_With_Error()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_MULTIPLE_LANGUAGE_PARAMETERS;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.MoreThanOneLanguageParameter];

            // Act
            SyntaxValidation.ValidateObjects(keyValuePairs, functions, report, syntaxConf);

            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.MoreThanOneLanguageParameterSection, report.FeedbackItems?[0].Feedback.Rule);
        }


        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIR_WITH_MULTIPLE_SPECIFIER_PARAMETER_SECTIONS_Returns_Result_With_Error()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_MULTIPLE_SPECIFIER_PARAMETER_SECTIONS;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.MoreThanOneSpecifierParameter];

            // Act
            SyntaxValidation.ValidateObjects(keyValuePairs, functions, report, syntaxConf);

            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.MoreThanOneSpecifierParameterSection, report.FeedbackItems?[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_IN_WRONG_ORDER_AND_MISSING_KEYWORD_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_IN_WRONG_ORDER_AND_MISSING_KEYWORD;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.WrongKeyOrderOrMissingKeyword];

            // Act
            SyntaxValidation.ValidateObjects(keyValuePairs, functions, report, syntaxConf);

            Assert.AreEqual(3, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeyHasWrongOrder, report.FeedbackItems?[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.KeyHasWrongOrder, report.FeedbackItems?[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.MissingKeyword, report.FeedbackItems?[2].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_WITH_INVALID_SPECIFIERS_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_INVALID_SPECIFIERS;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.MoreThanTwoSpecifierParts, SyntaxValidationFunctions.SpecifierPartNotEnclosed, SyntaxValidationFunctions.NoDelimiterBetweenSpecifierParts];

            // Act
            SyntaxValidation.ValidateObjects(keyValuePairs, functions, report, syntaxConf);

            Assert.AreEqual(3, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.TooManySpecifiers, report.FeedbackItems?[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.SpecifierPartNotEnclosed, report.FeedbackItems?[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.SpecifierDelimiterMissing, report.FeedbackItems?[2].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_WITH_ILLEGAL_SYMBOLS_IN_LANGUAGE_SECTION_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_ILLEGAL_SYMBOLS_IN_LANGUAGE_SECTIONS;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IllegalSymbolsInLanguageParamSection];

            // Act
            SyntaxValidation.ValidateObjects(keyValuePairs, functions, report, syntaxConf);

            Assert.AreEqual(3, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInLanguageParameter, report.FeedbackItems?[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInLanguageParameter, report.FeedbackItems?[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInLanguageParameter, report.FeedbackItems?[2].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIR_WITH_ILLEGAL_SYMBOLS_IN_SPECIFIER_SECTIONS_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_ILLEGAL_SYMBOLS_IN_SPECIFIER_SECTIONS;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IllegalSymbolsInSpecifierParamSection];

            // Act
            SyntaxValidation.ValidateObjects(keyValuePairs, functions, report, syntaxConf);

            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInSpecifierParameter, report.FeedbackItems?[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_WITH_BAD_VALUES_Returns_Errors()
        {

            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_BAD_VALUES;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.InvalidValueFormat];

            // Act
            SyntaxValidation.ValidateObjects(keyValuePairs, functions, report, syntaxConf);

            Assert.AreEqual(4, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, report.FeedbackItems?[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, report.FeedbackItems?[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, report.FeedbackItems?[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.InvalidValueFormat, report.FeedbackItems?[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIR_WITH_EXCESS_LIST_VALUE_WHITESPACE_Returns_Result_With_Warning()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_EXCESS_LIST_VALUE_WHITESPACE;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.ExcessWhitespaceInValue];

            // Act
            SyntaxValidation.ValidateObjects(keyValuePairs, functions, report, syntaxConf);

            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.ExcessWhitespaceInValue, report.FeedbackItems?[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIR_WITH_EXCESS_KEY_WHITESPACE_Returns_Result_With_Warning()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_EXCESS_KEY_WHITESPACE;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.KeyContainsExcessWhiteSpace];

            // Act
            SyntaxValidation.ValidateObjects(keyValuePairs, functions, report, syntaxConf);

            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeyContainsExcessWhiteSpace, report.FeedbackItems?[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_WITH_SHORT_MULTILINE_VALUES_Returns_Result_With_Warnings()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_SHORT_MULTILINE_VALUES;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.ExcessNewLinesInValue];

            // Act
            SyntaxValidation.ValidateObjects(keyValuePairs, functions, report, syntaxConf);

            Assert.AreEqual(2, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.ExcessNewLinesInValue, report.FeedbackItems?[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.ExcessNewLinesInValue, report.FeedbackItems?[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTS_WITH_INVALID_KEYWORDS_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationStruct> structs = SyntaxValidationFixtures.STRUCTS_WITH_INVALID_KEYWORDS;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.KeywordDoesntStartWithALetter, SyntaxValidationFunctions.KeywordContainsIllegalCharacters];

            // Act
            SyntaxValidation.ValidateObjects(structs, functions, report, syntaxConf);

            Assert.AreEqual(3, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeywordDoesntStartWithALetter, report.FeedbackItems?[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInKeyword, report.FeedbackItems?[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInKeyword, report.FeedbackItems?[2].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTS_WITH_VALID_LANGUAGES_Returns_With_No_Feedback()
        {
            // Arrange
            List<ValidationStruct> structs = SyntaxValidationFixtures.STRUCTS_WITH_VALID_LANGUAGES;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IllegalCharactersInLanguageParameter];

            // Act
            SyntaxValidation.ValidateObjects(structs, functions, report, syntaxConf);

            Assert.AreEqual(0, report.FeedbackItems?.Count);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTS_WITH_INVALID_LANGUAGES_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationStruct> structs = SyntaxValidationFixtures.STRUCTS_WITH_INVALID_LANGUAGES;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IllegalCharactersInLanguageParameter];

            // Act
            SyntaxValidation.ValidateObjects(structs, functions, report, syntaxConf);

            // This test also catches an earlier issue with excess whitespace in the key part
            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInLanguageParameter, report.FeedbackItems?[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTS_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIERS_Returns_With_Errors()
        {
            // Arrange
            List<ValidationStruct> structs = SyntaxValidationFixtures.STRUCT_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIERS;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IllegalCharactersInSpecifierParts];

            // Act
            SyntaxValidation.ValidateObjects(structs, functions, report, syntaxConf);

            // This test also catches an earlier issue with excess whitespace in the key part
            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInSpecifierPart, report.FeedbackItems?[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_ENTRY_WITHOUT_VALUE_Returns_With_Errors()
        {
            // Arrange
            List<ValidationEntry> entries = SyntaxValidationFixtures.ENTRY_WITHOUT_VALUE;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.EntryWithoutValue];

            // Act
            SyntaxValidation.ValidateObjects(entries, functions, report, syntaxConf);

            // This test also catches an earlier issue with excess whitespace in the key part
            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.EntryWithoutValue, report.FeedbackItems?[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTS_WITH_INCOMPLIANT_LANGUAGES_Returns_With_Warnings()
        {
            // Arrange
            List<ValidationStruct> structs = SyntaxValidationFixtures.STRUCTS_WITH_INCOMPLIANT_LANGUAGES;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IncompliantLanguage];

            // Act
            SyntaxValidation.ValidateObjects(structs, functions, report, syntaxConf);

            // This test also catches an earlier issue with excess whitespace in the key part
            Assert.AreEqual(2, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.IncompliantLanguage, report.FeedbackItems?[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IncompliantLanguage, report.FeedbackItems?[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTS_WITH_UNRECOMMENDED_KEYWORD_NAMING_Returns_With_Warnings()
        {
            // Arrange
            List<ValidationStruct> structs = SyntaxValidationFixtures.STRUCTS_WITH_UNRECOMMENDED_KEYWORD_NAMING;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.KeywordContainsUnderscore, SyntaxValidationFunctions.KeywordIsNotInUpperCase];

            // Act
            SyntaxValidation.ValidateObjects(structs, functions, report, syntaxConf);

            Assert.AreEqual(2, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeywordIsNotInUpperCase, report.FeedbackItems?[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.KeywordContainsUnderscore, report.FeedbackItems?[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCT_WITH_LONG_KEYWORD_Returns_With_Warnings()
        {
            // Arrange
            List<ValidationStruct> structs = SyntaxValidationFixtures.STRUCT_WITH_LONG_KEYWORD;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.KeywordIsExcessivelyLong];

            // Act
            SyntaxValidation.ValidateObjects(structs, functions, report, syntaxConf);

            // This test also catches an earlier issue with excess whitespace in the key part
            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeywordExcessivelyLong, report.FeedbackItems?[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidatePxFileSyntax_CalledWith_UNKNOWN_ENCODING_Returns_With_Error()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UNKNOWN_ENCODING);
            using Stream stream = new MemoryStream(data);
            Encoding? encoding = SyntaxValidation.GetEncoding(stream, PxFileSyntaxConf.Default, report, filename);
            stream.Seek(0, SeekOrigin.Begin);

            // Assert
            Assert.IsNull(encoding);
            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.AreEqual(ValidationFeedbackRule.NoEncoding, report.FeedbackItems?[0].Feedback.Rule);
        }
    }
}
