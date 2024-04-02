using PxUtils.PxFile;
using PxUtils.Validation.SyntaxValidation;
using System.Data.SqlTypes;

namespace PxUtils.Validation.ContentValidation
{
    public delegate ValidationFeedbackItem[]? ContentValidationEntryFunctionDelegate(ValidationStructuredEntry entry, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info);
    public delegate ValidationFeedbackItem[]? ContentValidationSearchFunctionDelegate(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info); 
    internal class ContentValidationFunctions
    {
        public List<ContentValidationEntryFunctionDelegate> DefaultContentValidationEntryFunctions { get; }
        public List<ContentValidationSearchFunctionDelegate> DefaultContentValidationSearchFunctions { get; }

        // TODO: Summmary
        public ContentValidationFunctions()
        {
            DefaultContentValidationEntryFunctions = [
            ];
            DefaultContentValidationSearchFunctions = [
                ValidateFindDefaultLanguage,
                ValidateFindAvailableLanguages,
                ValidateDefaultLanguageDefinedInAvailableLanguages,
                ValidateFindContentDimension,
                ValidateFindRequiredCommonKeys,
                ValidateFindStubOrHeading,
                ValidateFindRecommendedCommonKeys,
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
        public ValidationFeedbackItem[]? ValidateFindDefaultLanguage(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            ValidationStructuredEntry[] langEntries = entries.Where(
                e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.DefaultLanguage)).ToArray();

            if (langEntries.Length > 1)
            {
                return [
                    new ValidationFeedbackItem(
                    entries[0],
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MultipleInstancesOfUniqueKey,
                        entries[0].KeyStartLineIndex,
                        0,
                        string.Join(", ", entries.Select(e => e.Value).ToArray())
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
                        0,
                        string.Join(", ", entries.Select(e => e.Value).ToArray())
                        )
                    ),
                ];
            }

            info.DefaultLanguage = langEntries[0].Value;
            return null;
        }

        // TODO: Summary
        public ValidationFeedbackItem[]? ValidateFindAvailableLanguages(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            ValidationStructuredEntry[] availableLanguageEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.AvailableLanguages)).ToArray();

