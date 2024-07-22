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
            Assert.AreEqual(0, result.FeedbackItems.Length);
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
            Assert.AreEqual(0, result.FeedbackItems.Length);
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
            Assert.AreEqual(14, result.FeedbackItems.Length);
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
            Assert.AreEqual(14, result.FeedbackItems.Length);
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
                customFolderValidators: customValidators,
                fileSystem: fileSystem);

            // Act
            ValidationResult result = validator.Validate();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(29, result.FeedbackItems.Length);
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
                customFolderValidators: customValidators,
                fileSystem: fileSystem);

            // Act
            ValidationResult result = await validator.ValidateAsync();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.AreEqual(29, result.FeedbackItems.Length);
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
            // LANGUAGES keyword is recommended, but not required, three warning level feedback items are expected
            Assert.AreEqual(3, result.FeedbackItems.Length);
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
            // LANGUAGES keyword is recommended, but not required, three warning level feedback items are expected
            Assert.AreEqual(3, result.FeedbackItems.Length);
        }
    }
}
