using Px.Utils.Validation.DatabaseValidation;
using Px.Utils.Validation;

namespace Px.Utils.UnitTests.Validation.DatabaseValidation
{
    [TestClass]
    public class DatabaseValidatorTests
    {
        [TestMethod]
        public void ValidateDatabaseWithValidDatabaseReturnsValidResult()
        {
            // Arrange
            MockFileSystem fileSystem = new();
            DatabaseValidator validator = new("database", fileSystem: fileSystem);

            // Act
            ValidationResult result = validator.Validate();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(0, result.FeedbackItems.Count);
        }

        [TestMethod]
        public async Task ValidateDatabaseAsyncWithValidDatabaseReturnsValidResult()
        {
            // Arrange
            MockFileSystem fileSystem = new();
            DatabaseValidator validator = new("database", fileSystem: fileSystem);

            // Act
            ValidationResult result = await validator.ValidateAsync();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(0, result.FeedbackItems.Count);
        }

        [TestMethod]
        public void ValidateDatabaseWithCustomValidatorsReturnsValidResult()
        {
            // Arrange
            MockFileSystem fileSystem = new();
            DatabaseValidator validator = new("database_invalid", fileSystem: fileSystem);

            // Act
            ValidationResult result = validator.Validate();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(9, result.FeedbackItems.Count); // Unique feedbacks
            Assert.AreEqual(11, result.FeedbackItems.Values.SelectMany(f => f).Count()); // Total feedbacks including duplicates
        }

        [TestMethod]
        public async Task ValidateDatabaseAsyncWithInvalidPxFileReturnsFeedback()
        {
            // Arrange
            MockFileSystem fileSystem = new();
            DatabaseValidator validator = new("database_invalid", fileSystem: fileSystem);

            // Act
            ValidationResult result = await validator.ValidateAsync();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(9, result.FeedbackItems.Count); // Unique feedbacks
            Assert.AreEqual(11, result.FeedbackItems.Values.SelectMany(f => f).Count()); // Total feedbacks including duplicates
        }

        [TestMethod]
        public void ValidateDatabaseWithCustomFunctionsReturnsFeedback()
        {
            // Arrange
            MockFileSystem fileSystem = new();
            IDatabaseValidator[] customValidators =
            [
                new MockCustomDatabaseValidator()
            ];
            DatabaseValidator validator = new(
                "database_invalid", 
                customPxFileValidators: customValidators,
                customAliasFileValidators: customValidators,
                customDirectoryValidators: customValidators,
                fileSystem: fileSystem);

            // Act
            ValidationResult result = validator.Validate();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(10, result.FeedbackItems.Count); // Unique feedbacks
            Assert.AreEqual(26, result.FeedbackItems.Values.SelectMany(f => f).Count()); // Total feedbacks including duplicates
        }

        [TestMethod]
        public async Task ValidateDatabasAsynceWithCustomFunctionsReturnsFeedback()
        {
            // Arrange
            MockFileSystem fileSystem = new();
            IDatabaseValidator[] customValidators =
                [
                    new MockCustomDatabaseValidator()
                    ];
            DatabaseValidator validator = new(
                "database_invalid", 
                customPxFileValidators: customValidators,
                customAliasFileValidators: customValidators,
                customDirectoryValidators: customValidators,
                fileSystem: fileSystem);

            // Act
            ValidationResult result = await validator.ValidateAsync();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(10, result.FeedbackItems.Count); // Unique feedbacks
            Assert.AreEqual(26, result.FeedbackItems.Values.SelectMany(f => f).Count()); // Total feedbacks including duplicates
        }

        [TestMethod]
        public void ValidateDatabaseWithSingleLanguagePxFileReturnsValidResult()
        {
            // Arrange
            MockFileSystem fileSystem = new();
            DatabaseValidator validator = new("database_single_language", fileSystem: fileSystem);

            // Act
            ValidationResult result = validator.Validate();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(0, result.FeedbackItems.Count);
        }

        [TestMethod]
        public async Task ValidateDatabaseAsyncWithSingleLanguagePxFileReturnsValidResult()
        {
            // Arrange
            MockFileSystem fileSystem = new();
            DatabaseValidator validator = new("database_single_language", fileSystem: fileSystem);

            // Act
            ValidationResult result = await validator.ValidateAsync();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(0, result.FeedbackItems.Count);
        }
    }
}
