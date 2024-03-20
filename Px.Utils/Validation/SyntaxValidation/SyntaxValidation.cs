using PxUtils.Exceptions;
using PxUtils.PxFile;
using PxUtils.PxFile.Meta;
using System.Text;

namespace PxUtils.Validation.SyntaxValidation
{
    /// <summary>
    /// Provides methods for validating the syntax of a PX file.
    /// </summary>
    public static class SyntaxValidation
    {
        private const int DEFAULT_BUFFER_SIZE = 4096;

        ///<summary>
        /// Validates the syntax of a PX file's metadata.
        ///</summary>
        /// <param name="stream">The stream of the PX file to be validated.</param>
        /// <param name="filename">The name of the file to be validated.</param>
        /// <param name="syntaxConf">An optional <see cref="pxsynta"/> parameter that specifies the syntax configuration for the PX file. If not provided, the default syntax configuration is used.</param>
        /// <param name="bufferSize">An optional parameter that specifies the buffer size for reading the file. If not provided, a default buffer size of 4096 is used.</param>
        /// <param name="customStringValidationFunctions">An optional parameter that specifies custom <see cref="IValidationFunction"/> validation functions for string level entries. If not provided, only default validation functions are used.</param>
        /// <param name="customKeyValueValidationFunctions">An optional parameter that specifies custom key-value pair level <see cref="IValidationFunction"/>  validation functions. If not provided, only default key-value pair validation functions are used.</param>
        /// <param name="customStructuredValidationFunctions">An optional parameter that specifies custom <see cref="IValidationFunction"/> validation functions for structured key-value pairs. If not provided, only default structured validation functions are used.</param>
        /// <returns>A <see cref="SyntaxValidationResult"/> object which contains a <see cref="SyntaxValidationResult"/> and a list of <see cref="StructuredValidationEntry"/> objects. The ValidationReport contains feedback items that provide information about any syntax errors or warnings found during validation. The list of StructuredValidationEntry objects represents the structured entries in the PX file that were validated.</returns>
        public static SyntaxValidationResult ValidatePxFileMetadataSyntax(
            Stream stream,
            string filename,
            PxFileSyntaxConf? syntaxConf = null,
            int bufferSize = DEFAULT_BUFFER_SIZE,
            IEnumerable<ValidationFunctionDelegate>? customStringValidationFunctions = null,
            IEnumerable<ValidationFunctionDelegate>? customKeyValueValidationFunctions = null,
            IEnumerable<ValidationFunctionDelegate>? customStructuredValidationFunctions = null)
        {
            SyntaxValidationFunctions validationFunctions = new();

            IEnumerable<ValidationFunctionDelegate> stringValidationFunctions = [
                validationFunctions.MultipleEntriesOnLine,
                validationFunctions.EntryWithoutValue,
            ];
            stringValidationFunctions = stringValidationFunctions.Concat(customStringValidationFunctions ?? []);

            IEnumerable<ValidationFunctionDelegate> keyValueValidationFunctions = [
                validationFunctions.MoreThanOneLanguageParameter,
                validationFunctions.MoreThanOneSpecifierParameter,
                validationFunctions.WrongKeyOrderOrMissingKeyword,
                validationFunctions.SpecifierPartNotEnclosed,
                validationFunctions.MoreThanTwoSpecifierParts,
                validationFunctions.NoDelimiterBetweenSpecifierParts,
                validationFunctions.IllegalSymbolsInLanguageParamSection,
                validationFunctions.IllegalSymbolsInSpecifierParamSection,
                validationFunctions.InvalidValueFormat,
                validationFunctions.ExcessWhitespaceInValue,
                validationFunctions.KeyContainsExcessWhiteSpace,
                validationFunctions.ExcessNewLinesInValue
            ];
            keyValueValidationFunctions = keyValueValidationFunctions.Concat(customKeyValueValidationFunctions ?? []);

            IEnumerable<ValidationFunctionDelegate> structuredValidationFunctions = [
                validationFunctions.KeywordContainsIllegalCharacters,
                validationFunctions.KeywordDoesntStartWithALetter,
                validationFunctions.IllegalCharactersInLanguageParameter,
                validationFunctions.IllegalCharactersInSpecifierParameter,
                validationFunctions.IncompliantLanguage,
                validationFunctions.KeywordContainsUnderscore,
                validationFunctions.KeywordIsNotInUpperCase,
                validationFunctions.KeywordIsExcessivelyLong
            ];
            structuredValidationFunctions = structuredValidationFunctions.Concat(customStructuredValidationFunctions ?? []);

            syntaxConf ??= PxFileSyntaxConf.Default;
            ValidationReport report = new();

            try
            {
                Encoding encoding = PxFileMetadataReader.GetEncoding(stream, syntaxConf);

                List<StringValidationEntry> stringEntries = BuildStringEntries(stream, encoding, syntaxConf, filename, bufferSize);
                ValidateEntries(stringEntries, stringValidationFunctions, report);
                List<KeyValuePairValidationEntry> keyValuePairs = BuildKeyValuePairs(stringEntries, syntaxConf);
                ValidateEntries(keyValuePairs, keyValueValidationFunctions, report);
                List<StructuredValidationEntry> structuredEntries = BuildStructuredEntries(keyValuePairs);
                ValidateEntries(structuredEntries, structuredValidationFunctions, report);

                return new(report, structuredEntries);
            }
            catch (InvalidPxFileMetadataException)
            {
                report.FeedbackItems.Add(new ValidationFeedbackItem(new StringValidationEntry(0, 0, filename, string.Empty, syntaxConf, 0), new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.NoEncoding)));
                return new(report, []);
            }
        }

