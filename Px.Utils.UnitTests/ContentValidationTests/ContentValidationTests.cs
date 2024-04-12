﻿using PxUtils.PxFile;
using PxUtils.UnitTests.ContentValidationTests.Fixtures;
using PxUtils.Validation;
using PxUtils.Validation.ContentValidation;
using PxUtils.Validation.SyntaxValidation;
using System.Text;
using System.Reflection;

namespace PxUtils.UnitTests.ContentValidationTests
{
    [TestClass]
    public class ContentValidationTests
    {
        private static readonly string filename = "foo";
        private static readonly PxFileSyntaxConf syntaxConf = PxFileSyntaxConf.Default;
        private ValidationFeedbackItem[] feedback = [];
        private static readonly Encoding encoding = Encoding.UTF8;
        // TODO: readonly?
        private ContentValidator validator = new(filename, encoding);
        private static readonly string defaultLanguage = "fi";
        private static readonly string[] availableLanguages = ["fi", "en"];
        private static readonly Dictionary<string, string> contentDimensionNames = new ()
            {
                { "fi", "bar" },
                { "en", "bar-en" }
            };
        private static readonly Dictionary<string, string[]> stubDimensionNames = new ()
            {
                { "fi", ["bar", "bar-time"] },
                { "en", ["bar-en", "bar-time-en"] }
            };
        private static readonly Dictionary<KeyValuePair<string, string>, string[]> dimensionValueNames = new()
                {
                    { new KeyValuePair<string, string>( "fi", "bar" ), ["foo"] },
                    { new KeyValuePair<string, string>( "fi", "bar-time" ), ["foo-time"] },
                    { new KeyValuePair<string, string>( "en", "bar-en" ), ["foo-en"] },
                    { new KeyValuePair<string, string>( "en", "bar-time-en" ), ["foo-time-en"] },
                };

        [TestMethod]
        public void ValidatePxFileContent_Called_With_MINIMAL_STRUCTURED_ENTRY_ARRAY_Returns_Valid_Result()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.MINIMAL_STRUCTURED_ENTRY_ARRAY;

            // Act
            feedback = validator.ValidatePxFileContent(
                entries,
                syntaxConf
                );

            // Assert
            Assert.AreEqual(0, feedback.Length);
        }

        [TestMethod]
        public async Task ValidatePxFileContentAsync_Called_With_MINIMAL_STRUCTURED_ENTRY_ARRAY_Returns_Valid_Result()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.MINIMAL_STRUCTURED_ENTRY_ARRAY;

            // Act
            feedback = await validator.ValidatePxFileContentAsync(
                entries,
                syntaxConf
                );

