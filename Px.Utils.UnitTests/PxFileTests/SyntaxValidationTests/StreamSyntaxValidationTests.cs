using PxUtils.Validation;
using PxUtils.Validation.SyntaxValidation;
using PxUtils.UnitTests.PxFileTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Px.Utils.UnitTests.PxFileTests.Fixtures;

namespace PxUtils.UnitTests.PxFileTests.SyntaxValidationTests
{
    [TestClass]
    public class StreamSyntaxValidationTests
    {
        [TestMethod]
        public void ValidateStreamSyntax_CalledWith_MINIMAL_UTF8_Returns_Valid_Result()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(8, result.StructuredEntries.Count);
            Assert.AreEqual(0, result.Report.FeedbackItems?.Count);
        }

        [TestMethod]
        public void ValidateStreamSyntax_CalledWith_MINIMAL_UTF8_N_WITH_MULTIPLE_ENTRIES_IN_SINGLE_LINE_Returns_Result_With_Warnings()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_WITH_MULTIPLE_ENTRIES_IN_SINGLE_LINE);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(8, result.StructuredEntries.Count);
            Assert.AreEqual(2, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackMultipleEntriesOnLine));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackMultipleEntriesOnLine));
        }

        [TestMethod]
        public void ValidateStreamSyntax_CalledWith_UTF8_N_WITH_SPECIFIERS_Returns_Result_With_Right_Structure()
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
            Assert.AreEqual("lang", result.StructuredEntries[8].Key.Language);
            Assert.AreEqual("lang", result.StructuredEntries[9].Key.Language);
            Assert.AreEqual("first_specifier", result.StructuredEntries[8].Key.FirstSpecifier);
            Assert.AreEqual("first_specifier", result.StructuredEntries[9].Key.FirstSpecifier);
            Assert.AreEqual("second_specifier", result.StructuredEntries[8].Key.SecondSpecifier);
        }

        [TestMethod]
        public void ValidateStreamSyntax_CalledWith_UTF8_N_WITH_ENTRY_WITH_MULTIPLE_LANGUAGE_PARAMETERS_Returns_Result_With_Error()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_ENTRY_WITH_MULTIPLE_LANGUAGE_PARAMETERS);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(1, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackMoreThanOneLanguage));
        }

        [TestMethod]
        public void ValidateStreamSyntax_CalledWith_UTF8_N_WITH_ENTRY_WITH_MULTIPLE_SPECIFIER_PARAMETERS_Returns_Result_With_Error()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_SPECIFIERS_WITH_MULTIPLE_SPECIFIER_PARAMETERS);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(1, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackMoreThanOneSpecifier));
        }

        [TestMethod]
        public void ValidateStreamSyntax_CalledWith_UTF8_N_WITH_ENTRIES_IN_WRONG_ORDER_Returns_Result_With_Errors()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_ENTRIES_IN_WRONG_ORDER);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(2, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackKeyHasWrongOrder));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackKeyHasWrongOrder));
        }

        [TestMethod]
        public void ValidateStreamSyntax_CalledWith_UTF8_N_WITH_A_MISSING_KEYWORD_Returns_Result_With_Error()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_A_MISSING_KEYWORD);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(1, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackMissingKeyword));
        }

        [TestMethod]
        public void ValidateStreamSyntax_CalledWith_UTF8_N_WITH_INVALID_SPECIFIERS_Returns_Result_With_Errors()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_INVALID_SPECIFIERS);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(5, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackInvalidSpecifier));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackKeyContainsExcessWhitespace));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[2].Feedback, typeof(SyntaxValidationFeedbackInvalidSpecifier));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[3].Feedback, typeof(SyntaxValidationFeedbackInvalidSpecifier));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[4].Feedback, typeof(SyntaxValidationFeedbackKeyContainsExcessWhitespace));
        }

        [TestMethod]
        public void ValidateStreamSyntax_CalledWith_UTF8_N_WITH_ILLEGAL_SYMBOLS_IN_PARAMS_Returns_Result_With_Errors()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_ILLEGAL_SYMBOLS_IN_PARAMS);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(3, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackIllegalSymbolsInParamSections));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackIllegalSymbolsInParamSections));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[2].Feedback, typeof(SyntaxValidationFeedbackIllegalSymbolsInParamSections));
        }

        [TestMethod]
        public void ValidateStreamSyntax_CalledWith_UTF8_N_WITH_CORRECTLY_FORMATTED_LIST_AND_MULTILINE_STRING_Returns_Valid_Result()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_CORRECTLY_FORMATTED_LIST_AND_MULTILINE_STRING);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(10, result.StructuredEntries.Count);
            Assert.AreEqual(0, result.Report.FeedbackItems?.Count);
        }

        [TestMethod]
        public void ValidateStreamSyntax_CalledWith_UTF8_N_WITH_BAD_VALUES_Returns_Valid_Errors()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_BAD_VALUES);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(3, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackInvalidValueSection));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackInvalidValueSection));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[2].Feedback, typeof(SyntaxValidationFeedbackInvalidValueSection));
        }

        [TestMethod]
        public void ValidateStreamSyntax_CalledWith_UTF8_N_WITH_EXCESS_WHITESPACE_IN_LIST_Returns_Result_With_Warning()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_EXCESS_WHITESPACE_IN_LIST);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(1, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackValueContainsExcessWhitespace));
        }

        [TestMethod]
        public void ValidateStreamSyntax_CalledWith_UTF8_N_WITH_SHORT_MULTILINE_VALUES_Returns_Result_With_Warnings()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_SHORT_MULTILINE_VALUES);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            SyntaxValidationResult result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(2, result.Report.FeedbackItems?.Count);
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[0].Feedback, typeof(SyntaxValidationFeedbackExcessNewLinesInValue));
            Assert.IsInstanceOfType(result.Report.FeedbackItems?[1].Feedback, typeof(SyntaxValidationFeedbackExcessNewLinesInValue));
        }
    }
}
