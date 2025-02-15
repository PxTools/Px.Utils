﻿using Px.Utils.Validation.SyntaxValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Px.Utils.Validation.ContentValidation
{
    public delegate ValidationFeedback? ContentValidationFindKeywordValidator(ValidationStructuredEntry[] entries, ContentValidator validator);

    /// <summary>
    /// Collection of functions for validating Px file metadata contents by trying to find specific keywords from all entries.
    /// </summary>
    public sealed partial class ContentValidator
    {
        public List<ContentValidationFindKeywordValidator> DefaultContentValidationFindKeywordFunctions { get; } = [
            ValidateFindDefaultLanguage,
            ValidateFindAvailableLanguages,
            ValidateDefaultLanguageDefinedInAvailableLanguages,
            ValidateFindContentDimension,
            ValidateFindRequiredCommonKeys,
            ValidateFindStubAndHeading,
            ValidateFindRecommendedKeys,
            ValidateFindDimensionValues,
            ValidateFindContentDimensionKeys,
            ValidateFindDimensionRecommendedKeys
        ];

        /// <summary>
        /// Validates that default language is defined properly in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Null if no issues are found.
        /// Key value pairs containing information about rule violations are returned if entry defining default language is not found or if more than one are found.</returns>
        public static ValidationFeedback? ValidateFindDefaultLanguage(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            ValidationStructuredEntry[] langEntries = entries.Where(
                e => e.Key.Keyword.Equals(validator.Conf.Tokens.KeyWords.DefaultLanguage, StringComparison.Ordinal)).ToArray();

            if (langEntries.Length > 1)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entries[0].KeyStartLineIndex,
                    0,
                    entries[0].LineChangeIndexes);

                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MultipleInstancesOfUniqueKey),
                    new(validator._filename,
                        feedbackIndexes.Key,
                        0,
                        $"{validator.Conf.Tokens.KeyWords.DefaultLanguage}: " + string.Join(", ", entries.Select(e => e.Value).ToArray())
                    ));

                return new(feedback);
            }
            else if (langEntries.Length == 0)
            {
                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MissingDefaultLanguage),
                    new(validator._filename, 0, 0)
                );

                return new(feedback);
            }

            validator._defaultLanguage = SyntaxValidationUtilityMethods.CleanString(langEntries[0].Value, validator.Conf);
            return null;
        }

        /// <summary>
        /// Finds an entry that defines available languages in the Px file
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Null if no issues are found. Key value pair containing information about the rule violation with a warning is returned if available languages entry is not found. 
        /// Error is returned if multiple entries are found.</returns>
        public static ValidationFeedback? ValidateFindAvailableLanguages(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            ValidationStructuredEntry[] availableLanguageEntries = entries
                .Where(e => e.Key.Keyword.Equals(validator.Conf.Tokens.KeyWords.AvailableLanguages, StringComparison.Ordinal))
                .ToArray();

            if (availableLanguageEntries.Length > 1)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entries[0].KeyStartLineIndex,
                    0,
                    entries[0].LineChangeIndexes);

                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MultipleInstancesOfUniqueKey),
                    new(validator._filename,
                        feedbackIndexes.Key,
                        0,
                        $"{validator.Conf.Tokens.KeyWords.AvailableLanguages}: " + string.Join(", ", entries.Select(e => e.Value).ToArray()))
                );

                return new(feedback);
            }

            if (availableLanguageEntries.Length == 1)
            {
                List<string> languages = availableLanguageEntries[0].Value.Split(validator.Conf.Symbols.Value.ListSeparator).ToList();
                validator._availableLanguages = [.. languages.Select(lang => SyntaxValidationUtilityMethods.CleanString(lang, validator.Conf))];
                return null;
            }
            else
            {
                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                    new(ValidationFeedbackLevel.Warning,
                        ValidationFeedbackRule.RecommendedKeyMissing),
                    new(validator._filename,
                        0,
                        0,
                        validator.Conf.Tokens.KeyWords.AvailableLanguages)
                );

                return new(feedback);
            }
        }

        /// <summary>
        /// Validates that default language is defined in the available languages entry
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Null if no issues are found. 
        /// Key value pair containing information about the rule violation is returned if default language is not defined in the available languages entry</returns>
        public static ValidationFeedback? ValidateDefaultLanguageDefinedInAvailableLanguages(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            if (validator._availableLanguages is not null && !validator._availableLanguages.Contains(validator._defaultLanguage))
            {
                ValidationStructuredEntry? defaultLanguageEntry = Array.Find(entries, e => e.Key.Keyword.Equals(validator.Conf.Tokens.KeyWords.DefaultLanguage, StringComparison.Ordinal));

                if (defaultLanguageEntry is null)
                {
                    return null;
                }

                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    defaultLanguageEntry.KeyStartLineIndex,
                    0,
                    defaultLanguageEntry.LineChangeIndexes);

                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.UndefinedLanguageFound),
                    new(validator._filename,
                        feedbackIndexes.Key,
                        0,
                        validator._defaultLanguage)
                );

                return new(feedback);
            }

            return null;
        }

        /// <summary>
        /// Finds a content dimension and validates that it exists for all available languages
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Key value pairs containing information about the rule violation are returned if content dimension entry is not found for any available language</returns>
        public static ValidationFeedback? ValidateFindContentDimension(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            ValidationFeedback feedbackItems = [];
            ValidationStructuredEntry[] contentDimensionEntries = entries
                .Where(e => e.Key.Keyword.Equals(validator.Conf.Tokens.KeyWords.ContentVariableIdentifier, StringComparison.Ordinal))
                .ToArray();

            string defaultLanguage = validator._defaultLanguage ?? string.Empty;
            string[] languages = validator._availableLanguages ?? [defaultLanguage];
            foreach (string language in languages)
            {
                ValidationStructuredEntry? contentDimension = Array.Find(contentDimensionEntries, c => c.Key.Language == language || (c.Key.Language is null && language == defaultLanguage));
                if (contentDimension is null)
                {
                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                        new(ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.RecommendedKeyMissing),
                        new(validator._filename,
                            0,
                            0,
                            $"{validator.Conf.Tokens.KeyWords.ContentVariableIdentifier}, {language}")
                    );

                    feedbackItems.Add(feedback);
                }
            }

            validator._contentDimensionNames = contentDimensionEntries.ToDictionary(
                e => e.Key.Language ?? defaultLanguage,
                e => e.Value);

            return feedbackItems;
        }

        /// <summary>
        /// Validates that entries with required keys are found in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Key value pairs containing information about the rule violation are returned if entries required keywords are not found</returns>
        public static ValidationFeedback? ValidateFindRequiredCommonKeys(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            ValidationFeedback feedbackItems = [];
            string[] alwaysRequiredKeywords =
            [
                validator.Conf.Tokens.KeyWords.Charset,
                validator.Conf.Tokens.KeyWords.CodePage,
                validator.Conf.Tokens.KeyWords.DefaultLanguage,
            ];

            foreach (string keyword in alwaysRequiredKeywords)
            {
                if (!Array.Exists(entries, e => e.Key.Keyword.Equals(keyword, StringComparison.Ordinal)))
                {
                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                        new(ValidationFeedbackLevel.Error,
                                ValidationFeedbackRule.RequiredKeyMissing),
                        new(validator._filename,
                            0,
                            0,
                            keyword)
                    );

                    feedbackItems.Add(feedback);
                }
            }

            return feedbackItems;
        }

        /// <summary>
        /// Finds stub and heading dimensions in the Px file metadata and validates that they are defined for all available languages
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Key value pairs containing information about the rule violation are returned if stub and heading dimension entries are not found for any available language</returns>
        public static ValidationFeedback? ValidateFindStubAndHeading(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            ValidationStructuredEntry[] stubEntries = entries.Where(e => e.Key.Keyword.Equals(validator.Conf.Tokens.KeyWords.StubDimensions, StringComparison.Ordinal)).ToArray();
            ValidationStructuredEntry[] headingEntries = entries.Where(e => e.Key.Keyword.Equals(validator.Conf.Tokens.KeyWords.HeadingDimensions, StringComparison.Ordinal)).ToArray();

            if (stubEntries.Length == 0 && headingEntries.Length == 0)
            {
                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MissingStubAndHeading),
                    new(validator._filename, 0, 0)
                );

                return new(feedback);
            }

            string defaultLanguage = validator._defaultLanguage ?? string.Empty;

            validator._stubDimensionNames = GetDimensionNames(stubEntries, defaultLanguage, validator.Conf);

            validator._headingDimensionNames = GetDimensionNames(headingEntries, defaultLanguage, validator.Conf);

            ValidationFeedback feedbackItems = [];

            string[] languages = validator._availableLanguages ?? [defaultLanguage];

            foreach (string language in languages)
            {
                if ((validator._stubDimensionNames is null || !validator._stubDimensionNames.ContainsKey(language))
                    &&
                    (validator._headingDimensionNames is null || !validator._headingDimensionNames.ContainsKey(language)))
                {
                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                        new(ValidationFeedbackLevel.Error,
                                ValidationFeedbackRule.MissingStubAndHeading),
                        new(validator._filename,
                            0,
                            0,
                            $"{language}")
                    );

                    feedbackItems.Add(feedback);
                }
                // Check if any of the heading names are also in the stub names
                else if (validator._stubDimensionNames is not null &&
                    validator._headingDimensionNames is not null &&
                    validator._stubDimensionNames.TryGetValue(language, out string[]? stubValue) &&
                    validator._headingDimensionNames.TryGetValue(language, out string[]? headingValue) &&
                    stubValue.Intersect(headingValue).Any())
                {
                    string[] duplicates = stubValue.Intersect(headingValue).ToArray();
                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                        new(ValidationFeedbackLevel.Warning,
                        ValidationFeedbackRule.DuplicateDimension),
                        new(validator._filename,
                        0,
                        0,
                        $"{language}, {string.Join(", ", duplicates)}")
                    );

                    feedbackItems.Add(feedback);
                }
            }

            return feedbackItems;
        }

        /// <summary>
        /// Finds recommended languages dependent or language independent keys in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Key value pairs containing information about the rule violation are returned if recommended keys are missing</returns>
        public static ValidationFeedback? ValidateFindRecommendedKeys(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            ValidationFeedback feedbackItems = [];

            string[] commonKeys =
            [
                validator.Conf.Tokens.KeyWords.TableId
            ];
            string[] languageSpecific =
            [
                validator.Conf.Tokens.KeyWords.Description
            ];

            foreach (string keyword in commonKeys)
            {
                if (!Array.Exists(entries, e => e.Key.Keyword.Equals(keyword, StringComparison.Ordinal)))
                {
                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                        new(ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.RecommendedKeyMissing),
                        new(validator._filename,
                            0,
                            0,
                            keyword)
                    );

                    feedbackItems.Add(feedback);
                }
            }

            string defaultLanguage = validator._defaultLanguage ?? string.Empty;
            string[] languages = validator._availableLanguages ?? [defaultLanguage];

            foreach (string language in languages)
            {
                foreach (string keyword in languageSpecific)
                {
                    if (!Array.Exists(entries, e => e.Key.Keyword.Equals(keyword, StringComparison.Ordinal) &&
                    (e.Key.Language == language || (language == defaultLanguage && e.Key.Language is null))))
                    {
                        KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                            new(ValidationFeedbackLevel.Warning,
                                ValidationFeedbackRule.RecommendedKeyMissing),
                            new(validator._filename,
                                0,
                                0,
                                $"{language}, {keyword}")
                        );

                        feedbackItems.Add(feedback);
                    }
                }
            }

            return feedbackItems;
        }

        /// <summary>
        /// Finds and validates values for stub and heading dimensions in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Key value pairs containing information about the rule violation are returned if values are missing for any dimension</returns>
        public static ValidationFeedback? ValidateFindDimensionValues(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            ValidationFeedback feedbackItems = [];
            ValidationStructuredEntry[] dimensionEntries = entries.Where(
                e => e.Key.Keyword.Equals(validator.Conf.Tokens.KeyWords.VariableValues, StringComparison.Ordinal)).ToArray();

            IEnumerable<KeyValuePair<KeyValuePair<string, string>, string[]>> dimensionValues = [];
            dimensionValues = dimensionValues.Concat(
                FindDimensionValues(
                    dimensionEntries,
                    validator.Conf,
                    validator._stubDimensionNames,
                    ref feedbackItems,
                    validator._filename)
                );
            dimensionValues = dimensionValues.Concat(
                FindDimensionValues(
                    dimensionEntries,
                    validator.Conf,
                    validator._headingDimensionNames,
                    ref feedbackItems,
                    validator._filename)
                );

            validator._dimensionValueNames = [];
            foreach (KeyValuePair<KeyValuePair<string, string>, string[]> item in dimensionValues)
            {
                if (!validator._dimensionValueNames.TryAdd(item.Key, item.Value))
                {
                    var originalValue = validator._dimensionValueNames[item.Key];
                    validator._dimensionValueNames[item.Key] = [.. originalValue, .. item.Value];

                    feedbackItems.Add(new(
                        new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.DuplicateEntry),
                        new(validator._filename, additionalInfo: $"{item.Key.Key}, {item.Key.Value}")
                    ));
                }
            }

            return feedbackItems;
        }

        /// <summary>
        /// Finds and validates that entries with required keys related to the content dimension and its values are found in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Key value pairs containing information about the rule violation are returned if required entries are missing. 
        /// Warnings are returned if entries are defined in an unrecommended way</returns>
        public static ValidationFeedback? ValidateFindContentDimensionKeys(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            ValidationFeedback feedbackItems = [];

            string[] languageSpecificKeywords =
                [
                    validator.Conf.Tokens.KeyWords.Units,
                ];

            string[] requiredKeywords =
                [
                    validator.Conf.Tokens.KeyWords.LastUpdated,
                ];

            string[] recommendedKeywords =
                [
                    validator.Conf.Tokens.KeyWords.Precision
                ];

            string[] contentDimensionNames =
                validator._contentDimensionNames is not null ?
                [.. validator._contentDimensionNames.Values] : [];

            Dictionary<KeyValuePair<string, string>, string[]> dimensionValueNames =
                validator._dimensionValueNames ?? [];

            Dictionary<KeyValuePair<string, string>, string[]> contentDimensionValueNames = dimensionValueNames
                .Where(e => contentDimensionNames.Contains(e.Key.Value))
                .ToDictionary(e => e.Key, e => e.Value);

            foreach (KeyValuePair<KeyValuePair<string, string>, string[]> kvp in contentDimensionValueNames)
            {
                foreach (string dimensionValueName in kvp.Value)
                {
                    ValidationFeedback items = ProcessContentDimensionValue(languageSpecificKeywords, requiredKeywords, recommendedKeywords, entries, validator, kvp.Key, dimensionValueName);
                    feedbackItems.AddRange(items);
                }
            }

            return feedbackItems;
        }

        /// <summary>
        /// Validates that recommended entries related to dimensions are found in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Key value pairs containing information about the rule violation are returned if any dimensions are missing recommended entries related to them</returns>
        public static ValidationFeedback? ValidateFindDimensionRecommendedKeys(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            ValidationFeedback feedbackItems = [];

            string[] dimensionRecommendedKeywords =
                [
                    validator.Conf.Tokens.KeyWords.DimensionCode,
                    validator.Conf.Tokens.KeyWords.VariableValueCodes
                ];

            Dictionary<string, string[]> stubDimensions = validator._stubDimensionNames ?? [];
            Dictionary<string, string[]> headingDimensions = validator._headingDimensionNames ?? [];
            Dictionary<string, string[]> allDimensions = stubDimensions
                .Concat(headingDimensions)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.SelectMany(kvp => kvp.Value).ToArray());

            foreach (KeyValuePair<string, string[]> languageDimensions in allDimensions)
            {
                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? timeValFeedback = FindDimensionRecommendedKey(
                    entries,
                    validator.Conf.Tokens.KeyWords.TimeVal,
                    languageDimensions.Key,
                    validator);

                if (timeValFeedback is not null)
                {
                    feedbackItems.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)timeValFeedback);
                }

                foreach (string dimension in languageDimensions.Value)
                {
                    ValidationFeedback dimensionFeedback = ProcessDimension(
                        dimensionRecommendedKeywords,
                        validator.Conf.Tokens.KeyWords.DimensionType,
                        entries,
                        languageDimensions.Key,
                        validator,
                        dimension);
                    feedbackItems.AddRange(dimensionFeedback);
                }
            }

            return feedbackItems;
        }

    }
}
