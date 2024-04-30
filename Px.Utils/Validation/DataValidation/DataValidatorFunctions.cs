using PxUtils.PxFile;
using PxUtils.Validation;
using System.Text;

namespace PxUtils.Validation.DataValidation
{
    public class DataStringValidator : IDataValidator
    {
        private static readonly HashSet<string> ValidStringDataItems =
        [
            PxFileSyntaxConf.Default.Tokens.DataValues.DataIsMissing,
            PxFileSyntaxConf.Default.Tokens.DataValues.DataCategoryNotApplicable,
            PxFileSyntaxConf.Default.Tokens.DataValues.DataIsConfidential,
            PxFileSyntaxConf.Default.Tokens.DataValues.DataIsNotAvailable,
            PxFileSyntaxConf.Default.Tokens.DataValues.DataHasNotBeenAsked,
            PxFileSyntaxConf.Default.Tokens.DataValues.Missing6,
            PxFileSyntaxConf.Default.Tokens.DataValues.DataIsNone
        ];

        /// <summary>
        /// Validates a token as a string data item.
        /// </summary>
        /// <param name="token">The token to be validated.</param>
        /// <returns>An enumerable collection of <see cref="ValidationFeedback"/> objects containing any validation feedback.</returns>
        public IEnumerable<ValidationFeedback> Validate(List<byte> entry, EntryType entryType, Encoding encoding, int lineNumber, int charPos)
        {
            ArgumentNullException.ThrowIfNull(encoding);

            string value = encoding.GetString(entry.ToArray());
            if (ValidStringDataItems.Contains(value))
            {
                return Array.Empty<ValidationFeedback>();
            }

            return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error, 
                ValidationFeedbackRule.DataValidationFeedbackInvalidString, lineNumber, charPos, $"{value}") };
        }
    }

    public class DataNumberValidator : IDataValidator
    {
        /// <summary>
        /// Validates a token to determine if it represents a valid number data item.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>An enumerable of <see cref="ValidationFeedback"/> objects indicating any validation errors.</returns>
        public IEnumerable<ValidationFeedback> Validate(List<byte> entry, EntryType entryType, Encoding encoding, int lineNumber, int charPos)
        {
            if (double.TryParse(entry.ToArray(), out _))
            {
                return Array.Empty<ValidationFeedback>();
            }
            // Retry if the entry is at the end of the data section and contains a ";"
            else if (entry[^1] == 0x3B)
            {
                entry.RemoveAt(entry.Count - 1);
                Validate(entry, entryType, encoding, lineNumber, charPos);
                return Array.Empty<ValidationFeedback>();
            }
            else
            {
                string a = encoding.GetString(entry.ToArray());
                return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidNumber, lineNumber, charPos, a) };
            }
        }
    }

    public class DataSeparatorValidator : IDataValidator
    {
        private byte _separator = Sentinel;

        /// <summary>
        /// Validates the consistency of the data item separator.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>A collection of validation feedback.</returns>
        public IEnumerable<ValidationFeedback> Validate(List<byte> entry, EntryType entryType, Encoding encoding, int lineNumber, int charPos)
        {
            if (_separator == entry[0])
            {
                return Array.Empty<ValidationFeedback>();
            }
            else if (_separator == Sentinel)
            {
                _separator = entry[0];
                return Array.Empty<ValidationFeedback>();
            }
            else
            {
                return _separator == entry[0]
                    ? Array.Empty<ValidationFeedback>()
                    : [new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.DataValidationFeedbackInconsistentSeparator)];
            }
        }

        private const byte Sentinel = 0x0;
    }

    public class DataStructureValidator : IDataValidator
    {
        private EntryType _previousTokenType = EntryType.Unknown;

        private readonly Dictionary<EntryType, EntryType[]>_allowedPreviousTokens = new()
        {
            {EntryType.DataItem, new[] {EntryType.LineSeparator, EntryType.DataItemSeparator, EntryType.Unknown}},
            {EntryType.DataItemSeparator, new[] {EntryType.DataItem, EntryType.Unknown}},
            {EntryType.LineSeparator, new[] {EntryType.DataItemSeparator, EntryType.Unknown}},
            {EntryType.Unknown, new[] {EntryType.DataItem, EntryType.Unknown, EntryType.DataItemSeparator, EntryType.LineSeparator} }
        };

        /// <summary>
        /// Validates a token based on allowed previous tokens and returns validation feedback.
        /// </summary>
        /// <param name="token">The token to be validated.</param>
        /// <returns>A collection of validation feedback.</returns>
        public IEnumerable<ValidationFeedback> Validate(List<byte> entry, EntryType entryType, Encoding encoding, int lineNumber, int charPos)
        {
            if (_allowedPreviousTokens[entryType].Contains(_previousTokenType))
            {
                _previousTokenType = entryType;
                return Array.Empty<ValidationFeedback>();
            }

            ValidationFeedback[] feedback =
            [
                new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidStructure,
                lineNumber, charPos, $"{_previousTokenType},{entryType}")
            ];
            _previousTokenType = entryType;
            return feedback;
        }

    }
}