using Px.Utils.PxFile;
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
        Encoding? encoding,
        ValidationStructuredEntry[] entries,
        CustomContentValidationFunctions? customContentValidationFunctions = null,
        PxFileSyntaxConf? syntaxConf = null) : IValidator
    {
        private readonly string _filename = filename;
        private readonly Encoding? _encoding = encoding;
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

            ValidationFeedback feedbackItems = [];

            foreach (ContentValidationFindKeywordValidator findingFunction in contentValidationFindKeywordFunctions)
            {
                ValidationFeedback? feedback = findingFunction(entries, this);
                if (feedback is not null)
                {
                    feedbackItems.AddRange(feedback);
                }
            }
            foreach (ContentValidationEntryValidator entryFunction in contentValidationEntryFunctions)
            {
                foreach (ValidationStructuredEntry entry in entries)
                {
                    ValidationFeedback? feedback = entryFunction(entry, this);
                    if (feedback is not null)
                    {
                        feedbackItems.AddRange(feedback);
                    }
                }
            }
            int lengthOfDataRows = _headingDimensionNames is not null ? GetProductOfDimensionValues(_headingDimensionNames) : 0;
            int amountOfDataRows = _stubDimensionNames is not null ? GetProductOfDimensionValues(_stubDimensionNames) : 0;
            ResetFields();

            return new ContentValidationResult(feedbackItems, lengthOfDataRows, amountOfDataRows);
        }

        #region Interface implementation

        ValidationResult IValidator.Validate()
            => Validate();

        #endregion

        private int GetProductOfDimensionValues(Dictionary<string, string[]> dimensions)
        {
            string? lang = _defaultLanguage ?? _availableLanguages?[0] ?? string.Empty;
            if (lang is null || dimensions.Count == 0)
            {
                return 0;
            }
            string[] dimensionNames = dimensions[lang];
            if (dimensionNames is null || dimensionNames.Length == 0 || _dimensionValueNames is null || _dimensionValueNames.Count == 0)
            {
                return 0;
            }
            return _dimensionValueNames
                .Where(kvp => dimensionNames
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
