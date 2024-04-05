using PxUtils.PxFile;
using PxUtils.Validation.SyntaxValidation;

namespace PxUtils.Validation.ContentValidation
{
    public delegate ValidationFeedbackItem[]? ContentValidationEntryFunctionDelegate(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info);
    public delegate ValidationFeedbackItem[]? ContentValidationSearchFunctionDelegate(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info); 
    
    /// <summary>
    /// Collection of functions for validating Px file metadata contents
    /// </summary>
    public class ContentValidationFunctions
    {
        public List<ContentValidationEntryFunctionDelegate> DefaultContentValidationEntryFunctions { get; }
        public List<ContentValidationSearchFunctionDelegate> DefaultContentValidationSearchFunctions { get; }

        /// <summary>
        /// Constructor for <see cref="ContentValidationFunctions"/> class that contains default content validation functions/>
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
            DefaultContentValidationSearchFunctions = [
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
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Null if no issues are found. <see cref="ValidationFeedbackItem"/> objects are returned if entry defining default language is not found or if more than one are found.</returns>
        public static ValidationFeedbackItem[]? ValidateFindDefaultLanguage(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
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
                    new ContentValidationObject(info.Filename, 0, []),
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MissingDefaultLanguage,
                        0,
                        0
                        )
                    ),
                ];
            }

            info.DefaultLanguage = langEntries[0].Value;
            return null;
        }

        /// <summary>
        /// Finds an entry that defines available languages in the Px file
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Null if no issues are found. <see cref="ValidationFeedbackItem"/> object with a warning is returned if available languages entry is not found. Error is returned if multiple entries are found.</returns>
        public static ValidationFeedbackItem[]? ValidateFindAvailableLanguages(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
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
                info.AvailableLanguages = availableLanguageEntries[0].Value.Split(syntaxConf.Symbols.Value.ListSeparator); 
                return null;
            }
            else
            {
                return [
                    new ValidationFeedbackItem(
                    new ContentValidationObject(info.Filename, 0, []),
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
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns>Null of no issues are found. <see cref="ValidationFeedbackItem"/> object is returned if default language is not defined in the available languages entry</returns>
        public static ValidationFeedbackItem[]? ValidateDefaultLanguageDefinedInAvailableLanguages(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            if (info.AvailableLanguages is not null && !info.AvailableLanguages.Contains(info.DefaultLanguage))
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
                        info.DefaultLanguage
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
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if content dimension entry is not found for any available language</returns>
        public static ValidationFeedbackItem[]? ValidateFindContentDimension(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            ValidationStructuredEntry[] contentDimensionEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.ContentVariableIdentifier)).ToArray();

            string defaultLanguage = info.DefaultLanguage ?? string.Empty;
            string[] languages = info.AvailableLanguages ?? [defaultLanguage];
            foreach (string language in languages)
            {
                ValidationStructuredEntry? contentDimension = Array.Find(contentDimensionEntries, c => c.Key.Language == language || (c.Key.Language is null && language == defaultLanguage));
                if (contentDimension is null)
                {
                    feedbackItems.Add(
                        new ValidationFeedbackItem(
                        new ContentValidationObject(info.Filename, 0, []),
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

            info.ContentDimensionNames = contentDimensionEntries.ToDictionary(
                e => e.Key.Language ?? defaultLanguage, 
                e => e.Value);
            
            return [.. feedbackItems];
        }

        /// <summary>
        /// Validates that entries with required keys are found in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if entries required keywords are not found</returns>
        public static ValidationFeedbackItem[]? ValidateFindRequiredCommonKeys(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
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
                            new ContentValidationObject(info.Filename, 0, []),
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
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if stub and heading dimension entries are not found for any available language</returns>
        public static ValidationFeedbackItem[]? ValidateFindStubAndHeading(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            ValidationStructuredEntry[] stubEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.StubDimensions)).ToArray();
            ValidationStructuredEntry[] headingEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.HeadingDimensions)).ToArray();

            if (stubEntries.Length == 0 && headingEntries.Length == 0)
            {
                return [
                    new(
                    new ContentValidationObject(info.Filename, 0, []),
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MissingStubAndHeading,
                        0,
                        0
                        )
                    )
                ];
            }

            string defaultLanguage = info.DefaultLanguage ?? string.Empty;

            info.StubDimensionNames = stubEntries.ToDictionary(
                e => e.Key.Language ?? defaultLanguage, 
                e => e.Value.Split(syntaxConf.Symbols.Key.ListSeparator));
            
            info.HeadingDimensionNames = headingEntries.ToDictionary(
                e => e.Key.Language ?? defaultLanguage, 
                e => e.Value.Split(syntaxConf.Symbols.Key.ListSeparator));

            List<ValidationFeedbackItem> feedbackItems = [];

            string[] languages = info.AvailableLanguages ?? [defaultLanguage];

            foreach (string language in languages)
            {
                if ((info.StubDimensionNames is null || !info.StubDimensionNames.ContainsKey(language))
                    &&
                    (info.HeadingDimensionNames is null || !info.HeadingDimensionNames.ContainsKey(language)))
                {
                    feedbackItems.Add(
                        new ValidationFeedbackItem(
                            new ContentValidationObject(info.Filename, 0, []),
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
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects with warning are returned if recommended keys are missing</returns>
        public static ValidationFeedbackItem[]? ValidateFindRecommendedKeys(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
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
                            new ContentValidationObject(info.Filename, 0, []),
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

            string defaultLanguage = info.DefaultLanguage ?? string.Empty;
            string[] languages = info.AvailableLanguages ?? [defaultLanguage];

            foreach(string language in languages)
            {
                foreach(string keyword in languageSpecific)
                {
                    if (!Array.Exists(entries, e => e.Key.Keyword.Equals(keyword) && 
                    (e.Key.Language == language || (language == defaultLanguage && e.Key.Language is null))))
                    {
                        feedbackItems.Add(
                            new ValidationFeedbackItem(
                                new ContentValidationObject(info.Filename, 0, []),
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
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if values are missing for any dimension</returns>
        public static ValidationFeedbackItem[]? ValidateFindDimensionValues(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            ValidationStructuredEntry[] dimensionEntries = entries.Where(
                e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.VariableValues)).ToArray();

            IEnumerable<KeyValuePair<KeyValuePair<string, string>, string[]>> dimensionValues = [];
            dimensionValues = dimensionValues.Concat(
                ContentValidationUtilityMethods.FindDimensionValues(
                    dimensionEntries,
                    syntaxConf,
                    info.StubDimensionNames, 
                    ref feedbackItems, 
                    info.Filename)
                );
            dimensionValues = dimensionValues.Concat(
                ContentValidationUtilityMethods.FindDimensionValues(
                    dimensionEntries,
                    syntaxConf, 
                    info.HeadingDimensionNames, 
                    ref feedbackItems,
                    info.Filename)
                );

            info.DimensionValueNames = [];
            foreach (KeyValuePair<KeyValuePair<string, string>, string[]> item in dimensionValues)
            {
                info.DimensionValueNames.Add(item.Key, item.Value);
            }

            return [.. feedbackItems];
        }

        /// <summary>
        /// Finds and validates that entries with required keys related to the content dimension and its values are found in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if required entries are missing. Warnings are returned if entries are defined in an unrecommended way</returns>
        public static ValidationFeedbackItem[]? ValidateFindContentDimensionKeys(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            List<ValidationFeedbackItem> feedbackItems = [];

            string[] languageSpecificKeywords =
                [
                    syntaxConf.Tokens.KeyWords.Units,
                ];

            string[] commonKeywords =
                [
                    syntaxConf.Tokens.KeyWords.LastUpdated,
                    syntaxConf.Tokens.KeyWords.Precision
                ];

            string[] contentDimensionNames =
                info.ContentDimensionNames is not null ?
                [.. info.ContentDimensionNames.Values] : [];

            Dictionary<KeyValuePair<string, string>, string[]> dimensionValueNames = 
                info.DimensionValueNames ?? [];

            Dictionary<KeyValuePair<string, string>, string[]> contentDimensionValueNames = dimensionValueNames
                .Where(e => contentDimensionNames.Contains(e.Key.Value))
                .ToDictionary(e => e.Key, e => e.Value);

            string defaultLanguage = info.DefaultLanguage ?? string.Empty;

            foreach (KeyValuePair<KeyValuePair<string, string>, string[]> kvp in contentDimensionValueNames)
            {
                string language = kvp.Key.Key;
                string dimensionName = kvp.Key.Value;

                foreach (string dimensionValueName in kvp.Value)
                {
                    foreach (string keyword in languageSpecificKeywords)
                    {
                        ValidationFeedbackItem? issue = ContentValidationUtilityMethods.FindContentVariableKey(entries, keyword, language, dimensionName, dimensionValueName, defaultLanguage, info.Filename);
                        if (issue is not null)
                        {
                            feedbackItems.Add((ValidationFeedbackItem)issue);
                        }
                    }

                    if (language == defaultLanguage)
                    {
                        foreach (string keyword in commonKeywords)
                        {
                            ValidationFeedbackItem? issue = ContentValidationUtilityMethods.FindContentVariableKey(entries, keyword, language, dimensionName, dimensionValueName, defaultLanguage, info.Filename);
                            if (issue is not null)
                            {
                                feedbackItems.Add((ValidationFeedbackItem)issue);
                            }
                        }
                    }
                }
            }

            return [.. feedbackItems];
        }

        /// <summary>
        /// Validates that recommended entries related to dimensions are found in the Px file metadata
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects with warnings are returned if any dimensions are missing recommended entries related to them</returns>
        public static ValidationFeedbackItem[]? ValidateFindDimensionRecommendedKeys(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            List<ValidationFeedbackItem> feedbackItems = [];

            string[] dimensionRecommendedKeywords =
                [
                    syntaxConf.Tokens.KeyWords.DimensionCode,
                    syntaxConf.Tokens.KeyWords.VariableValueCodes
                ];

            Dictionary<string, string[]> stubDimensions =  info.StubDimensionNames ?? [];
            Dictionary<string, string[]> headingDimensions =  info.HeadingDimensionNames ?? [];
            Dictionary<string, string[]> allDimensions = stubDimensions
                .Concat(headingDimensions)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.SelectMany(kvp => kvp.Value).ToArray());

            string defaultLanguage = info.DefaultLanguage ?? string.Empty;

            foreach (KeyValuePair<string, string[]> languageDimensions in allDimensions)
            {
                ValidationFeedbackItem? timeValFeedback = ContentValidationUtilityMethods.FindDimensionRecommendedKey(entries, syntaxConf.Tokens.KeyWords.TimeVal, languageDimensions.Key, defaultLanguage, info.Filename);
                if (timeValFeedback is not null)
                {
                    feedbackItems.Add((ValidationFeedbackItem)timeValFeedback);
                }

                foreach (string dimension in languageDimensions.Value)
                {
                    foreach (string keyword in dimensionRecommendedKeywords)
                    {
                        ValidationFeedbackItem? keywordFeedback = ContentValidationUtilityMethods.FindDimensionRecommendedKey(entries, keyword, languageDimensions.Key, defaultLanguage, info.Filename, dimension);
                        if (keywordFeedback is not null)
                        {
                            feedbackItems.Add((ValidationFeedbackItem)keywordFeedback);
                        }
                    }

                    if (languageDimensions.Key == defaultLanguage)
                    {
                        ValidationFeedbackItem? variableTypeFeedback = ContentValidationUtilityMethods.FindDimensionRecommendedKey(entries, syntaxConf.Tokens.KeyWords.DimensionType, languageDimensions.Key, defaultLanguage, info.Filename, dimension);
                        if (variableTypeFeedback is not null)
                        {
                            feedbackItems.Add((ValidationFeedbackItem)variableTypeFeedback);
                        }
                    }
                }
            }

            return [.. feedbackItems];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entries">Px file metadata entries in an array of <see cref="ValidationStructuredEntry"/> objects</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that contains symbols and tokens that define the Px file syntax</param>
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if </returns>
        public static ValidationFeedbackItem[]? ValidateUnexpectedSpecifiers(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
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
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> objects are returned if an illegal or unrecommended language parameter is detected in the entry</returns>
        public static ValidationFeedbackItem[]? ValidateUnexpectedLanguageParams(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
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
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object is returned if an undefined language is found from the entry</returns>
        public static ValidationFeedbackItem[]? ValidateLanguageParams(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            if (entry.Key.Language is null)
            {
                return null;
            }

            if (info.AvailableLanguages is not null && !info.AvailableLanguages.Contains(entry.Key.Language))
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
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object is returned if the entry's specifier is defined in an unexpected way</returns>
        public static ValidationFeedbackItem[]? ValidateSpecifiers(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            if ((entry.Key.FirstSpecifier is null && entry.Key.SecondSpecifier is null) || 
                info.DimensionValueNames is null)
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

            if ((info.StubDimensionNames is null || !info.StubDimensionNames.Values.Any(v => v.Contains(entry.Key.FirstSpecifier))) &&
                (info.HeadingDimensionNames is null || !info.HeadingDimensionNames.Values.Any(v => v.Contains(entry.Key.FirstSpecifier))))
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
                !info.DimensionValueNames.Values.Any(v => v.Contains(entry.Key.SecondSpecifier)))
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
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object is returned if an unexpected value type is detected</returns>
        public static ValidationFeedbackItem[]? ValidateValueTypes(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
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
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object is returned if an unexpected value is detected</returns>
        public static ValidationFeedbackItem[]? ValidateValueContents(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
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
                (entry.Key.Keyword == syntaxConf.Tokens.KeyWords.CodePage && !entry.Value.Equals(info.Encoding.BodyName, StringComparison.CurrentCultureIgnoreCase)) ||
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
                string defaultLanguage = info.DefaultLanguage ?? string.Empty;
                string lang = entry.Key.Language ?? defaultLanguage;
                if (info.StubDimensionNames is not null && !Array.Exists(info.StubDimensionNames[lang], d => d == entry.Value) &&
                (info.HeadingDimensionNames is not null && !Array.Exists(info.HeadingDimensionNames[lang], d => d == entry.Value)))
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
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object is returned if an unexpected amount of values is detected</returns>
        public static ValidationFeedbackItem[]? ValidateValueAmounts(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            if (entry.Key.Keyword != syntaxConf.Tokens.KeyWords.VariableValueCodes ||
                info.DimensionValueNames is null ||
                entry.Key.FirstSpecifier is null)
            {
                return null;
            }

            string[] codes = entry.Value.Split(syntaxConf.Symbols.Value.ListSeparator);
            string defaultLanguage =  info.DefaultLanguage ?? string.Empty;
            string lang = entry.Key.Language ?? defaultLanguage;
            if (codes.Length != info.DimensionValueNames[new(lang, entry.Key.FirstSpecifier)].Length)
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
        /// <param name="info"><see cref="ContentValidationInfo"/> object that stores information that is gathered during the validation process</param>
        /// <returns><see cref="ValidationFeedbackItem"/> object with warning is returned if the found value is not written in upper case</returns>
        public static ValidationFeedbackItem[]? ValidateValueUppercaseRecommendations(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
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
