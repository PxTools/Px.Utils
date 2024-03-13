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
    }
}
