using PxUtils.PxFile;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PxUtils.Validation.SyntaxValidation
{
    public delegate ValidationFeedbackItem? ValidationFunctionDelegate(ValidationObject validationObject, PxFileSyntaxConf syntaxConf);

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
                IllegalCharactersInSpecifierParts,
                IncompliantLanguage,
                KeywordContainsUnderscore,
                KeywordIsNotInUpperCase,
                KeywordIsExcessivelyLong
            };
        }

        private const string ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP = "ValidationObject is not of type ValidationKeyValuePair";
        private const string ARGUMENT_EXCEPTION_MESSAGE_NOT_A_STRING_ENTRY = "ValidationObject is not of type ValidationEntry";
        private const string ARGUMENT_EXCEPTION_MESSAGE_NOT_A_VALIDATIONSTRUCT = "ValidationObject is not of type ValidationStruct";

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> If the entry does not start with a line separator, it is considered as not being on its own line, and a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the entry does not start with a line separator, null otherwise.</returns>
        public static ValidationFunctionDelegate MultipleEntriesOnLine { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationEntry? validationEntry = validationObject as ValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_STRING_ENTRY);

            // If the entry does not start with a line separator, it is not on its own line. For the first entry this is not relevant.
            if (
            validationEntry.EntryIndex == 0 || 
            validationEntry.EntryString.StartsWith(syntaxConf.Symbols.Linebreak) ||
            validationEntry.EntryString.StartsWith(syntaxConf.Symbols.CarriageReturn))
            {
                return null;
            }
            else
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.MultipleEntriesOnOneLine));
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the key contains more than one language parameter, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key contains more than one language parameter, null otherwise.</returns>
        public static ValidationFunctionDelegate MoreThanOneLanguageParameter { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationKeyValuePair? validationKeyValuePair = validationObject as ValidationKeyValuePair ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP);

            bool hasMultipleParameters = SyntaxValidationUtilityMethods.HasMoreThanOneSection(
                validationKeyValuePair.KeyValuePair.Key,
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.LangParamEnd,
                syntaxConf
            );

            if (hasMultipleParameters)
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MoreThanOneLanguageParameterSection));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the key contains more than one specifier parameter, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key contains more than one specifier parameter, null otherwise.</returns>
        public static ValidationFunctionDelegate MoreThanOneSpecifierParameter { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationKeyValuePair? validationKeyValuePair = validationObject as ValidationKeyValuePair ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP);

            bool hasMultipleParameters = SyntaxValidationUtilityMethods.HasMoreThanOneSection(
                validationKeyValuePair.KeyValuePair.Key,
                syntaxConf.Symbols.Key.SpecifierParamStart,
                syntaxConf.Symbols.Key.SpecifierParamEnd,
                syntaxConf
                );

            if (hasMultipleParameters)
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MoreThanOneSpecifierParameterSection));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the key is not defined in the order of KEYWORD[language](\"specifier\"), a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key is not defined in the order of KEYWORD[language](\"specifier\"), null otherwise.</returns>
        public static ValidationFunctionDelegate WrongKeyOrderOrMissingKeyword { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationKeyValuePair? validationKeyValuePair = validationObject as ValidationKeyValuePair ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP);

            string key = validationKeyValuePair.KeyValuePair.Key;

            // Remove language parameter section if it exists
            string languageRemoved = SyntaxValidationUtilityMethods.ExtractSectionFromString(key, syntaxConf.Symbols.Key.LangParamStart, syntaxConf, syntaxConf.Symbols.Key.LangParamEnd).Remainder;
            // Same for specifier section
            string keyword = SyntaxValidationUtilityMethods.ExtractSectionFromString(languageRemoved, syntaxConf.Symbols.Key.SpecifierParamStart, syntaxConf, syntaxConf.Symbols.Key.SpecifierParamEnd).Remainder;

            // Check for missing keyword
            if (keyword.Trim() == string.Empty)
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingKeyword));
            }

            // Check where language and parameter sections start
            int langParamStartIndex = key.IndexOf(syntaxConf.Symbols.Key.LangParamStart);
            int specifierParamStartIndex = key.IndexOf(syntaxConf.Symbols.Key.SpecifierParamStart);

            // If the key contains no language or specifier section, it is in the correct order
            if (langParamStartIndex == -1 && specifierParamStartIndex == -1)
            {
                return null;
            }
            // Check if the specifier section comes before the language section or that the key starts with a specifier or language section
            else if (
                specifierParamStartIndex != -1 && langParamStartIndex > specifierParamStartIndex ||
                key.Trim().StartsWith(syntaxConf.Symbols.Key.SpecifierParamStart) ||
                key.Trim().StartsWith(syntaxConf.Symbols.Key.LangParamStart)
                )
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.KeyHasWrongOrder));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the key contains more than two specifiers, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if more than two specifiers are found, null otherwise.</returns>
        public static ValidationFunctionDelegate MoreThanTwoSpecifierParts { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationKeyValuePair? validationKeyValuePair = validationObject as ValidationKeyValuePair ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP);

            string? specifierParamSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    validationKeyValuePair.KeyValuePair.Key,
                                    syntaxConf.Symbols.Key.SpecifierParamStart,
                                    syntaxConf,
                                    syntaxConf.Symbols.Key.SpecifierParamEnd
                                ).Sections.FirstOrDefault();

            if (specifierParamSection is null)
            {
                return null;
            }

            ExtractSectionResult specifierResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                specifierParamSection,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf
                );

            string[] specifiers = specifierResult.Sections;

            if (specifiers.Length > 2)
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.TooManySpecifiers));
            }

            return null;
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If there is no delimeter between specifier parts, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if there is no delimeter between specifier parts, null otherwise.</returns>
        public static ValidationFunctionDelegate NoDelimiterBetweenSpecifierParts { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationKeyValuePair? validationKeyValuePair = validationObject as ValidationKeyValuePair ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP);

            string? specifierParamSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    validationKeyValuePair.KeyValuePair.Key,
                                    syntaxConf.Symbols.Key.SpecifierParamStart,
                                    syntaxConf,
                                    syntaxConf.Symbols.Key.SpecifierParamEnd
                                ).Sections.FirstOrDefault();

            if (specifierParamSection is null)
            {
                return null;
            }

            ExtractSectionResult specifierResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                specifierParamSection,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf
                );

            string[] specifiers = specifierResult.Sections;

            if (specifiers.Length > 1 && !specifierResult.Remainder.Contains(syntaxConf.Symbols.Key.ListSeparator))
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.SpecifierDelimiterMissing));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If any of the specifiers are not enclosed a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if specifier parts are not enclosed, null otherwise.</returns>
        public static ValidationFunctionDelegate SpecifierPartNotEnclosed { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationKeyValuePair? validationKeyValuePair = validationObject as ValidationKeyValuePair ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP);

            string? specifierParamSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    validationKeyValuePair.KeyValuePair.Key,
                                    syntaxConf.Symbols.Key.SpecifierParamStart,
                                    syntaxConf,
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
                    return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.SpecifierPartNotEnclosed));
                }
            }

            return null;
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the key language section contains illegal symbols, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key language section contains illegal symbols, null otherwise.</returns>
        public static ValidationFunctionDelegate IllegalSymbolsInLanguageParamSection { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationKeyValuePair? validationKeyValuePair = validationObject as ValidationKeyValuePair ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP);

            string key = validationKeyValuePair.KeyValuePair.Key;

            string languageParamSections = string.Join(' ', SyntaxValidationUtilityMethods.ExtractSectionFromString(key, syntaxConf.Symbols.Key.LangParamStart, syntaxConf, syntaxConf.Symbols.Key.LangParamEnd).Sections);

            char[] languageParamIllegalSymbols = [
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.SpecifierParamStart,
                syntaxConf.Symbols.Key.SpecifierParamEnd,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.Key.ListSeparator
                ];

            // If FindIllegalCharactersInString returns false, it means that the regex has timed out
            if (SyntaxValidationUtilityMethods.FindIllegalCharactersInString(languageParamSections, languageParamIllegalSymbols, out char[] foundIllegalSymbols))
            {
                if (foundIllegalSymbols.Length > 0)
                {
                    string foundSymbols = string.Join(", ", foundIllegalSymbols);
                    return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInLanguageParameter, foundSymbols));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.RegexTimeout, languageParamSections));
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the key specifier section contains illegal symbols, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key specifier section contains illegal symbols, null otherwise.</returns>
        public static ValidationFunctionDelegate IllegalSymbolsInSpecifierParamSection { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationKeyValuePair? validationKeyValuePair = validationObject as ValidationKeyValuePair ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP);

            string key = validationKeyValuePair.KeyValuePair.Key;

            string specifierParamSections = string.Join(' ', SyntaxValidationUtilityMethods.ExtractSectionFromString(key, syntaxConf.Symbols.Key.SpecifierParamStart, syntaxConf, syntaxConf.Symbols.Key.SpecifierParamEnd).Sections);

            char[] specifierParamIllegalSymbols = [
                syntaxConf.Symbols.EntrySeparator,
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.LangParamEnd
            ];

            // If FindIllegalCharactersInString returns false, it means that the regex has timed out
            if (SyntaxValidationUtilityMethods.FindIllegalCharactersInString(specifierParamSections, specifierParamIllegalSymbols, out char[] foundIllegalSymbols))
            {
                if (foundIllegalSymbols.Length > 0)
                {
                    string foundSymbols = string.Join(", ", foundIllegalSymbols);
                    return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInSpecifierParameter, foundSymbols));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.RegexTimeout, specifierParamSections));
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the value section is not following a valid format, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the value section is not following a valid format, null otherwise.</returns>
        public static ValidationFunctionDelegate InvalidValueFormat { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationKeyValuePair? validationKeyValuePair = validationObject as ValidationKeyValuePair ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP);

            string value = validationKeyValuePair.KeyValuePair.Value;

            ValueType? type = SyntaxValidationUtilityMethods.GetValueTypeFromString(value, syntaxConf);

            if (type is null)
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.InvalidValueFormat, value));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the value section contains excess whitespace, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the value section contains excess whitespace, null otherwise.</returns>
        public static ValidationFunctionDelegate ExcessWhitespaceInValue { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationKeyValuePair? validationKeyValuePair = validationObject as ValidationKeyValuePair ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP);
            
            // Validation only matters if the value is a list of strings
            if (!SyntaxValidationUtilityMethods.IsStringListFormat(
                validationKeyValuePair.KeyValuePair.Value,
                syntaxConf.Symbols.Value.ListSeparator,
                syntaxConf.Symbols.Key.StringDelimeter
                ))
            {
                return null;
            }
            
            string value = validationKeyValuePair.KeyValuePair.Value;

            // Remove elements from the list. We only want to check whitespace between elements
            string stripItems = SyntaxValidationUtilityMethods.ExtractSectionFromString(value, syntaxConf.Symbols.Key.StringDelimeter, syntaxConf).Remainder;
            if (
                stripItems.Contains($"{syntaxConf.Symbols.Space}{syntaxConf.Symbols.Space}") ||
                stripItems.Contains($"{syntaxConf.Symbols.HorizontalTab}") ||
                stripItems.Contains($"{syntaxConf.Symbols.CarriageReturn}") ||
                stripItems.Contains($"{syntaxConf.Symbols.LineFeed}")
                )
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.ExcessWhitespaceInValue));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the key contains excess whitespace, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key contains excess whitespace, null otherwise.</returns>
        public static ValidationFunctionDelegate KeyContainsExcessWhiteSpace { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationKeyValuePair? validationKeyValuePair = validationObject as ValidationKeyValuePair ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP);

            string key = validationKeyValuePair.KeyValuePair.Key;
            string stripSpecifiers = SyntaxValidationUtilityMethods.ExtractSectionFromString(key, syntaxConf.Symbols.Key.StringDelimeter, syntaxConf).Remainder;

            IEnumerable<char> whiteSpaces = stripSpecifiers.Where(c => syntaxConf.Symbols.WhitespaceCharacters.Contains(c));
            if (!whiteSpaces.Any())
            {
                return null;
            }
            // There should be only one whitespace in the key part, separating specifier parts
            else if (whiteSpaces.Count() > 1)
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeyContainsExcessWhiteSpace));
            }
            // If whitespace is found without a comma separating the specifier parts, it is considered excess
            else if (!key.Contains($"{syntaxConf.Symbols.Key.ListSeparator} "))
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeyContainsExcessWhiteSpace));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the value section contains excess new lines, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the value section contains excess new lines, null otherwise.</returns>
        public static ValidationFunctionDelegate ExcessNewLinesInValue { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationKeyValuePair? validationKeyValuePair = validationObject as ValidationKeyValuePair ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_KVP);
            
            // We only need to run this validation if the value is less than 150 characters long and it is a string or list
            ValueType? type = SyntaxValidationUtilityMethods.GetValueTypeFromString(validationKeyValuePair.KeyValuePair.Value, syntaxConf);
            
            if (validationKeyValuePair.KeyValuePair.Value.Length > 150 ||
                (type != ValueType.String &&
                type != ValueType.ListOfStrings))
            {
                return null;
            }

            string value = validationKeyValuePair.KeyValuePair.Value;

            if (value.Contains(syntaxConf.Symbols.Linebreak) || value.Contains(syntaxConf.Symbols.CarriageReturn))
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.ExcessNewLinesInValue));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the keyword contains illegal characters, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the keyword contains illegal characters, null otherwise.</returns>
        public static ValidationFunctionDelegate KeywordContainsIllegalCharacters { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationStruct? validationStruct = validationObject as ValidationStruct ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_VALIDATIONSTRUCT);

            string keyword = validationStruct.Key.Keyword;
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
                    return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInKeyword, string.Join(", ", illegalSymbolsInKeyWord)));
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.RegexTimeout));
            }

            return null;
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the doesn't start with a letter, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the keyword doesn't start with a letter, null otherwise.</returns>
        public static ValidationFunctionDelegate KeywordDoesntStartWithALetter { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationStruct? validationStruct = validationObject as ValidationStruct ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_VALIDATIONSTRUCT);

            string keyword = validationStruct.Key.Keyword;
            // Missing keyword is catched earlier
            if (keyword.Length == 0)
            {
                return null;
            }

            // Check if keyword starts with a letter
            if (!char.IsLetter(keyword[0]))
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.KeywordDoesntStartWithALetter));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the language parameter is not following a valid format, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the language parameter is not following a valid format, null otherwise.</returns>
        public static ValidationFunctionDelegate IllegalCharactersInLanguageParameter { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationStruct? validationStruct = validationObject as ValidationStruct ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_VALIDATIONSTRUCT);
            // Running this validation is relevant only for objects with a language parameter
            if (validationStruct.Key.Language is null)
            {
                return null;
            }

            string lang = validationStruct.Key.Language;

            // Find illegal characters from language parameter string
            char[] illegalCharacters = [
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.LangParamEnd,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.EntrySeparator,
                syntaxConf.Symbols.KeywordSeparator,
                syntaxConf.Symbols.Space,
                syntaxConf.Symbols.CarriageReturn,
                syntaxConf.Symbols.LineFeed,
                syntaxConf.Symbols.HorizontalTab
            ];

            // If FindIllegalCharactersInString returns false, it means that the regex has timed out
            if (SyntaxValidationUtilityMethods.FindIllegalCharactersInString(lang, illegalCharacters, out char[] foundIllegalCharacters))
            {
                if (foundIllegalCharacters.Length > 0)
                {
                    string foundSymbols = string.Join(", ", foundIllegalCharacters);
                    return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInLanguageParameter, foundSymbols));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.RegexTimeout, lang));
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the specifier parameter is not following a valid format, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier parameter is not following a valid format, null otherwise.</returns>
        public static ValidationFunctionDelegate IllegalCharactersInSpecifierParts { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationStruct? validationStruct = validationObject as ValidationStruct ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_VALIDATIONSTRUCT);
            // Running this validation is relevant only for objects with a specifier
            if (validationStruct.Key.FirstSpecifier is null)
            {
                return null;
            }

            char[] illegalCharacters = [syntaxConf.Symbols.EntrySeparator, syntaxConf.Symbols.Key.StringDelimeter, syntaxConf.Symbols.Key.ListSeparator];
            bool firstSpecifierChecked = FindIllegalCharactersInSpecifierPart(validationStruct.Key.FirstSpecifier, illegalCharacters, out char[] illegalcharactersInFirstSpecifier);
            bool secondSpecifierChecked = FindIllegalCharactersInSpecifierPart(validationStruct.Key.SecondSpecifier, illegalCharacters, out char[] illegalcharactersInSecondSpecifier);

            if (!firstSpecifierChecked || !secondSpecifierChecked)
            {
                StringBuilder specifiersThatFailedRegex = firstSpecifierChecked
                    ? new StringBuilder()
                    : new StringBuilder(validationStruct.Key.FirstSpecifier);
                if (!secondSpecifierChecked)
                {
                    if (specifiersThatFailedRegex.Length > 0)
                    {
                        specifiersThatFailedRegex.Append(' ');
                    }
                    specifiersThatFailedRegex.Append(validationStruct.Key.SecondSpecifier);
                }

                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.RegexTimeout, specifiersThatFailedRegex.ToString()));
            }

            if (illegalcharactersInFirstSpecifier.Length > 0 || illegalcharactersInSecondSpecifier.Length > 0)
            {
                char[] characters = [.. illegalcharactersInFirstSpecifier, .. illegalcharactersInSecondSpecifier];
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInSpecifierPart, string.Join(", ", characters)));
            }
            else
            {
                return null;
            }
        };

        private static bool FindIllegalCharactersInSpecifierPart(string? specifier, char[] illegalCharacters, out char[] foundIllegalCharacters)
        {
            if (specifier is null)
            {
                foundIllegalCharacters = [];
                return true;
            }

            // If FindIllegalCharactersInString returns false, it means that the regex has timed out
            if (SyntaxValidationUtilityMethods.FindIllegalCharactersInString(specifier, illegalCharacters, out foundIllegalCharacters))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If there is no value section, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if there is no value section, null otherwise.</returns>
        public static ValidationFunctionDelegate EntryWithoutValue { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationEntry? stringEntry = validationObject as ValidationEntry ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_STRING_ENTRY);

            int[] keywordSeparatorIndeces = SyntaxValidationUtilityMethods.FindKeywordSeparatorIndeces(stringEntry.EntryString, syntaxConf);

            if (keywordSeparatorIndeces.Length == 0)
            {                 
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.EntryWithoutValue));
            }
            else if (keywordSeparatorIndeces.Length > 1)
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.EntryWithMultipleValues));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the language parameter is not compliant with ISO 639 or BCP 47, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the language parameter is not compliant with ISO 639 or BCP 47, null otherwise.</returns>
        public static ValidationFunctionDelegate IncompliantLanguage { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationStruct? validationStruct = validationObject as ValidationStruct ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_VALIDATIONSTRUCT);
            // Running this validation is relevant only for objects with a language
            if (validationStruct.Key.Language is null)
            {
                return null;
            }
            
            string lang = validationStruct.Key.Language;

            bool iso639OrMsLcidCompliant = CultureInfo.GetCultures(CultureTypes.AllCultures).ToList().Exists(c => c.Name == lang || c.ThreeLetterISOLanguageName == lang);
            bool bcp47Compliant = Bcp47Codes.Codes.Contains(lang);

            if (iso639OrMsLcidCompliant || bcp47Compliant)
            {
                return null;
            }
            else
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.IncompliantLanguage));
            }

        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the keyword contains unrecommended characters, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the keyword contains unrecommended characters, null otherwise.</returns>
        public static ValidationFunctionDelegate KeywordContainsUnderscore { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationStruct? validationStruct = validationObject as ValidationStruct ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_VALIDATIONSTRUCT);

            string keyword = validationStruct.Key.Keyword;
            if (keyword.Contains('_'))
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeywordContainsUnderscore));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationObject"/> validationObject. If the keyword is not in upper case, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> validationObject to validate</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the keyword is not in upper case, otherwise null</returns>
        public static ValidationFunctionDelegate KeywordIsNotInUpperCase { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationStruct? validationStruct = validationObject as ValidationStruct ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_VALIDATIONSTRUCT);

            string keyword = validationStruct.Key.Keyword;
            string uppercaseKeyword = keyword.ToUpper();

            if (uppercaseKeyword != keyword)
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeywordIsNotInUpperCase));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationStruct"/> object. If the keyword is excessively long, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationObject">The <see cref="ValidationObject"/> object to validate</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the keyword is excessively long, otherwise null</returns>
        public static ValidationFunctionDelegate KeywordIsExcessivelyLong { get; } = (ValidationObject validationObject, PxFileSyntaxConf syntaxConf) =>
        {
            ValidationStruct? validationStruct = validationObject as ValidationStruct ?? throw new ArgumentException(ARGUMENT_EXCEPTION_MESSAGE_NOT_A_VALIDATIONSTRUCT);

            string keyword = validationStruct.Key.Keyword;

            if (keyword.Length > 20)
            {
                return new ValidationFeedbackItem(validationObject, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeywordExcessivelyLong));
            }
            else
            {
                return null;
            }
        };
    }
}
