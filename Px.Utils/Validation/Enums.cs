namespace PxUtils.Validation
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
    /// Defines the types of values that can be validated. These types correspond to the types of values that can be found in a PX file.
    /// </summary>
    public enum ValueType
    {
        String,
        ListOfStrings,
        Number,
        Boolean,
        DateTime,
        Timeval
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
        KeyworRegexTimeout = 15,
        LanguageRegexTimeout = 16,
        IllegalCharactersInLanguageParameter = 17,
        IllegalCharactersInSpecifierParameter = 18,
        EntryWithoutValue = 19,
        IncompliantLanguage = 20,
        KeywordContainsUnderscore = 21,
        KeywordIsNotInUpperCase = 22,
        KeywordExcessivelyLong = 23,
        IllegalCharactersInSpecifierPart = 24,
        EntryWithMultipleValues = 25
    }
}
