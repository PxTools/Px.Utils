using PxUtils.PxFile;
using PxUtils.Validation;

namespace PxUtils.Validation.DataValidation
{
    /// <summary>
    /// Class for validating the count of rows in a data set.
    /// </summary>
    public class DataRowCountValidator(int numOfRows): IDataValidator
    {
        private int _currentRow;

        /// <summary>
        /// Validates the token based on the number of rows specified.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>A collection of validation feedbacks.</returns>
        public IEnumerable<ValidationFeedback> Validate(Token token)
        {
            if (token.Type == TokenType.LineSeparator)
                _currentRow++;
            else if (token.Type == TokenType.EndOfStream && _currentRow != numOfRows)
            {
                return new[]
                {
                    new ValidationFeedback(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.DataValidationFeedbackInvalidRowCount, token.LineNumber,
                        token.CharPosition, $"{numOfRows},{_currentRow}")
                };
            }

            return Array.Empty<ValidationFeedback>();
        }
    }

    public class DataRowLengthValidator(int rowLen) : IDataValidator
    {
        private int _currentRowLen = 0;

        /// <summary>
        /// Validates the given token for row length.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>An enumerable collection of ValidationFeedback objects.</returns>
        public IEnumerable<ValidationFeedback> Validate(Token token)
        {
        
            switch (token.Type)
            {
                case TokenType.DataItemSeparator:
                    _currentRowLen++;
                    break;
                case TokenType.LineSeparator:
                    var itemNum = _currentRowLen;
                    if (_currentRowLen != rowLen)
                    {
                        _currentRowLen = 0;
                        return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error, 
                            ValidationFeedbackRule.DataValidationFeedbackInvalidRowLength, token.LineNumber, token.CharPosition,$"{rowLen},{itemNum}") };
                    }
                    _currentRowLen = 0;
                    break;
            }

            return Array.Empty<ValidationFeedback>();
        }
    }

    public class DataStringValidator : IDataValidator
    {
        private static readonly string[] ValidStringDataItems =
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
        public IEnumerable<ValidationFeedback> Validate(Token token)
        {
            if (token.Type != TokenType.StringDataItem) return Array.Empty<ValidationFeedback>();

            if (ValidStringDataItems.Contains(token.Value))
            {
                return Array.Empty<ValidationFeedback>();
            }

            return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error, 
                ValidationFeedbackRule.DataValidationFeedbackInvalidString, token.LineNumber, token.CharPosition,$"{token.Value}") };
        }
    }

    public class DataNumberDataValidator : IDataValidator
    {
        private static readonly int MaxPositiveLength = decimal.MaxValue.ToString().Length;

        /// <summary>
        /// Validates a token to determine if it represents a valid number data item.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>An enumerable of <see cref="ValidationFeedback"/> objects indicating any validation errors.</returns>
        public IEnumerable<ValidationFeedback> Validate(Token token)
        {
            if( token.Type != TokenType.NumDataItem) return Array.Empty<ValidationFeedback>();

            var value = token.Value;

            var length = value.Length;
            var dotPosition = value.IndexOf('.');
            var minusPosition = value.IndexOf('-');
            var zeroPosition = value.IndexOf('0');
            var quotePosition = value.IndexOf('"');

            if (length >= MaxPositiveLength - 1)
            {
                try
                {
                    _ = decimal.Parse(value);
                }
                catch (Exception)
                {
                    return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidNumber, token.LineNumber, token.CharPosition,value) };
                }
            } else if (dotPosition == 0 || dotPosition == length -1 || (dotPosition != -1 && value.IndexOf('.', dotPosition+1) != -1))
            {
                return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidNumber, token.LineNumber, token.CharPosition,value) };
            } else if ( minusPosition != -1 && (minusPosition != 0 || (value.IndexOf('-', minusPosition + 1 ) != -1))) {
                return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidNumber, token.LineNumber, token.CharPosition,value) };
            } else if (quotePosition != -1)
            {
                return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidNumber, token.LineNumber, token.CharPosition,value) };
            } else if (zeroPosition == 0 && dotPosition != 1 && length > 1 || minusPosition == 0 && zeroPosition == 1)
            {
                return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidNumber, token.LineNumber, token.CharPosition,value) };
            }

            return Array.Empty<ValidationFeedback>();
        }
    }

    public class DataSeparatorValidator : IDataValidator
    {
        private char _separator = Sentinel;

        /// <summary>
        /// Validates the consistency of the data item separator.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>A collection of validation feedback.</returns>
        public IEnumerable<ValidationFeedback> Validate(Token token)
        {
            if (token.Type != TokenType.DataItemSeparator) return Array.Empty<ValidationFeedback>();
            if (_separator == Sentinel)
            {
                _separator = token.Value[0];
                return Array.Empty<ValidationFeedback>();
            }

            return _separator == token.Value[0]
                ? Array.Empty<ValidationFeedback>()
                : new[] { new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.DataValidationFeedbackInconsistentSeparator) };
        }

        private const char Sentinel = '\0';
    }

    public class DataStructureValidator : IDataValidator
    {
        private TokenType _previousTokenType = TokenType.EmptyToken;

        private readonly Dictionary<TokenType, TokenType[]>_allowedPreviousTokens = new Dictionary<TokenType, TokenType[]>
        {
            {TokenType.EmptyToken, new [] { TokenType.EmptyToken }},
            {TokenType.InvalidDataChar, Array.Empty<TokenType>()},
            {TokenType.NumDataItem, new[] {TokenType.LineSeparator, TokenType.DataItemSeparator, TokenType.EmptyToken, TokenType.EndOfData}},
            {TokenType.StringDataItem, new[] {TokenType.LineSeparator, TokenType.DataItemSeparator, TokenType.EmptyToken, TokenType.EndOfData}},
            {TokenType.DataItemSeparator, new[] {TokenType.NumDataItem, TokenType.StringDataItem, TokenType.EndOfData}},
            {TokenType.LineSeparator, new[] {TokenType.DataItemSeparator}},
            {TokenType.EndOfData, new[] {TokenType.NumDataItem, TokenType.StringDataItem}},
            {TokenType.EndOfStream, new[] {TokenType.LineSeparator}}
        };

        /// <summary>
        /// Validates a token based on allowed previous tokens and returns validation feedback.
        /// </summary>
        /// <param name="token">The token to be validated.</param>
        /// <returns>A collection of validation feedback.</returns>
        public IEnumerable<ValidationFeedback> Validate(Token token)
        {
            if (_allowedPreviousTokens[token.Type].Contains(_previousTokenType))
            {
                _previousTokenType = token.Type;
                return Array.Empty<ValidationFeedback>();
            }
            var feedback = new []{new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidStructure,
                token.LineNumber, token.CharPosition, $"{_previousTokenType},{token.Type}")};
            _previousTokenType = token.Type;
            return feedback;
        }

    }
}