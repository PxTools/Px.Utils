namespace PxUtils.Validation.SyntaxValidation
{
    public class SyntaxValidationFeedbackMultipleEntriesOnLine : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;
        public string Rule { get; } = "There are multiple entries on this line.";
    }

    public class SyntaxValidationFeedbackNoEncoding : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public string Rule { get; } = "The file has no encoding.";
    }

    public class SyntaxValidationFeedbackMoreThanOneLanguage : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public string Rule { get; } = "Entry has more than one language parameter.";
    }

    public class SyntaxValidationFeedbackMoreThanOneSpecifier: IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public string Rule { get; } = "Entry has more than one specifier parameter";
    }

    public class SyntaxValidationFeedbackKeyHasWrongOrder : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public string Rule { get; } = "The key is not defined in the order of KEYWORD[language](\"specifier\")";
    }

    public class SyntaxValidationFeedbackMissingKeyword : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public string Rule { get; } = "The entry has no keyword";
    }

    public class SyntaxValidationFeedbackInvalidSpecifier(string[] reasons) : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public string Rule { get; } = $"The specifier is not following valid format: {string.Join(". ", reasons)}";
    }

    public class SyntaxValidationFeedbackIllegalSymbolsInParamSections(string[] reasons) : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public string Rule { get; } = $"The key parameter section contains illegal symbols: {string.Join(". ", reasons)}";
    }

    public class SyntaxValidationFeedbackInvalidValueSection(string reason) : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public string Rule { get; } = "The value section is following a valid format. " + reason;
    }

    public class SyntaxValidationFeedbackValueContainsExcessWhitespace : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;
        public string Rule { get; } = "The value section contains excess whitespace.";
    }

    public class SyntaxValidationFeedbackKeyContainsExcessWhitespace : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;
        public string Rule { get; } = "The key section contains excess whitespace.";
    }

    public class SyntaxValidationFeedbackExcessNewLinesInValue : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;
        public string Rule { get; } = "The value section is split to multiple lines unnecessarily. Recommended only beyond length of 150";
    }

    public class SyntaxValidationFeedbackInvalidKeywordFormat(string[] reasons) : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public string Rule { get; } = $"The keyword is not following valid format. {string.Join(". ", reasons)}";
    }

    public class SyntaxValidationFeedbackInvalidLanguageFormat(string[] reasons) : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public string Rule { get; } = $"The language parameter is not following valid format. {string.Join(". ", reasons)}";
    }

    public class SyntaxValidationFeedbackIllegalCharactersInSpecifier(string[] reasons) : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public string Rule { get; } = $"The specifier contains illegal characters. {string.Join(". ", reasons)}";
    }

    public class SyntaxValidationFeedbackEntryWithoutValue() : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public string Rule { get; } = "The entry has no value section separated with =";
    }

    public class SyntaxValidationFeedbackIncompliantLanguageParam() : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;
        public string Rule { get; } = "The language parameter is not compliant with BCP 47, MS-LCID or ISO 639 standards.";
    }

    public class SyntaxValidationFeedbackKeywordContainsUnrecommendedCharacters(string[] reasons) : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;
        public string Rule { get; } = $"The keyword contains characters that are not recommended. {string.Join(". ", reasons)}";
    }

    public class SyntaxValidationFeedbackKeywordIsExcessivelyLong() : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;
        public string Rule { get; } = "The keyword is excessively long. Recommended length of up to 20 characters";
    }

    public class SyntaxValidationFeedbackRegexTimeOut(string type, string content) : IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public string Rule { get; } = $"The regex pattern for {type}: {content} has timed out. This might be as a result of a too long or too complex {type}.";
    }
}
