using Px.Utils.Validation;
using Px.Utils.Validation.DatabaseValidation;
using System.Text;

namespace Px.Utils.UnitTests.Validation.DatabaseValidation
{
    [TestClass]
    public class DatabaseValidatorFunctionTests
    {
        [TestMethod]
        public void DuplicatePxFileNameWithoutDuplicateNameReturnsNull()
        {
            // Arrange
            List<DatabaseFileInfo> pxFiles = [
                new("foo.px", "path/to/file", ["fi", "en", "sv"], Encoding.UTF8),
                new("bar.px", "path/to/file", ["fi", "en", "sv"], Encoding.UTF8)
            ];
            DuplicatePxFileName validator = new (pxFiles);
            DatabaseFileInfo fileInfo = new("baz.px", "path/to/file", ["fi", "en", "sv"], Encoding.UTF8);

            // Act
            ValidationFeedbackItem? feedback = validator.Validate(fileInfo);

            // Assert
            Assert.IsNull(feedback);
        }

        [TestMethod]
        public void DuplicatePxFileNameWithDuplicateNameReturnsFeedback()
        {
            // Arrange
            List<DatabaseFileInfo> pxFiles = [
                new("foo.px", "path/to/file", ["fi", "en", "sv"], Encoding.UTF8),
                new("bar.px", "path/to/file", ["fi", "en", "sv"], Encoding.UTF8)
            ];
            DuplicatePxFileName validator = new (pxFiles);
            DatabaseFileInfo fileInfo = new("bar.px", "path/to/file", ["fi", "en", "sv"], Encoding.UTF8);

            // Act
            ValidationFeedbackItem? feedback = validator.Validate(fileInfo);

            // Assert
            Assert.IsNotNull(feedback);
            Assert.AreEqual(ValidationFeedbackRule.DuplicateFileNames, feedback.Value.Feedback.Rule);
        }

        [TestMethod]
        public void MissingPxFileLanguagesWithoutMissingLanguagesReturnsNull()
        {
            // Arrange
            IEnumerable<string> allLanguages = ["fi", "en", "sv"];
            MissingPxFileLanguages validator = new (allLanguages);
            DatabaseFileInfo fileInfo = new("foo.px", "path/to/file", ["fi", "en", "sv"], Encoding.UTF8);

            // Act
            ValidationFeedbackItem? feedback = validator.Validate(fileInfo);

            // Assert
            Assert.IsNull(feedback);
        }

        [TestMethod]
        public void MissingPxFileLanguagesWithMissingLanguagesReturnsFeedback()
        {
            // Arrange
            IEnumerable<string> allLanguages = ["fi", "en", "sv"];
            MissingPxFileLanguages validator = new (allLanguages);
            DatabaseFileInfo fileInfo = new("foo.px", "path/to/file", ["fi", "en"], Encoding.UTF8);

            // Act
            ValidationFeedbackItem? feedback = validator.Validate(fileInfo);

            // Assert
            Assert.IsNotNull(feedback);
            Assert.AreEqual(ValidationFeedbackRule.FileLanguageDiffersFromDatabase, feedback.Value.Feedback.Rule);
        }

        [TestMethod]
        public void MismatchingEncodingWithMatchingEncodingReturnsNull()
        {
            // Arrange
            Encoding mostCommonEncoding = Encoding.UTF8;
            MismatchingEncoding validator = new (mostCommonEncoding);
            DatabaseFileInfo fileInfo = new("foo.px", "path/to/file", ["fi", "en", "sv"], Encoding.UTF8);

            // Act
            ValidationFeedbackItem? feedback = validator.Validate(fileInfo);

            // Assert
            Assert.IsNull(feedback);
        }

        [TestMethod]
        public void MismatchingEncodingWithMismatchingEncodingReturnsFeedback()
        {
            // Arrange
            Encoding mostCommonEncoding = Encoding.UTF8;
            MismatchingEncoding validator = new (mostCommonEncoding);
            DatabaseFileInfo fileInfo = new("foo.px", "path/to/file", ["fi", "en", "sv"], Encoding.UTF32);

            // Act
            ValidationFeedbackItem? feedback = validator.Validate(fileInfo);

            // Assert
            Assert.IsNotNull(feedback);
            Assert.AreEqual(ValidationFeedbackRule.FileEncodingDiffersFromDatabase, feedback.Value.Feedback.Rule);
        }

        [TestMethod]
        public void MissingAliasFilesWithoutMissingFilesReturnsNull()
        {
            // Arrange
            string path = "path/to/file";
            List<DatabaseFileInfo> aliasFiles = [
                new("Alias_fi.txt", path, ["fi"], Encoding.UTF8),
                new("Alias_en.txt", path, ["en"], Encoding.UTF8),
                new("Alias_sv.txt", path, ["sv"], Encoding.UTF8)
            ];
            IEnumerable<string> allLanguages = ["fi", "en", "sv"];
            MissingAliasFiles validator = new (aliasFiles, allLanguages);
            DatabaseValidationItem directoryInfo = new(path);

            // Act
            ValidationFeedbackItem? feedback = validator.Validate(directoryInfo);

            // Assert
            Assert.IsNull(feedback);
        }

        [TestMethod]
        public void MissingAliasFilesWithMissingFilesReturnsFeedback()
        {
            // Arrange
            string path = "path/to/file";
            List<DatabaseFileInfo> aliasFiles = [
                new("Alias_fi.txt", path, [], Encoding.UTF8),
                new("Alias_en.txt", path, [], Encoding.UTF8)
            ];
            IEnumerable<string> allLanguages = ["fi", "en", "sv"];
            MissingAliasFiles validator = new (aliasFiles, allLanguages);
            DatabaseValidationItem directoryInfo = new(path);

            // Act
            ValidationFeedbackItem? feedback = validator.Validate(directoryInfo);

            // Assert
            Assert.IsNotNull(feedback);
            Assert.AreEqual(ValidationFeedbackRule.AliasFileMissing, feedback.Value.Feedback.Rule);
        }
    }
}
