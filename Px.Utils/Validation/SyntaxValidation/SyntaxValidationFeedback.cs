using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation.SyntaxValidation
{
    public class SyntaxValidationFeedbackMultipleEntriesOnLine : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;
        public override string Rule { get; } = "There are multiple entries on this line.";
    }

    public class SyntaxValidationFeedbackNoEncoding : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public override string Rule { get; } = "The file has no encoding.";
    }

    public class SyntaxValidationFeedbackMoreThanOneLanguage : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public override string Rule { get; } = "Entry has more than one language parameter.";
    }

    public class SyntaxValidationFeedbackMoreThanOneSpecifier: ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public override string Rule { get; } = "Entry has more than one specifier parameter";
    }

    public class SyntaxValidationFeedbackKeyHasWrongOrder : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public override string Rule { get; } = "The key is not defined in the order of KEYWORD[language](\"specifier\")";
    }

    public class SyntaxValidationFeedbackMissingKeyword : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public override string Rule { get; } = "The entry has no keyword";
    }

    public class SyntaxValidationFeedbackInvalidSpecifier(string[] reasons) : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public override string Rule { get; } = $"The specifier is not following valid format: {string.Join(". ", reasons)}";
    }

    public class SyntaxValidationFeedbackIllegalSymbolsInParamSections(string[] reasons) : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public override string Rule { get; } = $"The key parameter section contains illegal symbols: {string.Join(". ", reasons)}";
    }

    public class SyntaxValidationFeedbackInvalidValueSection(string reason) : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public override string Rule { get; } = "The value section is following a valid format. " + reason;
    }

    public class SyntaxValidationFeedbackValueContainsExcessWhitespace : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;
        public override string Rule { get; } = "The value section contains excess whitespace.";
    }

    public class SyntaxValidationFeedbackKeyContainsExcessWhitespace : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;
        public override string Rule { get; } = "The key section contains excess whitespace.";
    }

    public class SyntaxValidationFeedbackExcessNewLinesInValue : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;
        public override string Rule { get; } = "The value section is split to multiple lines unnecessarily. Recommended only beyond length of 150";
    }

    public class SyntaxValidationFeedbackInvalidKeywordFormat(string[] reasons) : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public override string Rule { get; } = $"The keyword is not following valid format. {string.Join(". ", reasons)}";
    }

    public class SyntaxValidationFeedbackInvalidLanguageFormat(string[] reasons) : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public override string Rule { get; } = $"The language parameter is not following valid format. {string.Join(". ", reasons)}";
    }

    public class SyntaxValidationFeedbackIllegalCharactersInSpecifier(string[] reasons) : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public override string Rule { get; } = $"The specifier contains illegal characters. {string.Join(". ", reasons)}";
    }

    public class SyntaxValidationFeedbackEntryWithoutValue() : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Error;
        public override string Rule { get; } = "The entry has no value section separated with =";
    }

    public class SyntaxValidationFeedbackIncompliantLanguageParam() : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;
        public override string Rule { get; } = "The language parameter is not compliant with BCP 47, MS-LCID or ISO 639 standards.";
    }

    public class SyntaxValidationFeedbackKeywordContainsUnrecommendedCharacters(string[] reasons) : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;
        public override string Rule { get; } = $"The keyword contains characters that are not recommended. {string.Join(". ", reasons)}";
    }

    public class SyntaxValidationFeedbackKeywordIsExcessivelyLong() : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; } = ValidationFeedbackLevel.Warning;
        public override string Rule { get; } = "The keyword is excessively long. Recommended length of up to 20 characters";
    }
}
