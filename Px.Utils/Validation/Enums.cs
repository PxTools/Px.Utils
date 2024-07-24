namespace Px.Utils.Validation
{
    /// <summary>
    /// Defines the levels of validation feedback. These levels can be used to categorize feedback items by severity.
    /// </summary>
    public enum ValidationFeedbackLevel
    {
        Warning = 0,
        Error = 1
    }

    /// <summary>
    /// Defines the types of values found in Px files that can be validated.
    /// </summary>
    public enum ValueType
    {
        StringValue,
        ListOfStrings,
        Number,
        Boolean,
        DateTime,
        TimeValRange,
        TimeValSeries
    }

    /// <summary>
    /// Defines the types of validation feedback rules. Can be used to categorize feedback by rule type or for translations.
    /// </summary>
    public enum ValidationFeedbackRule
    {
        MultipleEntriesOnOneLine = 0,
        NoEncoding = 1,
        MoreThanOneLanguageParameterSection = 2,
        MoreThanOneSpecifierParameterSection = 3,
        KeyHasWrongOrder = 4,
        MissingKeyword = 5,
        TooManySpecifiers = 6,
        SpecifierDelimiterMissing = 7,
        SpecifierPartNotEnclosed = 8,
        InvalidValueFormat = 9,
        ExcessWhitespaceInValue = 10,
        KeyContainsExcessWhiteSpace = 11,
        ExcessNewLinesInValue = 12,
        IllegalCharactersInKeyword = 13,
        KeywordDoesntStartWithALetter = 14,
        RegexTimeout = 15,
        IllegalCharactersInLanguageParameter = 17,
        IllegalCharactersInSpecifierParameter = 18,
        EntryWithoutValue = 19,
        IncompliantLanguage = 20,
        KeywordContainsUnderscore = 21,
        KeywordIsNotInUpperCase = 22,
        KeywordExcessivelyLong = 23,
        IllegalCharactersInSpecifierPart = 24,
        EntryWithMultipleValues = 25,
        MissingDefaultLanguage = 26,
        RequiredKeyMissing = 27,
        RecommendedKeyMissing = 28,
        RequiredLanguageDefinitionMissing = 29,
        RecommendedLanguageDefinitionMissing = 30,
        UndefinedLanguageFound = 31,
        IllegalLanguageDefinitionFound = 32,
        UnrecommendedLanguageDefinitionFound = 33,
        RequiredSpecifierDefinitionMissing = 34,
        RecommendedSpecifierDefinitionMissing = 35,
        IllegalSpecifierDefinitionFound = 36,
        UnrecommendedSpecifierDefinitionFound = 37,
        InvalidValueFound = 38,
        ValueIsNotInUpperCase = 39,
        UnmatchingValueType = 40,
        UnmatchingValueAmount = 41,
        MultipleInstancesOfUniqueKey = 42,
        MissingStubAndHeading = 43,
        VariableValuesMissing = 44,
        DataValidationFeedbackInvalidStructure = 45,
        DataValidationFeedbackInvalidRowCount = 46,
        DataValidationFeedbackInvalidRowLength = 47,
        DataValidationFeedbackInconsistentSeparator = 48,
        DataValidationFeedbackInvalidString = 49,
        DataValidationFeedbackInvalidNumber = 50,
        DataValidationFeedbackInvalidChar = 51,
        FileLanguageDiffersFromDatabase = 52,
        FileEncodingDiffersFromDatabase = 53,
        AliasFileMissing = 54,
        DuplicateFileNames = 55
    }
}
