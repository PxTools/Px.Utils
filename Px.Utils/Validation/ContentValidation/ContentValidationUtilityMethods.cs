﻿using PxUtils.Models.Metadata.Dimensions;
using PxUtils.PxFile;
using PxUtils.Validation.SyntaxValidation;

namespace PxUtils.Validation.ContentValidation
{
    /// <summary>
    /// Contains a collection of utility methods used in content validation.
    /// </summary>
    internal static class ContentValidationUtilityMethods
    {
        /// <summary>
        /// Finds the values for given dimensions in the Px file
        /// </summary>
        /// <param name="entries">The structured entries of the Px file</param>
        /// <param name="syntaxConf">The syntax configuration for the Px file</param>
        /// <param name="dimensions">The dimensions to find values for</param>
        /// <param name="feedbackItems">A list of feedback items to add to</param>
        /// <param name="filename">The name of the Px file</param>
        /// <returns>A dictionary containing the values for the dimensions</returns>
        internal static Dictionary<KeyValuePair<string, string>, string[]> FindDimensionValues(
            ValidationStructuredEntry[] entries,
            PxFileSyntaxConf syntaxConf,
            Dictionary<string, string[]>? dimensions,
            ref List<ValidationFeedbackItem> feedbackItems,
            string filename)
        {
            if (dimensions is null)
            {
                return [];
            }

            Dictionary<KeyValuePair<string, string>, string[]> variableValues = [];

            foreach (string language in dimensions.Keys)
            {
                foreach (string dimension in dimensions[language])
                {
                    ValidationStructuredEntry? valuesEntry = Array.Find(entries,
                        e => e.Key.FirstSpecifier is not null &&
                        e.Key.FirstSpecifier.Equals(dimension));
                    if (valuesEntry is not null)
                    {
                        string[] values = valuesEntry.Value.Split(syntaxConf.Symbols.Value.ListSeparator);
                        for(int i = 0; i < values.Length; i++)
                        {
                            values[i] = SyntaxValidationUtilityMethods.CleanString(values[i], syntaxConf);
                        }

                        variableValues.Add(
                            new KeyValuePair<string, string> ( language, dimension ),
                            values
                            );
                    }
                    else
                    {
                        feedbackItems.Add(new ValidationFeedbackItem(
                            new ContentValidationObject(filename, 0, []),
                            new ValidationFeedback(
                                ValidationFeedbackLevel.Error,
                                ValidationFeedbackRule.VariableValuesMissing,
                                0,
                                0,
                                $"{dimension}, {language}"
                                )
                            ));
                    }
                }
            }
            return variableValues;
        }

        /// <summary>
        /// Finds a specific content variable related entry in the structured entries of a Px file.
        /// </summary>
        /// <param name="entries">Structured entries of the Px file</param>
        /// <param name="keyword">Keyword that the function searches for</param>
        /// <param name="language">Language that the function searches the entry for</param>
        /// <param name="dimensionName">Name of the content dimension</param>
        /// <param name="dimensionValueName">Name of the content dimension value requiring the entry</param>
        /// <param name="defaultLanguage">Default language of the Px file</param>
        /// <param name="filename">Name of the Px file</param>
        /// <returns>Returns a <see cref="ValidationFeedbackItem"/> object with an error if required entry is not found or with a warning if the entry specifiers are defined in an unexpected way</returns>
        internal static ValidationFeedbackItem? FindContentVariableKey(ValidationStructuredEntry[] entries, string keyword, KeyValuePair<string, string> languageAndDimensionPair, string dimensionValueName, ContentValidationInfo info)
        {
            string language = languageAndDimensionPair.Key;
            string dimensionName = languageAndDimensionPair.Value;

            ValidationStructuredEntry? entry = Array.Find(entries,
                            e => e.Key.Keyword.Equals(keyword) &&
                            (e.Key.Language == language || (language == info.DefaultLanguage && e.Key.Language is null)) &&
                            (e.Key.FirstSpecifier == dimensionName || e.Key.FirstSpecifier == dimensionValueName) &&
                            (e.Key.SecondSpecifier == dimensionValueName || e.Key.SecondSpecifier == null));

            if (entry is null)
            {
                return
                    new ValidationFeedbackItem(
                        new ContentValidationObject(info.Filename, 0, []),
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.RequiredKeyMissing,
                            0,
                            0,
                            $"{keyword}, {language}, {dimensionName}, {dimensionValueName}"
                            )
                        );
            }
            else if (entry.Key.FirstSpecifier is null || entry.Key.SecondSpecifier is null)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);

