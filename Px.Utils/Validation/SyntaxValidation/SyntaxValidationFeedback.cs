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
}
