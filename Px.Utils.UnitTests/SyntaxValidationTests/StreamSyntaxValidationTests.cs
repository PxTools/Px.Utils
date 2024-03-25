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
        private MethodInfo? validationMethod;

        [TestInitialize]
        public void Initialize()
        {
            validationMethod = typeof(SyntaxValidation)
                    .GetMethod("ValidateObjects", BindingFlags.NonPublic | BindingFlags.Static);
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
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.MultipleEntriesOnLine];

            // Act
            validationMethod?.Invoke(null, new object[] { entries, functions, feedback, syntaxConf });

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
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_WITH_MULTIPLE_LANGUAGE_PARAMETERS_Returns_Result_With_Error()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_MULTIPLE_LANGUAGE_PARAMETERS;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.MoreThanOneLanguageParameter];

            // Act
            validationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.MoreThanOneLanguageParameterSection, feedback[0].Feedback.Rule);
        }


        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIR_WITH_MULTIPLE_SPECIFIER_PARAMETER_SECTIONS_Returns_Result_With_Error()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_MULTIPLE_SPECIFIER_PARAMETER_SECTIONS;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.MoreThanOneSpecifierParameter];

            // Act
            validationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.MoreThanOneSpecifierParameterSection, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_IN_WRONG_ORDER_AND_MISSING_KEYWORD_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_IN_WRONG_ORDER_AND_MISSING_KEYWORD;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.WrongKeyOrderOrMissingKeyword];

            // Act
            validationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

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
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.MoreThanTwoSpecifierParts, SyntaxValidationFunctions.SpecifierPartNotEnclosed, SyntaxValidationFunctions.NoDelimiterBetweenSpecifierParts];

            // Act
            validationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(3, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.TooManySpecifiers, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.SpecifierPartNotEnclosed, feedback[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.SpecifierDelimiterMissing, feedback[2].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_WITH_ILLEGAL_SYMBOLS_IN_LANGUAGE_SECTION_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_ILLEGAL_SYMBOLS_IN_LANGUAGE_SECTIONS;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IllegalSymbolsInLanguageParamSection];

            // Act
            validationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

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
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IllegalSymbolsInSpecifierParamSection];

            // Act
            validationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInSpecifierParameter, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_WITH_BAD_VALUES_Returns_Errors()
        {

            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_BAD_VALUES;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.InvalidValueFormat];

            // Act
            validationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

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
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.ExcessWhitespaceInValue];

            // Act
            validationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.ExcessWhitespaceInValue, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIR_WITH_EXCESS_KEY_WHITESPACE_Returns_Result_With_Warning()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIR_WITH_EXCESS_KEY_WHITESPACE;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.KeyContainsExcessWhiteSpace];

            // Act
            validationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeyContainsExcessWhiteSpace, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_KEYVALUEPAIRS_WITH_SHORT_MULTILINE_VALUES_Returns_Result_With_Warnings()
        {
            // Arrange
            List<ValidationKeyValuePair> keyValuePairs = SyntaxValidationFixtures.KEYVALUEPAIRS_WITH_SHORT_MULTILINE_VALUES;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.ExcessNewLinesInValue];

            // Act
            validationMethod?.Invoke(null, new object[] { keyValuePairs, functions, feedback, syntaxConf });

            Assert.AreEqual(2, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.ExcessNewLinesInValue, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.ExcessNewLinesInValue, feedback[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTS_WITH_INVALID_KEYWORDS_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationStruct> structs = SyntaxValidationFixtures.STRUCTS_WITH_INVALID_KEYWORDS;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.KeywordDoesntStartWithALetter, SyntaxValidationFunctions.KeywordContainsIllegalCharacters];

            // Act
            validationMethod?.Invoke(null, new object[] { structs, functions, feedback, syntaxConf });

            Assert.AreEqual(3, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeywordDoesntStartWithALetter, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInKeyword, feedback[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInKeyword, feedback[2].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTS_WITH_VALID_LANGUAGES_Returns_With_No_Feedback()
        {
            // Arrange
            List<ValidationStruct> structs = SyntaxValidationFixtures.STRUCTS_WITH_VALID_LANGUAGES;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IllegalCharactersInLanguageParameter];

            // Act
            validationMethod?.Invoke(null, new object[] { structs, functions, feedback, syntaxConf });

            Assert.AreEqual(0, feedback.Count);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTS_WITH_INVALID_LANGUAGES_Returns_Result_With_Errors()
        {
            // Arrange
            List<ValidationStruct> structs = SyntaxValidationFixtures.STRUCTS_WITH_INVALID_LANGUAGES;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IllegalCharactersInLanguageParameter];

            // Act
            validationMethod?.Invoke(null, new object[] { structs, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInLanguageParameter, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTS_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIERS_Returns_With_Errors()
        {
            // Arrange
            List<ValidationStruct> structs = SyntaxValidationFixtures.STRUCT_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIERS;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IllegalCharactersInSpecifierParts];

            // Act
            validationMethod?.Invoke(null, new object[] { structs, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalCharactersInSpecifierPart, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_ENTRY_WITHOUT_VALUE_Returns_With_Errors()
        {
            // Arrange
            List<ValidationEntry> entries = SyntaxValidationFixtures.ENTRY_WITHOUT_VALUE;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.EntryWithoutValue];

            // Act
            validationMethod?.Invoke(null, new object[] { entries, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.EntryWithoutValue, feedback[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTS_WITH_INCOMPLIANT_LANGUAGES_Returns_With_Warnings()
        {
            // Arrange
            List<ValidationStruct> structs = SyntaxValidationFixtures.STRUCTS_WITH_INCOMPLIANT_LANGUAGES;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.IncompliantLanguage];

            // Act
            validationMethod?.Invoke(null, new object[] { structs, functions, feedback, syntaxConf });

            Assert.AreEqual(2, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.IncompliantLanguage, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.IncompliantLanguage, feedback[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCTS_WITH_UNRECOMMENDED_KEYWORD_NAMING_Returns_With_Warnings()
        {
            // Arrange
            List<ValidationStruct> structs = SyntaxValidationFixtures.STRUCTS_WITH_UNRECOMMENDED_KEYWORD_NAMING;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.KeywordContainsUnderscore, SyntaxValidationFunctions.KeywordIsNotInUpperCase];

            // Act
            validationMethod?.Invoke(null, new object[] { structs, functions, feedback, syntaxConf });

            Assert.AreEqual(2, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeywordIsNotInUpperCase, feedback[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.KeywordContainsUnderscore, feedback[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateObjects_CalledWith_STRUCT_WITH_LONG_KEYWORD_Returns_With_Warnings()
        {
            // Arrange
            List<ValidationStruct> structs = SyntaxValidationFixtures.STRUCT_WITH_LONG_KEYWORD;
            List<ValidationFunctionDelegate> functions = [SyntaxValidationFunctions.KeywordIsExcessivelyLong];

            // Act
            validationMethod?.Invoke(null, new object[] { structs, functions, feedback, syntaxConf });

            Assert.AreEqual(1, feedback.Count);
            Assert.AreEqual(ValidationFeedbackRule.KeywordExcessivelyLong, feedback[0].Feedback.Rule);
        }
    }
}
