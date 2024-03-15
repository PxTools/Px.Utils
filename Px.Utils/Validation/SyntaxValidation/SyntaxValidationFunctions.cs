using Px.Utils.Validation.SyntaxValidation;
using PxUtils.PxFile;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PxUtils.Validation.SyntaxValidation
{

    public class MultipleEntriesOnLine : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            StringValidationEntry? stringEntry = entry as StringValidationEntry ?? throw new ArgumentException("Entry is not of type StringValidationEntry");

            // Validation is not relevant for first entry
            return stringEntry.EntryIndex > 0;
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
        {
            StringValidationEntry? stringEntry = entry as StringValidationEntry ?? throw new ArgumentException("Entry is not of type StringValidationEntry");

            // If the entry does not start with a line separator, it is not on its own line
            if (!stringEntry.EntryString.StartsWith(stringEntry.SyntaxConf.Symbols.LineSeparator))
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackMultipleEntriesOnLine());
            }
            else
            {
                return null;
            }
        }
    }

    public class MoreThanOneLanguageParameter : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            return true;
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
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
        }
    }

    public class MoreThanOneSpecifierParameter : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            return true;
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
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
        }
    }

    public class WrongKeyOrderOrMissingKeyword : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            return true;
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
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
            return null;
        }
    }

    public class InvalidSpecifier : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            return true;
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");

            string? specifierParamSection = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                                    keyValueValidationEntry.KeyValueEntry.Key,
                                    keyValueValidationEntry.SyntaxConf.Symbols.Key.SpecifierParamStart,
                                    keyValueValidationEntry.SyntaxConf.Symbols.Key.SpecifierParamEnd
                                ).Sections.FirstOrDefault();

            if (specifierParamSection == null)
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
        }
    }

    public class IllegalSymbolsInKeyParamSection : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            return true;
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
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
        }
    }

    public class IllegalValueFormat : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            return true;
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");

            string value = keyValueValidationEntry.KeyValueEntry.Value;
            PxFileSyntaxConf syntaxConf = keyValueValidationEntry.SyntaxConf;

            bool isList = SyntaxValidationUtilityMethods.IsStringList(value);
            bool isString = SyntaxValidationUtilityMethods.IsStringFormat(value);
            bool isBoolean = SyntaxValidationUtilityMethods.IsBooleanFormat(value);
            bool isNumber = SyntaxValidationUtilityMethods.IsNumberFormat(value);

            if (!isList && !isString && !isBoolean && !isNumber)
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackInvalidValueSection("No compliant format was found."));
            }
            else if ((isList || isString) && !SyntaxValidationUtilityMethods.ValueLineChangesAreCompliant(value, syntaxConf, isList))
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackInvalidValueSection("Uncompliant line changes found."));
            }

            return null;
        }
    }

    public class ExcessWhitespaceInValue : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");
            // This only matters if the value is a list of strings
            return SyntaxValidationUtilityMethods.IsStringList(keyValueValidationEntry.KeyValueEntry.Value);
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");

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
        }
    }

    public class KeyContainsExcessWhiteSpace : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry) => true;

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
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
        }
    }

    public class ExcessNewLinesInValue : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");
            
            // We only need to run this validation if the value is less than 150 characters long and it is a string or list
            return
                keyValueValidationEntry.KeyValueEntry.Value.Length < 150 &&
                (SyntaxValidationUtilityMethods.IsStringList(keyValueValidationEntry.KeyValueEntry.Value) ||
                SyntaxValidationUtilityMethods.IsStringFormat(keyValueValidationEntry.KeyValueEntry.Value));
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
        {
            KeyValuePairValidationEntry? keyValueValidationEntry = entry as KeyValuePairValidationEntry ?? throw new ArgumentException("Entry is not of type KeyValueValidationEntry");

            string value = keyValueValidationEntry.KeyValueEntry.Value;

            if (value.Contains(keyValueValidationEntry.SyntaxConf.Symbols.LineSeparator))
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackExcessNewLinesInValue());
            }
            else
            {
                return null;
            }
        }
    }

    public class InvalidKeywordFormat : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry) => true;

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
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
            // Find all illegal symbols in keyword
            var matchesKeyword = Regex.Matches(keyword, $"[^{legalSymbolsKeyword}]");
            var illegalSymbolsInKeyWord = matchesKeyword.Cast<Match>().Select(m => m.Value).Distinct().ToList();
            if (illegalSymbolsInKeyWord.Count > 0)
            {
                reasons.Add($"Keyword contains illegal characters: {string.Join(", ", illegalSymbolsInKeyWord)}"); 
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
        }
    }

    public class IllegalCharactersInLanguageParameter : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException("Entry is not of type StructuredValidationEntry");
            return structuredValidationEntry.Key.Language != null;
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException("Entry is not of type StructuredValidationEntry");
            string? lang = structuredValidationEntry.Key.Language;

            // Language shouldn't be null because of IsRelevant call, but checking just to please Sonar gods.
            if (lang == null)
            {
                return null;
            }

            List<string> reasons = [];

            // Find illegal characters from language parameter string
            string illegalCharacters = @"[;=\[\]""]";
            MatchCollection matchesLang = Regex.Matches(lang, illegalCharacters);
            List<string> foundIllegalCharacters = matchesLang.Cast<Match>().Select(m => m.Value).Distinct().ToList();

            // Check if there are any special characters or whitespace in lang
            if (foundIllegalCharacters.Count > 0)
            {
                reasons.Add($"Language parameter contains illegal characters: {string.Join(", ", foundIllegalCharacters)}");
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
        }
    }

    public class IllegalCharactersInSpecifierParameter : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException("Entry is not of type StructuredValidationEntry");
            // Running this validation is relevant only for entries with a specifier
            return structuredValidationEntry.Key.FirstSpecifier != null;
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException("Entry is not of type StructuredValidationEntry");

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
        }

        private char[] FindIllegalCharactersInSpecifierPart(string? specifier)
        {
            if (specifier == null)
            {
                return [];
            }

            char[] illegalCharacters = [';', '"', ','];
            return illegalCharacters.Where(specifier.Contains).ToArray();
        }
    }

    public class EntryWithoutValue : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry) => true;

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
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
        }
    }

    public class IncompliantLanguage : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry)
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException("Entry is not of type StructuredValidationEntry");
            // Running this validation is relevant only for entries with a language
            return structuredValidationEntry.Key.Language != null;
        }

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
        {
            StructuredValidationEntry? structuredValidationEntry = entry as StructuredValidationEntry ?? throw new ArgumentException("Entry is not of type StructuredValidationEntry");

            string? lang = structuredValidationEntry.Key.Language;

            // Language shouldn't be null because of IsRelevant call, but checking just to please Sonar gods.
            if (lang == null)
            {
                return null;
            }

            // If language is ISO 639 or MS-LCID compliant, it returns something
            bool iso639OrMsLcidCompliant;
            try
            {
                CultureInfo iso639OrMsLcid = new(lang);
                iso639OrMsLcidCompliant = true;
            }
            catch (CultureNotFoundException)
            {
                iso639OrMsLcidCompliant = false;
            }

            bool bcp47Compliant = Bcp47Codes.Codes.Contains(lang);

            if (iso639OrMsLcidCompliant || bcp47Compliant)
            {
                return null;
            }
            else
            {
                return new ValidationFeedbackItem(entry, new SyntaxValidationFeedbackIncompliantLanguageParam());
            }
        }
    }

    public class KeywordHasUnrecommendedCharacters : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry) => true;

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
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
        }
    }

    public class KeywordIsExcessivelyLong : IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry) => true;

        public ValidationFeedbackItem? Validate(ValidationEntry entry)
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
        }
    }
}