            // Assert
            Assert.AreEqual(0, feedback.Length);
        }

        [TestMethod]
        public void ValidateFindDefaultLanguage_With_EMPTY_STRUCTURED_ENTRY_ARRAY_Returns_With_Error()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.EMPTY_STRUCTURED_ENTRY_ARRAY;

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateFindDefaultLanguage(
                entries,
                validator
                );
                
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.MissingDefaultLanguage, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateFindAvailableLanguages_With_EMPTY_STRUCTURED_ENTRY_ARRAY_Returns_With_Warning()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.EMPTY_STRUCTURED_ENTRY_ARRAY;

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateFindAvailableLanguages(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.RecommendedKeyMissing, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateDefaultLanguageDefinedInAvailableLanguages_Called_With_Undefined_DefaultLanguage_Returns_With_Error()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_DEFAULT_LANGUAGE;
            SetValidatorFIeld("defaultLanguage", "foo");
            SetValidatorFIeld("availableLanguages", availableLanguages);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateDefaultLanguageDefinedInAvailableLanguages(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.UndefinedLanguageFound, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateFindContentDimension_Called_With_STRUCTURED_ENTRY_ARRAY_WITH_MISSING_CONTVARIABLE_Returns_With_Error()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_MISSING_CONTVARIABLE;
            SetValidatorFIeld("defaultLanguage", defaultLanguage);
            SetValidatorFIeld("availableLanguages", availableLanguages);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateFindContentDimension(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.RecommendedKeyMissing, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateFindRequiredCommonKeys_Called_With_EMPTY_STRUCTURE_ENTRY_ARRAY_Returns_With_Error()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.EMPTY_STRUCTURED_ENTRY_ARRAY;

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateFindRequiredCommonKeys(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.RequiredKeyMissing, result[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.RequiredKeyMissing, result[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.RequiredKeyMissing, result[2].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateFindStubOrHeading_Called_With_STRUCTURED_ENTRY_ARRAY_WITH_STUB_Returns_With_Error()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_STUB;
            SetValidatorFIeld("defaultLanguage", defaultLanguage);
            SetValidatorFIeld("availableLanguages", availableLanguages);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateFindStubAndHeading(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.MissingStubAndHeading, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateFindRecommendedKeys_Called_With_STRUCTURED_ENTRY_ARRAY_WITH_DESCRIPTION_Returns_With_Warnings()
        {

            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_DESCRIPTION;
            SetValidatorFIeld("defaultLanguage", defaultLanguage);
            SetValidatorFIeld("availableLanguages", availableLanguages);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateFindRecommendedKeys(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.RecommendedKeyMissing, result[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.RecommendedKeyMissing, result[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateFindDimenionsValues_Called_With_STRUCTURED_ENTRY_ARRAY_WITH_DIMENSIONVALUES_Returns_Errors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_DIMENSIONVALUES;
            SetValidatorFIeld("defaultLanguage", defaultLanguage);
            SetValidatorFIeld("availableLanguages", availableLanguages);
            SetValidatorFIeld("stubDimensionNames", new Dictionary<string, string[]>
                {
                    { "fi", ["bar", "bar-time"] },
                    { "en", ["bar-en", "bar-time-en"] }
                });

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateFindDimensionValues(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.VariableValuesMissing, result[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.VariableValuesMissing, result[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.VariableValuesMissing, result[2].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateFindContentDimensionKeys_Called_With_STRUCTURED_ENTRY_ARRAY_WITH_INVALID_CONTENT_VALUE_KEY_ENTRIES_Returns_With_Errors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_INVALID_CONTENT_VALUE_KEY_ENTRIES;
            SetValidatorFIeld("defaultLanguage", defaultLanguage);
            SetValidatorFIeld("availableLanguages", availableLanguages);
            SetValidatorFIeld("contentDimensionNames", contentDimensionNames);
            SetValidatorFIeld("dimensionValueNames", new Dictionary<KeyValuePair<string, string>, string[]>
                {
                    { new KeyValuePair<string, string>( "fi", "bar" ), ["foo"] },
                    { new KeyValuePair<string, string>( "en", "bar-en" ), ["foo-en"] },
                });

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateFindContentDimensionKeys(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.RequiredKeyMissing, result[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.RequiredKeyMissing, result[1].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateFindDimensionRecommendedKeys_Called_With_STRUCTURED_ENTRY_ARRAY_WITH_INCOMPLETE_VARIABLE_RECOMMENDED_KEYS_Returns_With_Errors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_INCOMPLETE_VARIABLE_RECOMMENDED_KEYS;
            SetValidatorFIeld("defaultLanguage", defaultLanguage);
            SetValidatorFIeld("availableLanguages", availableLanguages);
            SetValidatorFIeld("contentDimensionNames", contentDimensionNames);
            SetValidatorFIeld("stubDimensionNames", stubDimensionNames);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateFindDimensionRecommendedKeys(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.RecommendedKeyMissing, result[0].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.RecommendedKeyMissing, result[1].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.RecommendedKeyMissing, result[2].Feedback.Rule);
            Assert.AreEqual(ValidationFeedbackRule.RecommendedKeyMissing, result[3].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateUnexpectedSpecifiers_Called_With_StructuredEntryWithIllegalSpecifiers_Returns_With_Errors()
        {
            // Arrange
            ValidationStructuredEntry entry = ContentValidationFixtures.StructuredEntryWithIllegalSpecifiers;

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateUnexpectedSpecifiers(
                entry,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.IllegalSpecifierDefinitionFound, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateUnexpectedSpecifiers_Called_With_StructuredEntryWithIllegalLanguageParameter_Returns_With_Errors()
        {
            // Arrange
            ValidationStructuredEntry entry = ContentValidationFixtures.StructuredEntryWithIllegalLanguageParameter;

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateUnexpectedLanguageParams(
                entry,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.IllegalLanguageDefinitionFound, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateLanguageParams_Called_With_Undefined_Language_Returns_With_Errors()
        {
            // Arrange
            ValidationStructuredEntry entry = ContentValidationFixtures.StructuredEntryWithUndefinedLanguage;
            SetValidatorFIeld("defaultLanguage", defaultLanguage);
            SetValidatorFIeld("availableLanguages", availableLanguages);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateLanguageParams(
                entry,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.UndefinedLanguageFound, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateSpecifiers_Called_With_Undefined_First_Specifier_Returns_With_Errors()
        {
            // Arrange
            ValidationStructuredEntry entry = ContentValidationFixtures.StructuredEntryWithUndefinedFirstSpecifier;
            SetValidatorFIeld("defaultLanguage", defaultLanguage);
            SetValidatorFIeld("availableLanguages", availableLanguages);
            SetValidatorFIeld("stubDimensionNames", stubDimensionNames);
            SetValidatorFIeld("dimensionValueNames", dimensionValueNames);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateSpecifiers(
                entry,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.IllegalSpecifierDefinitionFound, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateSpecifiers_Called_With_Undefined_Second_Specifier_Returns_With_Errors()
        {
            // Arrange
            ValidationStructuredEntry entry = ContentValidationFixtures.StructuredEntryWithUndefinedSecondSpecifier;
            SetValidatorFIeld("defaultLanguage", defaultLanguage);
            SetValidatorFIeld("availableLanguages", availableLanguages);
            SetValidatorFIeld("stubDimensionNames", stubDimensionNames);
            SetValidatorFIeld("dimensionValueNames", dimensionValueNames);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateSpecifiers(
                entry,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.IllegalSpecifierDefinitionFound, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateValueTypes_Called_With_STRUCTURED_ENTRY_ARRAY_WITH_INVALID_VALUE_TYPES_Returns_With_Errors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_INVALID_VALUE_TYPES;

            // Act
            foreach (ValidationStructuredEntry entry in entries)
            {
                ValidationFeedbackItem[]? result = ContentValidator.ValidateValueTypes(
                    entry,
                    validator
                    );

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Length);
                Assert.AreEqual(ValidationFeedbackRule.UnmatchingValueType, result[0].Feedback.Rule);
            }
        }

        [TestMethod]
        public void ValidateValueTypes_Called_With_STRUCTURED_ENTRY_ARRAY_WITH_WRONG_VALUES_Returns_With_Errors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_WRONG_VALUES;

            // Act
            foreach (ValidationStructuredEntry entry in entries)
            {
                ValidationFeedbackItem[]? result = ContentValidator.ValidateValueContents(
                    entry,
                    validator
                    );

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Length);
                Assert.AreEqual(ValidationFeedbackRule.InvalidValueFound, result[0].Feedback.Rule);
            }
        }

        [TestMethod]
        public void ValidateValueAmounts_Called_With_Unmatching_Amount_Of_Elements_Returns_With_Error()
        {
            // Arrange
            ValidationStructuredEntry entry = ContentValidationFixtures.StructuredEntryWithUnmatchingAmountOfElements;
            SetValidatorFIeld("defaultLanguage", defaultLanguage);
            SetValidatorFIeld("availableLanguages", availableLanguages);
            SetValidatorFIeld("stubDimensionNames", stubDimensionNames);
            SetValidatorFIeld("dimensionValueNames", dimensionValueNames);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateValueAmounts(
                entry,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.UnmatchingValueAmount, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateValueUppercaseRecommendations_Called_With_Lower_Case_Entry_Returns_With_Warning()
        {
            // Arrange
            ValidationStructuredEntry entry = ContentValidationFixtures.StructuredEntryWithLowerCaseValue;

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateValueUppercaseRecommendations(
                entry,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.ValueIsNotInUpperCase, result[0].Feedback.Rule);
        }

        private void SetValidatorFIeld(string fieldName, object value)
        {
            var propertyInfo = validator.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            propertyInfo?.SetValue(validator, value);
        }
    }
}