                return 
                    new ValidationFeedbackItem(
                        entry,
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.UnrecommendedSpecifierDefinitionFound,
                            feedbackIndexes.Key,
                            0,
                            $"{keyword}, {language}, {dimensionName}, {dimensionValueName}"
                            )
                        );
            }

            return null;
        }

        /// <summary>
        /// Finds a recommended dimension related entry in the structured entries of a Px file.
        /// </summary>
        /// <param name="entries">Structured entries of the Px file</param>
        /// <param name="keyword">Keyword that the function searches for</param>
        /// <param name="language">Language that the function searches the entry for</param>
        /// <param name="defaultLanguage">Default language of the Px file</param>
        /// <param name="filename">Name of the Px file</param>
        /// <param name="dimensionName">Name of the dimension</param>
        /// <returns>Returns a <see cref="ValidationFeedbackItem"/> object with a warning if the recommended entry is not found</returns>
        internal static ValidationFeedbackItem? FindDimensionRecommendedKey(ValidationStructuredEntry[] entries, string keyword, string language, ContentValidationInfo info, string? dimensionName = null)
        {
            ValidationStructuredEntry[] keywordEntries = entries.Where(e => e.Key.Keyword.Equals(keyword)).ToArray();
            ValidationStructuredEntry? entry = Array.Find(keywordEntries,
                e => ((language == info.DefaultLanguage && e.Key.Language is null) || language == e.Key.Language) &&
                (e.Key.FirstSpecifier == dimensionName || dimensionName is null));

            if (entry is null)
            {
                return new ValidationFeedbackItem(
                        new ContentValidationObject(info.Filename, 0, []),
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.RecommendedKeyMissing,
                            0,
                            0,
                            $"{language}, {keyword}, {dimensionName}")
                        );
            }

            return null;
        }

        /// <summary>
        /// Searches for specific content variable entries by value name, language and keyword
        /// </summary>
        /// <param name="languageSpecificKeywords">Keywords to search for that are language specific</param>
        /// <param name="commonKeywords">Language agnostic keywords</param>
        /// <param name="entries">Structured entries of Px file metadata to be searched from</param>
        /// <param name="info">Object that provides information of the ongoing content validation process</param>
        /// <param name="languageAndDimensionPair">KeyValuePair that contains the processed language as key and the dimension name as value</param>
        /// <param name="valueName">Name of the content dimension value to look entries for</param>
        /// <returns>Returns a list of <see cref="ValidationFeedbackItem"/> objects if required entries are not found</returns>
        internal static List<ValidationFeedbackItem> ProcessContentDimensionValue(
            string[] languageSpecificKeywords,
            string[] commonKeywords,
            ValidationStructuredEntry[] entries,
            ContentValidationInfo info,
            KeyValuePair<string, string> languageAndDimensionPair,
            string valueName)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            foreach (string keyword in languageSpecificKeywords)
            {
                ValidationFeedbackItem? issue = FindContentVariableKey(entries, keyword, languageAndDimensionPair, valueName, info);
                if (issue is not null)
                {
                    feedbackItems.Add((ValidationFeedbackItem)issue);
                }
            }

            if (languageAndDimensionPair.Key == info.DefaultLanguage)
            {
                foreach (string keyword in commonKeywords)
                {
                    ValidationFeedbackItem? issue = FindContentVariableKey(entries, keyword, languageAndDimensionPair, valueName, info);
                    if (issue is not null)
                    {
                        feedbackItems.Add((ValidationFeedbackItem)issue);
                    }
                }
            }

            return feedbackItems;
        }

        /// <summary>
        /// Processes a dimennsion by searching for recommended metadata keywords related to the dimension
        /// </summary>
        /// <param name="keywords">Language specific keywords to look for</param>
        /// <param name="dimensionTypeKeyword">DimensionType keyword, which is language agnostic</param>
        /// <param name="entries">Structured metadata entries to search through</param>
        /// <param name="language">Language assigned to the currently processed dimension</param>
        /// <param name="info">Object that provides information of the ongoing content validation process</param>
        /// <param name="dimensionName">Name of the dimension currently being processed</param>
        /// <returns></returns>
        internal static List<ValidationFeedbackItem> ProcessDimension(
            string[] keywords,
            string dimensionTypeKeyword,
            ValidationStructuredEntry[] entries, 
            string language,
            ContentValidationInfo 
            info, 
            string dimensionName)
        {
            List<ValidationFeedbackItem> feedbackItems = [];
            foreach (string keyword in keywords)
            {
                ValidationFeedbackItem? keywordFeedback = FindDimensionRecommendedKey(entries, keyword, language, info, dimensionName);
                if (keywordFeedback is not null)
                {
                    feedbackItems.Add((ValidationFeedbackItem)keywordFeedback);
                }
            }

            if (language == info.DefaultLanguage)
            {
                ValidationFeedbackItem? variableTypeFeedback = FindDimensionRecommendedKey(entries, dimensionTypeKeyword, language, info, dimensionName);
                if (variableTypeFeedback is not null)
                {
                    feedbackItems.Add((ValidationFeedbackItem)variableTypeFeedback);
                }
            }
            return feedbackItems;
        }
    }
}
