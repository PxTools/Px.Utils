using Px.Utils.PxFile;

namespace Px.Utils.Validation.SyntaxValidation
{
    public delegate KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? KeyValuePairValidationFunction(ValidationKeyValuePair validationObject, PxFileConfiguration conf);

    /// <summary>
    /// Contains the default validation functions for syntax validation key value pair entries.
    /// </summary>
    public partial class SyntaxValidationFunctions
    {
        public List<KeyValuePairValidationFunction> DefaultKeyValueValidationFunctions { get; } =
        [
            MoreThanOneLanguageParameter,
            MoreThanOneSpecifierParameter,
            WrongKeyOrderOrMissingKeyword,
            SpecifierPartNotEnclosed,
            MoreThanTwoSpecifierParts,
            NoDelimiterBetweenSpecifierParts,
            IllegalSymbolsInLanguageParamSection,
            IllegalCharactersInSpecifierSection,
            InvalidValueFormat,
            ExcessWhitespaceInValue,
            KeyContainsExcessWhiteSpace,
            ExcessNewLinesInValue
        ];

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> key contains more than one language parameter, a new feedback is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="conf">The configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the key contains more than one language parameter, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? MoreThanOneLanguageParameter(ValidationKeyValuePair validationKeyValuePair, PxFileConfiguration conf)
        {
            ExtractSectionResult languageSections = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                validationKeyValuePair.KeyValuePair.Key,
                conf.Symbols.Key.LangParamStart,
                conf.Symbols.Key.StringDelimeter,
                conf.Symbols.Key.LangParamEnd);

            if (languageSections.Sections.Length > 1)
            {
                KeyValuePair<int, int> lineAndCharacter = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    languageSections.StartIndexes[1],
                    validationKeyValuePair.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.MoreThanOneLanguageParameterSection),
                    new(validationKeyValuePair.File,
                    lineAndCharacter.Key,
                    lineAndCharacter.Value)
                );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> key contains more than one specifier parameter, a new feedback is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="conf">The configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the key contains more than one specifier parameter, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? MoreThanOneSpecifierParameter(ValidationKeyValuePair validationKeyValuePair, PxFileConfiguration conf)
        {
            ExtractSectionResult specifierSpections = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                validationKeyValuePair.KeyValuePair.Key,
                conf.Symbols.Key.SpecifierParamStart,
                conf.Symbols.Key.StringDelimeter,
                conf.Symbols.Key.SpecifierParamEnd);

