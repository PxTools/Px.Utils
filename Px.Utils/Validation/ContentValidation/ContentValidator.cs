using Px.Utils.PxFile;
using Px.Utils.Validation;
using Px.Utils.Validation.SyntaxValidation;
using System.Text;

namespace Px.Utils.Validation.ContentValidation
{
    /// <summary>
    /// Provides methods for validating the content of Px file metadata
    /// <param name="filename">Name of the Px file</param>
    /// <param name="encoding"> encoding of the Px file</param>
    /// <param name="entries">Array of <see cref="ValidationStructuredEntry"/> objects that represent key-value entries of the Px file metadata</param>"
    /// <param name="customContentValidationFunctions">Object that contains any optional additional validation functions</param>
    /// <param name="syntaxConf">Object that stores syntax specific symbols and tokens for the Px file</param>
    /// </summary>
    public sealed partial class ContentValidator(
        string filename,
        Encoding encoding,
        ValidationStructuredEntry[] entries,
        CustomContentValidationFunctions? customContentValidationFunctions = null,
        PxFileSyntaxConf? syntaxConf = null) : IPxFileValidator
    {
        private readonly string _filename = filename;
        private readonly Encoding _encoding = encoding;
        /// <summary>
        /// Default language of the Px file defined with LANGUAGE keyword by default
        /// </summary>
        private string? _defaultLanguage;
        /// <summary>
        /// Array of supported languages in the Px file. Defined with LANGUAGES keyword by default
        /// </summary>
        private string[]? _availableLanguages;
        /// <summary>
        /// Dictionary that define content dimension names for available languages. Key represents the language, while the value is the dimension name.
        /// </summary>
        private Dictionary<string, string>? _contentDimensionNames;
        /// <summary>
        /// Dictionary that define stub dimension names for available languages. Key represents the language, while the value is the dimension name.
        /// </summary>
        private Dictionary<string, string[]>? _stubDimensionNames;
        /// <summary>
        /// Dictionary that define heading dimension names for available languages. Key represents the language, while the value is the dimension name.
        /// </summary>
        private Dictionary<string, string[]>? _headingDimensionNames;
        /// <summary>
        /// Dictionary of key value pairs that define names for dimension values. Key is a key value pair of language and dimension name, while the value is an array of dimension value names.
        /// </summary>
        private Dictionary<KeyValuePair<string, string>, string[]>? _dimensionValueNames;
        /// <summary>
        /// Object that stores syntax specific symbols and tokens for the Px file
        /// </summary>
        private PxFileSyntaxConf SyntaxConf { get; } = syntaxConf ?? PxFileSyntaxConf.Default;

        /// <summary>
        /// Validates contents of Px file metadata. Metadata syntax must be valid for this method to work properly.
        /// </summary>
        /// <returns><see cref="ContentValidationResult"/> object that contains the feedback gathered during the validation process.</returns>
        public ContentValidationResult Validate()
        {

            IEnumerable<ContentValidationEntryValidator> contentValidationEntryFunctions = DefaultContentValidationEntryFunctions;
            IEnumerable<ContentValidationFindKeywordValidator> contentValidationFindKeywordFunctions = DefaultContentValidationFindKeywordFunctions;

            if (customContentValidationFunctions is not null)
            {
                contentValidationEntryFunctions = contentValidationEntryFunctions.Concat(customContentValidationFunctions.CustomContentValidationEntryFunctions);
                contentValidationFindKeywordFunctions = contentValidationFindKeywordFunctions.Concat(customContentValidationFunctions.CustomContentValidationFindKeywordFunctions);
            }

            List <ValidationFeedbackItem> feedbackItems = [];

            foreach (ContentValidationFindKeywordValidator findingFunction in contentValidationFindKeywordFunctions)
            {
                ValidationFeedbackItem[]? feedback = findingFunction(entries, this);
                if (feedback is not null)
                {
                    feedbackItems.AddRange(feedback);
                }
            }
            foreach (ContentValidationEntryValidator entryFunction in contentValidationEntryFunctions)
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

            int lengthOfDataRows = _headingDimensionNames is not null ? GetProductOfDimensionValues(_headingDimensionNames) : 0;
            int amountOfDataRows = _stubDimensionNames is not null ? GetProductOfDimensionValues(_stubDimensionNames) : 0;

            ResetFields();

            return new ContentValidationResult([.. feedbackItems], lengthOfDataRows, amountOfDataRows);
        }

        /// <summary>
        /// Validates contents of Px file metadata asynchronously. Metadata syntax must be valid for this method to work properly.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for cancelling the validation process</param>
        /// <returns><see cref="ContentValidationResult"/> object that contains the feedback gathered during the validation process.</returns>
        public async Task<ContentValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<ContentValidationEntryValidator> contentValidationEntryFunctions = DefaultContentValidationEntryFunctions;
            IEnumerable<ContentValidationFindKeywordValidator> contentValidationFindKeywordFunctions = DefaultContentValidationFindKeywordFunctions;

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

            int lengthOfDataRows = _headingDimensionNames is not null ? GetProductOfDimensionValues(_headingDimensionNames) : 0;
            int amountOfDataRows = _stubDimensionNames is not null ? GetProductOfDimensionValues(_stubDimensionNames) : 0;

            ResetFields();

            return new ContentValidationResult([.. feedbackItems], lengthOfDataRows, amountOfDataRows);
        }

        #region Interface implementation

        IValidationResult IPxFileValidator.Validate()
            => Validate();

        async Task<IValidationResult> IPxFileValidator.ValidateAsync(CancellationToken cancellationToken) 
            => await ValidateAsync(cancellationToken);

        #endregion

        private int GetProductOfDimensionValues(Dictionary<string, string[]> dimensions)
        {
            string? lang = _defaultLanguage ?? _availableLanguages?[0];
            if (lang is null)
            {
                return 0;
            }
            string[] headingDimensionNames = dimensions[lang];
            if (headingDimensionNames is null || headingDimensionNames.Length == 0 || _dimensionValueNames is null)
            {
                return 0;
            }
            return _dimensionValueNames
                .Where(kvp => headingDimensionNames
                .Contains(kvp.Key.Value)).Select(kvp => kvp.Value.Length)
                .Aggregate((a, b) => a * b);
        }

        private void ResetFields()
        {
            _defaultLanguage = null;
            _availableLanguages = null;
            _contentDimensionNames = null;
            _stubDimensionNames = null;
            _headingDimensionNames = null;
            _dimensionValueNames = null;
        }
    }
}
