using PxUtils.PxFile;
using PxUtils.PxFile.Meta;
using PxUtils.Validation.Planning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation.SyntaxValidation
{
    public static class SyntaxValidation
    {
        public static SyntaxValidationResult ValidatePxFileSyntax(Stream stream, string filename, PxFileSyntaxConf? syntaxConf = null, int bufferSize = 4096)
        {
            IEnumerable<IValidationFunction> stringValidationFunctions = [
                    new MultipleEntriesOnLine()
                ];

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

            IEnumerable<IValidationFunction> structuredValidationFunctions = [
                   
                ];

            syntaxConf ??= PxFileSyntaxConf.Default;
            ValidationReport report = new();

            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);
            if (encoding == null)
            {
                report.FeedbackItems.Add(new ValidationFeedbackItem(new ValidationEntry(0, 0, filename), new SyntaxValidationFeedbackNoEncoding()));
                return new(report, []);
            }

            List<StringValidationEntry> stringEntries = BuildEntries(stream, encoding, syntaxConf, filename, bufferSize);
            ValidateStringEntries(stringEntries, stringValidationFunctions, report);
            List<KeyValuePairValidationEntry> keyValuePairs = BuildKeyValuePairs(stringEntries);
            ValidateKeyValuePairEntries(keyValuePairs, keyValueValidationFunctions, report);
            List<StructuredValidationEntry> structuredEntries = BuildStructuredEntries(keyValuePairs);
            ValidateStructuredEntries(structuredEntries, structuredValidationFunctions, report);

            return new(report, structuredEntries);
        }

        private static List<StringValidationEntry> BuildEntries(Stream stream, Encoding encoding, PxFileSyntaxConf syntaxConf, string filename, int bufferSize)
        {
            int line = 0;
            int character = 0;
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new(stream, encoding);
            List<StringValidationEntry> stringEntries = [];
            StringBuilder entryBuilder = new();
            char[] buffer = new char[bufferSize];
            int charsRead;

            while ((charsRead = reader.Read(buffer, 0, bufferSize)) > 0)
            {
                for (int i = 0; i < charsRead; i++)
                {
                    char currentCharacter = buffer[i];
                    if (currentCharacter == syntaxConf.Symbols.KeywordSeparator)
                    {
                        string stringEntry = entryBuilder.ToString();
                        if (CleanString(stringEntry).Equals(syntaxConf.Tokens.KeyWords.Data))
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

        private static string CleanString(string input)
        {
            if (input == null)
            {
                return string.Empty;
            }
            else
            {
                return input.Replace("\n", "").Replace("\"", "");
            }
        }

        private static void ValidateStringEntries(List<StringValidationEntry> stringEntries, IEnumerable<IValidationFunction> stringValidationFunctions, ValidationReport report)
        {
            foreach (var entry in stringEntries)
            {
                foreach (IValidationFunction function in stringValidationFunctions
                    .Where(f => f.IsRelevant(entry)))
                {
                    ValidationFeedbackItem? feedback = function.Validate(entry);
                    if (feedback != null)
                    {
                        report.FeedbackItems.Add(feedback);
                    }
                }
            }
        }

        private static List<KeyValuePairValidationEntry> BuildKeyValuePairs(List<StringValidationEntry> stringEntries)
        {
            List<KeyValuePairValidationEntry> keyValuePairs = new();
            foreach (StringValidationEntry entry in stringEntries)
            {
                string[] split = entry.EntryString.Split("=");
                keyValuePairs.Add(new (entry.Line, entry.Character, entry.File, new KeyValuePair<string, string>(split[0], split[1]), entry.SyntaxConf));
            }
            return keyValuePairs;
        }

        private static void ValidateKeyValuePairEntries(List<KeyValuePairValidationEntry> keyValuePairs, IEnumerable<IValidationFunction> kvpValidationFunctions, ValidationReport report)
        {
            foreach (var entry in keyValuePairs)
            {
                foreach (IValidationFunction function in kvpValidationFunctions
                                       .Where(f => f.IsRelevant(entry)))
                {
                    ValidationFeedbackItem? feedback = function.Validate(entry);
                    if (feedback != null)
                    {
                        report.FeedbackItems.Add(feedback);
                    }
                }
            }   
        }

        private static List<StructuredValidationEntry> BuildStructuredEntries(List<KeyValuePairValidationEntry> keyValuePairs)
        {
            List<StructuredValidationEntry> structuredEntries = new();
            foreach (KeyValuePairValidationEntry entry in keyValuePairs)
            {
                ValidationEntryKey key = ParseValidationEntryKey(entry.KeyValueEntry.Key, entry.SyntaxConf);
                structuredEntries.Add(new StructuredValidationEntry(entry.Line, entry.Character, entry.File, key, entry.KeyValueEntry.Value));
            }
            return structuredEntries;
        }

        private static void ValidateStructuredEntries(List<StructuredValidationEntry> structuredEntries, IEnumerable<IValidationFunction> validationFunctions, ValidationReport report)
        {
            return;
        }

        private static ValidationEntryKey ParseValidationEntryKey(string input, PxFileSyntaxConf syntaxConf)
        {
            ExtractSectionResult languageResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(input, syntaxConf.Symbols.Key.LangParamStart, syntaxConf.Symbols.Key.LangParamEnd);
            string? language = languageResult.Sections.Length > 0 ? CleanString(languageResult.Sections[0]) : null;
            ExtractSectionResult specifierResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(languageResult.Remainder, syntaxConf.Symbols.Key.SpecifierParamStart, syntaxConf.Symbols.Key.SpecifierParamEnd);
            string[] specifiers = SyntaxValidationUtilityMethods.GetSpecifiersFromParameter(specifierResult.Sections.FirstOrDefault(), syntaxConf);
            string? firstSpecifier = specifiers.Length > 0 ? CleanString(specifiers[0]) : null;
            string? secondSpecifier = specifiers.Length > 1 ? CleanString(specifiers[1]) : null;
            string keyword = CleanString(specifierResult.Remainder);

            return new(keyword, language, firstSpecifier, secondSpecifier);
        }
    }
}
