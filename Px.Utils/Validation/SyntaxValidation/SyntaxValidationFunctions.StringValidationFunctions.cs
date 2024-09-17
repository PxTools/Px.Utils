using Px.Utils.PxFile;
using System.Text.RegularExpressions;

namespace Px.Utils.Validation.SyntaxValidation
{
    public delegate KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? EntryValidationFunction(ValidationEntry validationObject, PxFileSyntaxConf syntaxConf);

    /// <summary>
    /// Contains the default validation functions for syntax validation string entries.
    /// </summary>
    public partial class SyntaxValidationFunctions
    {
        public List<EntryValidationFunction> DefaultStringValidationFunctions { get; } =
        [
            MultipleEntriesOnLine,
            EntryWithoutValue,
        ];

        /// <summary>
        /// If <see cref="ValidationEntry"/> does not start with a line separator, it is considered as not being on its own line, and a new feedback is returned.
        /// </summary>
        /// <param name="validationEntry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the entry does not start with a line separator, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? MultipleEntriesOnLine (ValidationEntry validationEntry, PxFileSyntaxConf syntaxConf)
        {
            // If the entry does not start with a line separator, it is not on its own line. For the first entry this is not relevant.
            if (
                validationEntry.EntryIndex == 0 || 
                validationEntry.EntryString.StartsWith(CharacterConstants.UnixNewLine, StringComparison.Ordinal) ||
                validationEntry.EntryString.StartsWith(CharacterConstants.WindowsNewLine, StringComparison.Ordinal))
            {
                return null;
            }
            else
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationEntry.KeyStartLineIndex,
                    0,
                    validationEntry.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.MultipleEntriesOnOneLine),
                    new(validationEntry.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value)
                );
            }
        }

        /// <summary>
        /// If there is no <see cref="ValidationEntry"/> value section, a new feedback is returned.
        /// </summary>
        /// <param name="validationEntry">The <see cref="ValidationEntry"/> validationKeyValuePair to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if there is no value section, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? EntryWithoutValue(ValidationEntry validationEntry, PxFileSyntaxConf syntaxConf)
        {
            int[] keywordSeparatorIndeces = SyntaxValidationUtilityMethods.FindKeywordSeparatorIndeces(validationEntry.EntryString, syntaxConf);

            if (keywordSeparatorIndeces.Length == 0)
            {                 
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationEntry.KeyStartLineIndex,
                    validationEntry.EntryString.Length,
                    validationEntry.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error, 
                    ValidationFeedbackRule.EntryWithoutValue),
                    new(validationEntry.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value)
                );
            }
            else if (keywordSeparatorIndeces.Length > 1)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationEntry.KeyStartLineIndex,
                    keywordSeparatorIndeces[1],
                    validationEntry.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.EntryWithMultipleValues),
                    new(validationEntry.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value)
                );
            }
            else
            {
                return null;
            }
        }
    }
}