            if (availableLanguageEntries.Length > 1)
            {
                return [
                    new ValidationFeedbackItem(
                    entries[0],
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MultipleInstancesOfUniqueKey,
                        entries[0].KeyStartLineIndex,
                        0,
                        string.Join(", ", entries.Select(e => e.Value).ToArray())
                        )
                    )
               ];
            }

            if (availableLanguageEntries.Length == 1)
            {
                info.AvailableLanguages = availableLanguageEntries[0].Value.Split(syntaxConf.Symbols.Value.ListSeparator);    
            }

            return null;
        }

        // TODO: Summary
        public ValidationFeedbackItem[]? ValidateDefaultLanguageDefinedInAvailableLanguages(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            if (info.AvailableLanguages is not null && !info.AvailableLanguages.Contains(info.DefaultLanguage))
            {
                ValidationStructuredEntry defaultLanguageEntry = entries.First(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.DefaultLanguage));
                
                return [
                    new ValidationFeedbackItem(
                    defaultLanguageEntry,
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.UndefinedLanguageFound,
                        defaultLanguageEntry.KeyStartLineIndex,
                        0,
                        string.Join(", ", info.AvailableLanguages)
                        )
                    )
                ];
            }

            return null;
        }

        // TODO: Summary
        public ValidationFeedbackItem[]? ValidateFindContentDimension(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            ValidationStructuredEntry[] contentDimensionEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.ContentVariableIdentifier)).ToArray();
            if (contentDimensionEntries.Length == 0)
            {
                return [
                    new ValidationFeedbackItem(
                    new ContentValidationObject(info.Filename, 0, []),
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Warning,
                        ValidationFeedbackRule.RecommendedKeyMissing,
                        0,
                        0
                        )
                    ),
                ];
            }

            string defaultLanguage = info.DefaultLanguage is not null ? info.DefaultLanguage : string.Empty;

            info.ContentDimensionEntry = contentDimensionEntries.ToDictionary(
                e => e.Key.Language is not null ? 
                e.Key.Language : 
                defaultLanguage, 
                e => e.Value);

            return null;
        }

        // TODO: Summary
        public ValidationFeedbackItem[]? ValidateFindRequiredCommonKeys(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            string[] alwaysRequiredKeywords =
            [
                syntaxConf.Tokens.KeyWords.Charset,
                syntaxConf.Tokens.KeyWords.CodePage,
                syntaxConf.Tokens.KeyWords.DefaultLanguage,
                syntaxConf.Tokens.KeyWords.Data
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
        public ValidationFeedbackItem[]? ValidateFindStubOrHeading(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            ValidationStructuredEntry[] stubEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.StubDimensions)).ToArray();
            ValidationStructuredEntry[] headingEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.HeadingDimensions)).ToArray();

            // TODO: Check that exists for every language
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

            return null;
        }

        // TODO: Summary
        public ValidationFeedbackItem[]? ValidateFindRecommendedCommonKeys(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            string[] recommendedKeys =
            [
                syntaxConf.Tokens.KeyWords.TableId
            ];

            foreach (string keyword in recommendedKeys)
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

            return [.. feedbackItems];
        }

        // TODO: Summary
        public ValidationFeedbackItem[]? ValidateFindDimensionValues(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
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

            info.DimensionValues = (Dictionary<KeyValuePair<string, string>, string[]>?)dimensionValues;

            return [.. feedbackItems];
        }

        // TODO: Summary
        public ValidationFeedbackItem[]? ValidateFindContentDimensionKeys(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            if (info.ContentDimensionEntry is null || info.DimensionValues is null)
            {
                return null;
            }

            List<ValidationFeedbackItem> feedbackItems = [];

            ValidationStructuredEntry[] unitsEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.Units)).ToArray();
            ValidationStructuredEntry[] lastUpdatedEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.LastUpdated)).ToArray();
            ValidationStructuredEntry[] precisionEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.Precision)).ToArray();
            string[] contentDimensionNames = [.. info.ContentDimensionEntry.Values];

            Dictionary<KeyValuePair<string, string>, string[]> contentDimensionValueNames = (Dictionary<KeyValuePair<string, string>, string[]>)info.DimensionValues.Where(
                e => contentDimensionNames.Contains(e.Key.Value));

            string defaultLanguage = info.DefaultLanguage is not null ? info.DefaultLanguage : string.Empty;

            if (contentDimensionNames.Length > 0)
            {
                foreach (KeyValuePair<string, string> kvp in contentDimensionValueNames.Keys)
                {
                    var unitEntry = Array.Find(unitsEntries,
                        e => (e.Key.Language == kvp.Key || (kvp.Key == defaultLanguage && e.Key.Language is null)) &&
                        (e.Key.FirstSpecifier == kvp.Value ||
                        e.Key.SecondSpecifier == kvp.Value)
                        );

                    if (unitEntry is null)
                    {
                        feedbackItems.Add(
                            new ValidationFeedbackItem(
                                new ContentValidationObject(info.Filename, 0, []),
                                new ValidationFeedback(
                                    ValidationFeedbackLevel.Error,
                                    ValidationFeedbackRule.RequiredKeyMissing,
                                    0,
                                    0,
                                    $"{kvp.Key}, {kvp.Value}"
                                    )
                                )
                            );
                    }
                    else if (unitEntry.Key.SecondSpecifier is null)
                    {
                        feedbackItems.Add(
                            new ValidationFeedbackItem(
                                unitEntry,
                                new ValidationFeedback(
                                    ValidationFeedbackLevel.Warning,
                                    ValidationFeedbackRule.UnrecommendedSpecifierDefinitionFound,
                                    unitEntry.KeyStartLineIndex,
                                    0,
                                    $"{kvp.Key}, {kvp.Value}"
                                    )
                                ));
                    }

                    if (kvp.Key == defaultLanguage)
                    {
                        ValidationStructuredEntry? lastUpdatedEntry = Array.Find(lastUpdatedEntries,
                            e => e.Key.Language is null &&
                            (e.Key.FirstSpecifier == kvp.Value ||
                            e.Key.SecondSpecifier == kvp.Value)
                            );

                        if (lastUpdatedEntry is null)
                        {
                            feedbackItems.Add(
                                new ValidationFeedbackItem(
                                    new ContentValidationObject(info.Filename, 0, []),
                                    new ValidationFeedback(
                                        ValidationFeedbackLevel.Error,
                                        ValidationFeedbackRule.RequiredKeyMissing,
                                        0,
                                        0,
                                        $"{kvp.Key}, {kvp.Value}: {syntaxConf.Tokens.KeyWords.LastUpdated}"
                                        )
                                    )
                                );
                        }

                        ValidationStructuredEntry? precisionEntry = Array.Find(precisionEntries,
                            e => e.Key.Language is null &&
                            (e.Key.FirstSpecifier == kvp.Value ||
                            e.Key.SecondSpecifier == kvp.Value)
                            );

                        if (precisionEntry is null)
                        {
                            feedbackItems.Add(
                                    new ValidationFeedbackItem(
                                        new ContentValidationObject(info.Filename, 0, []),
                                        new ValidationFeedback(
                                            ValidationFeedbackLevel.Error,
                                            ValidationFeedbackRule.RequiredKeyMissing,
                                            0,
                                            0,
                                            $"{kvp.Key}, {kvp.Value}: {syntaxConf.Tokens.KeyWords.Precision}"
                                            )
                                        )
                                    );
                        }
                    }
                }
            }

            ValidationStructuredEntry[] lastUpdatedEntriesWithLanguage = lastUpdatedEntries.Where(e => e.Key.Language is not null).ToArray();
            ValidationStructuredEntry[] precisionEntriesWithLanguage = precisionEntries.Where(e => e.Key.Language is not null).ToArray();

            foreach(ValidationStructuredEntry entry in lastUpdatedEntriesWithLanguage)
            {
                feedbackItems.Add(
                    new ValidationFeedbackItem(
                        entry,
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.UnrecommendedLanguageDefinitionFound,
                            entry.KeyStartLineIndex,
                            0,
                            entry.Key.Language
                            )
                        )
                    );
            }

            foreach(ValidationStructuredEntry entry in precisionEntriesWithLanguage)
            {
                feedbackItems.Add(
                    new ValidationFeedbackItem(
                        entry,
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.UnrecommendedLanguageDefinitionFound,
                            entry.KeyStartLineIndex,
                            0,
                            entry.Key.Language
                            )
                        )
                    );
            }

            return [.. feedbackItems];
        }

        // TODO: Summary
        public ValidationFeedbackItem[]? ValidateFindDimensionRecommendedKeys(ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref ContentValidationInfo info)
        {
            if ((info.StubDimensions is null && info.HeadingDimensions is null) || info.AvailableLanguages is null)
            {
                return null;
            }

            List<ValidationFeedbackItem> feedbackItems = [];

            ValidationStructuredEntry[] dimensionCodeEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.DimensionCode)).ToArray();
            ValidationStructuredEntry[] valueCodeEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.VariableValueCodes)).ToArray();
            ValidationStructuredEntry[] dimensionTypeEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.DimensionType)).ToArray();
            ValidationStructuredEntry[] timevalEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.TimeVal)).ToArray();

            Dictionary<string, string[]> stubDimensions = info.StubDimensions is not null ? info.StubDimensions : [];
            Dictionary<string, string[]> headingDimensions = info.HeadingDimensions is not null ? info.HeadingDimensions : [];
            Dictionary<string, string[]> dimensions = (Dictionary<string, string[]>)stubDimensions.Concat(headingDimensions);

            string defaultLanguage = info.DefaultLanguage is not null ? info.DefaultLanguage : string.Empty;

            foreach(string language in info.AvailableLanguages)
            {
                ValidationStructuredEntry? timevalEntry = Array.Find(timevalEntries,
                    e => (language == defaultLanguage && e.Key.Language is null) || language == e.Key.Language
                );

                if (timevalEntry is null)
                {
                    feedbackItems.Add(
                        new ValidationFeedbackItem(
                            new ContentValidationObject(info.Filename, 0, []),
                            new ValidationFeedback(
                                ValidationFeedbackLevel.Warning,
                                ValidationFeedbackRule.RecommendedKeyMissing,
                                0,
                                0,
                                $"{language}: {syntaxConf.Tokens.KeyWords.TimeVal}")
                            )
                        );
                }

                foreach(var dimension in dimensions.Where(d => d.Key == language))
                {
                    ValidationStructuredEntry? dimensionCodeEntry = Array.Find(dimensionCodeEntries,
                        e => (language == defaultLanguage && e.Key.Language is null) || language == e.Key.Language
                    );

                    if (dimensionCodeEntry is null)
                    {
                        feedbackItems.Add(
                            new ValidationFeedbackItem(
                                new ContentValidationObject(info.Filename, 0, []),
                                new ValidationFeedback(
                                    ValidationFeedbackLevel.Warning,
                                    ValidationFeedbackRule.RecommendedKeyMissing,
                                    0,
                                    0,
                                    $"{language}: {syntaxConf.Tokens.KeyWords.TimeVal}")
                                )
                            );
                    }

                    ValidationStructuredEntry? valueCodeEntry = Array.Find(valueCodeEntries,
                        e => (language == defaultLanguage && e.Key.Language is null) || language == e.Key.Language
                    );

                    if (valueCodeEntry is null)
                    {
                        feedbackItems.Add(
                            new ValidationFeedbackItem(
                                new ContentValidationObject(info.Filename, 0, []),
                                new ValidationFeedback(
                                    ValidationFeedbackLevel.Warning,
                                    ValidationFeedbackRule.RecommendedKeyMissing,
                                    0,
                                    0,
                                    $"{language}: {syntaxConf.Tokens.KeyWords.TimeVal}")
                                )
                            );
                    }

                    ValidationStructuredEntry? dimensionTypeEntry = Array.Find(dimensionTypeEntries,
                        e => (language == defaultLanguage && e.Key.Language is null) || language == e.Key.Language
                    );

                    if (dimensionTypeEntry is null)
                    {
                        feedbackItems.Add(
                            new ValidationFeedbackItem(
                                new ContentValidationObject(info.Filename, 0, []),
                                new ValidationFeedback(
                                    ValidationFeedbackLevel.Warning,
                                    ValidationFeedbackRule.RecommendedKeyMissing,
                                    0,
                                    0,
                                    $"{language}: {syntaxConf.Tokens.KeyWords.TimeVal}")
                                )
                            );
                    }
                }
            }

            return [.. feedbackItems];
        }

        // Check that entries don't have illegal or unrecommended specifiers
        // just language specific keys?
        // just dimension specific keys?
        // value type checks
        // value content checks
        // value amount check
        // value uppercase recommendation check
    }
}
