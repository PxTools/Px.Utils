using PxUtils.PxFile;
using PxUtils.Validation.SyntaxValidation;

namespace PxUtils.Validation.ContentValidation
{
    // TODO: Add summary
    public static class ContentValidation
    {
        private static string filename = string.Empty;

        // TODO: Add summary
        public static void ValidatePxFileContent(string _filename, ValidationStructuredEntry[] entries, PxFileSyntaxConf syntaxConf, ref List<ValidationFeedbackItem> feedbackItems)
        {
            filename = _filename;
            string? defaultLanguage = FindDefaultLanguage(entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.DefaultLanguage, StringComparison.CurrentCultureIgnoreCase)).ToArray(), ref feedbackItems);
            string[]? languages = FindLanguages(
                entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.AvailableLanguages,
                StringComparison.CurrentCultureIgnoreCase)).ToArray(),
                ref feedbackItems, 
                syntaxConf.Symbols.Value.ListSeparator);

            if (!languages.Contains(defaultLanguage))
            {
                feedbackItems.Add(new ValidationFeedbackItem(
                new ContentValidationObject(filename, 0, []),
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.UndefinedLanguageFound,
                        0,
                        0,
                        defaultLanguage
                        )));
            }

            ValidationStructuredEntry[]? contentVariableEntries = entries.Where(e => e.Key.Keyword.Equals(syntaxConf.Tokens.KeyWords.ContentVariableIdentifier, StringComparison.CurrentCultureIgnoreCase)).ToArray();
            
            // Find required keys
                // Single language
                // Multiple languages
                    // Check that all languages and only those languages are present

            // Find stub
            // Find heading
            // Ensure one of either exists for all languages

            // Find recommended keys
                // Single language
                // Multiple languages
                    // Check that all languages and only those languages are present

            // Find recommended keys
                // Single language

            // Multiple languages entries
                // Check that all languages and only those languages are present

            // Check for illegal language definitions
            // Check for unrecommended language definitions

            // Check for missing specification definitions
            // Check for missing recommended specification definitions

            // Check for illegal specifier definitions

            // Check for illegal specifier definition formats?
            // Check for unrecommended specifier definition formats

            // Check for invalid value types
            // Check for unmatching value amounts
            // Check for invalid values
            // Check for uppercase recommendations

        }

        private static string? FindDefaultLanguage(ValidationStructuredEntry[] entries, ref List<ValidationFeedbackItem> feedbackItems)
        {
            if (entries.Length > 1)
            {
                feedbackItems.Add(new ValidationFeedbackItem(
                    entries[0],
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MultipleInstancesOfUniqueKey,
                        entries[0].KeyStartLineIndex,
                        0,
                        string.Join(", ", entries.Select(e => e.Value).ToArray())
                        )));
            }
            else if (entries.Length == 0)
            {
                feedbackItems.Add(new ValidationFeedbackItem(
                    new ContentValidationObject(filename, 0, []),
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MissingDefaultLanguage,
                        0,
                        0,
                        string.Join(", ", entries.Select(e => e.Value).ToArray())
                        )));
            }

            return entries[0].Value;
        }
        private static string[]? FindLanguages(ValidationStructuredEntry[] entries, ref List<ValidationFeedbackItem> feedbackItems, char listSeparator)
        {
            if (entries.Length > 1)
            {
                feedbackItems.Add(new ValidationFeedbackItem(
                    entries[0],
                    new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.MultipleInstancesOfUniqueKey,
                        entries[0].KeyStartLineIndex,
                        0,
                        string.Join(", ", entries.Select(e => e.Value).ToArray())
                        )));
            }

            return entries[0].Value.Split(listSeparator);
        }
    }
}
