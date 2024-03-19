using PxUtils.PxFile;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PxUtils.Validation.SyntaxValidation
{
    public delegate ValidationFeedbackItem? ValidationFunctionDelegate(ValidationEntry entry);

    public class SyntaxValidationFunctions
    {
        /// <summary>
        /// Validates the given entry. If the entry does not start with a line separator, it is considered as not being on its own line, and a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the entry does not start with a line separator, null otherwise.</returns>
        public readonly ValidationFunctionDelegate MultipleEntriesOnLine = (ValidationEntry entry) =>
        {
            StringValidationEntry? stringEntry = entry as StringValidationEntry ?? throw new ArgumentException("Entry is not of type StringValidationEntry");

            // If the entry does not start with a line separator, it is not on its own line. For the first entry this is not relevant.
            if (stringEntry.EntryIndex == 0 || stringEntry.EntryString.StartsWith(stringEntry.SyntaxConf.Symbols.LineSeparator))
            {
                return null;
            }
            else
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackMultipleEntriesOnLine());
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the key contains more than one language parameter, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key contains more than one language parameter, null otherwise.</returns>
        public readonly ValidationFunctionDelegate MoreThanOneLanguageParameter = (ValidationEntry entry) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");

            bool hasMultipleParameters = SyntaxValidationUtilityMethods.HasMoreThanOneSection(
                keyValueValidationEntry.KeyValueEntry.Key,
                keyValueValidationEntry.SyntaxConf.Symbols.Key.LangParamStart,
                keyValueValidationEntry.SyntaxConf.Symbols.Key.LangParamEnd
            );

            if (hasMultipleParameters)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackMoreThanOneLanguage());
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
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key contains more than one specifier parameter, null otherwise.</returns>
        public readonly ValidationFunctionDelegate MoreThanOneSpecifierParameter = (ValidationEntry entry) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");

            bool hasMultipleParameters = SyntaxValidationUtilityMethods.HasMoreThanOneSection(
                keyValueValidationEntry.KeyValueEntry.Key,
                keyValueValidationEntry.SyntaxConf.Symbols.Key.SpecifierParamStart,
                keyValueValidationEntry.SyntaxConf.Symbols.Key.SpecifierParamEnd
                );

            if (hasMultipleParameters)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackMoreThanOneSpecifier());
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
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key is not defined in the order of KEYWORD[language](\"specifier\"), null otherwise.</returns>
        public readonly ValidationFunctionDelegate WrongKeyOrderOrMissingKeyword = (ValidationEntry entry) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");

            string key = keyValueValidationEntry.KeyValueEntry.Key;
            PxFileSyntaxConf syntaxConf = keyValueValidationEntry.SyntaxConf;

            string languageRemoved = SyntaxValidationUtilityMethods.ExtractSectionFromString(key, syntaxConf.Symbols.Key.LangParamStart, syntaxConf.Symbols.Key.LangParamEnd).Remainder;
            string keyword = SyntaxValidationUtilityMethods.ExtractSectionFromString(languageRemoved, syntaxConf.Symbols.Key.SpecifierParamStart, syntaxConf.Symbols.Key.SpecifierParamEnd).Remainder;

            if (keyword.Trim() == string.Empty)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackMissingKeyword());
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
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackKeyHasWrongOrder());
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the specifier is not following a valid format, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier is not following a valid format, null otherwise.</returns>
        public readonly ValidationFunctionDelegate InvalidSpecifier = (ValidationEntry entry) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");

            string? specifierParamSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    keyValueValidationEntry.KeyValueEntry.Key,
                                    keyValueValidationEntry.SyntaxConf.Symbols.Key.SpecifierParamStart,
                                    keyValueValidationEntry.SyntaxConf.Symbols.Key.SpecifierParamEnd
                                ).Sections.FirstOrDefault();

            if (specifierParamSection is null)
            {
                return null;
            }
            ExtractSectionResult specifierResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                specifierParamSection,
                keyValueValidationEntry.SyntaxConf.Symbols.Key.StringDelimeter
                );

            string[] specifiers = specifierResult.Sections;

            List<string> validationFailureReasons = [];

            if (specifiers.Length > 2)
            {
                validationFailureReasons.Add("More than two specifier parts.");
            }

            if (specifiers.Length > 1 && !specifierResult.Remainder.Contains(','))
            {
                validationFailureReasons.Add("Specifiers are not separated with a \",\"");
            }

            foreach (string specifier in specifierParamSection.Split(","))
            {
                string trimmedSpecifier = specifier.Trim();
                if (!trimmedSpecifier.StartsWith(keyValueValidationEntry.SyntaxConf.Symbols.Key.StringDelimeter) || !trimmedSpecifier.EndsWith(keyValueValidationEntry.SyntaxConf.Symbols.Key.StringDelimeter))
                {
                    validationFailureReasons.Add($"{specifier} is not enclosed with \"\".");
                }
            }

            if (validationFailureReasons.Count > 0)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackInvalidSpecifier([.. validationFailureReasons]));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the key parameter section contains illegal symbols, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key parameter section contains illegal symbols, null otherwise.</returns>
        public readonly ValidationFunctionDelegate IllegalSymbolsInKeyParamSection = (ValidationEntry entry) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");

            string key = keyValueValidationEntry.KeyValueEntry.Key;
            PxFileSyntaxConf syntaxConf = keyValueValidationEntry.SyntaxConf;

            string languageParamSections = string.Join("", SyntaxValidationUtilityMethods.ExtractSectionFromString(key, syntaxConf.Symbols.Key.LangParamStart, syntaxConf.Symbols.Key.LangParamEnd).Sections);
            string specifierParamSections = string.Join("", SyntaxValidationUtilityMethods.ExtractSectionFromString(key, syntaxConf.Symbols.Key.SpecifierParamStart, syntaxConf.Symbols.Key.SpecifierParamEnd).Sections);

            List<string> reasons = [];

            char[] languageParamIllegalSymbols = [
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.SpecifierParamStart,
                syntaxConf.Symbols.Key.SpecifierParamEnd,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.Key.ListSeparator
                ];

            char[] specifierParamIllegalSymbols = [
                syntaxConf.Symbols.Key.SpecifierParamStart,
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.LangParamEnd
            ];

            char[] illegalSymbolsFoundInLanguageParam = languageParamIllegalSymbols.Where(languageParamSections.Contains).ToArray();
            char[] illegalSymbolsFoundInSpecifierParam = specifierParamIllegalSymbols.Where(specifierParamSections.Contains).ToArray();

            if (illegalSymbolsFoundInLanguageParam.Length > 0)
            {
                reasons.Add($"Illegal symbols found in language parameter section: {string.Join(", ", illegalSymbolsFoundInLanguageParam)}");
            }
            if (illegalSymbolsFoundInSpecifierParam.Length > 0)
            {
                reasons.Add($"Illegal symbols found in specifier parameter section: {string.Join(", ", illegalSymbolsFoundInSpecifierParam)}");
            }

            if (reasons.Count > 0)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackIllegalSymbolsInParamSections([.. reasons]));
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
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the value section is not following a valid format, null otherwise.</returns>
        public readonly ValidationFunctionDelegate IllegalValueFormat = (ValidationEntry entry) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");

            string value = keyValueValidationEntry.KeyValueEntry.Value;
            PxFileSyntaxConf syntaxConf = keyValueValidationEntry.SyntaxConf;

            ValueType? type = SyntaxValidationUtilityMethods.GetValueTypeFromString(value, syntaxConf);

            if (type is null)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackInvalidValueSection("No compliant format was found."));
            }
            else if ((type == ValueType.ListOfStrings || type == ValueType.String) && !SyntaxValidationUtilityMethods.ValueLineChangesAreCompliant(value, syntaxConf, type == ValueType.ListOfStrings))
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackInvalidValueSection("Uncompliant line changes found."));
            }

            return null;
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the value section contains excess whitespace, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the value section contains excess whitespace, null otherwise.</returns>
        public readonly ValidationFunctionDelegate ExcessWhitespaceInValue = (ValidationEntry entry) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");
            
            // Validation only matters if the value is a list of strings
            if (!SyntaxValidationUtilityMethods.IsStringListFormat(
                keyValueValidationEntry.KeyValueEntry.Value,
                keyValueValidationEntry.SyntaxConf.Symbols.Value.ListSeparator,
                keyValueValidationEntry.SyntaxConf.Symbols.Key.StringDelimeter
                ))
            {
                return null;
            }
            
            string value = keyValueValidationEntry.KeyValueEntry.Value;

            // Remove elements from the list. We only want to check whitespace between elements
            string stripItems = SyntaxValidationUtilityMethods.ExtractSectionFromString(value, keyValueValidationEntry.SyntaxConf.Symbols.Key.StringDelimeter).Remainder;
            if (stripItems.Contains("  "))
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackValueContainsExcessWhitespace());
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
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the key contains excess whitespace, null otherwise.</returns>
        public readonly ValidationFunctionDelegate KeyContainsExcessWhiteSpace = (ValidationEntry entry) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");

            string key = keyValueValidationEntry.KeyValueEntry.Key;

            IEnumerable<char> whiteSpaces = key.Where(c => c == ' ');
            if (!whiteSpaces.Any())
            {
                return null;
            }
            // There should be only one whitespace in the key part, separating specifier parts
            else if (whiteSpaces.Count() > 1)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackKeyContainsExcessWhitespace());
            }
            // If whitespace is found without a comma separating the specifier parts, it is considered excess
            else if (!key.Contains($"{keyValueValidationEntry.SyntaxConf.Symbols.Key.ListSeparator} "))
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackKeyContainsExcessWhitespace());
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
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the value section contains excess new lines, null otherwise.</returns>
        public readonly ValidationFunctionDelegate ExcessNewLinesInValue = (ValidationEntry entry) =>
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");
            
            // We only need to run this validation if the value is less than 150 characters long and it is a string or list
            ValueType? type = SyntaxValidationUtilityMethods.GetValueTypeFromString(keyValueValidationEntry.KeyValueEntry.Value, keyValueValidationEntry.SyntaxConf);
            
            if (keyValueValidationEntry.KeyValueEntry.Value.Length > 150 ||
                (type != ValueType.String &&
                type != ValueType.ListOfStrings))
            {
                return null;
            }

            string value = keyValueValidationEntry.KeyValueEntry.Value;

            if (value.Contains(keyValueValidationEntry.SyntaxConf.Symbols.LineSeparator))
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackExcessNewLinesInValue());
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the keyword is not following a valid format, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the keyword is not following a valid format, null otherwise.</returns>
        public readonly ValidationFunctionDelegate InvalidKeywordFormat = (ValidationEntry entry) =>
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException("Entry is not of type StructuredValidationEntry");

            string keyword = structuredValidationEntry.Key.Keyword;
            // Missing keyword is catched earlier
            if (keyword.Length == 0)
            {
                return null;
            }

            List<string> reasons = [];

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
                    reasons.Add($"Keyword contains illegal characters: {string.Join(", ", illegalSymbolsInKeyWord)}");
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackRegexTimeOut("keyword", keyword));
            }

            // Check if keyword starts with a letter
            if (!char.IsLetter(keyword[0]))
            {
                reasons.Add("Keyword doesn't start with a letter");
            }

            if (reasons.Count > 0)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackInvalidKeywordFormat([.. reasons]));
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
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the language parameter is not following a valid format, null otherwise.</returns>
        public readonly ValidationFunctionDelegate IllegalCharactersInLanguageParameter = (ValidationEntry entry) =>
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException("Entry is not of type StructuredValidationEntry");
            // Running this validation is relevant only for entries with a language parameter
            if (structuredValidationEntry.Key.Language is null)
            {
                return null;
            }

            string lang = structuredValidationEntry.Key.Language;

            List<string> reasons = [];

            // Find illegal characters from language parameter string
            string illegalCharacters = @"[;=\[\]""]";
            TimeSpan timeout = TimeSpan.FromSeconds(1);
            // Find all illegal symbols in keyword
            try
            {
                MatchCollection matchesLang = Regex.Matches(lang, illegalCharacters, RegexOptions.None, timeout);
                List<string> foundIllegalCharacters = matchesLang.Cast<Match>().Select(m => m.Value).Distinct().ToList();
                // Check if there are any special characters or whitespace in lang
                if (foundIllegalCharacters.Count > 0)
                {
                    reasons.Add($"Language parameter contains illegal characters: {string.Join(", ", foundIllegalCharacters)}");
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackRegexTimeOut("language", lang));
            }

            if (lang.Contains(' '))
            {
                reasons.Add("Language parameter contains whitespace character(s)");
            }

            if (reasons.Count > 0)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackInvalidLanguageFormat([.. reasons]));
            }
            else
            {
                return null;
            }
        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the specifier parameter is not following a valid format, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the specifier parameter is not following a valid format, null otherwise.</returns>
        public readonly ValidationFunctionDelegate IllegalCharactersInSpecifierParameter = (ValidationEntry entry) =>
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException("Entry is not of type StructuredValidationEntry");
            // Running this validation is relevant only for entries with a specifier
            if (structuredValidationEntry.Key.FirstSpecifier is null)
            {
                return null;
            }

            List<string> reasons = [];

            char[] illegalCharactersFoundInFirstSpecifier = FindIllegalCharactersInSpecifierPart(structuredValidationEntry.Key.FirstSpecifier);
            char[] illegalCharactersFoundInSecondSpecifier = FindIllegalCharactersInSpecifierPart(structuredValidationEntry.Key.SecondSpecifier);

            if (illegalCharactersFoundInFirstSpecifier.Length > 0)
            {
                reasons.Add($"Illegal characters found in first specifier: {string.Join(", ", illegalCharactersFoundInFirstSpecifier)}");
            }
            if (illegalCharactersFoundInSecondSpecifier.Length > 0)
            {
                reasons.Add($"Illegal characters found in second specifier: {string.Join(", ", illegalCharactersFoundInSecondSpecifier)}");
            }

            if (reasons.Count > 0)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackIllegalCharactersInSpecifier([.. reasons]));
            }
            else
            {
                return null;
            }
        };

        private static char[] FindIllegalCharactersInSpecifierPart(string? specifier)
        {
            if (specifier is null)
            {
                return [];
            }

            char[] illegalCharacters = [';', '"', ','];
            return illegalCharacters.Where(specifier.Contains).ToArray();
        }

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If there is no value section, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if there is no value section, null otherwise.</returns>
        public readonly ValidationFunctionDelegate EntryWithoutValue = (ValidationEntry entry) =>
        {
            StringValidationEntry? stringEntry = entry as StringValidationEntry ?? throw new ArgumentException("Entry is not of type StringValidationEntry");

            if (!stringEntry.EntryString.Contains(stringEntry.SyntaxConf.Symbols.KeywordSeparator))
            {                 
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackEntryWithoutValue());
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
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the language parameter is not compliant with ISO 639 or BCP 47, null otherwise.</returns>
        public readonly ValidationFunctionDelegate IncompliantLanguage = (ValidationEntry entry) =>
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException("Entry is not of type StructuredValidationEntry");
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
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackIncompliantLanguageParam());
            }

        };

        /// <summary>
        /// Validates the given <see cref="ValidationEntry"/> entry. If the keyword contains unrecommended characters, a new <see cref="ValidationFeedbackItem"/> is returned.
        /// </summary>
        /// <param name="entry">The <see cref="ValidationEntry"/> entry to validate.</param>
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the keyword contains unrecommended characters, null otherwise.</returns>
        public readonly ValidationFunctionDelegate KeywordHasUnrecommendedCharacters = (ValidationEntry entry) =>
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException("Entry is not of type StructuredValidationEntry");

            string keyword = structuredValidationEntry.Key.Keyword;
            List<string> reasons = [];
            string uppercaseKeyword = keyword.ToUpper();

            if (uppercaseKeyword != keyword)
            {
                reasons.Add("Keyword should be in upper case");    
            }
            if (keyword.Contains('_'))
            {
                reasons.Add("Keyword should not contain _. Using - is recommended instead");
            }

            if (reasons.Count > 0)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackKeywordContainsUnrecommendedCharacters([.. reasons]));
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
        /// <returns>A <see cref="ValidationFeedbackItem"/> if the keyword is excessively long, otherwise null</returns>
        public readonly ValidationFunctionDelegate KeywordIsExcessivelyLong = (ValidationEntry entry) =>
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException("Entry is not of type StructuredValidationEntry");

            string keyword = structuredValidationEntry.Key.Keyword;

            if (keyword.Length > 20)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackKeywordIsExcessivelyLong());
            }
            else
            {
                return null;
            }
        };
    }
}
