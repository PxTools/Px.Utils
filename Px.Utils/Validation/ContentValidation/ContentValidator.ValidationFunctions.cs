using PxUtils.PxFile;
using PxUtils.Validation.SyntaxValidation;

namespace PxUtils.Validation.ContentValidation
{
    public delegate ValidationFeedbackItem[]? ContentValidationEntryDelegate(ValidationStructuredEntry entry, ContentValidator validator);
    public delegate ValidationFeedbackItem[]? ContentValidationFindKeywordDelegate(ValidationStructuredEntry[] entries, ContentValidator validator); 
    
    /// <summary>
    /// Collection of functions for validating Px file metadata contents
    /// </summary>
    public sealed partial class ContentValidator
    {
        public List<ContentValidationEntryDelegate> DefaultContentValidationEntryFunctions { get; } = [
            ValidateUnexpectedSpecifiers,
            ValidateUnexpectedLanguageParams,
            ValidateLanguageParams,
            ValidateSpecifiers,
            ValidateValueTypes,
            ValidateValueContents,
            ValidateValueAmounts,
            ValidateValueUppercaseRecommendations
        ];

        public List<ContentValidationFindKeywordDelegate> DefaultContentValidationFindKeywordFunctions { get; } = [
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
        /// <returns>Null if no issues are found. <see cref="ValidationFeedbackItem"/> objects are returned if entry defining default language is not found or if more than one are found.</returns>
        public static ValidationFeedbackItem[]? ValidateFindDefaultLanguage(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            ValidationStructuredEntry[] langEntries = entries.Where(
                e => e.Key.Keyword.Equals(validator.SyntaxConf.Tokens.KeyWords.DefaultLanguage)).ToArray();

            if (langEntries.Length > 1)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entries[0].KeyStartLineIndex, 
                    0, 
                    entries[0].LineChangeIndexes);

                ValidationFeedback feedback = new (
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MultipleInstancesOfUniqueKey,
                        feedbackIndexes.Key,
                        0,
                        $"{validator.SyntaxConf.Tokens.KeyWords.DefaultLanguage}: " + string.Join(", ", entries.Select(e => e.Value).ToArray())
                        );

                return [
                    new ValidationFeedbackItem(
                    entries[0],
                    feedback
                    ),
                ];
            }
            else if (langEntries.Length == 0)
            {
                ValidationFeedback feedback = new (
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MissingDefaultLanguage,
                        0,
                        0
                        );

                return [
                    new ValidationFeedbackItem(
                    new ContentValidationObject(validator.filename, 0, []),
                    feedback
                    ),
                ];
            }

            validator.defaultLanguage = langEntries[0].Value;
            return null;
        }

        /// <summary>
        /// Finds an entry that defines available languages in the Px file
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Null if no issues are found. <see cref="ValidationFeedbackItem"/> object with a warning is returned if available languages entry is not found. Error is returned if multiple entries are found.</returns>
        public static ValidationFeedbackItem[]? ValidateFindAvailableLanguages(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            ValidationStructuredEntry[] availableLanguageEntries = entries.Where(e => e.Key.Keyword.Equals(validator.SyntaxConf.Tokens.KeyWords.AvailableLanguages)).ToArray();

            if (availableLanguageEntries.Length > 1)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entries[0].KeyStartLineIndex,
                    0,
                    entries[0].LineChangeIndexes);

                ValidationFeedback feedback = new (
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MultipleInstancesOfUniqueKey,
                        feedbackIndexes.Key,
                        0,
                        $"{validator.SyntaxConf.Tokens.KeyWords.AvailableLanguages}: " + string.Join(", ", entries.Select(e => e.Value).ToArray())
                        );

