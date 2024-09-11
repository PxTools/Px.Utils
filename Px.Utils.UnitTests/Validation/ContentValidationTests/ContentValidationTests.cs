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
            ContentValidationResult feedback = validator.Validate();

            // Assert
            Assert.AreEqual(0, feedback.FeedbackItems.Count);
        }

        [TestMethod]
        public void ValidateFindDefaultLanguageWithEmptyStructuredEntryArrayReturnsWithError()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.EMPTY_STRUCTURED_ENTRY_ARRAY;
            ContentValidator validator = new(filename, encoding, entries);

            // Act
            ValidationFeedback? result = ContentValidator.ValidateFindDefaultLanguage(
                entries,
                validator
                );
                
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.MissingDefaultLanguage, result.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateFindAvailableLanguagesWithEmptyStructuredArrayReturnsWithWarning()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.EMPTY_STRUCTURED_ENTRY_ARRAY;
            ContentValidator validator = new(filename, encoding, entries);

            // Act
            ValidationFeedback? result = ContentValidator.ValidateFindAvailableLanguages(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.RecommendedKeyMissing, result.First().Key.Rule);
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
            ValidationFeedback? result = ContentValidator.ValidateDefaultLanguageDefinedInAvailableLanguages(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.UndefinedLanguageFound, result.First().Key.Rule);
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
            ValidationFeedback? result = ContentValidator.ValidateFindContentDimension(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.RecommendedKeyMissing, result.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateFindRequiredCommonKeysCalledWithEmptyStructuredEntryArrayYReturnsWithError()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.EMPTY_STRUCTURED_ENTRY_ARRAY;
            ContentValidator validator = new(filename, encoding, entries);

            // Act
            ValidationFeedback? result = ContentValidator.ValidateFindRequiredCommonKeys(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, result.First().Value.Count);   
            Assert.AreEqual(ValidationFeedbackRule.RequiredKeyMissing, result.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateFindStubOrHeadingCalledWithListsOfNamesReturnWithNoErrors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_MULTIPLE_DIMENSION_NAMES;
            ContentValidator validator = new(filename, encoding, entries);
            SetValidatorField(validator, "_defaultLanguage", defaultLanguage);
            SetValidatorField(validator, "_availableLanguages", availableLanguages);

            // Act
            ValidationFeedback? result = ContentValidator.ValidateFindStubAndHeading(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
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
            ValidationFeedback? result = ContentValidator.ValidateFindStubAndHeading(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.MissingStubAndHeading, result.First().Key.Rule);
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
            ValidationFeedback? result = ContentValidator.ValidateFindRecommendedKeys(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.First().Value.Count);
            Assert.AreEqual(ValidationFeedbackRule.RecommendedKeyMissing, result.First().Key.Rule);
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
            ValidationFeedback? result = ContentValidator.ValidateFindDimensionValues(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, result.First().Value.Count);
            Assert.AreEqual(ValidationFeedbackRule.VariableValuesMissing, result.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateFindDimensionValuesCalledWithDuplicateEntriesReturnsErrors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_DUPLICATE_DIMENSION_VALUES;
            ContentValidator validator = new(filename, encoding, entries);
            SetValidatorField(validator, "_defaultLanguage", defaultLanguage);
            SetValidatorField(validator, "_availableLanguages", availableLanguages);
            SetValidatorField(validator, "_stubDimensionNames", new Dictionary<string, string[]>
            {
                { "fi", ["bar"] },
                { "en", ["bar-en"] }
            });
            SetValidatorField(validator, "_headingDimensionNames", new Dictionary<string, string[]>
            {
                { "fi", ["bar"] },
            });

            // Act
            ValidationFeedback? result = ContentValidator.ValidateFindDimensionValues(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.DuplicateEntry, result.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateFindContentDimensionKeysCalledWithMissingContentValueKeyEntriesReturnsWithErrors()
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
            ValidationFeedback? result = ContentValidator.ValidateFindContentDimensionKeys(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.First().Value.Count);
            Assert.AreEqual(ValidationFeedbackRule.RequiredKeyMissing, result.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateFindContentDimensionKeysCalledWithMissingRecommendedSpecifiersEntriesReturnsWithWarnings()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_MISSING_RECIMMENDED_SPECIFIERS;
            ContentValidator validator = new(filename, encoding, entries);
            SetValidatorField(validator, "_defaultLanguage", defaultLanguage);
            SetValidatorField(validator, "_availableLanguages", availableLanguages);
            SetValidatorField(validator, "_contentDimensionNames", contentDimensionNames);
            SetValidatorField(validator, "_dimensionValueNames", new Dictionary<KeyValuePair<string, string>, string[]>
                {
                    { new KeyValuePair<string, string>( "fi", "bar" ), ["foo"] },
                    { new KeyValuePair<string, string>( "en", "bar-en" ), ["foo-en"] },
                });
            ValidationFeedbackKey recommendedKeyFeedbackKey = new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.RecommendedKeyMissing);
            ValidationFeedbackKey recommendedSpecifierFeedbackKey = new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.RecommendedSpecifierDefinitionMissing); 


            // Act
            ValidationFeedback? result = ContentValidator.ValidateFindContentDimensionKeys(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey(recommendedKeyFeedbackKey));
            Assert.IsTrue(result.ContainsKey(recommendedSpecifierFeedbackKey));
            Assert.AreEqual(2, result[recommendedSpecifierFeedbackKey].Count);
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
            ValidationFeedback? result = ContentValidator.ValidateFindDimensionRecommendedKeys(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(4, result.First().Value.Count);
            Assert.AreEqual(ValidationFeedbackRule.RecommendedKeyMissing, result.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateUnexpectedSpecifiersCalledWithStructuredEntryWithIllegalSpecifiersReturnsWithErrors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = [ContentValidationFixtures.StructuredEntryWithIllegalSpecifiers];
            ContentValidator validator = new(filename, encoding, entries);

            // Act
            ValidationFeedback? result = ContentValidator.ValidateUnexpectedSpecifiers(
                entries[0],
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalSpecifierDefinitionFound, result.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateUnexpectedSpecifiersCalledWithStructuredEntryWithIllegalLanguageParameterReturnsWithErrors()
        {
            // Arrange
            ValidationStructuredEntry[] entries = [ContentValidationFixtures.StructuredEntryWithIllegalLanguageParameter];
            ContentValidator validator = new(filename, encoding, entries);

            // Act
            ValidationFeedback? result = ContentValidator.ValidateUnexpectedLanguageParams(
                entries[0],
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalLanguageDefinitionFound, result.First().Key.Rule);
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
            ValidationFeedback? result = ContentValidator.ValidateLanguageParams(
                entries[0],
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.UndefinedLanguageFound, result.First().Key.Rule);
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
            ValidationFeedback? result = ContentValidator.ValidateSpecifiers(
                entries[0],
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalSpecifierDefinitionFound, result.First().Key.Rule);
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
            ValidationFeedback? result = ContentValidator.ValidateSpecifiers(
                entries[0],
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.IllegalSpecifierDefinitionFound, result.First().Key.Rule);
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
                ValidationFeedback? result = ContentValidator.ValidateValueTypes(
                    entry,
                    validator
                    );

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(ValidationFeedbackRule.UnmatchingValueType, result.First().Key.Rule);
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
                ValidationFeedback? result = ContentValidator.ValidateValueContents(
                    entry,
                    validator
                    );

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(ValidationFeedbackRule.InvalidValueFound, result.First().Key.Rule);
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
            ValidationFeedback? result = ContentValidator.ValidateValueAmounts(
                entries[0],
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.UnmatchingValueAmount, result.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateValueUppercaseRecommendationsCalledWithLowerCaseEntryReturnsWithWarning()
        {
            // Arrange
            ValidationStructuredEntry[] entries = [ContentValidationFixtures.StructuredEntryWithLowerCaseValue];
            ContentValidator validator = new(filename, encoding, entries);

            // Act
            ValidationFeedback? result = ContentValidator.ValidateValueUppercaseRecommendations(
                entries[0],
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.ValueIsNotInUpperCase, result.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateContentWithCustomFunctionsReturnsValidResult()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.MINIMAL_STRUCTURED_ENTRY_ARRAY;
            ContentValidator validator = new(filename, encoding, entries, new MockCustomContentValidationFunctions());

            // Act
            ValidationFeedback feedback = validator.Validate().FeedbackItems;

            // Assert
            Assert.AreEqual(0, feedback.Count);
        }

        [TestMethod]
        public void ValidateFindDefaultWithMultipleDefaultLanguagesReturnsError()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_TWO_DEFAULT_LANGUAGES;
            ContentValidator validator = new(filename, encoding, entries);

            // Act
            ValidationFeedback? result = ContentValidator.ValidateFindDefaultLanguage(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.MultipleInstancesOfUniqueKey, result.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateFindAvailableLanguagesWithMultipleAvailableLanguagesEntriesReturnsError()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.STRUCTURED_ENTRY_ARRAY_WITH_TWO_AVAILABLE_LANGUAGES_ENTRIES;
            ContentValidator validator = new(filename, encoding, entries);

            // Act
            ValidationFeedback? result = ContentValidator.ValidateFindAvailableLanguages(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.MultipleInstancesOfUniqueKey, result.First().Key.Rule);
        }

        [TestMethod]
        public void ValidateFindStubAndHeadingWithMissingStubAndHeadingReturnsError()
        {
            // Arrange
            ValidationStructuredEntry[] entries =  [];
            ContentValidator validator = new(filename, encoding, entries);

            // Act
            ValidationFeedback? result = ContentValidator.ValidateFindStubAndHeading(
                entries,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ValidationFeedbackRule.MissingStubAndHeading, result.First().Key.Rule);
        }

        private static void SetValidatorField(ContentValidator validator, string fieldName, object value)
        {
            var propertyInfo = validator.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            propertyInfo?.SetValue(validator, value);
        }
    }
}
