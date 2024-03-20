using PxUtils.PxFile;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PxUtils.Validation.SyntaxValidation
{
    public delegate ValidationFeedbackItem? ValidationFunctionDelegate(ValidationEntry entry, PxFileSyntaxConf syntaxConf);

    /// <summary>
    /// Contains the default validation functions for syntax validation.
    /// </summary>
    public class SyntaxValidationFunctions
    {
        public IEnumerable<ValidationFunctionDelegate> DefaultStringValidationFunctions { get; }
        public IEnumerable<ValidationFunctionDelegate> DefaultKeyValueValidationFunctions { get; }
        public IEnumerable<ValidationFunctionDelegate> DefaultStructuredValidationFunctions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxValidationFunctions"/> class with default validation functions.
        /// </summary>
        public SyntaxValidationFunctions()
        {
            DefaultStringValidationFunctions = new List<ValidationFunctionDelegate>
            {
                MultipleEntriesOnLine,
                EntryWithoutValue,
            };

            DefaultKeyValueValidationFunctions = new List<ValidationFunctionDelegate>
            {
                MoreThanOneLanguageParameter,
                MoreThanOneSpecifierParameter,
                WrongKeyOrderOrMissingKeyword,
                SpecifierPartNotEnclosed,
                MoreThanTwoSpecifierParts,
                NoDelimiterBetweenSpecifierParts,
                IllegalSymbolsInLanguageParamSection,
                IllegalSymbolsInSpecifierParamSection,
                InvalidValueFormat,
                ExcessWhitespaceInValue,
                KeyContainsExcessWhiteSpace,
                ExcessNewLinesInValue
            };

            DefaultStructuredValidationFunctions = new List<ValidationFunctionDelegate>
            {
                KeywordContainsIllegalCharacters,
                KeywordDoesntStartWithALetter,
                IllegalCharactersInLanguageParameter,
                IllegalCharactersInSpecifierParameter,
                IncompliantLanguage,
                KeywordContainsUnderscore,
                KeywordIsNotInUpperCase,
                KeywordIsExcessivelyLong
            };
        }

        private const string ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP_ENTRY = "Entry is not of type KeyValueValidationEntry";
        private const string ARGUMENT_EXCEPTION_MESSAGE_NOT_A_STRING_ENTRY = "Entry is not of type StringValidationEntry";
        private const string ARGUMENT_EXCEPTION_MESSAGE_NOT_A_STRUCTURED_ENTRY = "Entry is not of type StructuredValidationEntry";

        /// <summary>
        /// Validates the given entry. If the entry does not start with a line separator, it is considered as not being on its own line, and a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the entry does not start with a line separator, null otherwise.</returns>
        public readonly ValidationFunctionDelegate MultipleEntriesOnLine = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            StringValidationEntry? stringEntry = entry as StringValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_STRING_ENTRY);

            // If the entry does not start with a line separator, it is not on its own line. For the first entry this is not relevant.
            if (
            stringEntry.EntryIndex == 0 || 
            stringEntry.EntryString.StartsWith(syntaxConf.Symbols.Linebreak) ||
            stringEntry.EntryString.StartsWith(syntaxConf.Symbols.WindowsLinebreak))
            {
                return null;
            }
            else
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.MultipleEntriesOnOneLine));
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the key contains more than one language parameter, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key contains more than one language parameter, null otherwise.</returns>
        public readonly ValidationFunctionDelegate MoreThanOneLanguageParameter = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP_ENTRY);

            bool hasMultipleParameters = SyntaxValidationUtilityMethods.HasMoreThanOneSection(
                keyValueValidationEntry.KeyValueEntry.Key,
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.LangParamEnd
            );

            if (hasMultipleParameters)
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MoreThanOneLanguageParameterSection));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the key contains more than one specifier parameter, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key contains more than one specifier parameter, null otherwise.</returns>
        public readonly ValidationFunctionDelegate MoreThanOneSpecifierParameter = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP_ENTRY);

            bool hasMultipleParameters = SyntaxValidationUtilityMethods.HasMoreThanOneSection(
                keyValueValidationEntry.KeyValueEntry.Key,
                syntaxConf.Symbols.Key.SpecifierParamStart,
                syntaxConf.Symbols.Key.SpecifierParamEnd
                );

            if (hasMultipleParameters)
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MoreThanOneSpecifierParameterSection));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the key is not defined in the order of KEYWORD[language](\"specifier\"), a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key is not defined in the order of KEYWORD[language](\"specifier\"), null otherwise.</returns>
        public readonly ValidationFunctionDelegate WrongKeyOrderOrMissingKeyword = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP_ENTRY);

            string key = keyValueValidationEntry.KeyValueEntry.Key;

            string languageRemoved = SyntaxValidationUtilityMethods.ExtractSectionFromString(key, syntaxConf.Symbols.Key.LangParamStart, syntaxConf.Symbols.Key.LangParamEnd).Remainder;
            string keyword = SyntaxValidationUtilityMethods.ExtractSectionFromString(languageRemoved, syntaxConf.Symbols.Key.SpecifierParamStart, syntaxConf.Symbols.Key.SpecifierParamEnd).Remainder;

            if (keyword.Trim() == string.Empty)
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingKeyword));
            }

            int langParamStartIndex = key.IndexOf(syntaxConf.Symbols.Key.LangParamStart);
            int specifierParamStartIndex = key.IndexOf(syntaxConf.Symbols.Key.SpecifierParamStart);

            if (langParamStartIndex == -1)
            {
                return null;
            }
            else if (
                specifierParamStartIndex != -1 && langParamStartIndex > specifierParamStartIndex ||
                key.Trim().StartsWith(syntaxConf.Symbols.Key.SpecifierParamStart) ||
                key.Trim().StartsWith(syntaxConf.Symbols.Key.LangParamStart)
                )
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.KeyHasWrongOrder));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the key contains more than two specifiers, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if more than two specifiers are found, null otherwise.</returns>
        public readonly ValidationFunctionDelegate MoreThanTwoSpecifierParts = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP_ENTRY);

            string? specifierParamSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    keyValueValidationEntry.KeyValueEntry.Key,
                                    syntaxConf.Symbols.Key.SpecifierParamStart,
                                    syntaxConf.Symbols.Key.SpecifierParamEnd
                                ).Sections.FirstOrDefault();

            if (specifierParamSection is null)
            {
                return null;
            }

            ExtractSectionResult specifierResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                specifierParamSection,
                syntaxConf.Symbols.Key.StringDelimeter
                );

            string[] specifiers = specifierResult.Sections;

            if (specifiers.Length > 2)
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.TooManySpecifiers));
            }

            return null;
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If there is no delimeter between specifier parts, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if there is no delimeter between specifier parts, null otherwise.</returns>
        public readonly ValidationFunctionDelegate NoDelimiterBetweenSpecifierParts = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP_ENTRY);

            string? specifierParamSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    keyValueValidationEntry.KeyValueEntry.Key,
                                    syntaxConf.Symbols.Key.SpecifierParamStart,
                                    syntaxConf.Symbols.Key.SpecifierParamEnd
                                ).Sections.FirstOrDefault();

            if (specifierParamSection is null)
            {
                return null;
            }

            ExtractSectionResult specifierResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                specifierParamSection,
                syntaxConf.Symbols.Key.StringDelimeter
                );

            string[] specifiers = specifierResult.Sections;

            if (specifiers.Length > 1 && !specifierResult.Remainder.Contains(syntaxConf.Symbols.Key.ListSeparator))
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.SpecifierDelimiterMissing));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If any of the specifiers are not enclosed a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if specifier parts are not enclosed, null otherwise.</returns>
        public readonly ValidationFunctionDelegate SpecifierPartNotEnclosed = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP_ENTRY);

            string? specifierParamSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    keyValueValidationEntry.KeyValueEntry.Key,
                                    syntaxConf.Symbols.Key.SpecifierParamStart,
                                    syntaxConf.Symbols.Key.SpecifierParamEnd
                                ).Sections.FirstOrDefault();

            if (specifierParamSection == null)
            {
                return null;
            }

            foreach (string specifier in specifierParamSection.Split(syntaxConf.Symbols.Key.ListSeparator))
            {
                string trimmedSpecifier = specifier.Trim();
                if (!trimmedSpecifier.StartsWith(syntaxConf.Symbols.Key.StringDelimeter) || !trimmedSpecifier.EndsWith(syntaxConf.Symbols.Key.StringDelimeter))
                {
                    return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.SpecifierPartNotEnclosed));
                }
            }

            return null;
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the key language section contains illegal symbols, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key language section contains illegal symbols, null otherwise.</returns>
        public readonly ValidationFunctionDelegate IllegalSymbolsInLanguageParamSection = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP_ENTRY);

            string key = keyValueValidationEntry.KeyValueEntry.Key;

            string languageParamSections = string.Join("", SyntaxValidationUtilityMethods.ExtractSectionFromString(key, syntaxConf.Symbols.Key.LangParamStart, syntaxConf.Symbols.Key.LangParamEnd).Sections);

            char[] languageParamIllegalSymbols = [
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.SpecifierParamStart,
                syntaxConf.Symbols.Key.SpecifierParamEnd,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.Key.ListSeparator
                ];

            char[] illegalSymbolsFoundInLanguageParam = languageParamIllegalSymbols.Where(languageParamSections.Contains).ToArray();

            if (illegalSymbolsFoundInLanguageParam.Length > 0)
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInLanguageParameter));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the key specifier section contains illegal symbols, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key specifier section contains illegal symbols, null otherwise.</returns>
        public readonly ValidationFunctionDelegate IllegalSymbolsInSpecifierParamSection = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP_ENTRY);

            string key = keyValueValidationEntry.KeyValueEntry.Key;

            string specifierParamSections = string.Join("", SyntaxValidationUtilityMethods.ExtractSectionFromString(key, syntaxConf.Symbols.Key.SpecifierParamStart, syntaxConf.Symbols.Key.SpecifierParamEnd).Sections);

            char[] specifierParamIllegalSymbols = [
                syntaxConf.Symbols.Key.SpecifierParamStart,
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.LangParamEnd
            ];

            char[] illegalSymbolsFoundInSpecifierParam = specifierParamIllegalSymbols.Where(specifierParamSections.Contains).ToArray();

            if (illegalSymbolsFoundInSpecifierParam.Length > 0)
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInSpecifierParameter));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the value section is not following a valid format, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the value section is not following a valid format, null otherwise.</returns>
        public readonly ValidationFunctionDelegate InvalidValueFormat = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP_ENTRY);

            string value = keyValueValidationEntry.KeyValueEntry.Value;

            ValueType? type = SyntaxValidationUtilityMethods.GetValueTypeFromString(value, syntaxConf);

            if (type is null)
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.InvalidValueFormat));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the value section contains excess whitespace, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the value section contains excess whitespace, null otherwise.</returns>
        public readonly ValidationFunctionDelegate ExcessWhitespaceInValue = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP_ENTRY);
            
            // Validation only matters if the value is a list of strings
            if (!SyntaxValidationUtilityMethods.IsStringListFormat(
                keyValueValidationEntry.KeyValueEntry.Value,
                syntaxConf.Symbols.Value.ListSeparator,
                syntaxConf.Symbols.Key.StringDelimeter
                ))
            {
                return null;
            }
            
            string value = keyValueValidationEntry.KeyValueEntry.Value;

            // Remove elements from the list. We only want to check whitespace between elements
            string stripItems = SyntaxValidationUtilityMethods.ExtractSectionFromString(value, syntaxConf.Symbols.Key.StringDelimeter).Remainder;
            if (stripItems.Contains("  "))
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.ExcessWhitespaceInValue));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the key contains excess whitespace, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key contains excess whitespace, null otherwise.</returns>
        public readonly ValidationFunctionDelegate KeyContainsExcessWhiteSpace = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP_ENTRY);

            string key = keyValueValidationEntry.KeyValueEntry.Key;
            string stripSpecifiers = SyntaxValidationUtilityMethods.ExtractSectionFromString(key, syntaxConf.Symbols.Key.StringDelimeter).Remainder;

            IEnumerable<char> whiteSpaces = stripSpecifiers.Where(c => c == ' ');
            if (!whiteSpaces.Any())
            {
                return null;
            }
            // There should be only one whitespace in the key part, separating specifier parts
            else if (whiteSpaces.Count() > 1)
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeyContainsExcessWhiteSpace));
            }
            // If whitespace is found without a comma separating the specifier parts, it is considered excess
            else if (!key.Contains($"{syntaxConf.Symbols.Key.ListSeparator} "))
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeyContainsExcessWhiteSpace));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the value section contains excess new lines, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the value section contains excess new lines, null otherwise.</returns>
        public readonly ValidationFunctionDelegate ExcessNewLinesInValue = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP_ENTRY);
            
            // We only need to run this validation if the value is less than 150 characters long and it is a string or list
            ValueType? type = SyntaxValidationUtilityMethods.GetValueTypeFromString(keyValueValidationEntry.KeyValueEntry.Value, syntaxConf);
            
            if (keyValueValidationEntry.KeyValueEntry.Value.Length > 150 ||
                (type != ValueType.String &&
                type != ValueType.ListOfStrings))
            {
                return null;
            }

            string value = keyValueValidationEntry.KeyValueEntry.Value;

            if (value.Contains(syntaxConf.Symbols.Linebreak))
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.ExcessNewLinesInValue));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the keyword contains illegal characters, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the keyword contains illegal characters, null otherwise.</returns>
        public readonly ValidationFunctionDelegate KeywordContainsIllegalCharacters = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_STRUCTURED_ENTRY);

            string keyword = structuredValidationEntry.Key.Keyword;
            // Missing keyword is catched earlier
            if (keyword.Length == 0)
            {
                return null;
            }

            // Check if keyword contains only letters, numbers, - and _
            string legalSymbolsKeyword = @"a-zA-Z0-9_-";
            // Define a timeout for the regex match
            TimeSpan timeout = TimeSpan.FromSeconds(1);
            // Find all illegal symbols in keyword
            try
            {
                MatchCollection matchesKeyword = Regex.Matches(keyword, $"[^{legalSymbolsKeyword}]", RegexOptions.None, timeout);
                List<string> illegalSymbolsInKeyWord = matchesKeyword.Cast<Match>().Select(m => m.Value).Distinct().ToList();
                if (illegalSymbolsInKeyWord.Count > 0)
                {
                    return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInKeyword, string.Join(", ", illegalSymbolsInKeyWord)));
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.KeyworRegexTimeout));
            }

            return null;
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the doesn't start with a letter, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the keyword doesn't start with a letter, null otherwise.</returns>
        public readonly ValidationFunctionDelegate KeywordDoesntStartWithALetter = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_STRUCTURED_ENTRY);

            string keyword = structuredValidationEntry.Key.Keyword;
            // Missing keyword is catched earlier
            if (keyword.Length == 0)
            {
                return null;
            }

            // Check if keyword starts with a letter
            if (!char.IsLetter(keyword[0]))
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.KeywordDoesntStartWithALetter));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the language parameter is not following a valid format, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the language parameter is not following a valid format, null otherwise.</returns>
        public readonly ValidationFunctionDelegate IllegalCharactersInLanguageParameter = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_STRUCTURED_ENTRY);
            // Running this validation is relevant only for entries with a language parameter
            if (structuredValidationEntry.Key.Language is null)
            {
                return null;
            }

            string lang = structuredValidationEntry.Key.Language;

            // Find illegal characters from language parameter string
            string illegalCharacters = @"[;=\[\]"" ]";
            TimeSpan timeout = TimeSpan.FromSeconds(1);
            // Find all illegal symbols in keyword
            try
            {
                MatchCollection matchesLang = Regex.Matches(lang, illegalCharacters, RegexOptions.None, timeout);
                List<string> foundIllegalCharacters = matchesLang.Cast<Match>().Select(m => m.Value).Distinct().ToList();
                // Check if there are any special characters or whitespace in lang
                if (foundIllegalCharacters.Count > 0)
                {
                    return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInLanguageParameter, string.Join(", ", foundIllegalCharacters)));
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.LanguageRegexTimeout));
            }

            return null;
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the specifier parameter is not following a valid format, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier parameter is not following a valid format, null otherwise.</returns>
        public readonly ValidationFunctionDelegate IllegalCharactersInSpecifierParameter = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_STRUCTURED_ENTRY);
            // Running this validation is relevant only for entries with a specifier
            if (structuredValidationEntry.Key.FirstSpecifier is null)
            {
                return null;
            }

            string[] illegalcharactersInFirstSpecifier = FindIllegalCharactersInSpecifierPart(structuredValidationEntry.Key.FirstSpecifier);
            string[] illegalcharactersInSecondSpecifier = FindIllegalCharactersInSpecifierPart(structuredValidationEntry.Key.SecondSpecifier);

            if (illegalcharactersInFirstSpecifier.Length > 0 || illegalcharactersInSecondSpecifier.Length > 0)
            {
                string[] characters = [.. illegalcharactersInFirstSpecifier, .. illegalcharactersInSecondSpecifier];
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInSpecifierParameter, string.Join(", ", characters)));
            }
            else
            {
                return null;
            }
        };

        private static string[] FindIllegalCharactersInSpecifierPart(string? specifier)
        {
            if (specifier is null)
            {
                return [];
            }

            string[] illegalCharacters = [";", "\"", ","];
            return illegalCharacters.Where(specifier.Contains).ToArray();
        }

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If there is no value section, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if there is no value section, null otherwise.</returns>
        public readonly ValidationFunctionDelegate EntryWithoutValue = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            StringValidationEntry? stringEntry = entry as StringValidationEntry ?? throw new ArgumentException("Entry is not of type StringValidationEntry");

            if (!stringEntry.EntryString.Contains(syntaxConf.Symbols.KeywordSeparator))
            {                 
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.EntryWithoutValue));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the language parameter is not compliant with ISO 639 or BCP 47, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the language parameter is not compliant with ISO 639 or BCP 47, null otherwise.</returns>
        public readonly ValidationFunctionDelegate IncompliantLanguage = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_STRUCTURED_ENTRY);
            // Running this validation is relevant only for entries with a language
            if (structuredValidationEntry.Key.Language is null)
            {
                return null;
            }
            
            string lang = structuredValidationEntry.Key.Language;

            bool iso639OrMsLcidCompliant = CultureInfo.GetCultures(CultureTypes.AllCultures).ToList().Exists(c => c.Name == lang || c.ThreeLetterISOLanguageName == lang);
            bool bcp47Compliant = Bcp47Codes.Codes.Contains(lang);

            if (iso639OrMsLcidCompliant || bcp47Compliant)
            {
                return null;
            }
            else
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.IncompliantLanguage));
            }

        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the keyword contains unrecommended characters, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the keyword contains unrecommended characters, null otherwise.</returns>
        public readonly ValidationFunctionDelegate KeywordContainsUnderscore = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_STRUCTURED_ENTRY);

            string keyword = structuredValidationEntry.Key.Keyword;
            if (keyword.Contains('_'))
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeywordContainsUnderscore));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the keyword is not in upper case, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the keyword is not in upper case, otherwise null</returns>
        public readonly ValidationFunctionDelegate KeywordIsNotInUpperCase = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_STRUCTURED_ENTRY);

            string keyword = structuredValidationEntry.Key.Keyword;
            string uppercaseKeyword = keyword.ToUpper();

            if (uppercaseKeyword != keyword)
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeywordIsNotInUpperCase));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the keyword is excessively long, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the keyword is excessively long, otherwise null</returns>
        public readonly ValidationFunctionDelegate KeywordIsExcessivelyLong = (ValidationEntry entry, PxFileSyntaxConf syntaxConf) =>
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_STRUCTURED_ENTRY);

            string keyword = structuredValidationEntry.Key.Keyword;

            if (keyword.Length > 20)
            {
                return new ValidationFeedbackItem(entry, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeywordExcessivelyLong));
            }
            else
            {
                return null;
            }
        };
    }
}
