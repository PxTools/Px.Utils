using Px.Utils.PxFile;
using Px.Utils.Validation.SyntaxValidation;

namespace Px.Utils.Validation.ContentValidation
{
    /// <summary>
    /// Contains a collection of utility methods used in content validation.
    /// </summary>
    public sealed partial class ContentValidator
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
        private static Dictionary<KeyValuePair<string, string>, string[]> FindDimensionValues(
            ValidationStructuredEntry[] entries,
            PxFileSyntaxConf syntaxConf,
            Dictionary<string, string[]>? dimensions,
            ref ValidationFeedback feedbackItems,
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
                        e.Key.FirstSpecifier.Equals(dimension, StringComparison.Ordinal));
                    if (valuesEntry is not null)
                    {
                        List<string> values = SyntaxValidationUtilityMethods.GetListItemsFromString(
                            valuesEntry.Value, 
                            syntaxConf.Symbols.Value.ListSeparator, 
                            syntaxConf.Symbols.Value.StringDelimeter);

                        variableValues.Add(
                            new KeyValuePair<string, string> ( language, dimension ),
                            [.. values.Select(v => SyntaxValidationUtilityMethods.CleanString(v, syntaxConf))]
                            );
                    }
                    else
                    {
                        KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback = new(
                            new(ValidationFeedbackLevel.Error,
                                ValidationFeedbackRule.VariableValuesMissing),
                            new(filename, additionalInfo: $"{dimension}, {language}")
                        );

                        feedbackItems.Add(feedback);
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
        /// <param name="languageAndDimensionPair">Contains language and dimension names</param>
        /// <param name="dimensionValueName">Name of the content dimension value requiring the entry</param>
        /// <param name="validator">Object that stores required information about the validation process</param>
        /// <param name="recommended">Optional that indicates if the keyword entry is recommended and should yield a warning if not found</param>
        /// <returns>Returns a key value pair containing information about the validation violation if required or recommended key is not found.</returns>
        private static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? FindContentDimensionKey(
            ValidationStructuredEntry[] entries, 
            string keyword, 
            KeyValuePair<string, string> languageAndDimensionPair, 
            string dimensionValueName,
            ContentValidator validator, 
            bool recommended = false)
        {
            string language = languageAndDimensionPair.Key;
            string dimensionName = languageAndDimensionPair.Value;

            ValidationStructuredEntry? entry = Array.Find(entries,
                            e => e.Key.Keyword.Equals(keyword, StringComparison.Ordinal) &&
                            (e.Key.Language == language || (language == validator._defaultLanguage && e.Key.Language is null)) &&
                            (e.Key.FirstSpecifier == dimensionName || e.Key.FirstSpecifier == dimensionValueName) &&
                            (e.Key.SecondSpecifier == dimensionValueName || e.Key.SecondSpecifier == null));

            if (entry is null)
            {
                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> (
                    new(recommended ? ValidationFeedbackLevel.Warning : ValidationFeedbackLevel.Error,
                    recommended ? ValidationFeedbackRule.RecommendedKeyMissing : ValidationFeedbackRule.RequiredKeyMissing),
                    new(validator._filename, additionalInfo: $"{keyword}, {language}, {dimensionName}, {dimensionValueName}")
                    );
            }
            else if (entry.Key.FirstSpecifier is null || entry.Key.SecondSpecifier is null)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    entry.KeyStartLineIndex,
                    0,
                    entry.LineChangeIndexes);

                return new(
                    new(ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.RecommendedSpecifierDefinitionMissing),
                    new(validator._filename, feedbackIndexes.Key, feedbackIndexes.Value, $"{keyword}, {language}, {dimensionName}, {dimensionValueName}")
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
        /// <param name="validator">Object that stores required information about the validation process</param>
        /// <param name="dimensionName">Name of the dimension</param>
        /// <returns>Returns a key value pair containing information about a validation violation warning if the recommended entry is not found</returns>
        private static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? FindDimensionRecommendedKey(
            ValidationStructuredEntry[] entries, 
            string keyword, string language, 
            ContentValidator validator, 
            string? dimensionName = null)
        {
            ValidationStructuredEntry[] keywordEntries = entries.Where(e => e.Key.Keyword.Equals(keyword, StringComparison.Ordinal)).ToArray();
            ValidationStructuredEntry? entry = Array.Find(keywordEntries,
                e => ((language == validator._defaultLanguage && e.Key.Language is null) || language == e.Key.Language) &&
                (e.Key.FirstSpecifier == dimensionName || dimensionName is null));

            if (entry is null)
            {
                return new(
                    new(ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.RecommendedKeyMissing),
                    new(validator._filename, additionalInfo: $"{keyword}, {language}, {dimensionName}")
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
        /// <param name="validator">Object that provides information of the ongoing content validation process</param>
        /// <param name="languageAndDimensionPair">KeyValuePair that contains the processed language as key and the dimension name as value</param>
        /// <param name="valueName">Name of the content dimension value to look entries for</param>
        /// <returns>Returns a <see cref="ValidationFeedback"/> object containing information of missing required entries</returns>
        private static ValidationFeedback ProcessContentDimensionValue(
            string[] languageSpecificKeywords,
            string[] commonKeywords,
            string[] recommendedKeywords,
            ValidationStructuredEntry[] entries,
            ContentValidator validator,
            KeyValuePair<string, string> languageAndDimensionPair,
            string valueName)
        {
            ValidationFeedback feedbackItems = [];
            foreach (string keyword in languageSpecificKeywords)
            {
                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? issue = FindContentDimensionKey(entries, keyword, languageAndDimensionPair, valueName, validator);
                if (issue is not null)
                {
                    feedbackItems.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)issue);
                }
            }

            if (languageAndDimensionPair.Key == validator._defaultLanguage)
            {
                foreach (string keyword in commonKeywords)
                {
                    KeyValuePair <ValidationFeedbackKey, ValidationFeedbackValue>? issue = FindContentDimensionKey(entries, keyword, languageAndDimensionPair, valueName, validator);
                    if (issue is not null)
                    {
                        feedbackItems.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)issue);
                    }
                }
                foreach (string keyword in recommendedKeywords)
                {
                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? issue = FindContentDimensionKey(entries, keyword, languageAndDimensionPair, valueName, validator, true);
                    if (issue is not null)
                    {
                        feedbackItems.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)issue);
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
        /// <param name="validator">Object that provides information of the ongoing content validation process</param>
        /// <param name="dimensionName">Name of the dimension currently being processed</param>
        /// <returns>Returns a <see cref="ValidationFeedback"/> object containing information of missing dimension entries</returns>
        private static ValidationFeedback ProcessDimension(
            string[] keywords,
            string dimensionTypeKeyword,
            ValidationStructuredEntry[] entries, 
            string language,
            ContentValidator validator,
            string dimensionName)
        {
            ValidationFeedback feedbackItems = [];
            foreach (string keyword in keywords)
            {
                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? keywordFeedback = FindDimensionRecommendedKey(entries, keyword, language, validator, dimensionName);
                if (keywordFeedback is not null)
                {
                    feedbackItems.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)keywordFeedback);
                }
            }

            if (language == validator._defaultLanguage)
            {
                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? variableTypeFeedback = FindDimensionRecommendedKey(entries, dimensionTypeKeyword, language, validator, dimensionName);
                if (variableTypeFeedback is not null)
                {
                    feedbackItems.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)variableTypeFeedback);
                }
            }
            return feedbackItems;
        }

        private static Dictionary<string, string[]> GetDimensionNames(
            ValidationStructuredEntry[] entries,
            string defaultLanguage,
            PxFileSyntaxConf syntaxConf)
        {
            Dictionary<string, string[]> dimensionNames = []; 
            foreach (ValidationStructuredEntry entry in entries)
            {
                string language = entry.Key.Language ?? defaultLanguage;
                List<string> names = SyntaxValidationUtilityMethods.GetListItemsFromString(entry.Value, syntaxConf.Symbols.Value.ListSeparator, syntaxConf.Symbols.Value.StringDelimeter);
                dimensionNames.Add(language, [.. names.Select(n => SyntaxValidationUtilityMethods.CleanString(n, syntaxConf))]);
            }
            return dimensionNames;
        }
    }
}