                return [
                    new ValidationFeedbackItem(
                    entries[0],
                    feedback
                    )
               ];
            }

            if (availableLanguageEntries.Length == 1)
            {
                validator.availableLanguages = availableLanguageEntries[0].Value.Split(validator.SyntaxConf.Symbols.Value.ListSeparator); 
                return null;
            }
            else
            {
                ValidationFeedback feedback = new (
                        ValidationFeedbackLevel.Warning,
                        ValidationFeedbackRule.RecommendedKeyMissing,
                        0,
                        0,
                        validator.SyntaxConf.Tokens.KeyWords.AvailableLanguages
                        );

                return [
                    new ValidationFeedbackItem(
                    new ContentValidationObject(validator.filename, 0, []),
                    feedback
                    )
               ];
            }
        }

        /// <summary>
        /// Validates that default language is defined in the available languages entry
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Null of no issues are found. <see cref="ValidationFeedbackItem"/> object is returned if default language is not defined in the available languages entry</returns>
        public static ValidationFeedbackItem[]? ValidateDefaultLanguageDefinedInAvailableLanguages(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            if (validator.availableLanguages is not null && !validator.availableLanguages.Contains(validator.defaultLanguage))
            {
                ValidationStructuredEntry? defaultLanguageEntry = Array.Find(entries, e => e.Key.Keyword.Equals(validator.SyntaxConf.Tokens.KeyWords.DefaultLanguage));

                if (defaultLanguageEntry is null)
                {
                    return null;
                }
                
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    defaultLanguageEntry.KeyStartLineIndex,
                    0,
                    defaultLanguageEntry.LineChangeIndexes);

                ValidationFeedback feedback = new (
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.UndefinedLanguageFound,
                        feedbackIndexes.Key,
                        0,
                        validator.defaultLanguage
                        );

                return [
                    new ValidationFeedbackItem(
                    defaultLanguageEntry,
                    feedback
                    )
                ];
            }

            return null;
        }

        /// <summary>
        /// Finds a content dimension and validates that it exists for all available languages
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if content dimension entry is not found for any available language</returns>
        public static ValidationFeedbackItem[]? ValidateFindContentDimension(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            ValidationStructuredEntry[] contentDimensionEntries = entries.Where(e => e.Key.Keyword.Equals(validator.SyntaxConf.Tokens.KeyWords.ContentVariableIdentifier)).ToArray();

            string defaultLanguage = validator.defaultLanguage ?? string.Empty;
            string[] languages = validator.availableLanguages ?? [defaultLanguage];
            foreach (string language in languages)
            {
                ValidationStructuredEntry? contentDimension = Array.Find(contentDimensionEntries, c => c.Key.Language == language || (c.Key.Language is null && language == defaultLanguage));
                if (contentDimension is null)
                {
                    ValidationFeedback feedback = new (
                            ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.RecommendedKeyMissing,
                            0,
                            0,
                            $"{validator.SyntaxConf.Tokens.KeyWords.ContentVariableIdentifier}, {language}"
                            );

                    feedbackItems.Add(
                        new ValidationFeedbackItem(
                        new ContentValidationObject(validator.filename, 0, []),
                        feedback
                        ));
                }
            }

            validator.contentDimensionNames = contentDimensionEntries.ToDictionary(
                e => e.Key.Language ?? defaultLanguage, 
                e => e.Value);
            
            return [.. feedbackItems];
        }

        /// <summary>
        /// Validates that entries with required keys are found in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if entries required keywords are not found</returns>
        public static ValidationFeedbackItem[]? ValidateFindRequiredCommonKeys(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            string[] alwaysRequiredKeywords =
            [
                validator.SyntaxConf.Tokens.KeyWords.Charset,
                validator.SyntaxConf.Tokens.KeyWords.CodePage,
                validator.SyntaxConf.Tokens.KeyWords.DefaultLanguage,
            ];

            foreach (string keyword in alwaysRequiredKeywords)
            {
                if (!Array.Exists(entries, e => e.Key.Keyword.Equals(keyword)))
                {
                    ValidationFeedback feedback = new (
                                ValidationFeedbackLevel.Error,
                                ValidationFeedbackRule.RequiredKeyMissing,
                                0,
                                0,
                                keyword
                                );

                    feedbackItems.Add(
                        new ValidationFeedbackItem(
                            new ContentValidationObject(validator.filename, 0, []),
                            feedback
                            )
                    );
                }
            }

            return [.. feedbackItems];
        }

        /// <summary>
        /// Finds stub and heading dimensions in the Px file metadata and validates that they are defined for all available languages
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if stub and heading dimension entries are not found for any available language</returns>
        public static ValidationFeedbackItem[]? ValidateFindStubAndHeading(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            ValidationStructuredEntry[] stubEntries = entries.Where(e => e.Key.Keyword.Equals(validator.SyntaxConf.Tokens.KeyWords.StubDimensions)).ToArray();
            ValidationStructuredEntry[] headingEntries = entries.Where(e => e.Key.Keyword.Equals(validator.SyntaxConf.Tokens.KeyWords.HeadingDimensions)).ToArray();

            if (stubEntries.Length == 0 && headingEntries.Length == 0)
            {
                ValidationFeedback feedback = new (
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MissingStubAndHeading,
                        0,
                        0
                        );

                return [
                    new(
                    new ContentValidationObject(validator.filename, 0, []),
                    feedback
                    )
                ];
            }

            string defaultLanguage = validator.defaultLanguage ?? string.Empty;

            validator.stubDimensionNames = stubEntries.ToDictionary(
                e => e.Key.Language ?? defaultLanguage, 
                e => e.Value.Split(validator.SyntaxConf.Symbols.Key.ListSeparator));
            
            validator.headingDimensionNames = headingEntries.ToDictionary(
                e => e.Key.Language ?? defaultLanguage, 
                e => e.Value.Split(validator.SyntaxConf.Symbols.Key.ListSeparator));

            List<ValidationFeedbackItem> feedbackItems = [];

            string[] languages = validator.availableLanguages ?? [defaultLanguage];

            foreach (string language in languages)
            {
                if ((validator.stubDimensionNames is null || !validator.stubDimensionNames.ContainsKey(language))
                    &&
                    (validator.headingDimensionNames is null || !validator.headingDimensionNames.ContainsKey(language)))
                {
                    ValidationFeedback feedback = new (
                                ValidationFeedbackLevel.Error,
                                ValidationFeedbackRule.MissingStubAndHeading,
                                0,
                                0,
                                $"{language}"
                                );

                    feedbackItems.Add(
                        new ValidationFeedbackItem(
                            new ContentValidationObject(validator.filename, 0, []),
                            feedback
                            )
                        );
                }
            }

            return [.. feedbackItems];
        }

        /// <summary>
        /// Finds recommended languages dependent or language independent keys in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects with warning are returned if recommended keys are missing</returns>
        public static ValidationFeedbackItem[]? ValidateFindRecommendedKeys(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            List<ValidationFeedbackItem> feedbackItems = [];

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
                if (!Array.Exists(entries, e => e.Key.Keyword.Equals(keyword)))
                {
                    ValidationFeedback feedback = new (
                                ValidationFeedbackLevel.Warning,
                                ValidationFeedbackRule.RecommendedKeyMissing,
                                0,
                                0,
                                keyword
                                );

                    feedbackItems.Add(
                        new ValidationFeedbackItem(
                            new ContentValidationObject(validator.filename, 0, []),
                            feedback
                            )
                        );
                }
            }

            string defaultLanguage = validator.defaultLanguage ?? string.Empty;
            string[] languages = validator.availableLanguages ?? [defaultLanguage];

            foreach(string language in languages)
            {
                foreach(string keyword in languageSpecific)
                {
                    if (!Array.Exists(entries, e => e.Key.Keyword.Equals(keyword) && 
                    (e.Key.Language == language || (language == defaultLanguage && e.Key.Language is null))))
                    {
                        ValidationFeedback feedback = new (
                                    ValidationFeedbackLevel.Warning,
                                    ValidationFeedbackRule.RecommendedKeyMissing,
                                    0,
                                    0,
                                    $"{language}, {keyword}"
                                    );

                        feedbackItems.Add(
                            new ValidationFeedbackItem(
                                new ContentValidationObject(validator.filename, 0, []),
                                feedback
                                )
                            );
                    }
                }
            }

            return [.. feedbackItems];
        }

        /// <summary>
        /// Finds and validates values for stub and heading dimensions in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if values are missing for any dimension</returns>
        public static ValidationFeedbackItem[]? ValidateFindDimensionValues(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            ValidationStructuredEntry[] dimensionEntries = entries.Where(
                e => e.Key.Keyword.Equals(validator.SyntaxConf.Tokens.KeyWords.VariableValues)).ToArray();

            IEnumerable<KeyValuePair<KeyValuePair<string, string>, string[]>> dimensionValues = [];
            dimensionValues = dimensionValues.Concat(
                FindDimensionValues(
                    dimensionEntries,
                    validator.SyntaxConf,
                    validator.stubDimensionNames, 
                    ref feedbackItems, 
                    validator.filename)
                );
            dimensionValues = dimensionValues.Concat(
                FindDimensionValues(
                    dimensionEntries,
                    validator.SyntaxConf, 
                    validator.headingDimensionNames, 
                    ref feedbackItems,
                    validator.filename)
                );

            validator.dimensionValueNames = [];
            foreach (KeyValuePair<KeyValuePair<string, string>, string[]> item in dimensionValues)
            {
                validator.dimensionValueNames.Add(item.Key, item.Value);
            }

            return [.. feedbackItems];
        }

        /// <summary>
        /// Finds and validates that entries with required keys related to the content dimension and its values are found in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if required entries are missing. Warnings are returned if entries are defined in an unrecommended way</returns>
        public static ValidationFeedbackItem[]? ValidateFindContentDimensionKeys(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            List<ValidationFeedbackItem>? feedbackItems = [];

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
                validator.contentDimensionNames is not null ?
                [.. validator.contentDimensionNames.Values] : [];

            Dictionary<KeyValuePair<string, string>, string[]> dimensionValueNames = 
                validator.dimensionValueNames ?? [];

            Dictionary<KeyValuePair<string, string>, string[]> contentDimensionValueNames = dimensionValueNames
                .Where(e => contentDimensionNames.Contains(e.Key.Value))
                .ToDictionary(e => e.Key, e => e.Value);

            foreach (KeyValuePair<KeyValuePair<string, string>, string[]> kvp in contentDimensionValueNames)
            {
                foreach (string dimensionValueName in kvp.Value)
                {
                    List<ValidationFeedbackItem> items = ProcessContentDimensionValue(languageSpecificKeywords, requiredKeywords, recommendedKeywords, entries, validator, kvp.Key, dimensionValueName);
                    feedbackItems.AddRange(items);
                }
            }

            return [.. feedbackItems];
        }

        /// <summary>
        /// Validates that recommended entries related to dimensions are found in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects with warnings are returned if any dimensions are missing recommended entries related to them</returns>
        public static ValidationFeedbackItem[]? ValidateFindDimensionRecommendedKeys(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            List<ValidationFeedbackItem> feedbackItems = [];

            string[] dimensionRecommendedKeywords =
                [
                    validator.SyntaxConf.Tokens.KeyWords.DimensionCode,
                    validator.SyntaxConf.Tokens.KeyWords.VariableValueCodes
                ];

            Dictionary<string, string[]> stubDimensions = validator.stubDimensionNames ?? [];
            Dictionary<string, string[]> headingDimensions =  validator.headingDimensionNames ?? [];
            Dictionary<string, string[]> allDimensions = stubDimensions
                .Concat(headingDimensions)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.SelectMany(kvp => kvp.Value).ToArray());

            foreach (KeyValuePair<string, string[]> languageDimensions in allDimensions)
            {
                ValidationFeedbackItem? timeValFeedback = FindDimensionRecommendedKey(entries, validator.SyntaxConf.Tokens.KeyWords.TimeVal, languageDimensions.Key, validator);
                if (timeValFeedback is not null)
                {
                    feedbackItems.Add((ValidationFeedbackItem)timeValFeedback);
                }

                foreach (string dimension in languageDimensions.Value)
                {
                    List<ValidationFeedbackItem> dimensionFeedback = ProcessDimension(
                        dimensionRecommendedKeywords,
                        validator.SyntaxConf.Tokens.KeyWords.DimensionType,
                        entries,
                        languageDimensions.Key,
                        validator,
                        dimension);
                    feedbackItems.AddRange(dimensionFeedback);
                }
            }

            return [.. feedbackItems];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if </returns>
        public static ValidationFeedbackItem[]? ValidateUnexpectedSpecifiers(ValidationStructuredEntry entry, ContentValidator validator)
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

                ValidationFeedback feedback = new (
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.IllegalSpecifierDefinitionFound,
                        feedbackIndexes.Key,
                        0,
                        $"{entry.Key.Keyword}: {entry.Key.FirstSpecifier}, {entry.Key.SecondSpecifier}"
                        );

                return [
                    new ValidationFeedbackItem(
                        entry,
                        feedback
                    )
                ];
            }

            return null;
        }

        /// <summary>
        /// Finds unexpected language parameters in the Px file metadata entry
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if an illegal or unrecommended language parameter is detected in the entry</returns>
        public static ValidationFeedbackItem[]? ValidateUnexpectedLanguageParams(ValidationStructuredEntry entry, ContentValidator validator)
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
                    ValidationFeedback feedback = new (
                            ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.IllegalLanguageDefinitionFound,
                            feedbackIndexes.Key,
                            0,
                            $"{entry.Key.Keyword}, {entry.Key.Language}, {entry.Key.FirstSpecifier}, {entry.Key.SecondSpecifier}");

                    return [
                        new ValidationFeedbackItem(
                            entry,
                            feedback
                        )
                    ];
                }
                else if (noLanguageParameterRecommendedKeywords.Contains(entry.Key.Keyword))
                {
                    ValidationFeedback feedback = new (
                            ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.UnrecommendedLanguageDefinitionFound,
                            feedbackIndexes.Key,
                            0,
                            $"{entry.Key.Keyword}, {entry.Key.Language}, {entry.Key.FirstSpecifier}, {entry.Key.SecondSpecifier}");

                    return [
                        new ValidationFeedbackItem(
                            entry,
                            feedback
                        )
                    ];
                }
            }

            return null;
        }

        /// <summary>
        /// Validates that the language defined in the Px file metadata entry is defined in the available languages entry
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object is returned if an undefined language is found from the entry</returns>
        public static ValidationFeedbackItem[]? ValidateLanguageParams(ValidationStructuredEntry entry, ContentValidator validator)
        {
            if (entry.Key.Language is null)
            {
                return null;
            }

            if (validator.availableLanguages is not null && !validator.availableLanguages.Contains(entry.Key.Language))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);

                ValidationFeedback feedback = new (
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.UndefinedLanguageFound,
                        feedbackIndexes.Key,
                        0,
                        entry.Key.Language);

                return [
                    new ValidationFeedbackItem(
                        entry,
                        feedback
                    )
                ];
            }

            return null;
        }

        /// <summary>
        /// Validates that specifiers are defined properly in the Px file metadata entry. Content dimension specifiers are allowed to be defined using only the first specifier at value level and are checked separately
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object is returned if the entry's specifier is defined in an unexpected way</returns>
        public static ValidationFeedbackItem[]? ValidateSpecifiers(ValidationStructuredEntry entry, ContentValidator validator)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            if ((entry.Key.FirstSpecifier is null && entry.Key.SecondSpecifier is null) || 
                validator.dimensionValueNames is null)
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

            if ((validator.stubDimensionNames is null || !validator.stubDimensionNames.Values.Any(v => v.Contains(entry.Key.FirstSpecifier))) &&
                (validator.headingDimensionNames is null || !validator.headingDimensionNames.Values.Any(v => v.Contains(entry.Key.FirstSpecifier))))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);

                ValidationFeedback feedback = new (
                            ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.IllegalSpecifierDefinitionFound,
                            feedbackIndexes.Key,
                            0,
                            entry.Key.FirstSpecifier);

                feedbackItems.Add(
                    new ValidationFeedbackItem(
                        entry,
                        feedback
                    )
                );
            }
            else if (entry.Key.SecondSpecifier is not null && 
                !validator.dimensionValueNames.Values.Any(v => v.Contains(entry.Key.SecondSpecifier)))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);

                ValidationFeedback feedback = new (
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.IllegalSpecifierDefinitionFound,
                        feedbackIndexes.Key,
                        0,
                        entry.Key.SecondSpecifier);

                feedbackItems.Add(
                    new ValidationFeedbackItem(
                        entry,
                        feedback
                    )
                );
            }

            return [.. feedbackItems];
        }

        /// <summary>
        /// Validates that certain keywords are defined with the correct value types in the Px file metadata entry
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object is returned if an unexpected value type is detected</returns>
        public static ValidationFeedbackItem[]? ValidateValueTypes(ValidationStructuredEntry entry, ContentValidator validator)
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

            if ((stringTypes.Contains(entry.Key.Keyword) && entry.ValueType != ValueType.String) ||
                (listOfStringTypes.Contains(entry.Key.Keyword) && entry.ValueType != ValueType.ListOfStrings) ||
                (dateTimeTypes.Contains(entry.Key.Keyword) && entry.ValueType != ValueType.DateTime) ||
                (numberTypes.Contains(entry.Key.Keyword) && entry.ValueType != ValueType.Number) ||
                (timeval.Contains(entry.Key.Keyword) && (entry.ValueType != ValueType.TimeValRange && entry.ValueType != ValueType.TimeValSeries)))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    entry.ValueStartIndex,
                    entry.LineChangeIndexes);

                ValidationFeedback feedback = new (
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.UnmatchingValueType,
                        feedbackIndexes.Key,
                        feedbackIndexes.Value,
                        $"{entry.Key.Keyword}: {entry.ValueType}");

                return [
                    new ValidationFeedbackItem(
                        entry,
                        feedback
                    )
                ];
            }

            return null;
        }

        /// <summary>
        /// Validates that entries with specific keywords have valid values in the Px file metadata entry
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object is returned if an unexpected value is detected</returns>
        public static ValidationFeedbackItem[]? ValidateValueContents(ValidationStructuredEntry entry, ContentValidator validator)
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

            if ((entry.Key.Keyword == validator.SyntaxConf.Tokens.KeyWords.Charset && !allowedCharsets.Contains(entry.Value)) ||
                (entry.Key.Keyword == validator.SyntaxConf.Tokens.KeyWords.CodePage && !entry.Value.Equals(validator.encoding.BodyName, StringComparison.CurrentCultureIgnoreCase)) ||
                (entry.Key.Keyword == validator.SyntaxConf.Tokens.KeyWords.DimensionType && !dimensionTypes.Contains(entry.Value)))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    entry.ValueStartIndex,
                    entry.LineChangeIndexes);

                ValidationFeedback feedback = new(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.InvalidValueFound,
                        feedbackIndexes.Key,
                        feedbackIndexes.Value,
                        $"{entry.Key.Keyword}: {entry.Value}");

                return [
                    new ValidationFeedbackItem(
                        entry,
                        feedback
                        )
                ];
            }
            else if (entry.Key.Keyword == validator.SyntaxConf.Tokens.KeyWords.ContentVariableIdentifier)
            {
                string defaultLanguage = validator.defaultLanguage ?? string.Empty;
                string lang = entry.Key.Language ?? defaultLanguage;
                if (validator.stubDimensionNames is not null && !Array.Exists(validator.stubDimensionNames[lang], d => d == entry.Value) &&
                (validator.headingDimensionNames is not null && !Array.Exists(validator.headingDimensionNames[lang], d => d == entry.Value)))
                {
                    KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                        entry.KeyStartLineIndex,
                        entry.ValueStartIndex,
                        entry.LineChangeIndexes);

                    ValidationFeedback feedback = new (
                                ValidationFeedbackLevel.Error,
                                ValidationFeedbackRule.InvalidValueFound,
                                feedbackIndexes.Key,
                                feedbackIndexes.Value,
                                $"{entry.Key.Keyword}: {entry.Value}");

                    return [
                        new ValidationFeedbackItem(
                            entry,
                            feedback
                        )
                    ];
                }
            }

            return null;
        }

        /// <summary>
        /// Validates that entries with specific keywords have the correct amount of values in the Px file metadata entry
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object is returned if an unexpected amount of values is detected</returns>
        public static ValidationFeedbackItem[]? ValidateValueAmounts(ValidationStructuredEntry entry, ContentValidator validator)
        {
            if (entry.Key.Keyword != validator.SyntaxConf.Tokens.KeyWords.VariableValueCodes ||
                validator.dimensionValueNames is null ||
                entry.Key.FirstSpecifier is null)
            {
                return null;
            }

            string[] codes = entry.Value.Split(validator.SyntaxConf.Symbols.Value.ListSeparator);
            string defaultLanguage =  validator.defaultLanguage ?? string.Empty;
            string lang = entry.Key.Language ?? defaultLanguage;
            if (codes.Length != validator.dimensionValueNames[new(lang, entry.Key.FirstSpecifier)].Length)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    entry.ValueStartIndex,
                    entry.LineChangeIndexes);

                ValidationFeedback feedback = new (
                            ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.UnmatchingValueAmount,
                            feedbackIndexes.Key,
                            feedbackIndexes.Value,
                            $"{entry.Key.Keyword}: {entry.Value}");

                return [
                    new ValidationFeedbackItem(
                        entry,
                        feedback
                    )
                ];
            }

            return null;
        }

        /// <summary>
        /// Validates that entries with specific keywords have their values written in upper case
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object with warning is returned if the found value is not written in upper case</returns>
        public static ValidationFeedbackItem[]? ValidateValueUppercaseRecommendations(ValidationStructuredEntry entry, ContentValidator validator)
        {
            string[] recommendedUppercaseValueKeywords = [
                validator.SyntaxConf.Tokens.KeyWords.CodePage
            ];

            if (!recommendedUppercaseValueKeywords.Contains(entry.Key.Keyword))
            {
                return null;
            }

            // Check if entry.value is in upper case
            string valueUppercase = entry.Value.ToUpper();
            if (entry.Value != valueUppercase)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    entry.ValueStartIndex,
                    entry.LineChangeIndexes);

                ValidationFeedback feedback = new (
                        ValidationFeedbackLevel.Warning,
                        ValidationFeedbackRule.ValueIsNotInUpperCase,
                        feedbackIndexes.Key,
                        entry.ValueStartIndex,
                        $"{entry.Key.Keyword}: {entry.Value}");

                return [
                    new ValidationFeedbackItem(
                        entry,
                        feedback
                    )
                ];
            }
            return null;
        }
    }
}
