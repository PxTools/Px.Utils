using PxUtils.PxFile;
using PxUtils.PxFile.Meta;
using System.Text;

namespace PxUtils.Validation.SyntaxValidation
{
    public static class SyntaxValidation
    {
        private const int DEFAULT_BUFFER_SIZE = 4096;

        public static SyntaxValidationResult ValidatePxFileSyntax(
            Stream stream,
            string filename,
            PxFileSyntaxConf? syntaxConf = null,
            int bufferSize = DEFAULT_BUFFER_SIZE,
            IEnumerable<IValidationFunction>? customStringValidationFunctions = null,
            IEnumerable<IValidationFunction>? customKeyValueValidationFunctions = null,
            IEnumerable<IValidationFunction>? customStructuredValidationFunctions = null)
        {
            IEnumerable<IValidationFunction> stringValidationFunctions = [
                    new MultipleEntriesOnLine(),
                    new EntryWithoutValue()
                ];
            stringValidationFunctions = stringValidationFunctions.Concat(customStringValidationFunctions ?? []);

            IEnumerable<IValidationFunction> keyValueValidationFunctions = [
                  new MoreThanOneLanguageParameter(),
                  new MoreThanOneSpecifierParameter(),
                  new WrongKeyOrderOrMissingKeyword(),
                  new InvalidSpecifier(),
                  new IllegalSymbolsInKeyParamSection(),
                  new IllegalValueFormat(),
                  new ExcessWhitespaceInValue(),
                  new KeyContainsExcessWhiteSpace(),
                  new ExcessNewLinesInValue()
                ];
            keyValueValidationFunctions = keyValueValidationFunctions.Concat(customKeyValueValidationFunctions ?? []);

            IEnumerable<IValidationFunction> structuredValidationFunctions = [
                  new InvalidKeywordFormat(),
                  new IllegalCharactersInLanguageParameter(),
                  new IllegalCharactersInSpecifierParameter(),
                  new IncompliantLanguage(),
                  new KeywordHasUnrecommendedCharacters(),
                  new KeywordIsExcessivelyLong()
                ];
            structuredValidationFunctions = structuredValidationFunctions.Concat(customStructuredValidationFunctions ?? []);

            syntaxConf ??= PxFileSyntaxConf.Default;
            ValidationReport report = new();

            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);
            if (encoding is null)
            {
                report.FeedbackItems.Add(new ValidationFeedbackItem(new StringValidationEntry(0, 0, filename, string.Empty, syntaxConf, 0), new SyntaxValidationFeedbackNoEncoding()));
                return new(report, []);
            }

            List<StringValidationEntry> stringEntries = BuildStringEntries(stream, encoding, syntaxConf, filename, bufferSize);
            ValidateEntries(stringEntries, stringValidationFunctions, report);
            List<KeyValuePairValidationEntry> keyValuePairs = BuildKeyValuePairs(stringEntries, syntaxConf);
            ValidateEntries(keyValuePairs, keyValueValidationFunctions, report);
            List<StructuredValidationEntry> structuredEntries = BuildStructuredEntries(keyValuePairs);
            ValidateEntries(structuredEntries, structuredValidationFunctions, report);

            return new(report, structuredEntries);
        }

        public static void ValidateEntries(IEnumerable<IValidationEntry> entries, IEnumerable<IValidationFunction> validationFunctions, ValidationReport report)
        {
            foreach (var entry in entries)
            {
                foreach (IValidationFunction function in validationFunctions
                                       .Where(f => f.IsRelevant(entry)))
                {
                    ValidationFeedbackItem? feedback = function.Validate(entry);
                    if (feedback is not null)
                    {
                        report.FeedbackItems.Add((ValidationFeedbackItem)feedback);
                    }
                }
            }
        }

        public static List<KeyValuePairValidationEntry> BuildKeyValuePairs(List<StringValidationEntry> stringEntries, PxFileSyntaxConf syntaxConf)
        {
            return stringEntries.Select(entry =>
            {
                string[] split = entry.EntryString.Split(syntaxConf.Symbols.KeywordSeparator);
                string value = split.Length > 1 ? split[1] : string.Empty;
                return new KeyValuePairValidationEntry(entry.Line, entry.Character, entry.File, new KeyValuePair<string, string>(split[0], value), entry.SyntaxConf);
            }).ToList();
        }

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
            StreamReader reader = new(stream, encoding);
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
