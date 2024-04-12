using PxUtils.PxFile;
using PxUtils.Validation.SyntaxValidation;
using System.Collections.Generic;
using System.Text;

namespace PxUtils.Validation.ContentValidation
{
    /// <summary>
    /// Provides methods for validating the content of Px file metadata
    /// <param name="_filename">Name of the Px file</param>
    /// <param name="_encoding"> _encoding of the Px file</param>
    /// <param name="_syntaxConf">Object that stores syntax specific symbols and tokens for the Px file</param>
    /// </summary>
    public sealed partial class ContentValidator(string _filename, Encoding _encoding, PxFileSyntaxConf? _syntaxConf = null)
    {
        private readonly string filename = _filename;
        private readonly Encoding encoding = _encoding;
        /// <summary>
        /// Default language of the Px file defined with LANGUAGE keyword by default
        /// </summary>
        private string? defaultLanguage;
        /// <summary>
        /// Array of supported languages in the Px file. Defined with LANGUAGES keyword by default
        /// </summary>
        private string[]? availableLanguages;
        /// <summary>
        /// Dictionary that define content dimension names for available languages. Key represents the language, while the value is the dimension name.
        /// </summary>
        private Dictionary<string, string>? contentDimensionNames;
        /// <summary>
        /// Dictionary that define stub dimension names for available languages. Key represents the language, while the value is the dimension name.
        /// </summary>
        private Dictionary<string, string[]>? stubDimensionNames;
        /// <summary>
        /// Dictionary that define heading dimension names for available languages. Key represents the language, while the value is the dimension name.
        /// </summary>
        private Dictionary<string, string[]>? headingDimensionNames;
        /// <summary>
        /// Dictionary of key value pairs that define names for dimension values. Key is a key value pair of language and dimension name, while the value is an array of dimension value names.
        /// </summary>
        private Dictionary<KeyValuePair<string, string>, string[]>? dimensionValueNames;
        /// <summary>
        /// Object that stores syntax specific symbols and tokens for the Px file
        /// </summary>
        private PxFileSyntaxConf SyntaxConf { get; } = _syntaxConf ?? PxFileSyntaxConf.Default;

        /// <summary>
        /// Validates contents of Px file metadata. Metadata syntax must be valid for this method to work properly.
        /// </summary>
        /// <param name="entries">Array of <see cref="ValidationStructuredEntry"/> objects that represent entries of the Px file metadata</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that defines keywords and symbols for Px file syntax</param>
        /// <param name="customContentValidationFunctions"><see cref="ContentValidationFunctions"/> object that contains any optional additional validation functions</param>
        public ValidationFeedbackItem[] Validate(
            ValidationStructuredEntry[] entries,
            PxFileSyntaxConf syntaxConf,
            CustomContentValidationFunctions? customContentValidationFunctions = null
            )
        {

            IEnumerable<ContentValidationEntryDelegate> contentValidationEntryFunctions = DefaultContentValidationEntryFunctions;
            IEnumerable<ContentValidationFindKeywordDelegate> contentValidationFindKeywordFunctions = DefaultContentValidationFindKeywordFunctions;

            if (customContentValidationFunctions is not null)
            {
                contentValidationEntryFunctions = contentValidationEntryFunctions.Concat(customContentValidationFunctions.CustomContentValidationEntryFunctions);
                contentValidationFindKeywordFunctions = contentValidationFindKeywordFunctions.Concat(customContentValidationFunctions.CustomContentValidationFindKeywordFunctions);
            }

            List <ValidationFeedbackItem> feedbackItems = [];

            foreach (ContentValidationFindKeywordDelegate findingFunction in contentValidationFindKeywordFunctions)
            {
                ValidationFeedbackItem[]? feedback = findingFunction(entries, this);
                if (feedback is not null)
                {
                    feedbackItems.AddRange(feedback);
                }
            }
            foreach (ContentValidationEntryDelegate entryFunction in contentValidationEntryFunctions)
            {
                foreach (ValidationStructuredEntry entry in entries)
                {
                    ValidationFeedbackItem[]? feedback = entryFunction(entry, this);
                    if (feedback is not null)
                    {
                        feedbackItems.AddRange(feedback);
                    }
                }
            }

            ResetFields();

            return [.. feedbackItems];
        }

        /// <summary>
        /// Validates contents of Px file metadata asynchronously. Metadata syntax must be valid for this method to work properly.
        /// </summary>
        /// <param name="entries">Array of <see cref="ValidationStructuredEntry"/> objects that represent entries of the Px file metadata</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> object that defines keywords and symbols for Px file syntax</param>
        /// <param name="customContentValidationFunctions"><see cref="ContentValidationFunctions"/> object that contains any optional additional validation functions</param>
        /// <param name="cancellationToken">Cancellation token for cancelling the validation process</param>
        /// <returns>Array of <see cref="ValidationFeedbackItem"/> objects. Any issues found during validation will be listed here</returns>
        public async Task<ValidationFeedbackItem[]> ValidateAsync(
            ValidationStructuredEntry[] entries,
            PxFileSyntaxConf syntaxConf,
            CustomContentValidationFunctions? customContentValidationFunctions = null,
            CancellationToken cancellationToken = default
            )
        {
            IEnumerable<ContentValidationEntryDelegate> contentValidationEntryFunctions = DefaultContentValidationEntryFunctions;
            IEnumerable<ContentValidationFindKeywordDelegate> contentValidationFindKeywordFunctions = DefaultContentValidationFindKeywordFunctions;

            if (customContentValidationFunctions is not null)
            {
                contentValidationEntryFunctions = [.. contentValidationEntryFunctions, .. customContentValidationFunctions.CustomContentValidationEntryFunctions];
                contentValidationFindKeywordFunctions = [.. contentValidationFindKeywordFunctions, .. customContentValidationFunctions.CustomContentValidationFindKeywordFunctions];
            }

            List<ValidationFeedbackItem> feedbackItems = [];

            // Some "find keyword" type content validation functions are dependent on the results of other content validation functions
            foreach (var findFunction in contentValidationFindKeywordFunctions)
            {
                ValidationFeedbackItem[]? feedback = await Task.Run(() => findFunction(entries, this));
                if (feedback is not null)
                {
                    feedbackItems.AddRange(feedback);
                }
            }

            // Entry tasks can be run asynchronously without worrying about dependencies
            IEnumerable<Task<ValidationFeedbackItem[]?>> entryTasks = contentValidationEntryFunctions
                .SelectMany(entryFunction => entries
                    .Select(entry => Task.Run(() => entryFunction(entry, this))));

            await Task.WhenAll(entryTasks);

            entryTasks.Select(task => task.Result).ToList().ForEach(feedback =>
                feedbackItems.AddRange(feedback is not null ? feedback : []));

            ResetFields();

            return [.. feedbackItems];
        }

        private void ResetFields()
        {
            defaultLanguage = null;
            availableLanguages = null;
            contentDimensionNames = null;
            stubDimensionNames = null;
            headingDimensionNames = null;
            dimensionValueNames = null;
        }
    }
}
