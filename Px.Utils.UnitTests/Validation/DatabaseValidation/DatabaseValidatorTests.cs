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
            Assert.AreEqual(9, result.FeedbackItems.Count); // Unique feedbacks
            Assert.AreEqual(11, result.FeedbackItems.Values.SelectMany(f => f).Count()); // Total feedbacks including duplicates
            Assert.IsTrue(result.FeedbackItems.ContainsKey(invalidValueFormatKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(recommendedKeyMissingKey));
            Assert.AreEqual(3, result.FeedbackItems[recommendedKeyMissingKey].Count); // 3 warnings
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
            Assert.AreEqual(9, result.FeedbackItems.Count); // Unique feedbacks
            Assert.AreEqual(11, result.FeedbackItems.Values.SelectMany(f => f).Count()); // Total feedbacks including duplicates
            Assert.IsTrue(result.FeedbackItems.ContainsKey(invalidValueFormatKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(recommendedKeyMissingKey));
            Assert.AreEqual(3, result.FeedbackItems[recommendedKeyMissingKey].Count); // 3 warnings
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
            Assert.AreEqual(10, result.FeedbackItems.Count); // Unique feedbacks
            Assert.AreEqual(26, result.FeedbackItems.Values.SelectMany(f => f).Count()); // Total feedbacks including duplicates
            Assert.IsTrue(result.FeedbackItems.ContainsKey(invalidValueFormatKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(aliasFileMissingKey));
            Assert.AreEqual(15, result.FeedbackItems[aliasFileMissingKey].Count); // 15 warnings
            Assert.IsTrue(result.FeedbackItems.ContainsKey(recommendedKeyMissingKey));
            Assert.AreEqual(3, result.FeedbackItems[recommendedKeyMissingKey].Count); // 3 warnings
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
            Assert.AreEqual(10, result.FeedbackItems.Count); // Unique feedbacks
            Assert.AreEqual(26, result.FeedbackItems.Values.SelectMany(f => f).Count()); // Total feedbacks including duplicates
            Assert.IsTrue(result.FeedbackItems.ContainsKey(invalidValueFormatKey));
            Assert.IsTrue(result.FeedbackItems.ContainsKey(aliasFileMissingKey));
            Assert.AreEqual(15, result.FeedbackItems[aliasFileMissingKey].Count); // 15 warnings
            Assert.IsTrue(result.FeedbackItems.ContainsKey(recommendedKeyMissingKey));
            Assert.AreEqual(3, result.FeedbackItems[recommendedKeyMissingKey].Count); // 3 warnings
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
