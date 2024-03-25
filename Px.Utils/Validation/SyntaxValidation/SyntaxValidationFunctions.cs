using PxUtils.PxFile;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PxUtils.Validation.SyntaxValidation
{
    public delegate ValidationFeedbackItem? EntryValidationFunctionDelegate(ValidationEntry validationObject, PxFileSyntaxConf syntaxConf);
    public delegate ValidationFeedbackItem? KeyValuePairValidationFunctionDelegate(ValidationKeyValuePair validationObject, PxFileSyntaxConf syntaxConf);
    public delegate ValidationFeedbackItem? StructuredValidationFunctionDelegate(ValidationStructuredEntry validationObject, PxFileSyntaxConf syntaxConf);

    /// <summary>
    /// Contains the default validation functions for syntax validation.
    /// </summary>
    public class SyntaxValidationFunctions
    {
        public List<EntryValidationFunctionDelegate> DefaultStringValidationFunctions { get; }
        public List<KeyValuePairValidationFunctionDelegate> DefaultKeyValueValidationFunctions { get; }
        public List<StructuredValidationFunctionDelegate> DefaultStructuredValidationFunctions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxValidationFunctions"/> class with default validation functions.
        /// </summary>
        public SyntaxValidationFunctions()
        {
            DefaultStringValidationFunctions =
            [
                MultipleEntriesOnLine,
                EntryWithoutValue,
            ];

            DefaultKeyValueValidationFunctions =
            [
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
            ];

            DefaultStructuredValidationFunctions =
            [
                KeywordContainsIllegalCharacters,
                KeywordDoesntStartWithALetter,
                IllegalCharactersInLanguageParameter,
                IllegalCharactersInSpecifierParts,
                IncompliantLanguage,
                KeywordContainsUnderscore,
                KeywordIsNotInUpperCase,
                KeywordIsExcessivelyLong
            ];
        }

        private static readonly TimeSpan regexTimeout = TimeSpan.FromMilliseconds(50);
        private static readonly Regex keywordIllegalSymbolsRegex = new(@"[^a-zA-Z0-9_-]", RegexOptions.Compiled, regexTimeout);

        /// <summary>
        /// If <see cref="ValidationEntry"/> does not start with a line separator, it is considered as not being on its own line, and a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationEntry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the entry does not start with a line separator, null otherwise.</returns>
        public static ValidationFeedbackItem? MultipleEntriesOnLine (ValidationEntry validationEntry, PxFileSyntaxConf syntaxConf)
        {
            // If the entry does not start with a line separator, it is not on its own line. For the first entry this is not relevant.
            if (
                validationEntry.EntryIndex == 0 || 
                validationEntry.EntryString.StartsWith(CharacterConstants.UnixNewLine) ||
                validationEntry.EntryString.StartsWith(CharacterConstants.WindowsNewLine))
            {
                return null;
            }
            else
            {
                return new ValidationFeedbackItem(validationEntry, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.MultipleEntriesOnOneLine));
            }
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> key contains more than one language parameter, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key contains more than one language parameter, null otherwise.</returns>
        public static ValidationFeedbackItem? MoreThanOneLanguageParameter(ValidationKeyValuePair validationKeyValuePair, PxFileSyntaxConf syntaxConf)
        {
            bool hasMultipleParameters = SyntaxValidationUtilityMethods.HasMoreThanOneSection(
                validationKeyValuePair.KeyValuePair.Key,
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.LangParamEnd,
                syntaxConf
            );

            if (hasMultipleParameters)
            {
                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MoreThanOneLanguageParameterSection));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> key contains more than one specifier parameter, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key contains more than one specifier parameter, null otherwise.</returns>
        public static ValidationFeedbackItem? MoreThanOneSpecifierParameter(ValidationKeyValuePair validationKeyValuePair, PxFileSyntaxConf syntaxConf)
        {
            bool hasMultipleParameters = SyntaxValidationUtilityMethods.HasMoreThanOneSection(
                validationKeyValuePair.KeyValuePair.Key,
                syntaxConf.Symbols.Key.SpecifierParamStart,
                syntaxConf.Symbols.Key.SpecifierParamEnd,
                syntaxConf
                );

            if (hasMultipleParameters)
            {
                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MoreThanOneSpecifierParameterSection));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> key is not defined in the order of KEYWORD[language](\"specifier\"), a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key is not defined in the order of KEYWORD[language](\"specifier\"), null otherwise.</returns>
        public static ValidationFeedbackItem? WrongKeyOrderOrMissingKeyword(ValidationKeyValuePair validationKeyValuePair, PxFileSyntaxConf syntaxConf)
        {
            string key = validationKeyValuePair.KeyValuePair.Key;

            // Remove language parameter section if it exists
            string languageRemoved = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                key, 
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.Key.LangParamEnd)
            .Remainder;

            // Remove specifier section
            ExtractSectionResult specifierRemoved = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                languageRemoved, 
                syntaxConf.Symbols.Key.SpecifierParamStart,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.Key.SpecifierParamEnd);

            // Check for missing specifierRemoved. Keyword is the remainder of the string after removing language and specifier sections
            if (specifierRemoved.Remainder.Trim() == string.Empty)
            {
                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.MissingKeyword));
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
                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.KeyHasWrongOrder));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> key contains more than two specifiers, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if more than two specifiers are found, null otherwise.</returns>
        public static ValidationFeedbackItem? MoreThanTwoSpecifierParts(ValidationKeyValuePair validationKeyValuePair, PxFileSyntaxConf syntaxConf)
        {
            string? specifierParamSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    validationKeyValuePair.KeyValuePair.Key,
                                    syntaxConf.Symbols.Key.SpecifierParamStart,
                                    syntaxConf.Symbols.Key.StringDelimeter,
                                    syntaxConf.Symbols.Key.SpecifierParamEnd
                                ).Sections.FirstOrDefault();

            // If no parameter section is found, there are no specifiers
            if (specifierParamSection is null)
            {
                return null;
            }

            // Extract specifiers from the specifier section
            ExtractSectionResult specifierResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                specifierParamSection,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.Key.StringDelimeter
                );

            string[] specifiers = specifierResult.Sections;

            if (specifiers.Length > 2)
            {
                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.TooManySpecifiers));
            }

            return null;
        }

        /// <summary>
        /// If there is no delimeter between <see cref="ValidationKeyValuePair"/> specifier parts, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if there is no delimeter between specifier parts, null otherwise.</returns>
        public static ValidationFeedbackItem? NoDelimiterBetweenSpecifierParts(ValidationKeyValuePair validationKeyValuePair, PxFileSyntaxConf syntaxConf)
        {
            string? specifierParamSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    validationKeyValuePair.KeyValuePair.Key,
                                    syntaxConf.Symbols.Key.SpecifierParamStart,
                                    syntaxConf.Symbols.Key.StringDelimeter,
                                    syntaxConf.Symbols.Key.SpecifierParamEnd
                                ).Sections.FirstOrDefault();

            // If no specifier section is found, there are no specifiers
            if (specifierParamSection is null)
            {
                return null;
            }

            // Extract specifiers from the specifier section
            ExtractSectionResult specifierResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                specifierParamSection,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.Key.StringDelimeter
                );

            string[] specifiers = specifierResult.Sections;

            // If there are more than one specifier and the remainder of the specifier section does not contain a list separator, a delimiter is missing
            if (specifiers.Length > 1 && !specifierResult.Remainder.Contains(syntaxConf.Symbols.Key.ListSeparator))
            {
                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.SpecifierDelimiterMissing));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If any of the <see cref="ValidationKeyValuePair"/> specifiers are not enclosed a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if specifier parts are not enclosed, null otherwise.</returns>
        public static ValidationFeedbackItem? SpecifierPartNotEnclosed(ValidationKeyValuePair validationKeyValuePair, PxFileSyntaxConf syntaxConf)
        {
            string? specifierParamSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    validationKeyValuePair.KeyValuePair.Key,
                                    syntaxConf.Symbols.Key.SpecifierParamStart,
                                    syntaxConf.Symbols.Key.StringDelimeter,
                                    syntaxConf.Symbols.Key.SpecifierParamEnd
                                ).Sections.FirstOrDefault();

            // If specifier section is not found, there are no specifiers
            if (specifierParamSection == null)
            {
                return null;
            }

            // Check if specifiers are enclosed in string delimeters
            foreach (string specifier in specifierParamSection.Split(syntaxConf.Symbols.Key.ListSeparator))
            {
                string trimmedSpecifier = specifier.Trim();
                if (!trimmedSpecifier.StartsWith(syntaxConf.Symbols.Key.StringDelimeter) || !trimmedSpecifier.EndsWith(syntaxConf.Symbols.Key.StringDelimeter))
                {
                    return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.SpecifierPartNotEnclosed));
                }
            }
            return null;
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> key language section contains illegal symbols, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key language section contains illegal symbols, null otherwise.</returns>
        public static ValidationFeedbackItem? IllegalSymbolsInLanguageParamSection(ValidationKeyValuePair validationKeyValuePair, PxFileSyntaxConf syntaxConf)
        {
            string key = validationKeyValuePair.KeyValuePair.Key;

            string? languageParamSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    key,
                                    syntaxConf.Symbols.Key.LangParamStart,
                                    syntaxConf.Symbols.Key.StringDelimeter,
                                    syntaxConf.Symbols.Key.LangParamEnd
                                ).Sections.FirstOrDefault();

            // If language section is not found, the validation is not relevant
            if (languageParamSection == null)
            {
                return null;
            }

            char[] languageIllegalSymbols = [
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.LangParamEnd,
                syntaxConf.Symbols.Key.SpecifierParamStart,
                syntaxConf.Symbols.Key.SpecifierParamEnd,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.Key.ListSeparator
                ];

            IEnumerable<char> foundIllegalSymbols = languageParamSection.Where(c => languageIllegalSymbols.Contains(c));
            if (foundIllegalSymbols.Any())
            {
                string foundSymbols = string.Join(", ", foundIllegalSymbols);
                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInLanguageParameter, foundSymbols));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> key specifier section contains illegal symbols, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key specifier section contains illegal symbols, null otherwise.</returns>
        public static ValidationFeedbackItem? IllegalSymbolsInSpecifierParamSection(ValidationKeyValuePair validationKeyValuePair, PxFileSyntaxConf syntaxConf)
        {
            string key = validationKeyValuePair.KeyValuePair.Key;

            string? specifierParamSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    key,
                                    syntaxConf.Symbols.Key.SpecifierParamStart,
                                    syntaxConf.Symbols.Key.StringDelimeter,
                                    syntaxConf.Symbols.Key.SpecifierParamEnd
                                ).Sections.FirstOrDefault();

            // If specifier section is not found, there are no specifiers
            if (specifierParamSection == null)
            {
                return null;
            }

            char[] specifierParamIllegalSymbols = [
                syntaxConf.Symbols.EntrySeparator,
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.LangParamEnd
            ];

            IEnumerable<char> foundIllegalSymbols = specifierParamSection.Where(c => specifierParamIllegalSymbols.Contains(c));
            if (foundIllegalSymbols.Any())
            {
                string foundSymbols = string.Join(", ", foundIllegalSymbols);
                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInSpecifierParameter, foundSymbols));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> value section is not following a valid format, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the value section is not following a valid format, null otherwise.</returns>
        public static ValidationFeedbackItem? InvalidValueFormat(ValidationKeyValuePair validationKeyValuePair, PxFileSyntaxConf syntaxConf)
        {
            string value = validationKeyValuePair.KeyValuePair.Value;

            // Try to parse the value section to a ValueType
            ValueType? type = SyntaxValidationUtilityMethods.GetValueTypeFromString(value, syntaxConf);

            if (type is null)
            {
                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.InvalidValueFormat, value));
            }

            if (type is ValueType.String || type is ValueType.ListOfStrings)
            {
                if (SyntaxValidationUtilityMethods.ValueLineChangesAreCompliant(value, syntaxConf, type is ValueType.ListOfStrings))
                {
                    return null;
                }
                else
                {
                    return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.InvalidValueFormat, value));
                }
            }

            return null;
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> value section contains excess whitespace, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the value section contains excess whitespace, null otherwise.</returns>
        public static ValidationFeedbackItem? ExcessWhitespaceInValue(ValidationKeyValuePair validationKeyValuePair, PxFileSyntaxConf syntaxConf)
        {
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
            string stripItems = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                value, 
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.Key.StringDelimeter)
            .Remainder;
            
            if (
                stripItems.Contains($"{CharacterConstants.Space}{CharacterConstants.Space}") ||
                stripItems.Contains($"{CharacterConstants.HorizontalTab}")
                )
            {
                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.ExcessWhitespaceInValue));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> key contains excess whitespace, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key contains excess whitespace, null otherwise.</returns>
        public static ValidationFeedbackItem? KeyContainsExcessWhiteSpace(ValidationKeyValuePair validationKeyValuePair, PxFileSyntaxConf syntaxConf)
        {
            string key = validationKeyValuePair.KeyValuePair.Key;
            string stripSpecifiers = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                key,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.Key.StringDelimeter)
            .Remainder;

            IEnumerable<char> whiteSpaces = stripSpecifiers.Where(c => CharacterConstants.WhitespaceCharacters.Contains(c));
            if (!whiteSpaces.Any())
            {
                return null;
            }
            // There should be only one whitespace in the key part, separating specifier parts
            else if (whiteSpaces.Count() > 1)
            {
                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeyContainsExcessWhiteSpace));
            }
            // If whitespace is found without a comma separating the specifier parts, it is considered excess
            else if (!key.Contains($"{syntaxConf.Symbols.Key.ListSeparator}{CharacterConstants.Space}"))
            {
                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeyContainsExcessWhiteSpace));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> value section contains excess new lines, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the value section contains excess new lines, null otherwise.</returns>
        public static ValidationFeedbackItem? ExcessNewLinesInValue(ValidationKeyValuePair validationKeyValuePair, PxFileSyntaxConf syntaxConf)
        {
            // We only need to run this validation if the value is less than 150 characters long and it is a string or list
            ValueType? type = SyntaxValidationUtilityMethods.GetValueTypeFromString(validationKeyValuePair.KeyValuePair.Value, syntaxConf);
            
            if (validationKeyValuePair.KeyValuePair.Value.Length > 150 ||
                (type != ValueType.String &&
                type != ValueType.ListOfStrings))
            {
                return null;
            }

            string value = validationKeyValuePair.KeyValuePair.Value;

            if (value.Contains(CharacterConstants.WindowsNewLine) || value.Contains(CharacterConstants.UnixNewLine))
            {
                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.ExcessNewLinesInValue));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> specifier contains illegal characters, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructedEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier contains illegal characters, null otherwise.</returns>
        public static ValidationFeedbackItem? KeywordContainsIllegalCharacters(ValidationStructuredEntry validationStructedEntry, PxFileSyntaxConf syntaxConf)
        {
            string keyword = validationStructedEntry.Key.Keyword;
            // Missing specifier is catched earlier
            if (keyword.Length == 0)
            {
                return null;
            }

            // Find all illegal symbols in specifier
            try
            {
                MatchCollection matchesKeyword = keywordIllegalSymbolsRegex.Matches(keyword, 0);
                IEnumerable<string> illegalSymbolsInKeyWord = matchesKeyword.Cast<Match>().Select(m => m.Value).Distinct();
                if (illegalSymbolsInKeyWord.Any())
                {
                    return new ValidationFeedbackItem(validationStructedEntry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInKeyword, string.Join(", ", illegalSymbolsInKeyWord)));
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return new ValidationFeedbackItem(validationStructedEntry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.RegexTimeout));
            }

            return null;
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> keyword doesn't start with a letter, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructedEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier doesn't start with a letter, null otherwise.</returns>
        public static ValidationFeedbackItem? KeywordDoesntStartWithALetter(ValidationStructuredEntry validationStructedEntry, PxFileSyntaxConf syntaxConf)
        {
            string keyword = validationStructedEntry.Key.Keyword;

            // Check if keyword starts with a letter
            if (!char.IsLetter(keyword[0]))
            {
                return new ValidationFeedbackItem(validationStructedEntry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.KeywordDoesntStartWithALetter));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> language parameter is not following a valid format, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructedEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the language parameter is not following a valid format, null otherwise.</returns>
        public static ValidationFeedbackItem? IllegalCharactersInLanguageParameter(ValidationStructuredEntry validationStructedEntry, PxFileSyntaxConf syntaxConf)
        {
            // Running this validation is relevant only for objects with a language parameter
            if (validationStructedEntry.Key.Language is null)
            {
                return null;
            }

            string lang = validationStructedEntry.Key.Language;

            // Find illegal characters from language parameter string
            char[] illegalCharacters = [
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.LangParamEnd,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.EntrySeparator,
                syntaxConf.Symbols.KeywordSeparator,
                CharacterConstants.Space,
                CharacterConstants.CarriageReturn,
                CharacterConstants.LineFeed,
                CharacterConstants.HorizontalTab
            ];

            IEnumerable<char> foundIllegalCharacters = lang.Where(c => illegalCharacters.Contains(c));
            if (foundIllegalCharacters.Any())
            {
                string foundSymbols = string.Join(", ", foundIllegalCharacters);
                return new ValidationFeedbackItem(validationStructedEntry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInLanguageParameter, foundSymbols));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> specifier parameter is not following a valid format, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructedEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier parameter is not following a valid format, null otherwise.</returns>
        public static ValidationFeedbackItem? IllegalCharactersInSpecifierParts(ValidationStructuredEntry validationStructedEntry, PxFileSyntaxConf syntaxConf)
        {
            // Running this validation is relevant only for objects with a specifier
            if (validationStructedEntry.Key.FirstSpecifier is null)
            {
                return null;
            }

            char[] illegalCharacters = [syntaxConf.Symbols.EntrySeparator, syntaxConf.Symbols.Key.StringDelimeter, syntaxConf.Symbols.Key.ListSeparator];
            IEnumerable<char> illegalcharactersInFirstSpecifier = validationStructedEntry.Key.FirstSpecifier.Where(c => illegalCharacters.Contains(c));
            IEnumerable<char> illegalcharactersInSecondSpecifier = [];
            if (validationStructedEntry.Key.SecondSpecifier is not null)
            {
                illegalcharactersInSecondSpecifier = validationStructedEntry.Key.SecondSpecifier.Where(c => illegalCharacters.Contains(c));
            }

            if (illegalcharactersInFirstSpecifier.Any() || illegalcharactersInSecondSpecifier.Any())
            {
                char[] characters = [.. illegalcharactersInFirstSpecifier, .. illegalcharactersInSecondSpecifier];
                return new ValidationFeedbackItem(validationStructedEntry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.IllegalCharactersInSpecifierPart, string.Join(", ", characters)));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If there is no <see cref="ValidationEntry"/> value section, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationEntry">The <see cref="ValidationEntry"/> validationKeyValuePair to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if there is no value section, null otherwise.</returns>
        public static ValidationFeedbackItem? EntryWithoutValue(ValidationEntry validationEntry, PxFileSyntaxConf syntaxConf)
        {
            int[] keywordSeparatorIndeces = SyntaxValidationUtilityMethods.FindKeywordSeparatorIndeces(validationEntry.EntryString, syntaxConf);

            if (keywordSeparatorIndeces.Length == 0)
            {                 
                return new ValidationFeedbackItem(validationEntry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.EntryWithoutValue));
            }
            else if (keywordSeparatorIndeces.Length > 1)
            {
                return new ValidationFeedbackItem(validationEntry, new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.EntryWithMultipleValues));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> language parameter is not compliant with ISO 639 or BCP 47, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructedEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the language parameter is not compliant with ISO 639 or BCP 47, null otherwise.</returns>
        public static ValidationFeedbackItem? IncompliantLanguage (ValidationStructuredEntry validationStructedEntry, PxFileSyntaxConf syntaxConf)
        {
            // Running this validation is relevant only for objects with a language
            if (validationStructedEntry.Key.Language is null)
            {
                return null;
            }
            
            string lang = validationStructedEntry.Key.Language;

            bool iso639OrMsLcidCompliant = CultureInfo.GetCultures(CultureTypes.AllCultures).ToList().Exists(c => c.Name == lang || c.ThreeLetterISOLanguageName == lang);
            bool bcp47Compliant = Bcp47Codes.Codes.Contains(lang);

            if (iso639OrMsLcidCompliant || bcp47Compliant)
            {
                return null;
            }
            else
            {
                return new ValidationFeedbackItem(validationStructedEntry, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.IncompliantLanguage));
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> specifier contains unrecommended characters, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructedEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier contains unrecommended characters, null otherwise.</returns>
        public static ValidationFeedbackItem? KeywordContainsUnderscore (ValidationStructuredEntry validationStructedEntry, PxFileSyntaxConf syntaxConf)
        {
            string keyword = validationStructedEntry.Key.Keyword;
            if (keyword.Contains('_'))
            {
                return new ValidationFeedbackItem(validationStructedEntry, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeywordContainsUnderscore));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> specifier is not in upper case, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructedEntry">The <see cref="ValidationStructuredEntry"/> to validate</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier is not in upper case, otherwise null</returns>
        public static ValidationFeedbackItem? KeywordIsNotInUpperCase(ValidationStructuredEntry validationStructedEntry, PxFileSyntaxConf syntaxConf)
        {
            string keyword = validationStructedEntry.Key.Keyword;
            string uppercaseKeyword = keyword.ToUpper();

            if (uppercaseKeyword != keyword)
            {
                return new ValidationFeedbackItem(validationStructedEntry, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeywordIsNotInUpperCase));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> specifier is excessively long, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructedEntry">The <see cref="ValidationStructuredEntry"/> object to validate</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier is excessively long, otherwise null</returns>
        public static ValidationFeedbackItem? KeywordIsExcessivelyLong(ValidationStructuredEntry validationStructedEntry, PxFileSyntaxConf syntaxConf)
        {
            string keyword = validationStructedEntry.Key.Keyword;

            if (keyword.Length > 20)
            {
                return new ValidationFeedbackItem(validationStructedEntry, new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.KeywordExcessivelyLong));
            }
            else
            {
                return null;
            }
        }
    }
}