        ///<summary>
        /// Validates a collection of entries using a collection of validation functions and adds any feedback to the provided report.
        ///</summary>
        /// <param name="entries">The collection of <see cref="ValidationEntry"/> entries to be validated.</param>
        /// <param name="validationFunctions">The collection of <see cref="IValidationFunction"/> validation functions to be used for validation.</param>
        /// <param name="report">The <see cref="ValidationReport"/> report where validation feedback will be added.</param>
        public static void ValidateEntries(IEnumerable<ValidationEntry> entries, IEnumerable<ValidationFunctionDelegate> validationFunctions, ValidationReport report)
        {
            foreach (var entry in entries)
            {
                foreach (ValidationFunctionDelegate function in validationFunctions)
                {
                    ValidationFeedbackItem? feedback = function(entry);
                    if (feedback is not null)
                    {
                        report.FeedbackItems.Add((ValidationFeedbackItem)feedback);
                    }
                }
            }
        }

        ///<summary>
        /// Builds a list of <see cref="KeyValuePairValidationEntry"/> objects from a list of StringValidationEntry objects.
        ///</summary>
        /// <param name="stringEntries">The list of <see cref="StringValidationEntry"/> objects to be converted.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A list of <see cref="KeyValuePairValidationEntry"/> objects built from the provided list of StringValidationEntry objects.</returns>
        public static List<KeyValuePairValidationEntry> BuildKeyValuePairs(List<StringValidationEntry> stringEntries, PxFileSyntaxConf syntaxConf)
        {
            return stringEntries.Select(entry =>
            {
                string[] split = entry.EntryString.Split(syntaxConf.Symbols.KeywordSeparator);
                string value = split.Length > 1 ? split[1] : string.Empty;
                return new KeyValuePairValidationEntry(entry.Line, entry.Character, entry.File, new KeyValuePair<string, string>(split[0], value), entry.SyntaxConf);
            }).ToList();
        }

        /// <summary>
        /// Builds a list of <see cref="StructuredValidationEntry"/> objects from a list of <see cref="KeyValuePairValidationEntry"/> objects.
        /// </summary>
        /// <param name="keyValuePairs">The list of <see cref="KeyValuePairValidationEntry"/> objects to be converted.</param>
        /// <returns>A list of <see cref="StructuredValidationEntry"/> objects built from the provided list of <see cref="KeyValuePairValidationEntry"/> objects.</returns>
        public static List<StructuredValidationEntry> BuildStructuredEntries(List<KeyValuePairValidationEntry> keyValuePairs)
        {
            return keyValuePairs.Select(entry =>
            {
                ValidationEntryKey key = ParseValidationEntryKey(entry.KeyValueEntry.Key, entry.SyntaxConf);
                return new StructuredValidationEntry(entry.Line, entry.Character, entry.File, key, entry.KeyValueEntry.Value);
            }).ToList();
        }

        private static List<StringValidationEntry> BuildStringEntries(Stream stream, Encoding encoding, PxFileSyntaxConf syntaxConf, string filename, int bufferSize)
        {
            int line = 0;
            int character = 0;
            stream.Seek(0, SeekOrigin.Begin);
            using StreamReader reader = new(stream, encoding);
            List<StringValidationEntry> stringEntries = [];
            StringBuilder entryBuilder = new();
            char[] buffer = new char[bufferSize];

            while (reader.Read(buffer, 0, bufferSize) > 0)
            {
                foreach (char currentCharacter in buffer)
                {
                    if (currentCharacter == syntaxConf.Symbols.KeywordSeparator)
                    {
                        string stringEntry = entryBuilder.ToString();
                        // When DATA keyword is reached, metadata parsing is complete
                        if (SyntaxValidationUtilityMethods.CleanString(stringEntry).Equals(syntaxConf.Tokens.KeyWords.Data))
                        {
                            break;
                        }
                    }
                    if (currentCharacter == syntaxConf.Symbols.LineSeparator)
                    {
                        line++;
                        character = 0;
                    }
                    else
                    {
                        character++;
                    }
                    if (currentCharacter == syntaxConf.Symbols.SectionSeparator)
                    {
                        string stringEntry = entryBuilder.ToString();
                        stringEntries.Add(new StringValidationEntry(line, character, filename, stringEntry, syntaxConf, stringEntries.Count));
                        entryBuilder.Clear();
                    }
                    else
                    {
                        entryBuilder.Append(currentCharacter);
                    }
                }
                Array.Clear(buffer, 0, buffer.Length);
            }
            return stringEntries;
        }

        private static ValidationEntryKey ParseValidationEntryKey(string input, PxFileSyntaxConf syntaxConf)
        {
            ExtractSectionResult languageResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(input, syntaxConf.Symbols.Key.LangParamStart, syntaxConf.Symbols.Key.LangParamEnd);
            string? language = languageResult.Sections.Length > 0 ? SyntaxValidationUtilityMethods.CleanString(languageResult.Sections[0]) : null;
            ExtractSectionResult specifierResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(languageResult.Remainder, syntaxConf.Symbols.Key.SpecifierParamStart, syntaxConf.Symbols.Key.SpecifierParamEnd);
            string[] specifiers = SyntaxValidationUtilityMethods.GetSpecifiersFromParameter(specifierResult.Sections.FirstOrDefault(), syntaxConf.Symbols.Key.StringDelimeter);
            string? firstSpecifier = specifiers.Length > 0 ? SyntaxValidationUtilityMethods.CleanString(specifiers[0]) : null;
            string? secondSpecifier = specifiers.Length > 1 ? SyntaxValidationUtilityMethods.CleanString(specifiers[1]) : null;
            string keyword = SyntaxValidationUtilityMethods.CleanString(specifierResult.Remainder);

            return new(keyword, language, firstSpecifier, secondSpecifier);
        }
    }
}
