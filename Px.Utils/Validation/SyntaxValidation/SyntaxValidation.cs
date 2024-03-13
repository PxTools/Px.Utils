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
        public static SyntaxValidationResult ValidatePxFileSyntax(Stream stream, string filename, PxFileSyntaxConf? syntaxConf = null)
        {
            IEnumerable<IValidationFunction> stringValidationFunctions = [
                    new MultipleEntriesOnLine()
                ];

            IEnumerable<IValidationFunction> keyValueValidationFunctions = [
                  new MoreThanOneLanguageParameter(),
                  new MoreThanOneSpecifierParameter(),
                  new WrongKeyOrder()
                ];

            IEnumerable<IValidationFunction> structuredValidationFunctions = [
                   
                ];

            syntaxConf ??= PxFileSyntaxConf.Default;
            ValidationReport report = new()
            {
                FeedbackItems = []
            };

            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);
            if (encoding == null)
            {
                report.FeedbackItems.Add(new ValidationFeedbackItem(new ValidationEntry(0, 0, filename), new SyntaxValidationFeedbackNoEncoding()));
                return new(report, []);
            }

            List<StringValidationEntry> stringEntries = BuildEntries(stream, encoding, syntaxConf, filename);
            ValidateStringEntries(stringEntries, stringValidationFunctions, report);
            List<KeyValuePairValidationEntry> keyValuePairs = BuildKeyValuePairs(stringEntries);
            ValidateKeyValuePairEntries(keyValuePairs, keyValueValidationFunctions, report);
            List<StructuredValidationEntry> structuredEntries = BuildStructuredEntries(keyValuePairs);
            ValidateStructuredEntries(structuredEntries, structuredValidationFunctions, report);

            return new(report, structuredEntries);
        }

        private static List<StringValidationEntry> BuildEntries(Stream stream, Encoding encoding, PxFileSyntaxConf syntaxConf, string filename)
        {
            int line = 0;
            int character = 0;
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new(stream, encoding);
            // Iterate each character of the reader
            List<StringValidationEntry> stringEntries = [];
            StringBuilder entryBuilder = new();
            while (!reader.EndOfStream)
            {
                char currentCharacter = (char)reader.Read();
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
            return stringEntries;
        }

        private static string CleanString(string input)
        {
            return input.Replace("\n", "").Replace("\"", "");
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
                string[] split = CleanString(entry.EntryString).Split("=");
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
            string? language = null;
            string? firstSpecifier = null;
            string? secondSpecifier = null;

            // Try to find the beginnning of the language parameter
            int languageParameterStartIndex = input.IndexOf(syntaxConf.Symbols.Key.LangParamStart);
            // Try to find the beginning of the specifier. If not found, return the keyword
            int specifierStartIndex = input.IndexOf(syntaxConf.Symbols.Key.SpecifierParamStart);
            int keywordEndIndex = languageParameterStartIndex;

            if (languageParameterStartIndex == -1)
            {
                keywordEndIndex = specifierStartIndex;
            }
            // No language or specifier were found, return keyword
            if (keywordEndIndex == -1)
            {
                return new(input, null, null, null);
            }

            // Extract the keyword
            string keyword = input[0..keywordEndIndex];

            // Extract the language if it exists
            if (languageParameterStartIndex != -1)
            {
                int languageParameterEndIndex = input.IndexOf(syntaxConf.Symbols.Key.LangParamEnd);
                language = input[(languageParameterStartIndex + 1)..languageParameterEndIndex];
            }

            // Extract the specifiers if they exist
            if (specifierStartIndex != -1)
            {
                int specifierEndIndex = input.IndexOf(syntaxConf.Symbols.Key.SpecifierParamEnd);
                string identifiers = input[(specifierStartIndex + 1)..specifierEndIndex];
                string[] splitSpecifiers = identifiers.Split(syntaxConf.Symbols.Key.ListSeparator);
                firstSpecifier = splitSpecifiers[0].Trim(syntaxConf.Symbols.Key.StringDelimeter);
                if (splitSpecifiers.Length > 1)
                {
                    secondSpecifier = splitSpecifiers[1].Trim(syntaxConf.Symbols.Key.StringDelimeter).Trim();
                }
            }

            return new(keyword, language, firstSpecifier, secondSpecifier);
        }
    }
}
