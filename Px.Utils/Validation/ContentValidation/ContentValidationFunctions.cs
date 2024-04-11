using PxUtils.PxFile;
using PxUtils.Validation.SyntaxValidation;

namespace PxUtils.Validation.ContentValidation
{
    public delegate ValidationFeedbackItem[]? ContentValidationEntryDelegate(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ContentValidator validator);
    public delegate ValidationFeedbackItem[]? ContentValidationFindKeywordDelegate(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ContentValidator validator); 
    
    /// <summary>
    /// Collection of functions for validating Px file metadata contents
    /// </summary>
    public class ContentValidationFunctions
    {
        public List<ContentValidationEntryDelegate> DefaultContentValidationEntryFunctions { get; }
        public List<ContentValidationFindKeywordDelegate> DefaultContentValidationFindKeywordFunctions { get; }

        /// <summary>
        /// Constructor that contains default content validation functions
        /// </summary>
        public ContentValidationFunctions()
        {
            DefaultContentValidationEntryFunctions = [
                ValidateUnexpectedSpecifiers,
                ValidateUnexpectedLanguageParams,
                ValidateLanguageParams,
                ValidateSpecifiers,
                ValidateValueTypes,
                ValidateValueContents,
                ValidateValueAmounts,
                ValidateValueUppercaseRecommendations
            ];
            DefaultContentValidationFindKeywordFunctions = [
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
        }

        /// <summary>
        /// Validates that default language is defined properly in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Null if no issues are found. <see cref="ValidationFeedbackItem"/> objects are returned if entry defining default language is not found or if more than one are found.</returns>
        public static ValidationFeedbackItem[]? ValidateFindDefaultLanguage(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            ValidationStructuredEntry[] langEntries = entries.Where(
                e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.DefaultLanguage)).ToArray();

            if (langEntries.Length > 1)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entries[0].KeyStartLineIndex, 
                    0, 
                    entries[0].LineChangeIndexes);

