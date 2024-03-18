using PxUtils.Validation.SyntaxValidation;
using PxUtils.UnitTests.SyntaxValidationTests.Fixtures;
using System.Text;

namespace PxUtils.UnitTests.SyntaxValidationTests
{
    [TestClass]
    public class StreamSyntaxValidationAsyncTests
    {
        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_MINIMAL_UTF8_Returns_Valid_Result()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            Assert.AreEqual(8, result.StructuredEntries.Count);
            Assert.AreEqual(0, result.Report.FeedbackItems?.Count);
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_MINIMAL_UTF8_N_WITH_MULTIPLE_ENTRIES_IN_SINGLE_LINE_Returns_Result_With_Warnings()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.MINIMAL_UTF8_N_WITH_MULTIPLE_ENTRIES_IN_SINGLE_LINE);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            Assert.AreEqual(8, result.StructuredEntries.Count);
            Assert.AreEqual(2, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackMultipleEntriesOnLine));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackMultipleEntriesOnLine));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_SPECIFIERS_Returns_Result_With_Right_Structure()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_SPECIFIERS);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

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
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_ENTRY_WITH_MULTIPLE_LANGUAGE_PARAMETERS_Returns_Result_With_Error()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_ENTRY_WITH_MULTIPLE_LANGUAGE_PARAMETERS);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            Assert.AreEqual(1, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackMoreThanOneLanguage));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_ENTRY_WITH_MULTIPLE_SPECIFIER_PARAMETERS_Returns_Result_With_Error()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_SPECIFIERS_WITH_MULTIPLE_SPECIFIER_PARAMETERS);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            Assert.AreEqual(1, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackMoreThanOneSpecifier));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_ENTRIES_IN_WRONG_ORDER_Returns_Result_With_Errors()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_ENTRIES_IN_WRONG_ORDER);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            Assert.AreEqual(2, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackKeyHasWrongOrder));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackKeyHasWrongOrder));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_A_MISSING_KEYWORD_Returns_Result_With_Error()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_A_MISSING_KEYWORD);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            Assert.AreEqual(1, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackMissingKeyword));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_INVALID_SPECIFIERS_Returns_Result_With_Errors()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_INVALID_SPECIFIERS);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            Assert.AreEqual(5, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackInvalidSpecifier));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackKeyContainsExcessWhitespace));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[2].Feedback, typeof(SyntaxValidationFeedbackInvalidSpecifier));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[3].Feedback, typeof(SyntaxValidationFeedbackInvalidSpecifier));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[4].Feedback, typeof(SyntaxValidationFeedbackKeyContainsExcessWhitespace));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_ILLEGAL_SYMBOLS_IN_PARAMS_Returns_Result_With_Errors()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_ILLEGAL_SYMBOLS_IN_PARAMS);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            // This test also catches a later issues with invalid language formats and badly formatted keywords
            Assert.AreEqual(10, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackIllegalSymbolsInParamSections));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackIllegalSymbolsInParamSections));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[2].Feedback, typeof(SyntaxValidationFeedbackIllegalSymbolsInParamSections));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_CORRECTLY_FORMATTED_LIST_AND_MULTILINE_STRING_Returns_Valid_Result()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_CORRECTLY_FORMATTED_LIST_AND_MULTILINE_STRING);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            Assert.AreEqual(10, result.StructuredEntries.Count);
            Assert.AreEqual(0, result.Report.FeedbackItems?.Count);
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_BAD_VALUES_Returns_Valid_Errors()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_BAD_VALUES);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            Assert.AreEqual(3, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackInvalidValueSection));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackInvalidValueSection));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[2].Feedback, typeof(SyntaxValidationFeedbackInvalidValueSection));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_EXCESS_WHITESPACE_IN_LIST_Returns_Result_With_Warning()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_EXCESS_WHITESPACE_IN_LIST);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            Assert.AreEqual(1, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackValueContainsExcessWhitespace));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_SHORT_MULTILINE_VALUES_Returns_Result_With_Warnings()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_SHORT_MULTILINE_VALUES);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            Assert.AreEqual(2, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackExcessNewLinesInValue));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackExcessNewLinesInValue));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_INVALID_KEYWORDS_Returns_Result_With_Errors()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_INVALID_KEYWORDS);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            Assert.AreEqual(3, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackInvalidKeywordFormat));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackInvalidKeywordFormat));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[2].Feedback, typeof(SyntaxValidationFeedbackInvalidKeywordFormat));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_VALID_LANGUAGES_Returns_Valid_Result()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_VALID_LANGUAGES);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            Assert.AreEqual(0, result.Report.FeedbackItems?.Count);
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_INVALID_LANGUAGES_Returns_Result_With_Errors()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_INVALID_LANGUAGES);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            // This test also catches an earlier issue with excess whitespace in the key part
            Assert.AreEqual(3, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackInvalidLanguageFormat));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[2].Feedback, typeof(SyntaxValidationFeedbackIncompliantLanguageParam));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIERS_Returns_With_Errors()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIERS);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            // This test also catches an earlier issue with illegal symbols in the key parameter section
            Assert.AreEqual(2, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackIllegalCharactersInSpecifier));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_VALUELESS_ENTRY_Returns_With_Errors()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_VALUELESS_ENTRY);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            // This test also catches an earlier issue with illegal symbols in the key parameter section
            Assert.AreEqual(1, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackEntryWithoutValue));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_INCOMPLIANT_LANGUAGES_Returns_With_Warnings()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_INCOMPLIANT_LANGUAGES);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            // This test also catches an earlier issue with illegal symbols in the key parameter section
            Assert.AreEqual(2, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackIncompliantLanguageParam));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackIncompliantLanguageParam));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_LONG_KEYWORD_Returns_With_Warnings()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_LONG_KEYWORD);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            Assert.AreEqual(1, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackKeywordIsExcessivelyLong));
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsync_CalledWith_UTF8_N_WITH_UNRECOMMENDED_KEYWORD_NAMING_Returns_With_Warnings()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_UNRECOMMENDED_KEYWORD_NAMING);

            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = await SyntaxValidationAsync.ValidatePxFileSyntaxAsync(stream, filename);

            Assert.AreEqual(2, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackKeywordContainsUnrecommendedCharacters));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackKeywordContainsUnrecommendedCharacters));
        }
    }
}
