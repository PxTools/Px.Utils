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
        MultipleEntriesOnOneLine,
        NoEncoding,
        MoreThanOneLanguageParameterSection,
        MoreThanOneSpecifierParameterSection,
        KeyHasWrongOrder,
        MissingKeyword,
        TooManySpecifiers,
        SpecifierDelimiterMissing,
        SpecifierPartNotEnclosed,
        InvalidValueFormat,
        ExcessWhitespaceInValue,
        KeyContainsExcessWhiteSpace,
        ExcessNewLinesInValue,
        IllegalCharactersInKeyword,
        KeywordDoesntStartWithALetter,
        KeyworRegexTimeout,
        LanguageRegexTimeout,
        IllegalCharactersInLanguageParameter,
        IllegalCharactersInSpecifierParameter,
        EntryWithoutValue,
        IncompliantLanguage,
        KeywordContainsUnderscore,
        KeywordIsNotInUpperCase,
        KeywordExcessivelyLong
    }
}
