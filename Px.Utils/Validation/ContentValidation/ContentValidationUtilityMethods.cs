using PxUtils.PxFile;
using PxUtils.Validation.SyntaxValidation;

namespace PxUtils.Validation.ContentValidation
{
    internal static class ContentValidationUtilityMethods
    {
        internal static Dictionary<KeyValuePair<string, string>, string[]> FindVariableValues(
            ValidationStructuredEntry[] entries,
            PxFileSyntaxConf syntaxConf,
            Dictionary<string, string[]>? variables,
            ref List<ValidationFeedbackItem> feedbackItems,
            string filename)
        {
            if (variables is null)
            {
                return [];
            }

            Dictionary<KeyValuePair<string, string>, string[]> variableValues = [];

            foreach (string language in variables.Keys)
            {
                foreach (string dimension in variables[language])
                {
                    ValidationStructuredEntry? valuesEntry = Array.Find(entries,
                        e => e.Key.FirstSpecifier is not null &&
                        e.Key.FirstSpecifier.Equals(dimension));
                    if (valuesEntry is not null)
                    {
                        variableValues.Add(
                            new KeyValuePair<string, string> ( language, dimension ),
                            valuesEntry.Value.Split(syntaxConf.Symbols.Value.ListSeparator)
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

        internal static ValidationFeedbackItem? FindContentVariableKey(ValidationStructuredEntry[] entries, string keyword, KeyValuePair<string, string> kvp, string defaultLanguage, string filename)
        {
            ValidationStructuredEntry? entry = Array.Find(entries,
                            e => e.Key.Keyword.Equals(keyword) &&
                            (e.Key.Language == kvp.Key || (kvp.Key == defaultLanguage && e.Key.Language is null)) &&
                            (e.Key.FirstSpecifier == kvp.Value || e.Key.FirstSpecifier is null));

            if (entry is null)
            {
                return
                    new ValidationFeedbackItem(
                        new ContentValidationObject(filename, 0, []),
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.RequiredKeyMissing,
                            0,
                            0,
                            $"{keyword}, {kvp.Key}, {kvp.Value}"
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
                            $"{keyword}, {kvp.Key}, {kvp.Value}"
                            )
                        );
            }

            return null;
        }

        internal static ValidationFeedbackItem? FindDimensionRecommendedKey(ValidationStructuredEntry[] entries, string keyword, string language, string defaultLanguage, string filename, string? dimensionName = null)
        {
            ValidationStructuredEntry[] keywordEntries = entries.Where(e => e.Key.Keyword.Equals(keyword)).ToArray();
            ValidationStructuredEntry? entry = Array.Find(keywordEntries,
                e => ((language == defaultLanguage && e.Key.Language is null) || language == e.Key.Language) &&
                (e.Key.FirstSpecifier == dimensionName || dimensionName is null));

            if (entry is null)
            {
                return new ValidationFeedbackItem(
                        new ContentValidationObject(filename, 0, []),
                        new ValidationFeedback(
                            ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.RecommendedKeyMissing,
                            0,
                            0,
                            $"{language}: {keyword}")
                        );
            }

            return null;
        }
    }
}
