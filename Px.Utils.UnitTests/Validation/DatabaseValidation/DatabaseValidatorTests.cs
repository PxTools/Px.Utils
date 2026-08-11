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
            Assert.HasCount(0, result.FeedbackItems);
        }

        [TestMethod]
        public void ValidateDatabaseWithNoFilesReturnsValidResult()
        {
            MockFileSystem fileSystem = new();
            DatabaseValidator validator = new("database_empty", fileSystem: fileSystem);

            ValidationResult result = validator.Validate();

            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.HasCount(0, result.FeedbackItems);
        }

        [TestMethod]
        public void ValidateDatabaseWithUnreadableAliasFileReturnsFeedback()
        {
            MockFileSystem fileSystem = new();
            DatabaseValidator validator = new("database_unreadable_alias", fileSystem: fileSystem);
            ValidationFeedbackKey unreadableAliasFileKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.UnreadableAliasFile);

            ValidationResult result = validator.Validate();

            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.HasCount(1, result.FeedbackItems);
            Assert.IsTrue(result.FeedbackItems.ContainsKey(unreadableAliasFileKey));
            Assert.HasCount(1, result.FeedbackItems[unreadableAliasFileKey]);
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
            Assert.HasCount(0, result.FeedbackItems);
        }

        [TestMethod]
        public void ValidateDatabaseWithInvalidDatabaseReturnsFeedback()
        {
            // Arrange
            MockFileSystem fileSystem = new();
            DatabaseValidator validator = new("database_invalid", fileSystem: fileSystem);
            ValidationFeedbackKey invalidValueFormatKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.InvalidValueFormat);
            ValidationFeedbackKey recommendedKeyMissingKey = new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.RecommendedKeyMissing);
            ValidationFeedbackKey entryWithMultipleValuesKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.EntryWithMultipleValues);
            ValidationFeedbackKey dataValidationFeedbackInvalidStructureKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidStructure);
            ValidationFeedbackKey dataValidationFeedbackInvalidRowCountKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidRowCount);
            ValidationFeedbackKey dataValidationFeedbackInvalidRowLengthKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidRowLength);
            ValidationFeedbackKey unmatchingValueTypeKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.UnmatchingValueType);
            ValidationFeedbackKey excessNewLinesInValueKey = new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.ExcessNewLinesInValue);
            ValidationFeedbackKey missingStubKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingStubDimensions);
            ValidationFeedbackKey missingHeadingKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingHeadingDimensions);

            // Act
            ValidationResult result = validator.Validate();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.HasCount(10, result.FeedbackItems); // Unique feedbacks
            Assert.HasCount(12, result.FeedbackItems.Values.SelectMany(f => f)); // Total feedbacks including duplicates
            Assert.IsTrue(result.FeedbackItems.ContainsKey(invalidValueFormatKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(recommendedKeyMissingKey));
            Assert.HasCount(3, result.FeedbackItems[recommendedKeyMissingKey]); // 3 warnings
            Assert.IsTrue(result.FeedbackItems.ContainsKey(entryWithMultipleValuesKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidStructureKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidRowCountKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidRowLengthKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(unmatchingValueTypeKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(excessNewLinesInValueKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(missingStubKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(missingHeadingKey));
        }

        [TestMethod]
        public async Task ValidateDatabaseAsyncWithInvaliDatabaseReturnsFeedback()
        {
            // Arrange
            MockFileSystem fileSystem = new();
            DatabaseValidator validator = new("database_invalid", fileSystem: fileSystem);
            ValidationFeedbackKey invalidValueFormatKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.InvalidValueFormat);
            ValidationFeedbackKey recommendedKeyMissingKey = new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.RecommendedKeyMissing);
            ValidationFeedbackKey entryWithMultipleValuesKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.EntryWithMultipleValues);
            ValidationFeedbackKey dataValidationFeedbackInvalidStructureKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidStructure);
            ValidationFeedbackKey dataValidationFeedbackInvalidRowCountKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidRowCount);
            ValidationFeedbackKey dataValidationFeedbackInvalidRowLengthKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidRowLength);
            ValidationFeedbackKey unmatchingValueTypeKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.UnmatchingValueType);
            ValidationFeedbackKey excessNewLinesInValueKey = new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.ExcessNewLinesInValue);
            ValidationFeedbackKey missingStubKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingStubDimensions);
            ValidationFeedbackKey missingHeadingKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingHeadingDimensions);

            // Act
            ValidationResult result = await validator.ValidateAsync();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.HasCount(10, result.FeedbackItems); // Unique feedbacks
            Assert.HasCount(12, result.FeedbackItems.Values.SelectMany(f => f)); // Total feedbacks including duplicates
            Assert.IsTrue(result.FeedbackItems.ContainsKey(invalidValueFormatKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(recommendedKeyMissingKey));
            Assert.HasCount(3, result.FeedbackItems[recommendedKeyMissingKey]); // 3 warnings
            Assert.IsTrue(result.FeedbackItems.ContainsKey(entryWithMultipleValuesKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidStructureKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidRowCountKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidRowLengthKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(unmatchingValueTypeKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(excessNewLinesInValueKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(missingStubKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(missingHeadingKey));
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
            ValidationFeedbackKey invalidValueFormatKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.InvalidValueFormat);
            ValidationFeedbackKey aliasFileMissingKey = new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.AliasFileMissing);
            ValidationFeedbackKey recommendedKeyMissingKey = new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.RecommendedKeyMissing);
            ValidationFeedbackKey entryWithMultipleValuesKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.EntryWithMultipleValues);
            ValidationFeedbackKey dataValidationFeedbackInvalidStructureKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidStructure);
            ValidationFeedbackKey dataValidationFeedbackInvalidRowCountKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidRowCount);
            ValidationFeedbackKey dataValidationFeedbackInvalidRowLengthKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidRowLength);
            ValidationFeedbackKey unmatchingValueTypeKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.UnmatchingValueType);
            ValidationFeedbackKey excessNewLinesInValueKey = new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.ExcessNewLinesInValue);
            ValidationFeedbackKey missingStubKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingStubDimensions);
            ValidationFeedbackKey missingHeadingKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingHeadingDimensions);

            // Act
            ValidationResult result = validator.Validate();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.HasCount(11, result.FeedbackItems); // Unique feedbacks
            Assert.HasCount(27, result.FeedbackItems.Values.SelectMany(f => f)); // Total feedbacks including duplicates
            Assert.IsTrue(result.FeedbackItems.ContainsKey(invalidValueFormatKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(aliasFileMissingKey));
            Assert.HasCount(15, result.FeedbackItems[aliasFileMissingKey]); // 15 warnings
            Assert.IsTrue(result.FeedbackItems.ContainsKey(recommendedKeyMissingKey));
            Assert.HasCount(3, result.FeedbackItems[recommendedKeyMissingKey]); // 3 warnings
            Assert.IsTrue(result.FeedbackItems.ContainsKey(entryWithMultipleValuesKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidStructureKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidRowCountKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidRowLengthKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(unmatchingValueTypeKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(excessNewLinesInValueKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(missingStubKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(missingHeadingKey));
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
            ValidationFeedbackKey invalidValueFormatKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.InvalidValueFormat);
            ValidationFeedbackKey aliasFileMissingKey = new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.AliasFileMissing);
            ValidationFeedbackKey recommendedKeyMissingKey = new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.RecommendedKeyMissing);
            ValidationFeedbackKey entryWithMultipleValuesKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.EntryWithMultipleValues);
            ValidationFeedbackKey dataValidationFeedbackInvalidStructureKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidStructure);
            ValidationFeedbackKey dataValidationFeedbackInvalidRowCountKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidRowCount);
            ValidationFeedbackKey dataValidationFeedbackInvalidRowLengthKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidRowLength);
            ValidationFeedbackKey unmatchingValueTypeKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.UnmatchingValueType);
            ValidationFeedbackKey excessNewLinesInValueKey = new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.ExcessNewLinesInValue);
            ValidationFeedbackKey missingStubKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingStubDimensions);
            ValidationFeedbackKey missingHeadingKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingHeadingDimensions);

            // Act
            ValidationResult result = await validator.ValidateAsync();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.HasCount(11, result.FeedbackItems); // Unique feedbacks
            Assert.HasCount(27, result.FeedbackItems.Values.SelectMany(f => f)); // Total feedbacks including duplicates
            Assert.IsTrue(result.FeedbackItems.ContainsKey(invalidValueFormatKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(aliasFileMissingKey));
            Assert.HasCount(15, result.FeedbackItems[aliasFileMissingKey]); // 15 warnings
            Assert.IsTrue(result.FeedbackItems.ContainsKey(recommendedKeyMissingKey));
            Assert.HasCount(3, result.FeedbackItems[recommendedKeyMissingKey]); // 3 warnings
            Assert.IsTrue(result.FeedbackItems.ContainsKey(entryWithMultipleValuesKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidStructureKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidRowCountKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidRowLengthKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(unmatchingValueTypeKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(excessNewLinesInValueKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(missingStubKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(missingHeadingKey));
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
            Assert.HasCount(0, result.FeedbackItems);
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
            Assert.HasCount(0, result.FeedbackItems);
        }

        [TestMethod]
        public async Task ValidateDatabaseAsyncWithNoFilesReturnsWarnings()
        {
            MockFileSystem fileSystem = new();
            DatabaseValidator validator = new("database_empty", fileSystem: fileSystem);

            ValidationResult result = await validator.ValidateAsync();

            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.HasCount(2, result.FeedbackItems);
            Assert.IsTrue(result.FeedbackItems.ContainsKey(new ValidationFeedbackKey(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.NoPxFilesFound)));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(new ValidationFeedbackKey(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.NoAliasFilesFound)));
        }

        [TestMethod]
        public async Task ValidateDatabaseAsyncWithUnreadableAliasFileReturnsFeedback()
        {
            MockFileSystem fileSystem = new();
            DatabaseValidator validator = new("database_unreadable_alias", fileSystem: fileSystem);
            ValidationFeedbackKey unreadableAliasFileKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.UnreadableAliasFile);

            ValidationResult result = await validator.ValidateAsync();

            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.IsTrue(result.FeedbackItems.ContainsKey(unreadableAliasFileKey));
            Assert.HasCount(1, result.FeedbackItems[unreadableAliasFileKey]);
        }
    }
}
