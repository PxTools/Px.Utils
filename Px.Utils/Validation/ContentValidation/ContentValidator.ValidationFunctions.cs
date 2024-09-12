﻿using Px.Utils.PxFile;
using Px.Utils.Validation.SyntaxValidation;
using System.Globalization;

namespace Px.Utils.Validation.ContentValidation
{
    public delegate ValidationFeedback? ContentValidationEntryValidator(ValidationStructuredEntry entry, ContentValidator validator);
    public delegate ValidationFeedback? ContentValidationFindKeywordValidator(ValidationStructuredEntry[] entries, ContentValidator validator); 
    
    /// <summary>
    /// Collection of functions for validating Px file metadata contents
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
                e => e.Key.Keyword.Equals(validator.SyntaxConf.Tokens.KeyWords.DefaultLanguage, StringComparison.Ordinal)).ToArray();

            if (langEntries.Length > 1)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entries[0].KeyStartLineIndex, 
                    0, 
                    entries[0].LineChangeIndexes);

                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new (
                    new (ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MultipleInstancesOfUniqueKey),
                    new(validator._filename,
                        feedbackIndexes.Key,
                        0,
                        $"{validator.SyntaxConf.Tokens.KeyWords.DefaultLanguage}: " + string.Join(", ", entries.Select(e => e.Value).ToArray())
                    ));

                return new(feedback);
            }
            else if (langEntries.Length == 0)
            {
                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MissingDefaultLanguage),
                    new(validator._filename, 0,0)
                );

                return new(feedback);
            }

            validator._defaultLanguage = SyntaxValidationUtilityMethods.CleanString(langEntries[0].Value, validator.SyntaxConf);
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
                .Where(e => e.Key.Keyword.Equals(validator.SyntaxConf.Tokens.KeyWords.AvailableLanguages, StringComparison.Ordinal))
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
                        $"{validator.SyntaxConf.Tokens.KeyWords.AvailableLanguages}: " + string.Join(", ", entries.Select(e => e.Value).ToArray()))
                );

                return new(feedback);
            }

            if (availableLanguageEntries.Length == 1)
            {
                List<string> languages = availableLanguageEntries[0].Value.Split(validator.SyntaxConf.Symbols.Value.ListSeparator).ToList();
                languages = CleanListOfStrings(languages, validator.SyntaxConf);
                validator._availableLanguages = [..languages];
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
                        validator.SyntaxConf.Tokens.KeyWords.AvailableLanguages)
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
                ValidationStructuredEntry? defaultLanguageEntry = Array.Find(entries, e => e.Key.Keyword.Equals(validator.SyntaxConf.Tokens.KeyWords.DefaultLanguage, StringComparison.Ordinal));

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
                .Where(e => e.Key.Keyword.Equals(validator.SyntaxConf.Tokens.KeyWords.ContentVariableIdentifier, StringComparison.Ordinal))
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
                            $"{validator.SyntaxConf.Tokens.KeyWords.ContentVariableIdentifier}, {language}")
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
                validator.SyntaxConf.Tokens.KeyWords.Charset,
                validator.SyntaxConf.Tokens.KeyWords.CodePage,
                validator.SyntaxConf.Tokens.KeyWords.DefaultLanguage,
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
            ValidationStructuredEntry[] stubEntries = entries.Where(e => e.Key.Keyword.Equals(validator.SyntaxConf.Tokens.KeyWords.StubDimensions, StringComparison.Ordinal)).ToArray();
            ValidationStructuredEntry[] headingEntries = entries.Where(e => e.Key.Keyword.Equals(validator.SyntaxConf.Tokens.KeyWords.HeadingDimensions, StringComparison.Ordinal)).ToArray();

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

            validator._stubDimensionNames = GetDimensionNames(stubEntries, defaultLanguage, validator.SyntaxConf);
            
            validator._headingDimensionNames = GetDimensionNames(headingEntries, defaultLanguage, validator.SyntaxConf);

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
                validator.SyntaxConf.Tokens.KeyWords.TableId
            ];
            string[] languageSpecific =
            [
                validator.SyntaxConf.Tokens.KeyWords.Description
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

            foreach(string language in languages)
            {
                foreach(string keyword in languageSpecific)
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
                e => e.Key.Keyword.Equals(validator.SyntaxConf.Tokens.KeyWords.VariableValues, StringComparison.Ordinal)).ToArray();

            IEnumerable<KeyValuePair<KeyValuePair<string, string>, string[]>> dimensionValues = [];
            dimensionValues = dimensionValues.Concat(
                FindDimensionValues(
                    dimensionEntries,
                    validator.SyntaxConf,
                    validator._stubDimensionNames, 
                    ref feedbackItems, 
                    validator._filename)
                );
            dimensionValues = dimensionValues.Concat(
                FindDimensionValues(
                    dimensionEntries,
                    validator.SyntaxConf, 
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
                    validator.SyntaxConf.Tokens.KeyWords.Units,
                ];

            string[] requiredKeywords =
                [
                    validator.SyntaxConf.Tokens.KeyWords.LastUpdated,
                ];

            string[] recommendedKeywords =
                [
                    validator.SyntaxConf.Tokens.KeyWords.Precision
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
                    validator.SyntaxConf.Tokens.KeyWords.DimensionCode,
                    validator.SyntaxConf.Tokens.KeyWords.VariableValueCodes
                ];

            Dictionary<string, string[]> stubDimensions = validator._stubDimensionNames ?? [];
            Dictionary<string, string[]> headingDimensions =  validator._headingDimensionNames ?? [];
            Dictionary<string, string[]> allDimensions = stubDimensions
                .Concat(headingDimensions)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.SelectMany(kvp => kvp.Value).ToArray());

            foreach (KeyValuePair<string, string[]> languageDimensions in allDimensions)
            {
                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? timeValFeedback = FindDimensionRecommendedKey(
                    entries,
                    validator.SyntaxConf.Tokens.KeyWords.TimeVal,
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
                        validator.SyntaxConf.Tokens.KeyWords.DimensionType,
                        entries,
                        languageDimensions.Key,
                        validator,
                        dimension);
                    feedbackItems.AddRange(dimensionFeedback);
                }
            }

            return feedbackItems;
        }

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
                validator.SyntaxConf.Tokens.KeyWords.DefaultLanguage,
                validator.SyntaxConf.Tokens.KeyWords.AvailableLanguages,
                validator.SyntaxConf.Tokens.KeyWords.Charset,
                validator.SyntaxConf.Tokens.KeyWords.CodePage,
                validator.SyntaxConf.Tokens.KeyWords.StubDimensions,
                validator.SyntaxConf.Tokens.KeyWords.HeadingDimensions,
                validator.SyntaxConf.Tokens.KeyWords.TableId,
                validator.SyntaxConf.Tokens.KeyWords.Description,
                validator.SyntaxConf.Tokens.KeyWords.ContentVariableIdentifier,
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
                validator.SyntaxConf.Tokens.KeyWords.Charset,
                validator.SyntaxConf.Tokens.KeyWords.CodePage,
                validator.SyntaxConf.Tokens.KeyWords.TableId,
                validator.SyntaxConf.Tokens.KeyWords.DefaultLanguage,
                validator.SyntaxConf.Tokens.KeyWords.AvailableLanguages,
            ];

            string[] noLanguageParameterRecommendedKeywords = [
                validator.SyntaxConf.Tokens.KeyWords.LastUpdated,
                validator.SyntaxConf.Tokens.KeyWords.DimensionType,
                validator.SyntaxConf.Tokens.KeyWords.Precision
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
                    validator.SyntaxConf.Tokens.KeyWords.Precision,
                    validator.SyntaxConf.Tokens.KeyWords.Units,
                    validator.SyntaxConf.Tokens.KeyWords.LastUpdated,
                    validator.SyntaxConf.Tokens.KeyWords.Contact,
                    validator.SyntaxConf.Tokens.KeyWords.ValueNote
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
                    validator.SyntaxConf.Tokens.KeyWords.Charset,
                    validator.SyntaxConf.Tokens.KeyWords.CodePage,
                    validator.SyntaxConf.Tokens.KeyWords.DefaultLanguage,
                    validator.SyntaxConf.Tokens.KeyWords.Units,
                    validator.SyntaxConf.Tokens.KeyWords.Description,
                    validator.SyntaxConf.Tokens.KeyWords.TableId,
                    validator.SyntaxConf.Tokens.KeyWords.ContentVariableIdentifier,
                    validator.SyntaxConf.Tokens.KeyWords.DimensionCode
                ];

            string[] listOfStringTypes =
                [
                    validator.SyntaxConf.Tokens.KeyWords.AvailableLanguages,
                    validator.SyntaxConf.Tokens.KeyWords.StubDimensions,
                    validator.SyntaxConf.Tokens.KeyWords.HeadingDimensions,
                    validator.SyntaxConf.Tokens.KeyWords.VariableValues,
                    validator.SyntaxConf.Tokens.KeyWords.VariableValueCodes
                ];

            string[] dateTimeTypes =
                [
                    validator.SyntaxConf.Tokens.KeyWords.LastUpdated
                ];

            string[] numberTypes =
                [
                    validator.SyntaxConf.Tokens.KeyWords.Precision
                ];

            string[] timeval =
                [
                    validator.SyntaxConf.Tokens.KeyWords.TimeVal
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
                    validator.SyntaxConf.Tokens.VariableTypes.Content,
                validator.SyntaxConf.Tokens.VariableTypes.Time,
                validator.SyntaxConf.Tokens.VariableTypes.Geographical,
                validator.SyntaxConf.Tokens.VariableTypes.Ordinal,
                validator.SyntaxConf.Tokens.VariableTypes.Nominal,
                validator.SyntaxConf.Tokens.VariableTypes.Other,
                validator.SyntaxConf.Tokens.VariableTypes.Unknown,
                validator.SyntaxConf.Tokens.VariableTypes.Classificatory
                    ];

            string value = SyntaxValidationUtilityMethods.CleanString(entry.Value, validator.SyntaxConf);
            if ((entry.Key.Keyword == validator.SyntaxConf.Tokens.KeyWords.Charset && !allowedCharsets.Contains(value)) ||
                (entry.Key.Keyword == validator.SyntaxConf.Tokens.KeyWords.CodePage && !value.Equals(validator._encoding.BodyName, StringComparison.OrdinalIgnoreCase)) ||
                (entry.Key.Keyword == validator.SyntaxConf.Tokens.KeyWords.DimensionType && !dimensionTypes.Contains(value)))
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
            else if (entry.Key.Keyword == validator.SyntaxConf.Tokens.KeyWords.ContentVariableIdentifier)
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
            if (entry.Key.Keyword != validator.SyntaxConf.Tokens.KeyWords.VariableValueCodes ||
                validator._dimensionValueNames is null ||
                entry.Key.FirstSpecifier is null)
            {
                return null;
            }

            string[] codes = entry.Value.Split(validator.SyntaxConf.Symbols.Value.ListSeparator);
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
                validator.SyntaxConf.Tokens.KeyWords.CodePage
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
