using PxUtils.PxFile;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PxUtils.Validation.SyntaxValidation
{
    public delegate ValidationFeedbackItem? EntryValidationFunction(ValidationEntry validationObject, PxFileSyntaxConf syntaxConf);
    public delegate ValidationFeedbackItem? KeyValuePairValidationFunction(ValidationKeyValuePair validationObject, PxFileSyntaxConf syntaxConf);
    public delegate ValidationFeedbackItem? StructuredValidationFunction(ValidationStructuredEntry validationObject, PxFileSyntaxConf syntaxConf);

    /// <summary>
    /// Contains the default validation functions for syntax validation.
    /// </summary>
    public class SyntaxValidationFunctions
    {
        public List<EntryValidationFunction> DefaultStringValidationFunctions { get; }
        public List<KeyValuePairValidationFunction> DefaultKeyValueValidationFunctions { get; }
        public List<StructuredValidationFunction> DefaultStructuredValidationFunctions { get; }

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
                validationEntry.EntryString.StartsWith(CharacterConstants.UnixNewLine, StringComparison.Ordinal) ||
                validationEntry.EntryString.StartsWith(CharacterConstants.WindowsNewLine, StringComparison.Ordinal))
            {
                return null;
            }
            else
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationEntry.KeyStartLineIndex,
                    0,
                    validationEntry.LineChangeIndexes);

                return new ValidationFeedbackItem(validationEntry, new ValidationFeedback(
                    ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.MultipleEntriesOnOneLine,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value
                ));
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
            ExtractSectionResult languageSections = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                validationKeyValuePair.KeyValuePair.Key,
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.Key.LangParamEnd);

            if (languageSections.Sections.Length > 1)
            {
                KeyValuePair<int, int> lineAndCharacter = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    languageSections.StartIndexes[1],
                    validationKeyValuePair.LineChangeIndexes);

                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.MoreThanOneLanguageParameterSection,
                    lineAndCharacter.Key,
                    lineAndCharacter.Value
                ));
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
            ExtractSectionResult specifierSpections = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                validationKeyValuePair.KeyValuePair.Key,
                syntaxConf.Symbols.Key.SpecifierParamStart,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.Key.SpecifierParamEnd);

            if (specifierSpections.Sections.Length > 1)
            {
                KeyValuePair<int, int> lineAndCharacter = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    specifierSpections.StartIndexes[1],
                    validationKeyValuePair.LineChangeIndexes);

                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.MoreThanOneSpecifierParameterSection,
                    lineAndCharacter.Key,
                    lineAndCharacter.Value
                    ));
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
            ExtractSectionResult languageSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                key,
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.Key.LangParamEnd);

            string languageRemoved = languageSection.Remainder;

            // Remove specifier section
            ExtractSectionResult specifierRemoved = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                languageRemoved, 
                syntaxConf.Symbols.Key.SpecifierParamStart,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.Key.SpecifierParamEnd);

            // Check for missing specifierRemoved. Keyword is the remainder of the string after removing language and specifier sections
            if (specifierRemoved.Remainder.Trim() == string.Empty)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    0,
                    validationKeyValuePair.LineChangeIndexes);

                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.MissingKeyword,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value));
            }

            // Check where language and parameter sections start
            Dictionary<int, int> stringSections = SyntaxValidationUtilityMethods.GetEnclosingCharacterIndexes(key, syntaxConf.Symbols.Key.StringDelimeter);
            int langParamStartIndex = SyntaxValidationUtilityMethods.FindSymbolIndex(key, syntaxConf.Symbols.Key.LangParamStart, stringSections);
            int specifierParamStartIndex = SyntaxValidationUtilityMethods.FindSymbolIndex(key, syntaxConf.Symbols.Key.SpecifierParamStart, stringSections);

            // If the key contains no language or specifier section, it is in the correct order
            if (langParamStartIndex == -1 && specifierParamStartIndex == -1)
            {
                return null;
            }
            // Check if the specifier section comes before the language 
            else if (specifierParamStartIndex != -1 && langParamStartIndex > specifierParamStartIndex)
            {
                KeyValuePair<int, int> specifierIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    specifierParamStartIndex,
                    validationKeyValuePair.LineChangeIndexes
                    );

                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.KeyHasWrongOrder,
                    specifierIndexes.Key,
                    specifierIndexes.Value
                    ));
            }
            // Check if the key starts with a language or specifier parameter section
            else if (
                key.Trim().StartsWith(syntaxConf.Symbols.Key.SpecifierParamStart) ||
                key.Trim().StartsWith(syntaxConf.Symbols.Key.LangParamStart))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    0,
                    validationKeyValuePair.LineChangeIndexes
                    );

                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.KeyHasWrongOrder,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value
                    ));
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
            ExtractSectionResult specifierSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    validationKeyValuePair.KeyValuePair.Key,
                                    syntaxConf.Symbols.Key.SpecifierParamStart,
                                    syntaxConf.Symbols.Key.StringDelimeter,
                                    syntaxConf.Symbols.Key.SpecifierParamEnd
                                );

            string? specifierParamSection = specifierSection.Sections.FirstOrDefault();

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

            if (specifierResult.Sections.Length > 2)
            {
                KeyValuePair<int, int> secondSpecifierIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    specifierSection.StartIndexes[0] + specifierResult.StartIndexes[2],
                    validationKeyValuePair.LineChangeIndexes
                    );

                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.TooManySpecifiers,
                    secondSpecifierIndexes.Key,
                    secondSpecifierIndexes.Value
                    ));
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
            ExtractSectionResult specifierSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    validationKeyValuePair.KeyValuePair.Key,
                                    syntaxConf.Symbols.Key.SpecifierParamStart,
                                    syntaxConf.Symbols.Key.StringDelimeter,
                                    syntaxConf.Symbols.Key.SpecifierParamEnd
                                );

            string? specifierParamSection = specifierSection.Sections.FirstOrDefault();

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
                KeyValuePair<int, int> specifierIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    specifierSection.StartIndexes[0] + specifierResult.StartIndexes[1],
                    validationKeyValuePair.LineChangeIndexes);

                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.SpecifierDelimiterMissing,
                    specifierIndexes.Key,
                    specifierIndexes.Value));
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
            ExtractSectionResult specifierSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    validationKeyValuePair.KeyValuePair.Key,
                                    syntaxConf.Symbols.Key.SpecifierParamStart,
                                    syntaxConf.Symbols.Key.StringDelimeter,
                                    syntaxConf.Symbols.Key.SpecifierParamEnd
                                );

            string? specifierParamSection = specifierSection.Sections.FirstOrDefault();

            // If specifier section is not found, there are no specifiers
            if (specifierParamSection == null)
            {
                return null;
            }

            string[] specifiers = specifierParamSection.Split(syntaxConf.Symbols.Key.ListSeparator);
            // Check if specifiers are enclosed in string delimeters
            for (int i = 0; i < specifiers.Length; i++)
            {
                string specifier = specifiers[i];
                string trimmedSpecifier = specifier.Trim();
                if (!trimmedSpecifier.StartsWith(syntaxConf.Symbols.Key.StringDelimeter) || !trimmedSpecifier.EndsWith(syntaxConf.Symbols.Key.StringDelimeter))
                {
                    KeyValuePair<int, int> specifierIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                        validationKeyValuePair.KeyStartLineIndex,
                        specifierSection.StartIndexes[0],
                        validationKeyValuePair.LineChangeIndexes
                    );

                    return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                        ValidationFeedbackLevel.Error, 
                        ValidationFeedbackRule.SpecifierPartNotEnclosed,
                        specifierIndexes.Key,
                        specifierIndexes.Value
                        ));
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

            ExtractSectionResult languageParam = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    key,
                                    syntaxConf.Symbols.Key.LangParamStart,
                                    syntaxConf.Symbols.Key.StringDelimeter,
                                    syntaxConf.Symbols.Key.LangParamEnd
                                );

            string? languageParamSection = languageParam.Sections.FirstOrDefault();

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
                int indexOfFirstIllegalSymbol = languageParam.StartIndexes[0] + languageParamSection.IndexOf(foundIllegalSymbols.First());
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    languageParam.StartIndexes[0] + indexOfFirstIllegalSymbol,
                    validationKeyValuePair.LineChangeIndexes);

                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.IllegalCharactersInLanguageParameter,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value,
                    foundSymbols));
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

            ExtractSectionResult specifierParam = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    key,
                                    syntaxConf.Symbols.Key.SpecifierParamStart,
                                    syntaxConf.Symbols.Key.StringDelimeter,
                                    syntaxConf.Symbols.Key.SpecifierParamEnd
                                );
            string? specifierParamSection = specifierParam.Sections.FirstOrDefault();

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
                int indexOfFirstIllegalSymbol = specifierParam.StartIndexes[0] + specifierParamSection.IndexOf(foundIllegalSymbols.First());
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    specifierParam.StartIndexes[0] + indexOfFirstIllegalSymbol,
                    validationKeyValuePair.LineChangeIndexes);

                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.IllegalCharactersInSpecifierParameter,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value,
                    foundSymbols));
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
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    validationKeyValuePair.ValueStartIndex,
                    validationKeyValuePair.LineChangeIndexes);

                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.InvalidValueFormat,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value,
                    value));
            }

            // If value is of type String or ListOfStrings, check if the line changes are compliant to the specification
            if (type is ValueType.String || type is ValueType.ListOfStrings)
            {
                int lineChangeValidityIndex = SyntaxValidationUtilityMethods.GetLineChangesValidity(value, syntaxConf, (ValueType)type);
                if (lineChangeValidityIndex != -1)
                {
                    KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                        validationKeyValuePair.KeyStartLineIndex,
                        lineChangeValidityIndex,
                        validationKeyValuePair.LineChangeIndexes);

                    return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.InvalidValueFormat,
                        feedbackIndexes.Key,
                        feedbackIndexes.Value,
                        value));
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

            int firstExcessWhitespaceIndex = SyntaxValidationUtilityMethods.FirstSubstringIndexFromString(
                value,
                [
                    $"{CharacterConstants.SPACE}{CharacterConstants.SPACE}",
                ],
                syntaxConf.Symbols.Key.StringDelimeter);

            if (firstExcessWhitespaceIndex != -1)
            { 
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    validationKeyValuePair.ValueStartIndex + firstExcessWhitespaceIndex,
                    validationKeyValuePair.LineChangeIndexes);

                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                    ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.ExcessWhitespaceInValue,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value,
                    value));
            }

            return null;
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
            bool insideString = false;
            bool insideSpecifierSection = false;
            int i = 0;
            while (i < key.Length)
            {
                char currentCharacter = key[i];
                if (currentCharacter == syntaxConf.Symbols.Key.StringDelimeter)
                {
                    insideString = !insideString;
                }
                else if (currentCharacter == syntaxConf.Symbols.Key.SpecifierParamStart)
                {
                    insideSpecifierSection = true;
                }
                else if (currentCharacter == syntaxConf.Symbols.Key.SpecifierParamEnd)
                {
                    insideSpecifierSection = false;
                }
                if (!insideString && currentCharacter == CharacterConstants.SPACE)
                {
                    // If whitespace is found outside a string or specifier section, it is considered excess whitespace
                    if (!insideSpecifierSection)
                    {
                        KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                            validationKeyValuePair.KeyStartLineIndex,
                            i,
                            validationKeyValuePair.LineChangeIndexes);

                        return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                            ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.KeyContainsExcessWhiteSpace,
                            feedbackIndexes.Key,
                            feedbackIndexes.Value
                        ));
                    }

                    // Only expected whitespace outside string sections within key is between specifier parts, after list separator
                    if (key[i - 1] != syntaxConf.Symbols.Key.ListSeparator)
                    {
                        KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                            validationKeyValuePair.KeyStartLineIndex,
                            i,
                            validationKeyValuePair.LineChangeIndexes);

                        return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                            ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.KeyContainsExcessWhiteSpace,
                            feedbackIndexes.Key,
                            feedbackIndexes.Value
                        ));
                    }
                }
                i++;
            }
            return null;
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
            const int oneLineValueLengthRecommendedLimit = 150;

            if (validationKeyValuePair.KeyValuePair.Value.Length > oneLineValueLengthRecommendedLimit ||
                (type != ValueType.String &&
                type != ValueType.ListOfStrings))
            {
                return null;
            }

            string value = validationKeyValuePair.KeyValuePair.Value;

            int firstNewLineIndex = SyntaxValidationUtilityMethods.FirstSubstringIndexFromString(
                value,
                [
                    CharacterConstants.WindowsNewLine,
                    CharacterConstants.UnixNewLine
                ],
                syntaxConf.Symbols.Key.StringDelimeter);

            if (firstNewLineIndex != -1)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    validationKeyValuePair.ValueStartIndex + firstNewLineIndex,
                    validationKeyValuePair.LineChangeIndexes);

                return new ValidationFeedbackItem(validationKeyValuePair, new ValidationFeedback(
                    ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.ExcessNewLinesInValue,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> specifier contains illegal characters, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier contains illegal characters, null otherwise.</returns>
        public static ValidationFeedbackItem? KeywordContainsIllegalCharacters(ValidationStructuredEntry validationStructuredEntry, PxFileSyntaxConf syntaxConf)
        {
            string keyword = validationStructuredEntry.Key.Keyword;
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
                    KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                        validationStructuredEntry.KeyStartLineIndex,
                        keyword.IndexOf(illegalSymbolsInKeyWord.First(), StringComparison.Ordinal),
                        validationStructuredEntry.LineChangeIndexes);

                    return new ValidationFeedbackItem(validationStructuredEntry, new ValidationFeedback(
                        ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.IllegalCharactersInKeyword,
                        feedbackIndexes.Key,
                        feedbackIndexes.Value,
                        string.Join(", ", illegalSymbolsInKeyWord)));
                }
            }
            catch (RegexMatchTimeoutException)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    0,
                    validationStructuredEntry.LineChangeIndexes);

                return new ValidationFeedbackItem(validationStructuredEntry, new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.RegexTimeout,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value));
            }

            return null;
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> keyword doesn't start with a letter, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier doesn't start with a letter, null otherwise.</returns>
        public static ValidationFeedbackItem? KeywordDoesntStartWithALetter(ValidationStructuredEntry validationStructuredEntry, PxFileSyntaxConf syntaxConf)
        {
            string keyword = validationStructuredEntry.Key.Keyword;

            // Check if keyword starts with a letter
            if (!char.IsLetter(keyword[0]))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    0,
                    validationStructuredEntry.LineChangeIndexes);

                return new ValidationFeedbackItem(validationStructuredEntry, new ValidationFeedback(
                    ValidationFeedbackLevel.Error, 
                    ValidationFeedbackRule.KeywordDoesntStartWithALetter,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value
                    ));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> language parameter is not following a valid format, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the language parameter is not following a valid format, null otherwise.</returns>
        public static ValidationFeedbackItem? IllegalCharactersInLanguageParameter(ValidationStructuredEntry validationStructuredEntry, PxFileSyntaxConf syntaxConf)
        {
            // Running this validation is relevant only for objects with a language parameter
            if (validationStructuredEntry.Key.Language is null)
            {
                return null;
            }

            string lang = validationStructuredEntry.Key.Language;

            // Find illegal characters from language parameter string
            char[] illegalCharacters = [
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.LangParamEnd,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.EntrySeparator,
                syntaxConf.Symbols.KeywordSeparator,
                CharacterConstants.SPACE,
                CharacterConstants.CARRIAGERETURN,
                CharacterConstants.LINEFEED,
                CharacterConstants.HORIZONTALTAB
            ];

            IEnumerable<char> foundIllegalCharacters = lang.Where(c => illegalCharacters.Contains(c));
            if (foundIllegalCharacters.Any())
            {
                int indexOfFirstIllegalCharacter = lang.IndexOf(foundIllegalCharacters.First());

                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    validationStructuredEntry.Key.Keyword.Length + indexOfFirstIllegalCharacter + 1,
                    validationStructuredEntry.LineChangeIndexes);

                string foundSymbols = string.Join(", ", foundIllegalCharacters);
                return new ValidationFeedbackItem(validationStructuredEntry, new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.IllegalCharactersInLanguageParameter,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value,
                    foundSymbols));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> specifier parameter is not following a valid format, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier parameter is not following a valid format, null otherwise.</returns>
        public static ValidationFeedbackItem? IllegalCharactersInSpecifierParts(ValidationStructuredEntry validationStructuredEntry, PxFileSyntaxConf syntaxConf)
        {
            // Running this validation is relevant only for objects with a specifier
            if (validationStructuredEntry.Key.FirstSpecifier is null)
            {
                return null;
            }

            char[] illegalCharacters = [syntaxConf.Symbols.EntrySeparator, syntaxConf.Symbols.Key.StringDelimeter, syntaxConf.Symbols.Key.ListSeparator];
            IEnumerable<char> illegalcharactersInFirstSpecifier = validationStructuredEntry.Key.FirstSpecifier.Where(c => illegalCharacters.Contains(c));
            IEnumerable<char> illegalcharactersInSecondSpecifier = [];
            if (validationStructuredEntry.Key.SecondSpecifier is not null)
            {
                illegalcharactersInSecondSpecifier = validationStructuredEntry.Key.SecondSpecifier.Where(c => illegalCharacters.Contains(c));
            }

            if (illegalcharactersInFirstSpecifier.Any() || illegalcharactersInSecondSpecifier.Any())
            {
                int languageSectionLength = validationStructuredEntry.Key.Language is not null ? validationStructuredEntry.Key.Language.Length + 3 : 1;

                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    validationStructuredEntry.Key.Keyword.Length + languageSectionLength,
                    validationStructuredEntry.LineChangeIndexes);

                char[] characters = [..illegalcharactersInFirstSpecifier, ..illegalcharactersInSecondSpecifier];
                return new ValidationFeedbackItem(validationStructuredEntry, new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.IllegalCharactersInSpecifierPart,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value,
                    string.Join(", ", characters)));
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
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationEntry.KeyStartLineIndex,
                    validationEntry.EntryString.Length,
                    validationEntry.LineChangeIndexes);

                return new ValidationFeedbackItem(validationEntry, new ValidationFeedback(
                    ValidationFeedbackLevel.Error, 
                    ValidationFeedbackRule.EntryWithoutValue,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value));
            }
            else if (keywordSeparatorIndeces.Length > 1)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationEntry.KeyStartLineIndex,
                    keywordSeparatorIndeces[1],
                    validationEntry.LineChangeIndexes);

                return new ValidationFeedbackItem(validationEntry, new ValidationFeedback(
                    ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.EntryWithMultipleValues,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> language parameter is not compliant with ISO 639 or BCP 47, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the language parameter is not compliant with ISO 639 or BCP 47, null otherwise.</returns>
        public static ValidationFeedbackItem? IncompliantLanguage (ValidationStructuredEntry validationStructuredEntry, PxFileSyntaxConf syntaxConf)
        {
            // Running this validation is relevant only for objects with a language
            if (validationStructuredEntry.Key.Language is null)
            {
                return null;
            }
            
            string lang = validationStructuredEntry.Key.Language;

            bool iso639OrMsLcidCompliant = CultureInfo.GetCultures(CultureTypes.AllCultures).ToList().Exists(c => c.Name == lang || c.ThreeLetterISOLanguageName == lang);
            bool bcp47Compliant = Bcp47Codes.Codes.Contains(lang);

            if (iso639OrMsLcidCompliant || bcp47Compliant)
            {
                return null;
            }
            else
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    validationStructuredEntry.Key.Keyword.Length + 1,
                    validationStructuredEntry.LineChangeIndexes);

                return new ValidationFeedbackItem(validationStructuredEntry, new ValidationFeedback(
                    ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.IncompliantLanguage,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value
                    ));
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> specifier contains unrecommended characters, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier contains unrecommended characters, null otherwise.</returns>
        public static ValidationFeedbackItem? KeywordContainsUnderscore (ValidationStructuredEntry validationStructuredEntry, PxFileSyntaxConf syntaxConf)
        {
            int underscoreIndex = validationStructuredEntry.Key.Keyword.IndexOf('_');
            if (underscoreIndex != -1)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    underscoreIndex,
                    validationStructuredEntry.LineChangeIndexes);

                return new ValidationFeedbackItem(validationStructuredEntry, new ValidationFeedback(
                    ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.KeywordContainsUnderscore,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> specifier is not in upper case, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> to validate</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier is not in upper case, otherwise null</returns>
        public static ValidationFeedbackItem? KeywordIsNotInUpperCase(ValidationStructuredEntry validationStructuredEntry, PxFileSyntaxConf syntaxConf)
        {
            string keyword = validationStructuredEntry.Key.Keyword;
            string uppercaseKeyword = keyword.ToUpper(CultureInfo.InvariantCulture);

            if (uppercaseKeyword != keyword)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    0,
                    validationStructuredEntry.LineChangeIndexes);

                return new ValidationFeedbackItem(validationStructuredEntry, new ValidationFeedback(
                    ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.KeywordIsNotInUpperCase,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> specifier is excessively long, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> object to validate</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier is excessively long, otherwise null</returns>
        public static ValidationFeedbackItem? KeywordIsExcessivelyLong(ValidationStructuredEntry validationStructuredEntry, PxFileSyntaxConf syntaxConf)
        {
            const int keywordLengthRecommendedLimit = 20;
            string keyword = validationStructuredEntry.Key.Keyword;

            if (keyword.Length > keywordLengthRecommendedLimit)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    keyword.Length,
                    validationStructuredEntry.LineChangeIndexes);

                return new ValidationFeedbackItem(validationStructuredEntry, new ValidationFeedback(
                    ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.KeywordExcessivelyLong,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value));
            }
            else
            {
                return null;
            }
        }
    }
}