            if (specifierSpections.Sections.Length > 1)
            {
                KeyValuePair<int, int> lineAndCharacter = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    specifierSpections.StartIndexes[1],
                    validationKeyValuePair.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.MoreThanOneSpecifierParameterSection),
                    new(validationKeyValuePair.File,
                    lineAndCharacter.Key,
                    lineAndCharacter.Value)
                );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> key is not defined in the order of KEYWORD[language](\"specifier\"), a new feedback is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="conf">The configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the key is not defined in the order of KEYWORD[language](\"specifier\"), null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? WrongKeyOrderOrMissingKeyword(ValidationKeyValuePair validationKeyValuePair, PxFileConfiguration conf)
        {
            string key = validationKeyValuePair.KeyValuePair.Key;

            // Remove language parameter section if it exists
            ExtractSectionResult languageSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                key,
                conf.Symbols.Key.LangParamStart,
                conf.Symbols.Key.StringDelimeter,
                conf.Symbols.Key.LangParamEnd);

            string languageRemoved = languageSection.Remainder;

            // Remove specifier section
            ExtractSectionResult specifierRemoved = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                languageRemoved,
                conf.Symbols.Key.SpecifierParamStart,
                conf.Symbols.Key.StringDelimeter,
                conf.Symbols.Key.SpecifierParamEnd);

            // Check for missing specifierRemoved. Keyword is the remainder of the string after removing language and specifier sections
            if (specifierRemoved.Remainder.Trim() == string.Empty)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    0,
                    validationKeyValuePair.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.MissingKeyword),
                    new(validationKeyValuePair.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value)
                );
            }

            // Check where language and parameter sections start
            Dictionary<int, int> stringSections = SyntaxValidationUtilityMethods.GetEnclosingCharacterIndexes(key, conf.Symbols.Key.StringDelimeter);
            int langParamStartIndex = SyntaxValidationUtilityMethods.FindSymbolIndex(key, conf.Symbols.Key.LangParamStart, stringSections);
            int specifierParamStartIndex = SyntaxValidationUtilityMethods.FindSymbolIndex(key, conf.Symbols.Key.SpecifierParamStart, stringSections);

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

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.KeyHasWrongOrder),
                    new(validationKeyValuePair.File,
                    specifierIndexes.Key,
                    specifierIndexes.Value)
                );
            }
            // Check if the key starts with a language or specifier parameter section
            else if (
                key.Trim().StartsWith(conf.Symbols.Key.SpecifierParamStart) ||
                key.Trim().StartsWith(conf.Symbols.Key.LangParamStart))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    0,
                    validationKeyValuePair.LineChangeIndexes
                    );

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.KeyHasWrongOrder),
                    new(validationKeyValuePair.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value)
                );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> key contains more than two specifiers, a new feedback is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="conf">The configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if more than two specifiers are found, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? MoreThanTwoSpecifierParts(ValidationKeyValuePair validationKeyValuePair, PxFileConfiguration conf)
        {
            ExtractSectionResult specifierSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    validationKeyValuePair.KeyValuePair.Key,
                                    conf.Symbols.Key.SpecifierParamStart,
                                    conf.Symbols.Key.StringDelimeter,
                                    conf.Symbols.Key.SpecifierParamEnd
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
                conf.Symbols.Key.StringDelimeter,
                conf.Symbols.Key.StringDelimeter
                );

            if (specifierResult.Sections.Length > 2)
            {
                KeyValuePair<int, int> secondSpecifierIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    specifierSection.StartIndexes[0] + specifierResult.StartIndexes[2],
                    validationKeyValuePair.LineChangeIndexes
                    );

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.TooManySpecifiers),
                    new(validationKeyValuePair.File,
                    secondSpecifierIndexes.Key,
                    secondSpecifierIndexes.Value)
                );
            }
            return null;
        }

        /// <summary>
        /// If there is no delimeter between <see cref="ValidationKeyValuePair"/> specifier parts, a new feedback is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="conf">The configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if there is no delimeter between specifier parts, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? NoDelimiterBetweenSpecifierParts(ValidationKeyValuePair validationKeyValuePair, PxFileConfiguration conf)
        {
            ExtractSectionResult specifierSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    validationKeyValuePair.KeyValuePair.Key,
                                    conf.Symbols.Key.SpecifierParamStart,
                                    conf.Symbols.Key.StringDelimeter,
                                    conf.Symbols.Key.SpecifierParamEnd
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
                conf.Symbols.Key.StringDelimeter,
                conf.Symbols.Key.StringDelimeter
                );

            string[] specifiers = specifierResult.Sections;

            // If there are more than one specifier and the remainder of the specifier section does not contain a list separator, a delimiter is missing
            if (specifiers.Length > 1 && !specifierResult.Remainder.Contains(conf.Symbols.Key.ListSeparator))
            {
                KeyValuePair<int, int> specifierIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    specifierSection.StartIndexes[0] + specifierResult.StartIndexes[1],
                    validationKeyValuePair.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.SpecifierDelimiterMissing),
                    new(validationKeyValuePair.File,
                    specifierIndexes.Key,
                    specifierIndexes.Value)
                );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If any of the <see cref="ValidationKeyValuePair"/> specifiers are not enclosed a new feedback is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="conf">The configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if specifier parts are not enclosed, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? SpecifierPartNotEnclosed(ValidationKeyValuePair validationKeyValuePair, PxFileConfiguration conf)
        {
            ExtractSectionResult specifierSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    validationKeyValuePair.KeyValuePair.Key,
                                    conf.Symbols.Key.SpecifierParamStart,
                                    conf.Symbols.Key.StringDelimeter,
                                    conf.Symbols.Key.SpecifierParamEnd
                                );

            string? specifierParamSection = specifierSection.Sections.FirstOrDefault();

            // If specifier section is not found, there are no specifiers
            if (specifierParamSection == null)
            {
                return null;
            }
            List<string> specifiers = SyntaxValidationUtilityMethods.GetListItemsFromString(specifierParamSection, conf.Symbols.Key.ListSeparator, conf.Symbols.Key.StringDelimeter);
            // Check if specifiers are enclosed in string delimeters
            for (int i = 0; i < specifiers.Count; i++)
            {
                string specifier = specifiers[i];
                string trimmedSpecifier = specifier.Trim();
                if (!trimmedSpecifier.StartsWith(conf.Symbols.Key.StringDelimeter) || !trimmedSpecifier.EndsWith(conf.Symbols.Key.StringDelimeter))
                {
                    KeyValuePair<int, int> specifierIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                        validationKeyValuePair.KeyStartLineIndex,
                        specifierSection.StartIndexes[0],
                        validationKeyValuePair.LineChangeIndexes
                    );

                    return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                        new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.SpecifierPartNotEnclosed),
                        new(validationKeyValuePair.File,
                        specifierIndexes.Key,
                        specifierIndexes.Value)
                    );
                }
            }
            return null;
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> key language section contains illegal symbols, a new feedback is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="conf">The configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the key language section contains illegal symbols, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? IllegalSymbolsInLanguageParamSection(ValidationKeyValuePair validationKeyValuePair, PxFileConfiguration conf)
        {
            string key = validationKeyValuePair.KeyValuePair.Key;

            ExtractSectionResult languageParam = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    key,
                                    conf.Symbols.Key.LangParamStart,
                                    conf.Symbols.Key.StringDelimeter,
                                    conf.Symbols.Key.LangParamEnd
                                );

            string? languageParamSection = languageParam.Sections.FirstOrDefault();

            // If language section is not found, the validation is not relevant
            if (languageParamSection == null)
            {
                return null;
            }

            char[] languageIllegalSymbols = [
                conf.Symbols.Key.LangParamStart,
                conf.Symbols.Key.LangParamEnd,
                conf.Symbols.Key.SpecifierParamStart,
                conf.Symbols.Key.SpecifierParamEnd,
                conf.Symbols.Key.StringDelimeter,
                conf.Symbols.Key.ListSeparator
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



                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.IllegalCharactersInLanguageSection),
                    new(validationKeyValuePair.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value,
                    foundSymbols)
                );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Checks that the <see cref="ValidationKeyValuePair"/> key specifier contains only specifier parts, whitespace and 0-1 list separators.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="conf">The configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the key specifier section contains illegal symbols, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? IllegalCharactersInSpecifierSection(ValidationKeyValuePair validationKeyValuePair, PxFileConfiguration conf)
        {
            string key = validationKeyValuePair.KeyValuePair.Key;

            ExtractSectionResult specifierParam = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    key,
                                    conf.Symbols.Key.SpecifierParamStart,
                                    conf.Symbols.Key.StringDelimeter,
                                    conf.Symbols.Key.SpecifierParamEnd
                                );

            string? specifierParamSection = specifierParam.Sections.FirstOrDefault();

            // If specifier section is not found, there are no specifiers
            if (specifierParamSection == null)
            {
                return null;
            }

            // Remove specifier parts from the section
            string removedSpecifiers = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                specifierParamSection,
                conf.Symbols.Key.StringDelimeter,
                conf.Symbols.Key.StringDelimeter
            ).Remainder;

            // Allowed symbols in the specifier section after removing specifiers
            char[] allowedSymbols = [
                conf.Symbols.Key.ListSeparator,
                conf.Symbols.Key.StringDelimeter
            ];
            allowedSymbols = [.. allowedSymbols, .. CharacterConstants.WhitespaceCharacters];

            IEnumerable<char> foundIllegalSymbols = removedSpecifiers.Where(c => !allowedSymbols.Contains(c));
            if (foundIllegalSymbols.Any())
            {
                string foundSymbols = string.Join(", ", foundIllegalSymbols);
                int indexOfFirstIllegalSymbol = specifierParam.StartIndexes[0] + specifierParamSection.IndexOf(foundIllegalSymbols.First());
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    specifierParam.StartIndexes[0] + indexOfFirstIllegalSymbol,
                    validationKeyValuePair.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.IllegalCharactersInSpecifierSection),
                    new(validationKeyValuePair.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value,
                    foundSymbols)
                );
            }
            // Only one list separator is allowed
            else
            {
                int listSeparatorCount = removedSpecifiers.Count(c => c == conf.Symbols.Key.ListSeparator);
                if (listSeparatorCount > 1)
                {
                    KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                        validationKeyValuePair.KeyStartLineIndex,
                        specifierParam.StartIndexes[0] + removedSpecifiers.IndexOf(conf.Symbols.Key.ListSeparator),
                        validationKeyValuePair.LineChangeIndexes);

                    return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                        new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.IllegalCharactersInSpecifierSection),
                        new(validationKeyValuePair.File,
                        feedbackIndexes.Key,
                        feedbackIndexes.Value,
                        "Only one list separator is allowed.")
                    );
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> value section is not following a valid format, a new feedback is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="conf">The configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the value section is not following a valid format, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? InvalidValueFormat(ValidationKeyValuePair validationKeyValuePair, PxFileConfiguration conf)
        {
            string value = validationKeyValuePair.KeyValuePair.Value;

            // Try to parse the value section to a ValueType
            ValueType? type = SyntaxValidationUtilityMethods.GetValueTypeFromString(value, conf);

            if (type is null)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    validationKeyValuePair.ValueStartIndex,
                    validationKeyValuePair.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.InvalidValueFormat),
                    new(validationKeyValuePair.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value,
                    value)
                );
            }

            // If value is of type String or ListOfStrings, check if the line changes are compliant to the specification
            if (type is ValueType.StringValue || type is ValueType.ListOfStrings)
            {
                int lineChangeValidityIndex = SyntaxValidationUtilityMethods.GetLineChangesValidity(value, conf, (ValueType)type);
                if (lineChangeValidityIndex != -1)
                {
                    KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                        validationKeyValuePair.KeyStartLineIndex,
                        lineChangeValidityIndex,
                        validationKeyValuePair.LineChangeIndexes);

                    return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                        new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.InvalidValueFormat),
                        new(validationKeyValuePair.File,
                        feedbackIndexes.Key,
                        feedbackIndexes.Value,
                        value)
                    );
                }
            }
            return null;
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> value section contains excess whitespace, a new feedback is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="conf">The configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the value section contains excess whitespace, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? ExcessWhitespaceInValue(ValidationKeyValuePair validationKeyValuePair, PxFileConfiguration conf)
        {
            // Validation only matters if the value is a list of strings
            if (!SyntaxValidationUtilityMethods.IsStringListFormat(
                validationKeyValuePair.KeyValuePair.Value,
                conf.Symbols.Value.ListSeparator,
                conf.Symbols.Key.StringDelimeter
                ))
            {
                return null;
            }

            string value = validationKeyValuePair.KeyValuePair.Value;

            int firstExcessWhitespaceIndex = SyntaxValidationUtilityMethods.FirstSubstringIndexFromString(
                value,
                [
                    $"{CharacterConstants.CHARSPACE}{CharacterConstants.CHARSPACE}",
                ],
                conf.Symbols.Key.StringDelimeter);

            if (firstExcessWhitespaceIndex != -1)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationKeyValuePair.KeyStartLineIndex,
                    validationKeyValuePair.ValueStartIndex + firstExcessWhitespaceIndex,
                    validationKeyValuePair.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.ExcessWhitespaceInValue),
                    new(validationKeyValuePair.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value,
                    value)
                );
            }

            return null;
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> key contains excess whitespace, a new feedback is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="conf">The configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the key contains excess whitespace, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? KeyContainsExcessWhiteSpace(ValidationKeyValuePair validationKeyValuePair, PxFileConfiguration conf)
        {
            string key = validationKeyValuePair.KeyValuePair.Key;
            bool insideString = false;
            bool insideSpecifierSection = false;
            int i = 0;
            while (i < key.Length)
            {
                char currentCharacter = key[i];
                if (currentCharacter == conf.Symbols.Key.StringDelimeter)
                {
                    insideString = !insideString;
                }
                else if (currentCharacter == conf.Symbols.Key.SpecifierParamStart)
                {
                    insideSpecifierSection = true;
                }
                else if (currentCharacter == conf.Symbols.Key.SpecifierParamEnd)
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

                        return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                            new(ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.KeyContainsExcessWhiteSpace),
                            new(validationKeyValuePair.File,
                            feedbackIndexes.Key,
                            feedbackIndexes.Value)
                        );
                    }

                    // Only expected whitespace outside string sections within key is between specifier parts, after list separator
                    if (key[i - 1] != conf.Symbols.Key.ListSeparator)
                    {
                        KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                            validationKeyValuePair.KeyStartLineIndex,
                            i,
                            validationKeyValuePair.LineChangeIndexes);

                        return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                            new(ValidationFeedbackLevel.Warning,
                            ValidationFeedbackRule.KeyContainsExcessWhiteSpace),
                            new(validationKeyValuePair.File,
                            feedbackIndexes.Key,
                            feedbackIndexes.Value)
                        );
                    }
                }
                i++;
            }
            return null;
        }

        /// <summary>
        /// If the <see cref="ValidationKeyValuePair"/> value section contains excess new lines, a new feedback is returned.
        /// </summary>
        /// <param name="validationKeyValuePair">The <see cref="ValidationKeyValuePair"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the value section contains excess new lines, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? ExcessNewLinesInValue(ValidationKeyValuePair validationKeyValuePair, PxFileConfiguration syntaxConf)
        {
            // We only need to run this validation if the value is less than 150 characters long and it is a string or list
            ValueType? type = SyntaxValidationUtilityMethods.GetValueTypeFromString(validationKeyValuePair.KeyValuePair.Value, syntaxConf);
            const int oneLineValueLengthRecommendedLimit = 150;

            if (validationKeyValuePair.KeyValuePair.Value.Length > oneLineValueLengthRecommendedLimit ||
                (type != ValueType.StringValue &&
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

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.ExcessNewLinesInValue),
                    new(validationKeyValuePair.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value)
                );
            }
            else
            {
                return null;
            }
        }
    }
}
