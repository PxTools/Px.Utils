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
            ValidationFeedbackKey missingStubAndHeadingKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingStubAndHeading);

            // Act
            ValidationResult result = validator.Validate();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.HasCount(9, result.FeedbackItems); // Unique feedbacks
            Assert.HasCount(11, result.FeedbackItems.Values.SelectMany(f => f)); // Total feedbacks including duplicates
            Assert.IsTrue(result.FeedbackItems.ContainsKey(invalidValueFormatKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(recommendedKeyMissingKey));
            Assert.HasCount(3, result.FeedbackItems[recommendedKeyMissingKey]); // 3 warnings
            Assert.IsTrue(result.FeedbackItems.ContainsKey(entryWithMultipleValuesKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidStructureKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidRowCountKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidRowLengthKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(unmatchingValueTypeKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(excessNewLinesInValueKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(missingStubAndHeadingKey));
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
            ValidationFeedbackKey missingStubAndHeadingKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingStubAndHeading);

            // Act
            ValidationResult result = await validator.ValidateAsync();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.HasCount(9, result.FeedbackItems); // Unique feedbacks
            Assert.HasCount(11, result.FeedbackItems.Values.SelectMany(f => f)); // Total feedbacks including duplicates
            Assert.IsTrue(result.FeedbackItems.ContainsKey(invalidValueFormatKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(recommendedKeyMissingKey));
            Assert.HasCount(3, result.FeedbackItems[recommendedKeyMissingKey]); // 3 warnings
            Assert.IsTrue(result.FeedbackItems.ContainsKey(entryWithMultipleValuesKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidStructureKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidRowCountKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(dataValidationFeedbackInvalidRowLengthKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(unmatchingValueTypeKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(excessNewLinesInValueKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(missingStubAndHeadingKey));
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
            ValidationFeedbackKey missingStubAndHeadingKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingStubAndHeading);

            // Act
            ValidationResult result = validator.Validate();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.HasCount(10, result.FeedbackItems); // Unique feedbacks
            Assert.HasCount(26, result.FeedbackItems.Values.SelectMany(f => f)); // Total feedbacks including duplicates
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
            Assert.IsTrue(result.FeedbackItems.ContainsKey(missingStubAndHeadingKey));
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
            ValidationFeedbackKey missingStubAndHeadingKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingStubAndHeading);


            // Act
            ValidationResult result = await validator.ValidateAsync();

            // Assert
            Assert.IsNotNull(result, "Validation result should not be null");
            Assert.HasCount(10, result.FeedbackItems); // Unique feedbacks
            Assert.HasCount(26, result.FeedbackItems.Values.SelectMany(f => f)); // Total feedbacks including duplicates
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
            Assert.IsTrue(result.FeedbackItems.ContainsKey(missingStubAndHeadingKey));
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
    }
}
