using PxUtils.Validation.SyntaxValidation;
using PxUtils.UnitTests.SyntaxValidationTests.Fixtures;
using System.Text;
using PxUtils.Validation;

namespace PxUtils.UnitTests.SyntaxValidationTests
{
    [TestClass]
    public class StreamSyntaxValidationTests
    {
        [TestMethod]
        public void ValidatePxFileSyntax_CalledWith_MINIMAL_UTF8_Returns_Valid_Result()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(8, result.StructuredEntries.Count);
            Assert.AreEqual(0, result.Report.FeedbackItems?.Count);
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_MULTIPLE_ENTRIES_IN_SINGLE_LINE_Returns_Result_With_Warnings()
        {
            // Arrange
            List<StringValidationEntry> entries = SyntaxValidationFixtures.MULTIPLE_ENTRIES_IN_SINGLE_LINE;
            List<IValidationFunction> functions = [new MultipleEntriesOnLine()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            Assert.AreEqual(2, report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackMultipleEntriesOnLine));
            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackMultipleEntriesOnLine));
        }

        [TestMethod]
        public void ValidatePxFileSyntax_CalledWith_UTF8_N_WITH_SPECIFIERS_Returns_Result_With_Right_Structure()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_SPECIFIERS);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(10, result.StructuredEntries.Count);
            Assert.AreEqual("YES", result.StructuredEntries[8].Value);
            Assert.AreEqual("NO", result.StructuredEntries[9].Value);
            Assert.AreEqual("fi", result.StructuredEntries[8].Key.Language);
            Assert.AreEqual("fi", result.StructuredEntries[9].Key.Language);
            Assert.AreEqual("first_specifier", result.StructuredEntries[8].Key.FirstSpecifier);
            Assert.AreEqual("first_specifier", result.StructuredEntries[9].Key.FirstSpecifier);
            Assert.AreEqual("second_specifier", result.StructuredEntries[8].Key.SecondSpecifier);
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRY_WITH_MULTIPLE_LANGUAGE_PARAMETERS_Returns_Result_With_Error()
        {
            // Arrange
            List<KeyValuePairValidationEntry> entries = SyntaxValidationFixtures.ENTRY_WITH_MULTIPLE_LANGUAGE_PARAMETERS;
            List<IValidationFunction> functions = [new MoreThanOneLanguageParameter()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackMoreThanOneLanguage));
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRY_WITH_MULTIPLE_SPECIFIER_PARAMETER_SECTIONS_Returns_Result_With_Error()
        {
            // Arrange
            List<KeyValuePairValidationEntry> entries = SyntaxValidationFixtures.ENTRY_WITH_MULTIPLE_SPECIFIER_PARAMETER_SECTIONS;
            List<IValidationFunction> functions = [new MoreThanOneSpecifierParameter()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackMoreThanOneSpecifier));
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRIES_WITH_INVALID_SPECIFIERS_Returns_Result_With_Errors()
        {
            // Arrange
            List<KeyValuePairValidationEntry> entries = SyntaxValidationFixtures.ENTRIES_IN_WRONG_ORDER_AND_MISSING_KEYWORD;
            List<IValidationFunction> functions = [new WrongKeyOrderOrMissingKeyword()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            Assert.AreEqual(3, report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackKeyHasWrongOrder));
            Assert.IsInstanceOfType(report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackKeyHasWrongOrder));
            Assert.IsInstanceOfType(report.FeedbackItems?[2].Feedback, typeof(SyntaxValidationFeedbackMissingKeyword));
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRIES_IN_WRONG_ORDER_AND_MISSING_KEYWORD_Returns_Result_With_Errors()
        {
            // Arrange
            List<KeyValuePairValidationEntry> entries = SyntaxValidationFixtures.ENTRIES_WITH_INVALID_SPECIFIERS;
            List<IValidationFunction> functions = [new InvalidSpecifier()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            Assert.AreEqual(3, report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackInvalidSpecifier));
            Assert.IsInstanceOfType(report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackInvalidSpecifier));
            Assert.IsInstanceOfType(report.FeedbackItems?[2].Feedback, typeof(SyntaxValidationFeedbackInvalidSpecifier));
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRIES_WITH_ILLEGAL_SYMBOLS_IN_PARAM_SECTIONS_Returns_Result_With_Errors()
        {
            // Arrange
            List<KeyValuePairValidationEntry> entries = SyntaxValidationFixtures.ENTRIES_WITH_ILLEGAL_SYMBOLS_IN_PARAM_SECTIONS;
            List<IValidationFunction> functions = [new IllegalSymbolsInKeyParamSection()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            Assert.AreEqual(3, report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackIllegalSymbolsInParamSections));
            Assert.IsInstanceOfType(report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackIllegalSymbolsInParamSections));
            Assert.IsInstanceOfType(report.FeedbackItems?[2].Feedback, typeof(SyntaxValidationFeedbackIllegalSymbolsInParamSections));
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRIES_WITH_CORRECTLY_FORMATTED_LIST_AND_MULTILINE_STRING_Returns_No_Feedback()
        {
            // Arrange
            List<KeyValuePairValidationEntry> entries = SyntaxValidationFixtures.ENTRIES_WITH_CORRECTLY_FORMATTED_LIST_AND_MULTILINE_STRING;
            List<IValidationFunction> functions = [new IllegalValueFormat()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            Assert.AreEqual(0, report.FeedbackItems?.Count);
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRIES_WITH_BAD_VALUES_Returns_Valid_Errors()
        {

            // Arrange
            List<KeyValuePairValidationEntry> entries = SyntaxValidationFixtures.ENTRIES_WITH_BAD_VALUES;
            List<IValidationFunction> functions = [new IllegalValueFormat()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            Assert.AreEqual(3, report.FeedbackItems?.Count);

            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackInvalidValueSection));
            Assert.IsInstanceOfType(report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackInvalidValueSection));
            Assert.IsInstanceOfType(report.FeedbackItems?[2].Feedback, typeof(SyntaxValidationFeedbackInvalidValueSection));
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRY_WITH_EXCESS_LIST_VALUE_WHITESPACE_Returns_Result_With_Warning()
        {
            // Arrange
            List<KeyValuePairValidationEntry> entries = SyntaxValidationFixtures.ENTRY_WITH_EXCESS_LIST_VALUE_WHITESPACE;
            List<IValidationFunction> functions = [new ExcessWhitespaceInValue()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackValueContainsExcessWhitespace));
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRY_WITH_SHORT_MULTILINE_VALUES_Returns_Result_With_Warnings()
        {
            // Arrange
            List<KeyValuePairValidationEntry> entries = SyntaxValidationFixtures.ENTRY_WITH_SHORT_MULTILINE_VALUES;
            List<IValidationFunction> functions = [new ExcessNewLinesInValue()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            Assert.AreEqual(2, report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackExcessNewLinesInValue));
            Assert.IsInstanceOfType(report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackExcessNewLinesInValue));
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRIES_WITH_INVALID_KEYWORDS_Returns_Result_With_Errors()
        {
            // Arrange
            List<StructuredValidationEntry> entries = SyntaxValidationFixtures.ENTRIES_WITH_INVALID_KEYWORDS;
            List<IValidationFunction> functions = [new InvalidKeywordFormat()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            Assert.AreEqual(3, report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackInvalidKeywordFormat));
            Assert.IsInstanceOfType(report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackInvalidKeywordFormat));
            Assert.IsInstanceOfType(report.FeedbackItems?[2].Feedback, typeof(SyntaxValidationFeedbackInvalidKeywordFormat));
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRIES_WITH_VALID_LANGUAGES_Returns_With_No_Feedback()
        {
            // Arrange
            List<StructuredValidationEntry> entries = SyntaxValidationFixtures.ENTRIES_WITH_VALID_LANGUAGES;
            List<IValidationFunction> functions = [new IllegalCharactersInLanguageParameter(), new IncompliantLanguage()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            Assert.AreEqual(0, report.FeedbackItems?.Count);
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRIES_WITH_INVALID_LANGUAGES_Returns_Result_With_Errors()
        {
            // Arrange
            List<StructuredValidationEntry> entries = SyntaxValidationFixtures.ENTRIES_WITH_INVALID_LANGUAGES;
            List<IValidationFunction> functions = [new IllegalCharactersInLanguageParameter()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            // This test also catches an earlier issue with excess whitespace in the key part
            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackInvalidLanguageFormat));
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRY_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIERS_Returns_With_Errors()
        {
            // Arrange
            List<StructuredValidationEntry> entries = SyntaxValidationFixtures.ENTRY_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIERS;
            List<IValidationFunction> functions = [new IllegalCharactersInSpecifierParameter()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            // This test also catches an earlier issue with excess whitespace in the key part
            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackIllegalCharactersInSpecifier));
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRY_WITHOUT_VALUE_Returns_With_Errors()
        {
            // Arrange
            List<StringValidationEntry> entries = SyntaxValidationFixtures.ENTRY_WITHOUT_VALUE;
            List<IValidationFunction> functions = [new EntryWithoutValue()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            // This test also catches an earlier issue with excess whitespace in the key part
            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackEntryWithoutValue));
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRIES_WITH_INCOMPLIANT_LANGUAGES_Returns_With_Warnings()
        {
            // Arrange
            List<StructuredValidationEntry> entries = SyntaxValidationFixtures.ENTRIES_WITH_INCOMPLIANT_LANGUAGES;
            List<IValidationFunction> functions = [new IncompliantLanguage()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            // This test also catches an earlier issue with excess whitespace in the key part
            Assert.AreEqual(2, report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackIncompliantLanguageParam));
            Assert.IsInstanceOfType(report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackIncompliantLanguageParam));
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRY_WITH_LONG_KEYWORD_Returns_With_Warnings()
        {
            // Arrange
            List<StructuredValidationEntry> entries = SyntaxValidationFixtures.ENTRY_WITH_LONG_KEYWORD;
            List<IValidationFunction> functions = [new KeywordIsExcessivelyLong()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            // This test also catches an earlier issue with excess whitespace in the key part
            Assert.AreEqual(1, report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackKeywordIsExcessivelyLong));
        }

        [TestMethod]
        public void ValidateEntries_CalledWith_ENTRIES_WITH_UNRECOMMENDED_KEYWORD_NAMING_Returns_With_Warnings()
        {
            // Arrange
            List<StructuredValidationEntry> entries = SyntaxValidationFixtures.ENTRIES_WITH_UNRECOMMENDED_KEYWORD_NAMING;
            List<IValidationFunction> functions = [new KeywordHasUnrecommendedCharacters()];
            ValidationReport report = new();

            // Act
            SyntaxValidation.ValidateEntries(entries, functions, report);

            // This test also catches an earlier issue with excess whitespace in the key part
            Assert.AreEqual(2, report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackKeywordContainsUnrecommendedCharacters));
            Assert.IsInstanceOfType(report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackKeywordContainsUnrecommendedCharacters));
        }
    }
}
