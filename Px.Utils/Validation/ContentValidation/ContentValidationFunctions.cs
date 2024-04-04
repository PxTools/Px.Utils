using PxUtils.PxFile;
using PxUtils.Validation.SyntaxValidation;

namespace PxUtils.Validation.ContentValidation
{
    public delegate ValidationFeedbackItem[]? ContentValidationEntryFunctionDelegate(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info);
    public delegate ValidationFeedbackItem[]? ContentValidationSearchFunctionDelegate(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info); 
    
    // TODO: Summary
    public class ContentValidationFunctions
    {
        public List<ContentValidationEntryFunctionDelegate> DefaultContentValidationEntryFunctions { get; }
        public List<ContentValidationSearchFunctionDelegate> DefaultContentValidationSearchFunctions { get; }

        // TODO: Summmary
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
                ValidateFindStubOrHeading,
                ValidateFindRecommendedKeys,
                ValidateFindDimensionValues,
                ValidateFindContentDimensionKeys,
                ValidateFindDimensionRecommendedKeys
            ];
        }

        /// <summary>
        /// TODO: Summary
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="syntaxConf"></param>
        /// <param name="info"></param>
        /// <returns></returns>
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

        // TODO: Summary
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

        // TODO: Summary
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

        // TODO: Summary
        public static ValidationFeedbackItem[]? ValidateFindContentDimension(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            ValidationStructuredEntry[] contentDimensionEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.ContentVariableIdentifier)).ToArray();

            string defaultLanguage = info.DefaultLanguage is not null ? info.DefaultLanguage : string.Empty;
            string[] languages = info.AvailableLanguages is not null ? info.AvailableLanguages : [defaultLanguage];
            foreach (string language in languages)
            {
                ValidationStructuredEntry? contentDimension = Array.Find(contentDimensionEntries, c => c.Key.Language == language || (c.Key.Language is null && language == defaultLanguage));
                if (contentDimension is null)
                {
                    return [
                        new ValidationFeedbackItem(
                        new ContentValidationObject(info.Filename, 0, []),
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.RecommendedKeyMissing,
                            0,
                            0,
                            $"{syntaxConf.Tokens.KeyWords.ContentVariableIdentifier}, {language}"
                            )
                        ),
                    ];
                }
            }

            info.ContentDimensionEntries = contentDimensionEntries.ToDictionary(
                e => e.Key.Language is not null ? 
                e.Key.Language : 
                defaultLanguage, 
                e => e.Value);
            
            return null;
        }

        // TODO: Summary
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

        // TODO: Summary
        public static ValidationFeedbackItem[]? ValidateFindStubOrHeading(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
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

            string defaultLanguage = info.DefaultLanguage is not null ? info.DefaultLanguage : string.Empty;

            if (stubEntries.Length > 0)
            {
                info.StubDimensions = stubEntries.ToDictionary(
                    e => e.Key.Language is not null ? 
                        e.Key.Language : 
                        defaultLanguage, 
                    e => e.Value.Split(syntaxConf.Symbols.Key.ListSeparator));
            }
            if (headingEntries.Length > 0)
            {
                info.HeadingDimensions = headingEntries.ToDictionary(
                    e => e.Key.Language is not null ? 
                    e.Key.Language : 
                    defaultLanguage, 
                    e => e.Value.Split(syntaxConf.Symbols.Key.ListSeparator));
            }

            if (info.AvailableLanguages is not null)
            {
                foreach (string language in info.AvailableLanguages)
                {
                    if ((info.StubDimensions is null || !info.StubDimensions.ContainsKey(language))
                        &&
                        (info.HeadingDimensions is null || !info.HeadingDimensions.ContainsKey(language)))
                    {
                        return [
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
                            ];
                    }
                }
            }

            return null;
        }

        // TODO: Summary
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

            string defaultLanguage = info.DefaultLanguage is not null ? info.DefaultLanguage : string.Empty;
            if (info.AvailableLanguages is not null)
            {
                foreach(string language in info.AvailableLanguages)
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
                                        $"{language}: {keyword}"
                                        )
                                    )
                                );
                        }
                    }
                }
            }

            return [.. feedbackItems];
        }

        // TODO: Summary
        public static ValidationFeedbackItem[]? ValidateFindDimensionValues(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            ValidationStructuredEntry[] dimensionEntries = entries.Where(
                e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.VariableValues)).ToArray();

            IEnumerable<KeyValuePair<KeyValuePair<string, string>, string[]>> dimensionValues = [];
            dimensionValues = dimensionValues.Concat(
                ContentValidationUtilityMethods.FindVariableValues(
                    dimensionEntries,
                    syntaxConf,
                    info.StubDimensions, 
                    ref feedbackItems, 
                    info.Filename)
                );
            dimensionValues = dimensionValues.Concat(
                ContentValidationUtilityMethods.FindVariableValues(
                    dimensionEntries,
                    syntaxConf, 
                    info.HeadingDimensions, 
                    ref feedbackItems,
                    info.Filename)
                );

            info.DimensionValues = [];
            foreach (KeyValuePair<KeyValuePair<string, string>, string[]> item in dimensionValues)
            {
                info.DimensionValues.Add(item.Key, item.Value);
            }

            return [.. feedbackItems];
        }

        // TODO: Summary
        public static ValidationFeedbackItem[]? ValidateFindContentDimensionKeys(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            if (info.ContentDimensionEntries is null || info.DimensionValues is null)
            {
                return null;
            }

            List<ValidationFeedbackItem> feedbackItems = [];

            string[] languageSpecificKeywords =
                [
                    syntaxConf.Tokens.KeyWords.Units,
                ];

            string[] commonEntries =
                [
                    syntaxConf.Tokens.KeyWords.LastUpdated,
                    syntaxConf.Tokens.KeyWords.Precision
                ];

            string[] contentDimensionNames = [.. info.ContentDimensionEntries.Values];

            Dictionary<KeyValuePair<string, string>, string[]> contentDimensionValueNames = [];
            foreach(KeyValuePair<KeyValuePair<string, string>, string[]> item in info.DimensionValues.Where(
                e => contentDimensionNames.Contains(e.Key.Value)))
            {
                contentDimensionValueNames.Add(item.Key, item.Value);
            }

            string defaultLanguage = info.DefaultLanguage is not null ? info.DefaultLanguage : string.Empty;

            if (contentDimensionNames.Length > 0)
            {
                foreach (KeyValuePair<string, string> kvp in contentDimensionValueNames.Keys)
                {
                    foreach (string keyword in languageSpecificKeywords)
                    {
                        ValidationFeedbackItem? issue = ContentValidationUtilityMethods.FindContentVariableKey(entries, keyword, kvp, defaultLanguage, info.Filename);
                        if (issue is not null)
                        {
                            feedbackItems.Add((ValidationFeedbackItem)issue);
                        }
                    }

                    if (kvp.Key == defaultLanguage)
                    {
                        foreach(string keyword in commonEntries)
                        {
                            ValidationFeedbackItem? issue = ContentValidationUtilityMethods.FindContentVariableKey(entries, keyword, kvp, defaultLanguage, info.Filename);
                            if (issue is not null)
                            {
                                feedbackItems.Add((ValidationFeedbackItem)issue);
                            }
                        }
                    }
                }
            }

            foreach (string keyword in commonEntries)
            {
                IEnumerable<ValidationStructuredEntry> entriesWithLanguage = entries.Where(
                    e => e.Key.Keyword.Equals(keyword) && e.Key.Language is not null);

                foreach (ValidationStructuredEntry entry in entriesWithLanguage)
                {
                    KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                        entry.KeyStartLineIndex,
                        0,
                        entry.LineChangeIndexes);

                    feedbackItems.Add(
                        new ValidationFeedbackItem(
                            entry,
                            new ValidationFeedback(
                                ValidationFeedbackLevel.Warning,
                                ValidationFeedbackRule.UnrecommendedLanguageDefinitionFound,
                                feedbackIndexes.Key,
                                0,
                                entry.Key.Language
                                )
                            )
                        );
                }
            }

            return [.. feedbackItems];
        }

        // TODO: Summary
        public static ValidationFeedbackItem[]? ValidateFindDimensionRecommendedKeys(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            if (info.StubDimensions is null && info.HeadingDimensions is null)
            {
                return null;
            }

            List<ValidationFeedbackItem> feedbackItems = [];

            string[] dimensionRecommendedKeywords =
                [
                    syntaxConf.Tokens.KeyWords.DimensionCode,
                    syntaxConf.Tokens.KeyWords.VariableValueCodes
                ];

            Dictionary<string, string[]> stubDimensions = info.StubDimensions is not null ? info.StubDimensions : [];
            Dictionary<string, string[]> headingDimensions = info.HeadingDimensions is not null ? info.HeadingDimensions : [];
            Dictionary<string, string[]> allDimensions = [];
            
            foreach(KeyValuePair<string, string[]> item in stubDimensions)
            {
                allDimensions.Add(item.Key, item.Value);
            }
            foreach(KeyValuePair<string, string[]> item in headingDimensions)
            {
                if (allDimensions.TryGetValue(item.Key, out string[]? value))
                {
                    allDimensions[item.Key] = [.. value.Concat(item.Value)];
                }
                else
                {
                    allDimensions.Add(item.Key, item.Value);
                }
            }

            string defaultLanguage = info.DefaultLanguage is not null ? info.DefaultLanguage : string.Empty;

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

        // TODO: Summary
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

        // TODO: Summary
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
                            $"{entry.Key.Keyword}: {entry.Key.Language}"))
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
                            $"{entry.Key.Keyword}: {entry.Key.Language}"))
                        ];
                }
            }

            return null;
        }

        // TODO: Summary
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

        // TODO: Summary
        public static ValidationFeedbackItem[]? ValidateSpecifiers(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            if ((entry.Key.FirstSpecifier is null && entry.Key.SecondSpecifier is null) || 
                info.DimensionValues is null)
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

            if ((info.StubDimensions is null || !info.StubDimensions.Values.Any(v => v.Contains(entry.Key.FirstSpecifier))) &&
                (info.HeadingDimensions is null || !info.HeadingDimensions.Values.Any(v => v.Contains(entry.Key.FirstSpecifier))))
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
                !info.DimensionValues.Values.Any(v => v.Contains(entry.Key.SecondSpecifier)))
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

        // TODO: Summary
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
                    0,
                    entry.LineChangeIndexes);

                return [new ValidationFeedbackItem(
                    entry,
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.UnmatchingValueType,
                        feedbackIndexes.Key,
                        0,
                        $"{entry.Key.Keyword}: {entry.ValueType}"))
                    ];
            }

            return null;
        }

        // TODO: Summary
        public static ValidationFeedbackItem[]? ValidateValueContents(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            if (entry.Key.Keyword == syntaxConf.Tokens.KeyWords.Charset)
            {
                string[] allowedCharsets = ["ANSI", "Unicode"];
                if (!allowedCharsets.Contains(entry.Value))
                {
                    KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                        entry.KeyStartLineIndex,
                        0,
                        entry.LineChangeIndexes);

                    return [new ValidationFeedbackItem(
                        entry,
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.InvalidValueFound,
                            feedbackIndexes.Key,
                            0,
                            $"{entry.Key.Keyword}: {entry.Value}"))
                        ];
                }
            }
            else if (entry.Key.Keyword == syntaxConf.Tokens.KeyWords.CodePage &&
                !entry.Value.Equals(info.Encoding.BodyName, StringComparison.CurrentCultureIgnoreCase))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);

                return [new ValidationFeedbackItem(
                    entry,
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.InvalidValueFound,
                        feedbackIndexes.Key,
                        0,
                        $"{entry.Key.Keyword}: {entry.Value}"))
                    ];
            }
            else if (entry.Key.Keyword == syntaxConf.Tokens.KeyWords.DimensionType)
            {
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

                if (!dimensionTypes.Contains(entry.Value))
                {
                    KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);

                    return [new ValidationFeedbackItem(
                        entry,
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.InvalidValueFound,
                            feedbackIndexes.Key,
                            0,
                            $"{entry.Key.Keyword}: {entry.Value}"))
                        ];
                }
            }
            else if (entry.Key.Keyword == syntaxConf.Tokens.KeyWords.ContentVariableIdentifier)
            {
                string defaultLanguage = info.DefaultLanguage is not null ? info.DefaultLanguage : string.Empty;
                string lang = entry.Key.Language is not null ? entry.Key.Language : defaultLanguage;
                if (info.StubDimensions is not null && !Array.Exists(info.StubDimensions[lang], d => d == entry.Value) &&
                (info.HeadingDimensions is not null && !Array.Exists(info.HeadingDimensions[lang], d => d == entry.Value)))
                {
                    KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);

                    return [new ValidationFeedbackItem(
                        entry,
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.InvalidValueFound,
                            feedbackIndexes.Key,
                            0,
                            $"{entry.Key.Keyword}: {entry.Value}"))
                        ];
                }
            }

            return null;
        }

        // TODO: Summary
        public static ValidationFeedbackItem[]? ValidateValueAmounts(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            if (entry.Key.Keyword != syntaxConf.Tokens.KeyWords.VariableValueCodes ||
                info.DimensionValues is null ||
                entry.Key.FirstSpecifier is null)
            {
                return null;
            }

            string[] codes = entry.Value.Split(syntaxConf.Symbols.Value.ListSeparator);
            string defaultLanguage = info.DefaultLanguage is not null ? info.DefaultLanguage : string.Empty;
            string lang = entry.Key.Language is not null ? entry.Key.Language : defaultLanguage;
            if (codes.Length != info.DimensionValues[new(lang, entry.Key.FirstSpecifier)].Length)
            {
                return [
                    new ValidationFeedbackItem(
                        entry,
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.UnmatchingValueAmount,
                            entry.KeyStartLineIndex,
                            0,
                            $"{entry.Key.Keyword}: {entry.Value}"))
                    ];
            }

            return null;
        }

        // TODO: Summary
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
                KeyValuePair<int, int> feebackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);
                
                return [new ValidationFeedbackItem(
                    entry,
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Warning,
                        ValidationFeedbackRule.LowerCaseValueFound,
                        feebackIndexes.Key,
                        0,
                        $"{entry.Key.Keyword}: {entry.Value}"))];
            }
            return null;
        }
    }
}
