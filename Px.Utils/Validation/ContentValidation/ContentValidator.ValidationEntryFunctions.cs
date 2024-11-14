using Px.Utils.Validation.SyntaxValidation;
using System.Globalization;

namespace Px.Utils.Validation.ContentValidation
{
    public delegate ValidationFeedback? ContentValidationEntryValidator(ValidationStructuredEntry entry, ContentValidator validator);
    
    /// <summary>
    /// Collection of functions for validating Px file metadata contents by processing individual entries
    /// </summary>
    public sealed partial class ContentValidator
    {
        public List<ContentValidationEntryValidator> DefaultContentValidationEntryFunctions { get; } = [
            ValidateUnexpectedSpecifiers,
            ValidateUnexpectedLanguageParams,
            ValidateLanguageParams,
            ValidateSpecifiers,
            ValidateValueTypes,
            ValidateValueContents,
            ValidateValueAmounts,
            ValidateValueUppercaseRecommendations
        ];

        /// <summary>
        /// Validates that given entry does not contain specifiers if it is not allowed to have them based on keyword.
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Key value pair containing information about the rule violation is returned if an unexpected specifier is detected.</returns>
        public static ValidationFeedback? ValidateUnexpectedSpecifiers(ValidationStructuredEntry entry, ContentValidator validator)
        {
            string[] noSpecifierAllowedKeywords =
            [
                validator.Conf.Tokens.KeyWords.DefaultLanguage,
                validator.Conf.Tokens.KeyWords.AvailableLanguages,
                validator.Conf.Tokens.KeyWords.Charset,
                validator.Conf.Tokens.KeyWords.CodePage,
                validator.Conf.Tokens.KeyWords.StubDimensions,
                validator.Conf.Tokens.KeyWords.HeadingDimensions,
                validator.Conf.Tokens.KeyWords.TableId,
                validator.Conf.Tokens.KeyWords.Description,
                validator.Conf.Tokens.KeyWords.ContentVariableIdentifier,
            ];

            if (!noSpecifierAllowedKeywords.Contains(entry.Key.Keyword))
            {
                return null;
            }

            if (entry.Key.FirstSpecifier is not null || entry.Key.SecondSpecifier is not null)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);

                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.IllegalSpecifierDefinitionFound),
                    new(validator._filename,
                        feedbackIndexes.Key,
                        0,
                        $"{entry.Key.Keyword}: {entry.Key.FirstSpecifier}, {entry.Key.SecondSpecifier}")
                );

                return new(feedback);
            }

            return null;
        }

        /// <summary>
        /// Finds unexpected language parameters in the Px file metadata entry
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Key value pair containing information about the rule violation is returned if an illegal or unrecommended language parameter is detected in the entry</returns>
        public static ValidationFeedback? ValidateUnexpectedLanguageParams(ValidationStructuredEntry entry, ContentValidator validator)
        {
            string[] noLanguageParameterAllowedKeywords = [
                validator.Conf.Tokens.KeyWords.Charset,
                validator.Conf.Tokens.KeyWords.CodePage,
                validator.Conf.Tokens.KeyWords.TableId,
                validator.Conf.Tokens.KeyWords.DefaultLanguage,
                validator.Conf.Tokens.KeyWords.AvailableLanguages,
            ];

            string[] noLanguageParameterRecommendedKeywords = [
                validator.Conf.Tokens.KeyWords.LastUpdated,
                validator.Conf.Tokens.KeyWords.DimensionType,
                validator.Conf.Tokens.KeyWords.Precision
            ];

            if (!noLanguageParameterAllowedKeywords.Contains(entry.Key.Keyword) && !noLanguageParameterRecommendedKeywords.Contains(entry.Key.Keyword))
            {
                return null;
            }

            if (entry.Key.Language is not null)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);

                if (noLanguageParameterAllowedKeywords.Contains(entry.Key.Keyword))
                {
                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                        new(ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.IllegalLanguageDefinitionFound),
                        new(validator._filename,
                            feedbackIndexes.Key,
                            0,
                            $"{entry.Key.Keyword}, {entry.Key.Language}, {entry.Key.FirstSpecifier}, {entry.Key.SecondSpecifier}")
                    );

                    return new(feedback);
                }
                else if (noLanguageParameterRecommendedKeywords.Contains(entry.Key.Keyword))
                {
                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                        new(ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.UnrecommendedLanguageDefinitionFound),
                        new(validator._filename,
                            feedbackIndexes.Key,
                            0,
                            $"{entry.Key.Keyword}, {entry.Key.Language}, {entry.Key.FirstSpecifier}, {entry.Key.SecondSpecifier}")
                    );

                    return new(feedback);
                }
            }

            return null;
        }

        /// <summary>
        /// Validates that the language defined in the Px file metadata entry is defined in the available languages entry
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Key value pair containing information about the rule violation is returned if an undefined language is found from the entry</returns>
        public static ValidationFeedback? ValidateLanguageParams(ValidationStructuredEntry entry, ContentValidator validator)
        {
            if (entry.Key.Language is null)
            {
                return null;
            }

            if (validator._availableLanguages is not null && !validator._availableLanguages.Contains(entry.Key.Language))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);


                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.UndefinedLanguageFound),
                    new(validator._filename,
                        feedbackIndexes.Key,
                        0,
                        entry.Key.Language)
                );

                return new(feedback);
            }

            return null;
        }

        /// <summary>
        /// Validates that specifiers are defined properly in the Px file metadata entry.
        /// Content dimension specifiers are allowed to be defined using only the first specifier at value level and are checked separately
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Key value pair containing information about the rule violation is returned if the entry's specifier is defined in an unexpected way</returns>
        public static ValidationFeedback? ValidateSpecifiers(ValidationStructuredEntry entry, ContentValidator validator)
        {
            ValidationFeedback feedbackItems = [];
            if ((entry.Key.FirstSpecifier is null && entry.Key.SecondSpecifier is null) || 
                validator._dimensionValueNames is null)
            {
                return null;
            }

            // Content dimension specifiers are allowed to be defined using only the first specifier at value level and are checked separately
            string[] excludeKeywords =
                [
                    validator.Conf.Tokens.KeyWords.Precision,
                    validator.Conf.Tokens.KeyWords.Units,
                    validator.Conf.Tokens.KeyWords.LastUpdated,
                    validator.Conf.Tokens.KeyWords.Contact,
                    validator.Conf.Tokens.KeyWords.ValueNote
                ];

            if (excludeKeywords.Contains(entry.Key.Keyword))
            {
                return null;
            }

            if ((validator._stubDimensionNames is null || !validator._stubDimensionNames.Values.Any(v => v.Contains(entry.Key.FirstSpecifier))) &&
                (validator._headingDimensionNames is null || !validator._headingDimensionNames.Values.Any(v => v.Contains(entry.Key.FirstSpecifier))))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);

                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.IllegalSpecifierDefinitionFound),
                    new(validator._filename,
                        feedbackIndexes.Key,
                        0,
                        entry.Key.FirstSpecifier)
                );
                feedbackItems.Add(feedback);
            }
            else if (entry.Key.SecondSpecifier is not null && 
                !validator._dimensionValueNames.Values.Any(v => v.Contains(entry.Key.SecondSpecifier)))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);

                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.IllegalSpecifierDefinitionFound),
                    new(validator._filename,
                        feedbackIndexes.Key,
                        0,
                        entry.Key.SecondSpecifier)
                );

                feedbackItems.Add(feedback);
            }

            return feedbackItems;
        }

        /// <summary>
        /// Validates that certain keywords are defined with the correct value types in the Px file metadata entry
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Key value pair containing information about the rule violation is returned if an unexpected value type is detected</returns>
        public static ValidationFeedback? ValidateValueTypes(ValidationStructuredEntry entry, ContentValidator validator)
        {
            string[] stringTypes =
                [
                    validator.Conf.Tokens.KeyWords.Charset,
                    validator.Conf.Tokens.KeyWords.CodePage,
                    validator.Conf.Tokens.KeyWords.DefaultLanguage,
                    validator.Conf.Tokens.KeyWords.Units,
                    validator.Conf.Tokens.KeyWords.Description,
                    validator.Conf.Tokens.KeyWords.TableId,
                    validator.Conf.Tokens.KeyWords.ContentVariableIdentifier,
                    validator.Conf.Tokens.KeyWords.DimensionCode
                ];

            string[] listOfStringTypes =
                [
                    validator.Conf.Tokens.KeyWords.AvailableLanguages,
                    validator.Conf.Tokens.KeyWords.StubDimensions,
                    validator.Conf.Tokens.KeyWords.HeadingDimensions,
                    validator.Conf.Tokens.KeyWords.VariableValues,
                    validator.Conf.Tokens.KeyWords.VariableValueCodes
                ];

            string[] dateTimeTypes =
                [
                    validator.Conf.Tokens.KeyWords.LastUpdated
                ];

            string[] numberTypes =
                [
                    validator.Conf.Tokens.KeyWords.Precision
                ];

            string[] timeval =
                [
                    validator.Conf.Tokens.KeyWords.TimeVal
                ];

            if ((stringTypes.Contains(entry.Key.Keyword) && entry.ValueType != ValueType.StringValue) ||
                (listOfStringTypes.Contains(entry.Key.Keyword) && (entry.ValueType != ValueType.ListOfStrings && entry.ValueType != ValueType.StringValue)) ||
                (dateTimeTypes.Contains(entry.Key.Keyword) && entry.ValueType != ValueType.DateTime) ||
                (numberTypes.Contains(entry.Key.Keyword) && entry.ValueType != ValueType.Number) ||
                (timeval.Contains(entry.Key.Keyword) && (entry.ValueType != ValueType.TimeValRange && entry.ValueType != ValueType.TimeValSeries)))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    entry.ValueStartIndex,
                    entry.LineChangeIndexes);

                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.UnmatchingValueType),
                    new(validator._filename,
                        feedbackIndexes.Key,
                        feedbackIndexes.Value,
                        $"{entry.Key.Keyword}: {entry.ValueType}")
                );

                return new(feedback);
            }

            return null;
        }

        /// <summary>
        /// Validates that entries with specific keywords have valid values in the Px file metadata entry
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Key value pair containing information about the rule violation is returned if an unexpected value is detected</returns>
        public static ValidationFeedback? ValidateValueContents(ValidationStructuredEntry entry, ContentValidator validator)
        {
            string[] allowedCharsets = ["ANSI", "Unicode"];

            string[] dimensionTypes = [
                    validator.Conf.Tokens.VariableTypes.Content,
                validator.Conf.Tokens.VariableTypes.Time,
                validator.Conf.Tokens.VariableTypes.Geographical,
                validator.Conf.Tokens.VariableTypes.Ordinal,
                validator.Conf.Tokens.VariableTypes.Nominal,
                validator.Conf.Tokens.VariableTypes.Other,
                validator.Conf.Tokens.VariableTypes.Unknown,
                validator.Conf.Tokens.VariableTypes.Classificatory
                    ];

            string value = SyntaxValidationUtilityMethods.CleanString(entry.Value, validator.Conf);
            if ((entry.Key.Keyword == validator.Conf.Tokens.KeyWords.Charset && !allowedCharsets.Contains(value)) ||
                (entry.Key.Keyword == validator.Conf.Tokens.KeyWords.CodePage && !value.Equals(validator._encoding.BodyName, StringComparison.OrdinalIgnoreCase)) ||
                (entry.Key.Keyword == validator.Conf.Tokens.KeyWords.DimensionType && !dimensionTypes.Contains(value)))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    entry.ValueStartIndex,
                    entry.LineChangeIndexes);

                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.InvalidValueFound),
                    new(validator._filename,
                        feedbackIndexes.Key,
                        feedbackIndexes.Value,
                        $"{entry.Key.Keyword}: {entry.Value}")
                );

                return new(feedback);
            }
            else if (entry.Key.Keyword == validator.Conf.Tokens.KeyWords.ContentVariableIdentifier)
            {
                string defaultLanguage = validator._defaultLanguage ?? string.Empty;
                string lang = entry.Key.Language ?? defaultLanguage;
                if (validator._stubDimensionNames is not null && validator._stubDimensionNames.TryGetValue(lang, out string[]? stubValues) && 
                    !Array.Exists(stubValues, d => d == value) &&
                (validator._headingDimensionNames is not null && validator._headingDimensionNames.TryGetValue(lang, out string[]? headingValues) && 
                    !Array.Exists(headingValues, d => d == value)))
                {
                    KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                        entry.KeyStartLineIndex,
                        entry.ValueStartIndex,
                        entry.LineChangeIndexes);

                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                        new(ValidationFeedbackLevel.Error,
                                ValidationFeedbackRule.InvalidValueFound),
                        new(validator._filename,
                            feedbackIndexes.Key,
                            feedbackIndexes.Value,
                            $"{entry.Key.Keyword}: {entry.Value}")
                    );

                    return new(feedback);
                }
            }

            return null;
        }

        /// <summary>
        /// Validates that entries with specific keywords have the correct amount of values in the Px file metadata entry
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Key value pair containing information about the rule violation is returned if an unexpected amount of values is detected</returns>
        public static ValidationFeedback? ValidateValueAmounts(ValidationStructuredEntry entry, ContentValidator validator)
        {
            if (entry.Key.Keyword != validator.Conf.Tokens.KeyWords.VariableValueCodes ||
                validator._dimensionValueNames is null ||
                entry.Key.FirstSpecifier is null)
            {
                return null;
            }

            string[] codes = entry.Value.Split(validator.Conf.Symbols.Value.ListSeparator);
            string defaultLanguage = validator._defaultLanguage ?? string.Empty;
            string lang = entry.Key.Language ?? defaultLanguage;
            if (codes.Length != validator._dimensionValueNames[new(lang, entry.Key.FirstSpecifier)].Length)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    entry.ValueStartIndex,
                    entry.LineChangeIndexes);

                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                    new(ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.UnmatchingValueAmount),
                    new(validator._filename,
                        feedbackIndexes.Key,
                        feedbackIndexes.Value,
                        $"{entry.Key.Keyword}: {entry.Value}")
                );

                return new(feedback);
            }

            return null;
        }

        /// <summary>
        /// Validates that entries with specific keywords have their values written in upper case
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Key value pair containing information about the rule violation is returned if the found value is not written in upper case</returns>
        public static ValidationFeedback? ValidateValueUppercaseRecommendations(ValidationStructuredEntry entry, ContentValidator validator)
        {
            string[] recommendedUppercaseValueKeywords = [
                validator.Conf.Tokens.KeyWords.CodePage
            ];

            if (!recommendedUppercaseValueKeywords.Contains(entry.Key.Keyword))
            {
                return null;
            }

            // Check if entry.value is in upper case
            string valueUppercase = entry.Value.ToUpper(CultureInfo.InvariantCulture);
            if (entry.Value != valueUppercase)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    entry.ValueStartIndex,
                    entry.LineChangeIndexes);

                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                    new(ValidationFeedbackLevel.Warning,
                        ValidationFeedbackRule.ValueIsNotInUpperCase),
                    new(validator._filename,
                        feedbackIndexes.Key,
                        entry.ValueStartIndex,
                        $"{entry.Key.Keyword}: {entry.Value}")
                );

                return new(feedback);
            }
            return null;
        }
    }
}
