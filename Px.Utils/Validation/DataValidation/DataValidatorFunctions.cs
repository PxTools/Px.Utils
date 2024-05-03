﻿using PxUtils.PxFile;
using System.Globalization;
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
        /// Validates an entry that has been identified as a string type data item.
        /// </summary>
        /// <param name="entry">List of bytes that represents the entry being validated.</param>
        /// <param name="entryType"><see cref="EntryType"/> enum that represents the type of the validation entry.</param>
        /// <param name="encoding">Encoding format of the Px file.</param>
        /// <param name="lineNumber">Line number for the validation item.</param>
        /// <param name="charPos">Represents the position relative to the line for the validation item.</param>
        /// <param name="feedbacks">Reference to a list of feedback items to which any validation feedback is added to.</param>
        public void Validate(List<byte> entry, EntryType entryType, Encoding encoding, int lineNumber, int charPos, ref List<ValidationFeedback> feedbacks)
        {
            string value = encoding.GetString(entry.ToArray());
            if (!ValidStringDataItems.Contains(value))
            {
                feedbacks.Add(new ValidationFeedback(ValidationFeedbackLevel.Error,
                ValidationFeedbackRule.DataValidationFeedbackInvalidString, lineNumber, charPos, $"{value}"));
            }
        }
    }

    public class DataNumberValidator : IDataValidator
    {
        private static readonly int MaxLength = decimal.MaxValue.ToString(CultureInfo.InvariantCulture).Length;
        private static readonly int zero = 0x30;
        private static readonly int nine = 0x39;

        /// <summary>
        /// Validates a token to determine if it represents a valid number data item.
        /// </summary>        
        /// <param name="entry">List of bytes that represents the entry being validated.</param>
        /// <param name="entryType"><see cref="EntryType"/> enum that represents the type of the validation entry.</param>
        /// <param name="encoding">Encoding format of the Px file.</param>
        /// <param name="lineNumber">Line number for the validation item.</param>
        /// <param name="charPos">Represents the position relative to the line for the validation item.</param>
        /// <param name="feedbacks">Reference to a list of feedback items to which any validation feedback is added to.</param>
        public void Validate(List<byte> entry, EntryType entryType, Encoding encoding, int lineNumber, int charPos, ref List<ValidationFeedback> feedbacks)
        {
            if (entry.Count >= MaxLength && !decimal.TryParse(entry.ToArray(), out _))
            {
                feedbacks.Add(new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.DataValidationFeedbackInvalidNumber,
                    lineNumber,
                    charPos,
                    encoding.GetString(entry.ToArray())));

                return;
            }

            int decimalSeparatorIndex = entry.IndexOf(0x2E);
            if (decimalSeparatorIndex == -1)
            {
                if (!IsValidIntegerPart(entry, true))
                {
                    feedbacks.Add(new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.DataValidationFeedbackInvalidNumber,
                        lineNumber,
                        charPos,
                        encoding.GetString(entry.ToArray())));
                }
            }
            else if (decimalSeparatorIndex == 0 || !IsValidIntegerPart(entry[0..decimalSeparatorIndex], false) || !IsValidDecimalPart(entry[decimalSeparatorIndex..]))
            {
                feedbacks.Add(new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.DataValidationFeedbackInvalidNumber,
                    lineNumber,
                    charPos,
                    encoding.GetString(entry.ToArray())));
            }
        }

        private static bool IsValidIntegerPart(List<byte> entry, bool isInteger)
        {
            bool isNegative = entry[0] == 0x2D;
            bool startsWithZero = isNegative ? entry[1] == zero : entry[0] == zero;
            List<byte> digits = isNegative ? entry.Skip(1).ToList() : entry;
            if (digits.Count > 1)
            {
                if (isInteger && digits.Sum(x => x - zero) == 0)
                {
                    return false;
                }
                if (startsWithZero && isInteger)
                {
                    return false;
                }
            }
            if (isNegative && isInteger && digits[0] == zero)
            {
                return false;
            }

            return digits.TrueForAll(x => x >= zero && x <= nine);
        }

        private static bool IsValidDecimalPart(List<byte> entry)
        {
            return entry.Count > 1 && entry.Skip(1).All(x => x >= zero && x <= nine);
        }
    }

    public class DataSeparatorValidator : IDataValidator
    {
        private byte _separator = Sentinel;

        /// <summary>
        /// Validates the consistency of the data item separator.
        /// </summary>        
        /// <param name="entry">List of bytes that represents the entry being validated.</param>
        /// <param name="entryType"><see cref="EntryType"/> enum that represents the type of the validation entry.</param>
        /// <param name="encoding">Encoding format of the Px file.</param>
        /// <param name="lineNumber">Line number for the validation item.</param>
        /// <param name="charPos">Represents the position relative to the line for the validation item.</param>
        /// <param name="feedbacks">Reference to a list of feedback items to which any validation feedback is added to.</param>
        public void Validate(List<byte> entry, EntryType entryType, Encoding encoding, int lineNumber, int charPos, ref List<ValidationFeedback> feedbacks)
        {
            if (_separator == entry[0])
            {
                return;
            }
            else if (_separator == Sentinel)
            {
                _separator = entry[0];
                return;
            }

            feedbacks.Add(
                new ValidationFeedback(
                    ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.DataValidationFeedbackInconsistentSeparator,
                    lineNumber,
                    charPos));
        }

        private const byte Sentinel = 0x0;
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
        /// <param name="entry">List of bytes that represents the entry being validated.</param>
        /// <param name="entryType"><see cref="EntryType"/> enum that represents the type of the validation entry.</param>
        /// <param name="encoding">Encoding format of the Px file.</param>
        /// <param name="lineNumber">Line number for the validation item.</param>
        /// <param name="charPos">Represents the position relative to the line for the validation item.</param>
        /// <param name="feedbacks">Reference to a list of feedback items to which any validation feedback is added to.</param>
        public void Validate(List<byte> entry, EntryType entryType, Encoding encoding, int lineNumber, int charPos, ref List<ValidationFeedback> feedbacks)
        {
            if (_allowedPreviousTokens[entryType].Contains(_previousTokenType))
            {
                _previousTokenType = entryType;
                return;
            }

            feedbacks.Add(
                new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.DataValidationFeedbackInvalidStructure,
                    lineNumber,
                    charPos,
                    $"{_previousTokenType},{entryType}"));
            
            _previousTokenType = entryType;
        }

    }
}