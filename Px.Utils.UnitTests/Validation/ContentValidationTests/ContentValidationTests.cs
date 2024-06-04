using Px.Utils.PxFile;
using Px.Utils.UnitTests.Validation.Fixtures;
using Px.Utils.Validation;
using Px.Utils.Validation.ContentValidation;
using Px.Utils.Validation.SyntaxValidation;
using System.Text;
using System.Reflection;

namespace Px.Utils.UnitTests.Validation.ContentValidationTests
{
    [TestClass]
    public class ContentValidationTests
    {
        private static readonly string filename = "foo";
        private static readonly Encoding encoding = Encoding.UTF8;
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
        public void ValidatePxFileContentCalledWithMinimalStructuredEntryReturnsValidResult()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.MINIMAL_STRUCTURED_ENTRY_ARRAY;
            ContentValidator validator = new(filename, encoding, entries);

            // Act
            ValidationFeedbackItem[] feedback = validator.Validate().FeedbackItems;

            // Assert
            Assert.AreEqual(0, feedback.Length);
        }

        [TestMethod]
        public void ValidateFindDefaultLanguageWithEmptyStructuredEntryArrayReturnsWithError()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.EMPTY_STRUCTURED_ENTRY_ARRAY;
            ContentValidator validator = new(filename, encoding, entries);

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
        public void ValidateFindAvailableLanguagesWithEmptyStructuredArrayReturnsWithWarning()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.EMPTY_STRUCTURED_ENTRY_ARRAY;
            ContentValidator validator = new(filename, encoding, entries);

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
        public void ValidateDefaultLanguageDefinedInAvailableLanguagesCalledWithUndefinedDefaultLanguageReturnsWithError()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_DEFAULT_LANGUAGE;
            ContentValidator validator = new(filename, encoding, entries);
            SetValidatorField(validator, "_defaultLanguage", "foo");
            SetValidatorField(validator, "_availableLanguages", availableLanguages);

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
        public void ValidateFindContentDimensionCalledWithMissingContVariableReturnsWithError()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_MISSING_CONTVARIABLE;
            ContentValidator validator = new(filename, encoding, entries);
            SetValidatorField(validator, "_defaultLanguage", defaultLanguage);
            SetValidatorField(validator, "_availableLanguages", availableLanguages);

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
        public void ValidateFindRequiredCommonKeysCalledWithEmptyStructuredEntryArrayYReturnsWithError()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.EMPTY_STRUCTURED_ENTRY_ARRAY;
            ContentValidator validator = new(filename, encoding, entries);

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
        public void ValidateFindStubOrHeadingCalledWithWithMissingHeadingReturnsWithError()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_STUB;
            ContentValidator validator = new(filename, encoding, entries);
            SetValidatorField(validator, "_defaultLanguage", defaultLanguage);
            SetValidatorField(validator, "_availableLanguages", availableLanguages);

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
        public void ValidateFindRecommendedKeysCalledWithMissingDescriptionsReturnsWithWarnings()
        {

            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_DESCRIPTION;
            ContentValidator validator = new(filename, encoding, entries);
            SetValidatorField(validator, "_defaultLanguage", defaultLanguage);
            SetValidatorField(validator, "_availableLanguages", availableLanguages);

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
        public void ValidateFindDimenionsValuesCalledWithMissingDimensionValuesReturnsErrors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_DIMENSIONVALUES;
            ContentValidator validator = new(filename, encoding, entries);
            SetValidatorField(validator, "_defaultLanguage", defaultLanguage);
            SetValidatorField(validator, "_availableLanguages", availableLanguages);
            SetValidatorField(validator, "_stubDimensionNames", new Dictionary<string, string[]>
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
        public void ValidateFindContentDimensionKeysCalledWithInvalidContentValueKeyEntriesSReturnsWithErrors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_INVALID_CONTENT_VALUE_KEY_ENTRIES;
            ContentValidator validator = new(filename, encoding, entries);
            SetValidatorField(validator, "_defaultLanguage", defaultLanguage);
            SetValidatorField(validator, "_availableLanguages", availableLanguages);
            SetValidatorField(validator, "_contentDimensionNames", contentDimensionNames);
            SetValidatorField(validator, "_dimensionValueNames", new Dictionary<KeyValuePair<string, string>, string[]>
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
        public void ValidateFindDimensionRecommendedKeysCalledWithIncompleteVariableRecommendedKeysReturnsWithErrors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_INCOMPLETE_VARIABLE_RECOMMENDED_KEYS;
            ContentValidator validator = new(filename, encoding, entries);
            SetValidatorField(validator, "_defaultLanguage", defaultLanguage);
            SetValidatorField(validator, "_availableLanguages", availableLanguages);
            SetValidatorField(validator, "contentDimensionNames", contentDimensionNames);
            SetValidatorField(validator, "_stubDimensionNames", stubDimensionNames);

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
        public void ValidateUnexpectedSpecifiersCalledWithStructuredEntryWithIllegalSpecifiersReturnsWithErrors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = [ContentValidationFixtures.StructuredEntryWithIllegalSpecifiers];
            ContentValidator validator = new(filename, encoding, entries);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateUnexpectedSpecifiers(
                entries[0],
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.IllegalSpecifierDefinitionFound, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateUnexpectedSpecifiersCalledWithStructuredEntryWithIllegalLanguageParameterReturnsWithErrors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = [ContentValidationFixtures.StructuredEntryWithIllegalLanguageParameter];
            ContentValidator validator = new(filename, encoding, entries);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateUnexpectedLanguageParams(
                entries[0],
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.IllegalLanguageDefinitionFound, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateLanguageParamsCalledWithUndefinedLanguageReturnshWithErrors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = [ContentValidationFixtures.StructuredEntryWithUndefinedLanguage];
            ContentValidator validator = new(filename, encoding, entries);
            SetValidatorField(validator, "_defaultLanguage", defaultLanguage);
            SetValidatorField(validator, "_availableLanguages", availableLanguages);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateLanguageParams(
                entries[0],
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.UndefinedLanguageFound, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateSpecifiersCalledWithUndefinedFirstSpecifierReturnsWithErrors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = [ContentValidationFixtures.StructuredEntryWithUndefinedFirstSpecifier];
            ContentValidator validator = new(filename, encoding, entries);
            SetValidatorField(validator, "_defaultLanguage", defaultLanguage);
            SetValidatorField(validator, "_availableLanguages", availableLanguages);
            SetValidatorField(validator, "_stubDimensionNames", stubDimensionNames);
            SetValidatorField(validator, "_dimensionValueNames", dimensionValueNames);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateSpecifiers(
                entries[0],
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.IllegalSpecifierDefinitionFound, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateSpecifiersCalledWithUndefinedSecondSpecifierReturnsWithErrors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = [ContentValidationFixtures.StructuredEntryWithUndefinedSecondSpecifier];
            ContentValidator validator = new(filename, encoding, entries);
            SetValidatorField(validator, "_defaultLanguage", defaultLanguage);
            SetValidatorField(validator, "_availableLanguages", availableLanguages);
            SetValidatorField(validator, "_stubDimensionNames", stubDimensionNames);
            SetValidatorField(validator, "_dimensionValueNames", dimensionValueNames);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateSpecifiers(
                entries[0],
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.IllegalSpecifierDefinitionFound, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateValueTypesCalledWithStructuredEntryArrayWithInvalidValueTypesReturnsWithErrors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_INVALID_VALUE_TYPES;
            ContentValidator validator = new(filename, encoding, entries);

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
        public void ValidateValueTypesCalledWithStructuredEntryArrayWithWrongValuesReturnsWithErrors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_WRONG_VALUES;
            ContentValidator validator = new(filename, encoding, entries);

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
        public void ValidateValueAmountsCalledWithUnmatchingAmountOfElementsReturnsWithError()
        {
            // Arrange
            ValidationStructuredEntry[] entries = [ContentValidationFixtures.StructuredEntryWithUnmatchingAmountOfElements];
            ContentValidator validator = new(filename, encoding, entries);
            SetValidatorField(validator, "_defaultLanguage", defaultLanguage);
            SetValidatorField(validator, "_availableLanguages", availableLanguages);
            SetValidatorField(validator, "_stubDimensionNames", stubDimensionNames);
            SetValidatorField(validator, "_dimensionValueNames", dimensionValueNames);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateValueAmounts(
                entries[0],
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.UnmatchingValueAmount, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateValueUppercaseRecommendationsCalledWithLowerCaseEntryReturnsWithWarning()
        {
            // Arrange
            ValidationStructuredEntry[] entries = [ContentValidationFixtures.StructuredEntryWithLowerCaseValue];
            ContentValidator validator = new(filename, encoding, entries);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateValueUppercaseRecommendations(
                entries[0],
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.ValueIsNotInUpperCase, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateContentWithCustomFunctionsReturnsValidResult()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.MINIMAL_STRUCTURED_ENTRY_ARRAY;
            ContentValidator validator = new(filename, encoding, entries, new MockCustomContentValidationFunctions());

            // Act
            ValidationFeedbackItem[] feedback = validator.Validate().FeedbackItems;

            // Assert
            Assert.AreEqual(0, feedback.Length);
        }

        [TestMethod]
        public void ValidateFindDefaultWithMultipleDefaultLanguagesReturnsError()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_TWO_DEFAULT_LANGUAGES;
            ContentValidator validator = new(filename, encoding, entries);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateFindDefaultLanguage(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.MultipleInstancesOfUniqueKey, result[0].Feedback.Rule);
        }

        [TestMethod]
        public void ValidateFindAvailableLanguagesWithMultipleAvailableLanguagesEntriesReturnsError()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_TWO_AVAILABLE_LANGUAGES_ENTRIES;
            ContentValidator validator = new(filename, encoding, entries);

            // Act
            ValidationFeedbackItem[]? result = ContentValidator.ValidateFindAvailableLanguages(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.MultipleInstancesOfUniqueKey, result[0].Feedback.Rule);
        }

        private static void SetValidatorField(ContentValidator validator, string fieldName, object value)
        {
            var propertyInfo = validator.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            propertyInfo?.SetValue(validator, value);
        }
    }
}