                return [
                    new ValidationFeedbackItem(
                    entries[0],
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MultipleInstancesOfUniqueKey,
                        feedbackIndexes.Key,
                        0,
                        $"{syntaxConf.Tokens.KeyWords.DefaultLanguage}: " + string.Join(", ", entries.Select(e => e.Value).ToArray())
                        )
                    ),
                ];
            }
            else if (langEntries.Length == 0)
            {
                return [
                    new ValidationFeedbackItem(
                    new ContentValidationObject(validator.Filename, 0, []),
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MissingDefaultLanguage,
                        0,
                        0
                        )
                    ),
                ];
            }

            validator.DefaultLanguage = langEntries[0].Value;
            return null;
        }

        /// <summary>
        /// Finds an entry that defines available languages in the Px file
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Null if no issues are found. <see cref="ValidationFeedbackItem"/> object with a warning is returned if available languages entry is not found. Error is returned if multiple entries are found.</returns>
        public static ValidationFeedbackItem[]? ValidateFindAvailableLanguages(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            ValidationStructuredEntry[] availableLanguageEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.AvailableLanguages)).ToArray();

            if (availableLanguageEntries.Length > 1)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entries[0].KeyStartLineIndex,
                    0,
                    entries[0].LineChangeIndexes);

                return [
                    new ValidationFeedbackItem(
                    entries[0],
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MultipleInstancesOfUniqueKey,
                        feedbackIndexes.Key,
                        0,
                        $"{syntaxConf.Tokens.KeyWords.AvailableLanguages}: " + string.Join(", ", entries.Select(e => e.Value).ToArray())
                        )
                    )
               ];
            }

            if (availableLanguageEntries.Length == 1)
            {
                validator.AvailableLanguages = availableLanguageEntries[0].Value.Split(syntaxConf.Symbols.Value.ListSeparator); 
                return null;
            }
            else
            {
                return [
                    new ValidationFeedbackItem(
                    new ContentValidationObject(validator.Filename, 0, []),
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Warning,
                        ValidationFeedbackRule.RecommendedKeyMissing,
                        0,
                        0,
                        syntaxConf.Tokens.KeyWords.AvailableLanguages
                        )
                    )
               ];
            }
        }

        /// <summary>
        /// Validates that default language is defined in the available languages entry
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Null of no issues are found. <see cref="ValidationFeedbackItem"/> object is returned if default language is not defined in the available languages entry</returns>
        public static ValidationFeedbackItem[]? ValidateDefaultLanguageDefinedInAvailableLanguages(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            if (validator.AvailableLanguages is not null && !validator.AvailableLanguages.Contains(validator.DefaultLanguage))
            {
                ValidationStructuredEntry? defaultLanguageEntry = Array.Find(entries, e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.DefaultLanguage));

                if (defaultLanguageEntry is null)
                {
                    return null;
                }
                
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    defaultLanguageEntry.KeyStartLineIndex,
                    0,
                    defaultLanguageEntry.LineChangeIndexes);

                return [
                    new ValidationFeedbackItem(
                    defaultLanguageEntry,
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.UndefinedLanguageFound,
                        feedbackIndexes.Key,
                        0,
                        validator.DefaultLanguage
                        )
                    )
                ];
            }

            return null;
        }

        /// <summary>
        /// Finds a content dimension and validates that it exists for all available languages
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if content dimension entry is not found for any available language</returns>
        public static ValidationFeedbackItem[]? ValidateFindContentDimension(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            ValidationStructuredEntry[] contentDimensionEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.ContentVariableIdentifier)).ToArray();

            string defaultLanguage = validator.DefaultLanguage ?? string.Empty;
            string[] languages = validator.AvailableLanguages ?? [defaultLanguage];
            foreach (string language in languages)
            {
                ValidationStructuredEntry? contentDimension = Array.Find(contentDimensionEntries, c => c.Key.Language == language || (c.Key.Language is null && language == defaultLanguage));
                if (contentDimension is null)
                {
                    feedbackItems.Add(
                        new ValidationFeedbackItem(
                        new ContentValidationObject(validator.Filename, 0, []),
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.RecommendedKeyMissing,
                            0,
                            0,
                            $"{syntaxConf.Tokens.KeyWords.ContentVariableIdentifier}, {language}"
                            )
                        ));
                }
            }

            validator.ContentDimensionNames = contentDimensionEntries.ToDictionary(
                e => e.Key.Language ?? defaultLanguage, 
                e => e.Value);
            
            return [.. feedbackItems];
        }

        /// <summary>
        /// Validates that entries with required keys are found in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if entries required keywords are not found</returns>
        public static ValidationFeedbackItem[]? ValidateFindRequiredCommonKeys(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            string[] alwaysRequiredKeywords =
            [
                syntaxConf.Tokens.KeyWords.Charset,
                syntaxConf.Tokens.KeyWords.CodePage,
                syntaxConf.Tokens.KeyWords.DefaultLanguage,
            ];

            foreach (string keyword in alwaysRequiredKeywords)
            {
                if (!Array.Exists(entries, e => e.Key.Keyword.Equals(keyword)))
                {
                    feedbackItems.Add(
                        new ValidationFeedbackItem(
                            new ContentValidationObject(validator.Filename, 0, []),
                            new ValidationFeedback(
                                ValidationFeedbackLevel.Error,
                                ValidationFeedbackRule.RequiredKeyMissing,
                                0,
                                0,
                                keyword
                                )
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
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if stub and heading dimension entries are not found for any available language</returns>
        public static ValidationFeedbackItem[]? ValidateFindStubAndHeading(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            ValidationStructuredEntry[] stubEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.StubDimensions)).ToArray();
            ValidationStructuredEntry[] headingEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.HeadingDimensions)).ToArray();

            if (stubEntries.Length == 0 && headingEntries.Length == 0)
            {
                return [
                    new(
                    new ContentValidationObject(validator.Filename, 0, []),
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MissingStubAndHeading,
                        0,
                        0
                        )
                    )
                ];
            }

            string defaultLanguage = validator.DefaultLanguage ?? string.Empty;

            validator.StubDimensionNames = stubEntries.ToDictionary(
                e => e.Key.Language ?? defaultLanguage, 
                e => e.Value.Split(syntaxConf.Symbols.Key.ListSeparator));
            
            validator.HeadingDimensionNames = headingEntries.ToDictionary(
                e => e.Key.Language ?? defaultLanguage, 
                e => e.Value.Split(syntaxConf.Symbols.Key.ListSeparator));

            List<ValidationFeedbackItem> feedbackItems = [];

            string[] languages = validator.AvailableLanguages ?? [defaultLanguage];

            foreach (string language in languages)
            {
                if ((validator.StubDimensionNames is null || !validator.StubDimensionNames.ContainsKey(language))
                    &&
                    (validator.HeadingDimensionNames is null || !validator.HeadingDimensionNames.ContainsKey(language)))
                {
                    feedbackItems.Add(
                        new ValidationFeedbackItem(
                            new ContentValidationObject(validator.Filename, 0, []),
                            new ValidationFeedback(
                                ValidationFeedbackLevel.Error,
                                ValidationFeedbackRule.MissingStubAndHeading,
                                0,
                                0,
                                $"{language}"
                                )
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
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects with warning are returned if recommended keys are missing</returns>
        public static ValidationFeedbackItem[]? ValidateFindRecommendedKeys(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            List<ValidationFeedbackItem> feedbackItems = [];

            string[] commonKeys =
            [
                syntaxConf.Tokens.KeyWords.TableId
            ];
            string[] languageSpecific =
            [
                syntaxConf.Tokens.KeyWords.Description
            ];

            foreach (string keyword in commonKeys)
            {
                if (!Array.Exists(entries, e => e.Key.Keyword.Equals(keyword)))
                {
                    feedbackItems.Add(
                        new ValidationFeedbackItem(
                            new ContentValidationObject(validator.Filename, 0, []),
                            new ValidationFeedback(
                                ValidationFeedbackLevel.Warning,
                                ValidationFeedbackRule.RecommendedKeyMissing,
                                0,
                                0,
                                keyword
                                )
                            )
                        );
                }
            }

            string defaultLanguage = validator.DefaultLanguage ?? string.Empty;
            string[] languages = validator.AvailableLanguages ?? [defaultLanguage];

            foreach(string language in languages)
            {
                foreach(string keyword in languageSpecific)
                {
                    if (!Array.Exists(entries, e => e.Key.Keyword.Equals(keyword) && 
                    (e.Key.Language == language || (language == defaultLanguage && e.Key.Language is null))))
                    {
                        feedbackItems.Add(
                            new ValidationFeedbackItem(
                                new ContentValidationObject(validator.Filename, 0, []),
                                new ValidationFeedback(
                                    ValidationFeedbackLevel.Warning,
                                    ValidationFeedbackRule.RecommendedKeyMissing,
                                    0,
                                    0,
                                    $"{language}, {keyword}"
                                    )
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
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if values are missing for any dimension</returns>
        public static ValidationFeedbackItem[]? ValidateFindDimensionValues(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            ValidationStructuredEntry[] dimensionEntries = entries.Where(
                e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.VariableValues)).ToArray();

            IEnumerable<KeyValuePair<KeyValuePair<string, string>, string[]>> dimensionValues = [];
            dimensionValues = dimensionValues.Concat(
                ContentValidationUtilityMethods.FindDimensionValues(
                    dimensionEntries,
                    syntaxConf,
                    validator.StubDimensionNames, 
                    ref feedbackItems, 
                    validator.Filename)
                );
            dimensionValues = dimensionValues.Concat(
                ContentValidationUtilityMethods.FindDimensionValues(
                    dimensionEntries,
                    syntaxConf, 
                    validator.HeadingDimensionNames, 
                    ref feedbackItems,
                    validator.Filename)
                );

            validator.DimensionValueNames = [];
            foreach (KeyValuePair<KeyValuePair<string, string>, string[]> item in dimensionValues)
            {
                validator.DimensionValueNames.Add(item.Key, item.Value);
            }

            return [.. feedbackItems];
        }

        /// <summary>
        /// Finds and validates that entries with required keys related to the content dimension and its values are found in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if required entries are missing. Warnings are returned if entries are defined in an unrecommended way</returns>
        public static ValidationFeedbackItem[]? ValidateFindContentDimensionKeys(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            List<ValidationFeedbackItem>? feedbackItems = [];

            string[] languageSpecificKeywords =
                [
                    syntaxConf.Tokens.KeyWords.Units,
                ];

            string[] requiredKeywords =
                [
                    syntaxConf.Tokens.KeyWords.LastUpdated,
                ];

            string[] recommendedKeywords =
                [
                    syntaxConf.Tokens.KeyWords.Precision
                ];

            string[] contentDimensionNames =
                validator.ContentDimensionNames is not null ?
                [.. validator.ContentDimensionNames.Values] : [];

            Dictionary<KeyValuePair<string, string>, string[]> dimensionValueNames = 
                validator.DimensionValueNames ?? [];

            Dictionary<KeyValuePair<string, string>, string[]> contentDimensionValueNames = dimensionValueNames
                .Where(e => contentDimensionNames.Contains(e.Key.Value))
                .ToDictionary(e => e.Key, e => e.Value);

            foreach (KeyValuePair<KeyValuePair<string, string>, string[]> kvp in contentDimensionValueNames)
            {
                foreach (string dimensionValueName in kvp.Value)
                {
                    List<ValidationFeedbackItem> items = ContentValidationUtilityMethods.ProcessContentDimensionValue(languageSpecificKeywords, requiredKeywords, recommendedKeywords, entries, validator, kvp.Key, dimensionValueName);
                    feedbackItems.AddRange(items);
                }
            }

            return [.. feedbackItems];
        }

        /// <summary>
        /// Validates that recommended entries related to dimensions are found in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects with warnings are returned if any dimensions are missing recommended entries related to them</returns>
        public static ValidationFeedbackItem[]? ValidateFindDimensionRecommendedKeys(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            List<ValidationFeedbackItem> feedbackItems = [];

            string[] dimensionRecommendedKeywords =
                [
                    syntaxConf.Tokens.KeyWords.DimensionCode,
                    syntaxConf.Tokens.KeyWords.VariableValueCodes
                ];

            Dictionary<string, string[]> stubDimensions =  validator.StubDimensionNames ?? [];
            Dictionary<string, string[]> headingDimensions =  validator.HeadingDimensionNames ?? [];
            Dictionary<string, string[]> allDimensions = stubDimensions
                .Concat(headingDimensions)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.SelectMany(kvp => kvp.Value).ToArray());

            foreach (KeyValuePair<string, string[]> languageDimensions in allDimensions)
            {
                ValidationFeedbackItem? timeValFeedback = ContentValidationUtilityMethods.FindDimensionRecommendedKey(entries, syntaxConf.Tokens.KeyWords.TimeVal, languageDimensions.Key, validator);
                if (timeValFeedback is not null)
                {
                    feedbackItems.Add((ValidationFeedbackItem)timeValFeedback);
                }

                foreach (string dimension in languageDimensions.Value)
                {
                    List<ValidationFeedbackItem> dimensionFeedback = ContentValidationUtilityMethods.ProcessDimension(
                        dimensionRecommendedKeywords,
                        syntaxConf.Tokens.KeyWords.DimensionType,
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
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if </returns>
        public static ValidationFeedbackItem[]? ValidateUnexpectedSpecifiers(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            string[] noSpecifierAllowedKeywords =
            [
                syntaxConf.Tokens.KeyWords.DefaultLanguage,
                syntaxConf.Tokens.KeyWords.AvailableLanguages,
                syntaxConf.Tokens.KeyWords.Charset,
                syntaxConf.Tokens.KeyWords.CodePage,
                syntaxConf.Tokens.KeyWords.StubDimensions,
                syntaxConf.Tokens.KeyWords.HeadingDimensions,
                syntaxConf.Tokens.KeyWords.TableId,
                syntaxConf.Tokens.KeyWords.Description,
                syntaxConf.Tokens.KeyWords.ContentVariableIdentifier,
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

                return [new ValidationFeedbackItem(
                    entry,
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.IllegalSpecifierDefinitionFound,
                        feedbackIndexes.Key,
                        0,
                        $"{entry.Key.Keyword}: {entry.Key.FirstSpecifier}, {entry.Key.SecondSpecifier}"
                        )
                    )];
            }

            return null;
        }

        /// <summary>
        /// Finds unexpected language parameters in the Px file metadata entry
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if an illegal or unrecommended language parameter is detected in the entry</returns>
        public static ValidationFeedbackItem[]? ValidateUnexpectedLanguageParams(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            string[] noLanguageParameterAllowedKeywords = [
                syntaxConf.Tokens.KeyWords.Charset,
                syntaxConf.Tokens.KeyWords.CodePage,
                syntaxConf.Tokens.KeyWords.TableId,
                syntaxConf.Tokens.KeyWords.DefaultLanguage,
                syntaxConf.Tokens.KeyWords.AvailableLanguages,
            ];

            string[] noLanguageParameterRecommendedKeywords = [
                syntaxConf.Tokens.KeyWords.LastUpdated,
                syntaxConf.Tokens.KeyWords.DimensionType,
                syntaxConf.Tokens.KeyWords.Precision
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
                    return [new ValidationFeedbackItem(
                        entry,
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.IllegalLanguageDefinitionFound,
                            feedbackIndexes.Key,
                            0,
                            $"{entry.Key.Keyword}, {entry.Key.Language}, {entry.Key.FirstSpecifier}, {entry.Key.SecondSpecifier}"))
                        ];
                }
                else if (noLanguageParameterRecommendedKeywords.Contains(entry.Key.Keyword))
                {
                    return [new ValidationFeedbackItem(
                        entry,
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.UnrecommendedLanguageDefinitionFound,
                            feedbackIndexes.Key,
                            0,
                            $"{entry.Key.Keyword}, {entry.Key.Language}, {entry.Key.FirstSpecifier}, {entry.Key.SecondSpecifier}"))
                        ];
                }
            }

            return null;
        }

        /// <summary>
        /// Validates that the language defined in the Px file metadata entry is defined in the available languages entry
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object is returned if an undefined language is found from the entry</returns>
        public static ValidationFeedbackItem[]? ValidateLanguageParams(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            if (entry.Key.Language is null)
            {
                return null;
            }

            if (validator.AvailableLanguages is not null && !validator.AvailableLanguages.Contains(entry.Key.Language))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);

                return [new ValidationFeedbackItem(
                    entry,
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.UndefinedLanguageFound,
                        feedbackIndexes.Key,
                        0,
                        entry.Key.Language))
                    ];
            }

            return null;
        }

        /// <summary>
        /// Validates that specifiers are defined properly in the Px file metadata entry. Content dimension specifiers are allowed to be defined using only the first specifier at value level and are checked separately
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object is returned if the entry's specifier is defined in an unexpected way</returns>
        public static ValidationFeedbackItem[]? ValidateSpecifiers(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            if ((entry.Key.FirstSpecifier is null && entry.Key.SecondSpecifier is null) || 
                validator.DimensionValueNames is null)
            {
                return null;
            }

            // Content dimension specifiers are allowed to be defined using only the first specifier at value level and are checked separately
            string[] excludeKeywords =
                [
                    syntaxConf.Tokens.KeyWords.Precision,
                    syntaxConf.Tokens.KeyWords.Units,
                    syntaxConf.Tokens.KeyWords.LastUpdated,
                    syntaxConf.Tokens.KeyWords.Contact,
                    syntaxConf.Tokens.KeyWords.ValueNote
                ];

            if (excludeKeywords.Contains(entry.Key.Keyword))
            {
                return null;
            }

            if ((validator.StubDimensionNames is null || !validator.StubDimensionNames.Values.Any(v => v.Contains(entry.Key.FirstSpecifier))) &&
                (validator.HeadingDimensionNames is null || !validator.HeadingDimensionNames.Values.Any(v => v.Contains(entry.Key.FirstSpecifier))))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);
                    
                    feedbackItems.Add(new ValidationFeedbackItem(
                        entry,
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.IllegalSpecifierDefinitionFound,
                            feedbackIndexes.Key,
                            0,
                            entry.Key.FirstSpecifier))
                        );
            }
            else if (entry.Key.SecondSpecifier is not null && 
                !validator.DimensionValueNames.Values.Any(v => v.Contains(entry.Key.SecondSpecifier)))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);

                feedbackItems.Add(new ValidationFeedbackItem(
                    entry,
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.IllegalSpecifierDefinitionFound,
                        feedbackIndexes.Key,
                        0,
                        entry.Key.SecondSpecifier))
                    );
            }

            return [.. feedbackItems];
        }

        /// <summary>
        /// Validates that certain keywords are defined with the correct value types in the Px file metadata entry
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object is returned if an unexpected value type is detected</returns>
        public static ValidationFeedbackItem[]? ValidateValueTypes(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            string[] stringTypes =
                [
                    syntaxConf.Tokens.KeyWords.Charset,
                    syntaxConf.Tokens.KeyWords.CodePage,
                    syntaxConf.Tokens.KeyWords.DefaultLanguage,
                    syntaxConf.Tokens.KeyWords.Units,
                    syntaxConf.Tokens.KeyWords.Description,
                    syntaxConf.Tokens.KeyWords.TableId,
                    syntaxConf.Tokens.KeyWords.ContentVariableIdentifier,
                    syntaxConf.Tokens.KeyWords.DimensionCode
                ];

            string[] listOfStringTypes =
                [
                    syntaxConf.Tokens.KeyWords.AvailableLanguages,
                    syntaxConf.Tokens.KeyWords.StubDimensions,
                    syntaxConf.Tokens.KeyWords.HeadingDimensions,
                    syntaxConf.Tokens.KeyWords.VariableValues,
                    syntaxConf.Tokens.KeyWords.VariableValueCodes
                ];

            string[] dateTimeTypes =
                [
                    syntaxConf.Tokens.KeyWords.LastUpdated
                ];

            string[] numberTypes =
                [
                    syntaxConf.Tokens.KeyWords.Precision
                ];

            string[] timeval =
                [
                    syntaxConf.Tokens.KeyWords.TimeVal
                ];

            if ((stringTypes.Contains(entry.Key.Keyword) && entry.ValueType != ValueType.String) ||
                (listOfStringTypes.Contains(entry.Key.Keyword) && entry.ValueType != ValueType.ListOfStrings) ||
                (dateTimeTypes.Contains(entry.Key.Keyword) && entry.ValueType != ValueType.DateTime) ||
                (numberTypes.Contains(entry.Key.Keyword) && entry.ValueType != ValueType.Number) ||
                (timeval.Contains(entry.Key.Keyword) && entry.ValueType != ValueType.Timeval))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    entry.ValueStartIndex,
                    entry.LineChangeIndexes);

                return [new ValidationFeedbackItem(
                    entry,
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.UnmatchingValueType,
                        feedbackIndexes.Key,
                        feedbackIndexes.Value,
                        $"{entry.Key.Keyword}: {entry.ValueType}"))
                    ];
            }

            return null;
        }

        /// <summary>
        /// Validates that entries with specific keywords have valid values in the Px file metadata entry
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object is returned if an unexpected value is detected</returns>
        public static ValidationFeedbackItem[]? ValidateValueContents(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            string[] allowedCharsets = ["ANSI", "Unicode"];

            string[] dimensionTypes = [
                    syntaxConf.Tokens.VariableTypes.Content,
                syntaxConf.Tokens.VariableTypes.Time,
                syntaxConf.Tokens.VariableTypes.Geographical,
                syntaxConf.Tokens.VariableTypes.Ordinal,
                syntaxConf.Tokens.VariableTypes.Nominal,
                syntaxConf.Tokens.VariableTypes.Other,
                syntaxConf.Tokens.VariableTypes.Unknown,
                syntaxConf.Tokens.VariableTypes.Classificatory
                    ];

            if ((entry.Key.Keyword == syntaxConf.Tokens.KeyWords.Charset && !allowedCharsets.Contains(entry.Value)) ||
                (entry.Key.Keyword == syntaxConf.Tokens.KeyWords.CodePage && !entry.Value.Equals(validator.Encoding.BodyName, StringComparison.CurrentCultureIgnoreCase)) ||
                (entry.Key.Keyword == syntaxConf.Tokens.KeyWords.DimensionType && !dimensionTypes.Contains(entry.Value)))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    entry.ValueStartIndex,
                    entry.LineChangeIndexes);

                return [new ValidationFeedbackItem(
                    entry,
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.InvalidValueFound,
                        feedbackIndexes.Key,
                        feedbackIndexes.Value,
                        $"{entry.Key.Keyword}: {entry.Value}"))
                    ];
            }
            else if (entry.Key.Keyword == syntaxConf.Tokens.KeyWords.ContentVariableIdentifier)
            {
                string defaultLanguage = validator.DefaultLanguage ?? string.Empty;
                string lang = entry.Key.Language ?? defaultLanguage;
                if (validator.StubDimensionNames is not null && !Array.Exists(validator.StubDimensionNames[lang], d => d == entry.Value) &&
                (validator.HeadingDimensionNames is not null && !Array.Exists(validator.HeadingDimensionNames[lang], d => d == entry.Value)))
                {
                    KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                        entry.KeyStartLineIndex,
                        entry.ValueStartIndex,
                        entry.LineChangeIndexes);

                    return [new ValidationFeedbackItem(
                        entry,
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.InvalidValueFound,
                            feedbackIndexes.Key,
                            feedbackIndexes.Value,
                            $"{entry.Key.Keyword}: {entry.Value}"))
                        ];
                }
            }

            return null;
        }

        /// <summary>
        /// Validates that entries with specific keywords have the correct amount of values in the Px file metadata entry
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object is returned if an unexpected amount of values is detected</returns>
        public static ValidationFeedbackItem[]? ValidateValueAmounts(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            if (entry.Key.Keyword != syntaxConf.Tokens.KeyWords.VariableValueCodes ||
                validator.DimensionValueNames is null ||
                entry.Key.FirstSpecifier is null)
            {
                return null;
            }

            string[] codes = entry.Value.Split(syntaxConf.Symbols.Value.ListSeparator);
            string defaultLanguage =  validator.DefaultLanguage ?? string.Empty;
            string lang = entry.Key.Language ?? defaultLanguage;
            if (codes.Length != validator.DimensionValueNames[new(lang, entry.Key.FirstSpecifier)].Length)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    entry.ValueStartIndex,
                    entry.LineChangeIndexes);

                return [
                    new ValidationFeedbackItem(
                        entry,
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.UnmatchingValueAmount,
                            feedbackIndexes.Key,
                            feedbackIndexes.Value,
                            $"{entry.Key.Keyword}: {entry.Value}"))
                    ];
            }

            return null;
        }

        /// <summary>
        /// Validates that entries with specific keywords have their values written in upper case
        /// </summary>
        /// <param name="entry">Entry in the Px file metadata. Represented by a <see cref="ValidationStructuredEntry"/> object</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="validator"><see cref="ContentValidator"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object with warning is returned if the found value is not written in upper case</returns>
        public static ValidationFeedbackItem[]? ValidateValueUppercaseRecommendations(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ContentValidator validator)
        {
            string[] recommendedUppercaseValueKeywords = [
                syntaxConf.Tokens.KeyWords.CodePage
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
                
                return [new ValidationFeedbackItem(
                    entry,
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Warning,
                        ValidationFeedbackRule.ValueIsNotInUpperCase,
                        feedbackIndexes.Key,
                        entry.ValueStartIndex,
                        $"{entry.Key.Keyword}: {entry.Value}"))];
            }
            return null;
        }
    }
}
