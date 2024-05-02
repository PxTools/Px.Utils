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
        private static readonly int MaxLength = decimal.MaxValue.ToString().Length;
        private static readonly int zero = 0x30;
        private static readonly int nine = 0x39;

        /// <summary>
        /// Validates a token to determine if it represents a valid number data item.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>An enumerable of <see cref="ValidationFeedback"/> objects indicating any validation errors.</returns>
        public IEnumerable<ValidationFeedback> Validate(List<byte> entry, EntryType entryType, Encoding encoding, int lineNumber, int charPos)
        {
            if (entry.Count >= MaxLength && !decimal.TryParse(entry.ToArray(), out _))
            {
                return [new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.DataValidationFeedbackInvalidNumber,
                    lineNumber,
                    charPos,
                    encoding.GetString(entry.ToArray()))
                ];
            }

            int decimalSeparatorIndex = entry.IndexOf(0x2E);
            if (decimalSeparatorIndex == -1)
            {
                return IsValidIntegerPart(entry, true) ?
                    Array.Empty<ValidationFeedback>() :
                    [new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.DataValidationFeedbackInvalidNumber,
                        lineNumber,
                        charPos,
                        encoding.GetString(entry.ToArray()))
                    ];
            }
            else if (decimalSeparatorIndex == 0 || !IsValidIntegerPart(entry[0..decimalSeparatorIndex], false))
            {
                return
                    [new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.DataValidationFeedbackInvalidNumber,
                        lineNumber,
                        charPos,
                        encoding.GetString(entry.ToArray()))
                    ];
            }
            else return IsValidDecimalPart(entry[decimalSeparatorIndex..]) ?
                    Array.Empty<ValidationFeedback>() :
                    [new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.DataValidationFeedbackInvalidNumber,
                        lineNumber,
                        charPos,
                        encoding.GetString(entry.ToArray()))
                    ];
        }

        private static bool IsValidIntegerPart(List<byte> entry, bool isInteger)
        {
            bool isNegative = entry[0] == 0x2D;
            bool startsWithZero = isNegative ? entry[1] == zero : entry[0] == zero;
            List<byte> numbers = isNegative ? entry.Skip(1).ToList() : entry;
            if (numbers.Count > 1)
            {
                if (isInteger && numbers.Sum(x => x - zero) == 0)
                {
                    return false;
                }
                if (startsWithZero && isInteger)
                {
                    return false;
                }
            }
            if (isNegative && isInteger && numbers[0] == zero)
            {
                return false;
            }
            for (int i = isNegative ? 1 : 0; i < entry.Count; i++)
            {
                if (entry[i] < zero || entry[i] > nine)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsValidDecimalPart(List<byte> entry)
        {
            if (entry.Count == 1)
            {
                return false;
            }
            for (int i = 1; i < entry.Count; i++)
            {
                if (entry[i] < zero || entry[i] > nine)
                {
                    return false;
                }
            }
            return true;
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
                    : [new ValidationFeedback(
                        ValidationFeedbackLevel.Warning,
                        ValidationFeedbackRule.DataValidationFeedbackInconsistentSeparator,
                        lineNumber,
                        charPos)];
            }
        }

        private const byte Sentinel = 0x0;

        public DataSeparatorValidator()
        {
        }
    }

    public class DataStructureValidator : IDataValidator
    {
        private EntryType _previousTokenType = EntryType.Unknown;

        private readonly Dictionary<EntryType, EntryType[]>_allowedPreviousTokens = new()
        {
            {EntryType.DataItem, new[] {EntryType.LineSeparator, EntryType.DataItemSeparator, EntryType.Unknown}},
            {EntryType.DataItemSeparator, new[] {EntryType.DataItem, EntryType.Unknown, EntryType.EndOfData}},
            {EntryType.LineSeparator, new[] {EntryType.DataItemSeparator, EntryType.Unknown}},
            {EntryType.Unknown, new[] {EntryType.DataItem, EntryType.Unknown, EntryType.DataItemSeparator, EntryType.LineSeparator}},
            {EntryType.EndOfData, new[] {EntryType.DataItem}}
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