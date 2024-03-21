﻿using PxUtils.Validation;

namespace PxUtils.Validation.DataValidation;

/// <summary>
/// Class for validating the count of rows in a data set.
/// </summary>
public class DataRowCountValidator(int numOfRows): IDataValidator
{
    private int _currentRow;
    public IEnumerable<ValidationFeedback> Validate(Token token)
    {
        switch (token.Type)
        {
            case TokenType.LineSeparator:
                _currentRow++;
                break;
            case TokenType.EndOfStream:
                if (_currentRow != numOfRows)
                {
                    return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.DataValidationFeedbackInvalidRowCount, $"{numOfRows},{_currentRow}") };
                }
                break;
        }
        return Array.Empty<ValidationFeedback>();
    }
}

public class DataRowLengthValidator(int rowLen) : IDataValidator
{
    private int _currentRowLen = 0;
    
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
                        ValidationFeedbackRule.DataValidationFeedbackInvalidRowLength, $"{rowLen},{itemNum}") };
                }
                _currentRowLen = 0;
                break;
        }

        return Array.Empty<ValidationFeedback>();
    }
}

public class DataStringValidator : IDataValidator
{
    private static readonly string[] ValidStringDataItems = new[]
        { "\".\"", "\"..\"", "\"...\"", "\"....\"", "\".....\"", "\"......\"", "\"-\"" };

    public IEnumerable<ValidationFeedback> Validate(Token token)
    {
        if (token.Type != TokenType.StringDataItem) return Array.Empty<ValidationFeedback>();

        if (ValidStringDataItems.Contains(token.Value))
        {
            return Array.Empty<ValidationFeedback>();
        }

        return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error, 
            ValidationFeedbackRule.DataValidationFeedbackInvalidString, $"{token.Value}") };
    }
}

public class DataNumberDataValidator : IDataValidator
{
    private static readonly int MaxPositiveLength = decimal.MaxValue.ToString().Length;
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
                return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidNumber, value) };
            }
        } else if (dotPosition == 0 || dotPosition == length -1 || (dotPosition != -1 && value.IndexOf('.', dotPosition+1) != -1))
        {
            return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidNumber, value) };
        } else if ( minusPosition != -1 && (minusPosition != 0 || (value.IndexOf('-', minusPosition + 1 ) != -1))) {
                return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidNumber, value) };
        } else if (quotePosition != -1)
        {
            return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidNumber, value) };
        } else if (zeroPosition == 0 && dotPosition != 1 && length > 1 || minusPosition == 0 && zeroPosition == 1)
        {
            return new[] { new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidNumber, value) };
        }

        return Array.Empty<ValidationFeedback>();
    }
}

public class DataSeparatorValidator : IDataValidator
{
    private char _separator = Sentinel;

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
    
    public IEnumerable<ValidationFeedback> Validate(Token token)
    {
        if (_allowedPreviousTokens[token.Type].Contains(_previousTokenType))
        {
            _previousTokenType = token.Type;
            return Array.Empty<ValidationFeedback>();
        }
        var feedback = new []{new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidStructure,
            $"{_previousTokenType},{token.Type}")};
        _previousTokenType = token.Type;
        return feedback;
    }

}