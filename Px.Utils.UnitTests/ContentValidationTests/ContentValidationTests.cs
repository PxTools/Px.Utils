using PxUtils.PxFile;
using PxUtils.UnitTests.ContentValidationTests.Fixtures;
using PxUtils.Validation;
using PxUtils.Validation.ContentValidation;
using PxUtils.Validation.SyntaxValidation;
using System.Text;

namespace PxUtils.UnitTests.ContentValidationTests
{
    [TestClass]
    public class ContentValidationTests
    {
        private readonly string filename = "foo";
        private readonly PxFileSyntaxConf syntaxConf = PxFileSyntaxConf.Default;
        private ValidationFeedbackItem[] feedback = [];
        private readonly Encoding encoding = Encoding.UTF8;

        [TestMethod]
        public void ValidatePxFileContent_Called_With_MINIMAL_STRUCTURED_ENTRY_ARRAY_Returns_Valid_Result()
        {
            // Arrange
            ValidationStructuredEntry[] entries = ContentValidationFixtures.MINIMAL_STRUCTURED_ENTRY_ARRAY;
            ContentValidator validator = new(filename, encoding);

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
            ContentValidator validator = new(filename, encoding);

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
            ContentValidator validator = new(filename, encoding);

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateFindDefaultLanguage(
                entries,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding);

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateFindAvailableLanguages(
                entries,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding)
            {
                DefaultLanguage = "foo",
                AvailableLanguages = ["fi", "en"]
            };

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateDefaultLanguageDefinedInAvailableLanguages(
                entries,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding)
            {
                DefaultLanguage = "fi",
                AvailableLanguages = ["fi", "en"]
            };

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateFindContentDimension(
                entries,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding);

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateFindRequiredCommonKeys(
                entries,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding)
            {
                DefaultLanguage = "fi",
                AvailableLanguages = ["fi", "en"]
            };

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateFindStubAndHeading(
                entries,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding)
            {
                DefaultLanguage = "fi",
                AvailableLanguages = ["fi", "en"]

            };

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateFindRecommendedKeys(
                entries,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding)
            {
                DefaultLanguage = "fi",
                AvailableLanguages = ["fi", "en"],
                StubDimensionNames = new()
                {
                    { "fi", ["bar", "bar-time"] },
                    { "en", ["bar-en", "bar-time-en"] }
                }
            };

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateFindDimensionValues(
                entries,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding)
            {
                DefaultLanguage = "fi",
                AvailableLanguages = ["fi", "en"],
                ContentDimensionNames = new()
                {
                    { "fi", "bar" },
                    { "en", "bar-en" }
                },
                DimensionValueNames = new()
                {
                    { new KeyValuePair<string, string>( "fi", "bar" ), ["foo"] },
                    { new KeyValuePair<string, string>( "en", "bar-en" ), ["foo-en"] },
                }
            };

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateFindContentDimensionKeys(
                entries,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding)
            {
                DefaultLanguage = "fi",
                AvailableLanguages = ["fi", "en"],
                ContentDimensionNames = new()
                {
                    { "fi", "bar" },
                    { "en", "bar-en" }
                },
                StubDimensionNames = new()
                {
                    { "fi", ["bar", "bar-time"] },
                    { "en", ["bar-en", "bar-time-en"] }
                }
            };

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateFindDimensionRecommendedKeys(
                entries,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding);

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateUnexpectedSpecifiers(
                entry,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding);

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateUnexpectedLanguageParams(
                entry,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding)
            {
                DefaultLanguage = "fi",
                AvailableLanguages = ["fi", "en"]
            };

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateLanguageParams(
                entry,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding)
            {
                DefaultLanguage = "fi",
                AvailableLanguages = ["fi", "en"],
                StubDimensionNames = new()
                {
                    { "fi", ["bar", "bar-time"] },
                    { "en", ["bar-en", "bar-time-en"] }
                },
                DimensionValueNames = new()
                {
                    { new KeyValuePair<string, string>( "fi", "bar" ), ["foo"] },
                    { new KeyValuePair<string, string>( "fi", "bar-time" ), ["foo-time"] },
                    { new KeyValuePair<string, string>( "en", "bar-en" ), ["foo-en"] },
                    { new KeyValuePair<string, string>( "en", "bar-time-en" ), ["foo-time-en"] },
                }
            };

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateSpecifiers(
                entry,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding)
            {
                DefaultLanguage = "fi",
                AvailableLanguages = ["fi", "en"],
                StubDimensionNames = new()
                {
                    { "fi", ["bar", "bar-time"] },
                    { "en", ["bar-en", "bar-time-en"] }
                },
                DimensionValueNames = new()
                {
                    { new KeyValuePair<string, string>( "fi", "bar" ), ["foo"] },
                    { new KeyValuePair<string, string>( "fi", "bar-time" ), ["foo-time"] },
                    { new KeyValuePair<string, string>( "en", "bar-en" ), ["foo-en"] },
                    { new KeyValuePair<string, string>( "en", "bar-time-en" ), ["foo-time-en"] },
                }
            };

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateSpecifiers(
                entry,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding);

            // Act
            foreach (ValidationStructuredEntry entry in entries)
            {
                ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateValueTypes(
                    entry,
                    syntaxConf,
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
            ContentValidator validator = new(filename, encoding);

            // Act
            foreach (ValidationStructuredEntry entry in entries)
            {
                ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateValueContents(
                    entry,
                    syntaxConf,
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
            ContentValidator validator = new(filename, encoding)
            {
                DefaultLanguage = "fi",
                AvailableLanguages = ["fi", "en"],
                StubDimensionNames = new()
                {
                    { "fi", ["bar", "bar-time"] },
                    { "en", ["bar-en", "bar-time-en"] }
                },
                DimensionValueNames = new()
                {
                    { new KeyValuePair<string, string>( "fi", "bar" ), ["foo"] },
                    { new KeyValuePair<string, string>( "fi", "bar-time" ), ["foo-time"] },
                    { new KeyValuePair<string, string>( "en", "bar-en" ), ["foo-en"] },
                    { new KeyValuePair<string, string>( "en", "bar-time-en" ), ["foo-time-en"] },
                }
            };

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateValueAmounts(
                entry,
                syntaxConf,
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
            ContentValidator validator = new(filename, encoding);

            // Act
            ValidationFeedbackItem[]? result = ContentValidationFunctions.ValidateValueUppercaseRecommendations(
                entry,
                syntaxConf,
                validator
                );

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(ValidationFeedbackRule.ValueIsNotInUpperCase, result[0].Feedback.Rule);
        }
    }
}
