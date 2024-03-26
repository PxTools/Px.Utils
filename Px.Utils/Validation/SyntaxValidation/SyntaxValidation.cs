﻿using PxUtils.Exceptions;
using PxUtils.PxFile;
using PxUtils.PxFile.Meta;
using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace PxUtils.Validation.SyntaxValidation
{
    /// <summary>
    /// Provides methods for validating the syntax of a PX file. Validation can be done using both synchronous and asynchronous methods.
    /// Additionally custom validation functions can be provided to be used during validation.
    /// </summary>
    public static class SyntaxValidation
    {
        /// <summary>
        /// Collection of custom validation functions to be used during validation.
        /// </summary>
        public class CustomValidationFunctions(
            List<EntryValidationFunctionDelegate> stringValidationFunctions,
            List<KeyValuePairValidationFunctionDelegate> keyValueValidationFunctions,
            List<StructuredValidationFunctionDelegate> structuredValidationFunctions)
        {
            public List<EntryValidationFunctionDelegate> CustomStringValidationFunctions { get; } = stringValidationFunctions;
            public List<KeyValuePairValidationFunctionDelegate> CustomKeyValueValidationFunctions { get; } = keyValueValidationFunctions;
            public List<StructuredValidationFunctionDelegate> CustomStructuredValidationFunctions { get; } = structuredValidationFunctions;
        }

        private const int DEFAULT_BUFFER_SIZE = 4096;

        ///<summary>
        /// Validates the syntax of a PX file's metadata.
        ///</summary>
        /// <param name="stream">The stream of the PX file to be validated.</param>
        /// <param name="encoding">The encoding format to use for the PX file reading</param>
        /// <param name="filename">The name of the file to be validated.</param>
        /// <param name="syntaxConf">An optional <see cref="PxFileSyntaxConf"/> parameter that specifies the syntax configuration for the PX file. If not provided, the default syntax configuration is used.</param>
        /// <param name="bufferSize">An optional parameter that specifies the buffer size for reading the file. If not provided, a default buffer size of 4096 is used.</param>
        /// <param name="customValidationFunctions">An optional <see cref="CustomValidationFunctions"/> parameter that specifies custom validation functions to be used during validation. If not provided, the default validation functions are used.</param>
        /// <returns>A <see cref="SyntaxValidationResult"/> entry which contains a list of <see cref="ValidationStructuredEntry"/> entries and a list of <see cref="ValidationFeedbackItem"/> entries accumulated during the validation.</returns>
        public static SyntaxValidationResult ValidatePxFileMetadataSyntax(
            Stream stream,
            Encoding encoding,
            string filename,
            PxFileSyntaxConf? syntaxConf = null,
            int bufferSize = DEFAULT_BUFFER_SIZE,
            CustomValidationFunctions? customValidationFunctions = null)
        {
            SyntaxValidationFunctions validationFunctions = new();
            IEnumerable<EntryValidationFunctionDelegate> stringValidationFunctions = validationFunctions.DefaultStringValidationFunctions;
            IEnumerable<KeyValuePairValidationFunctionDelegate> keyValueValidationFunctions = validationFunctions.DefaultKeyValueValidationFunctions;
            IEnumerable<StructuredValidationFunctionDelegate> structuredValidationFunctions = validationFunctions.DefaultStructuredValidationFunctions;

            if (customValidationFunctions is not null)
            {
                stringValidationFunctions = stringValidationFunctions.Concat(customValidationFunctions.CustomStringValidationFunctions);
                keyValueValidationFunctions = keyValueValidationFunctions.Concat(customValidationFunctions.CustomKeyValueValidationFunctions);
                structuredValidationFunctions = structuredValidationFunctions.Concat(customValidationFunctions.CustomStructuredValidationFunctions);
            }

            syntaxConf ??= PxFileSyntaxConf.Default;

            List<ValidationFeedbackItem> validationFeedback = [];
            List<int> lineChangeIndexes = [];
            List<ValidationEntry> stringEntries = BuildValidationEntries(stream, encoding, syntaxConf, filename, bufferSize, lineChangeIndexes);
            int[] lineChanges = [..lineChangeIndexes];
            ValidateEntries(stringEntries, stringValidationFunctions, validationFeedback, syntaxConf, lineChanges);
            List<ValidationKeyValuePair> keyValuePairs = BuildKeyValuePairs(stringEntries, syntaxConf);
            ValidateKeyValuePairs(keyValuePairs, keyValueValidationFunctions, validationFeedback, syntaxConf, lineChanges);
            List<ValidationStructuredEntry> structuredEntries = BuildValidationStructureEntries(keyValuePairs, syntaxConf);
            ValidateStructs(structuredEntries, structuredValidationFunctions, validationFeedback, syntaxConf, lineChanges);

            return new([.. validationFeedback], structuredEntries);
        }

        /// <summary>
        /// Asynchronously validates the syntax of a PX file's metadata.
        /// </summary>
        /// <param name="stream">The stream of the PX file to be validated.</param>
        /// <param name="encoding">The encoding format to use for the PX file reading</param>
        /// <param name="filename">The name of the file to be validated.</param>
        /// <param name="syntaxConf">An optional <see cref="PxFileSyntaxConf"/> parameter that specifies the syntax configuration for the PX file. If not provided, the default syntax configuration is used.</param>
        /// <param name="bufferSize">An optional parameter that specifies the buffer size for reading the file. If not provided, a default buffer size of 4096 is used.</param>
        /// <param name="customValidationFunctions">An optional <see cref="CustomValidationFunctions"/> parameter that specifies custom validation functions to be used during validation. If not provided, the default validation functions are used.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> parameter that can be used to cancel the operation.</param>
        /// <returns>A task that contains a <see cref="SyntaxValidationResult"/> entry, which contains the structured validation entries and a list of <see cref="ValidationStructuredEntry"/> entries accumulated during the validation.</returns>
        public static async Task<SyntaxValidationResult> ValidatePxFileMetadataSyntaxAsync(
            Stream stream,
            Encoding encoding,
            string filename,
            PxFileSyntaxConf? syntaxConf = null,
            int bufferSize = DEFAULT_BUFFER_SIZE,
            CustomValidationFunctions? customValidationFunctions = null,
            CancellationToken cancellationToken = default)
        {
            SyntaxValidationFunctions validationFunctions = new();
            IEnumerable<EntryValidationFunctionDelegate> stringValidationFunctions = validationFunctions.DefaultStringValidationFunctions;
            IEnumerable<KeyValuePairValidationFunctionDelegate> keyValueValidationFunctions = validationFunctions.DefaultKeyValueValidationFunctions;
            IEnumerable<StructuredValidationFunctionDelegate> structuredValidationFunctions = validationFunctions.DefaultStructuredValidationFunctions;

            if (customValidationFunctions is not null)
            {
                stringValidationFunctions = stringValidationFunctions.Concat(customValidationFunctions.CustomStringValidationFunctions);
                keyValueValidationFunctions = keyValueValidationFunctions.Concat(customValidationFunctions.CustomKeyValueValidationFunctions);
                structuredValidationFunctions = structuredValidationFunctions.Concat(customValidationFunctions.CustomStructuredValidationFunctions);
            }

            syntaxConf ??= PxFileSyntaxConf.Default;
            List<int> lineChangeIndexes = [];
            List<ValidationFeedbackItem> validationFeedback = [];
            List<ValidationEntry> entries = await BuildValidationEntriesAsync(stream, encoding, syntaxConf, filename, bufferSize, cancellationToken, lineChangeIndexes);
            int[] lineChanges = [..lineChangeIndexes];
            ValidateEntries(entries, stringValidationFunctions, validationFeedback, syntaxConf, lineChanges);
            List<ValidationKeyValuePair> keyValuePairs = BuildKeyValuePairs(entries, syntaxConf);
            ValidateKeyValuePairs(keyValuePairs, keyValueValidationFunctions, validationFeedback, syntaxConf, lineChanges);
            List<ValidationStructuredEntry> structuredEntries = BuildValidationStructureEntries(keyValuePairs, syntaxConf);
            ValidateStructs(structuredEntries, structuredValidationFunctions, validationFeedback, syntaxConf, lineChanges);

            return new([.. validationFeedback], structuredEntries);
        }

        private static void ValidateEntries(IEnumerable<ValidationEntry> entries, IEnumerable<EntryValidationFunctionDelegate> validationFunctions, List<ValidationFeedbackItem> validationFeedback, PxFileSyntaxConf syntaxConf, int[] lineChangeIndexes)
        {
            foreach (ValidationEntry entry in entries)
            {
                foreach (EntryValidationFunctionDelegate function in validationFunctions)
                {
                    ValidationFeedbackItem? feedback = function(entry, syntaxConf, lineChangeIndexes);
                    if (feedback is not null)
                    {
                        validationFeedback.Add((ValidationFeedbackItem)feedback);
                    }
                }
            }
        }

        private static void ValidateKeyValuePairs(IEnumerable<ValidationKeyValuePair> kvpObjects, IEnumerable<KeyValuePairValidationFunctionDelegate> validationFunctions, List<ValidationFeedbackItem> validationFeedback, PxFileSyntaxConf syntaxConf, int[] lineChangeIndexes)
        {
            foreach (ValidationKeyValuePair kvpObject in kvpObjects)
            {
                foreach (KeyValuePairValidationFunctionDelegate function in validationFunctions)
                {
                    ValidationFeedbackItem? feedback = function(kvpObject, syntaxConf, lineChangeIndexes);
                    if (feedback is not null)
                    {
                        validationFeedback.Add((ValidationFeedbackItem)feedback);
                    }
                }
            }
        }

        private static void ValidateStructs(IEnumerable<ValidationStructuredEntry> structuredEntries, IEnumerable<StructuredValidationFunctionDelegate> validationFunctions, List<ValidationFeedbackItem> validationFeedback, PxFileSyntaxConf syntaxConf, int[] lineChangeIndexes)
        {
            foreach (ValidationStructuredEntry structuredEntry in structuredEntries)
            {
                foreach (StructuredValidationFunctionDelegate function in validationFunctions)
                {
                    ValidationFeedbackItem? feedback = function(structuredEntry, syntaxConf, lineChangeIndexes);
                    if (feedback is not null)
                    {
                        validationFeedback.Add((ValidationFeedbackItem)feedback);
                    }
                }
            }
        }

        private static List<ValidationKeyValuePair> BuildKeyValuePairs(List<ValidationEntry> validationEntries, PxFileSyntaxConf syntaxConf)
        {
            return validationEntries.Select(entry =>
            {
                // Split the entry string into a key and a value from the first valid keyword separator
                int[] keywordSeparatorIndeces = SyntaxValidationUtilityMethods.FindKeywordSeparatorIndeces(entry.EntryString, syntaxConf);

                // Key is the part of the entry string before the first keyword separator index
                string key = entry.EntryString[..keywordSeparatorIndeces[0]];

                // Value is the part of the entry string after the first keyword separator index
                int valueStart = keywordSeparatorIndeces[0] + 1;
                string value = entry.EntryString[valueStart..];
                int valueStartIndex = entry.KeyStartIndex + valueStart;

                return new ValidationKeyValuePair(entry.File, new KeyValuePair<string, string>(key, value), entry.KeyStartIndex, valueStartIndex);
            }).ToList();
        }

        private static List<ValidationStructuredEntry> BuildValidationStructureEntries(List<ValidationKeyValuePair> keyValuePairs, PxFileSyntaxConf syntaxConf)
        {
            return keyValuePairs.Select(entry =>
            {
                ValidationStructuredEntryKey key = ParseStructuredValidationEntryKey(entry.KeyValuePair.Key, syntaxConf);
                return new ValidationStructuredEntry(entry.File, key, entry.KeyValuePair.Value, entry.KeyStartIndex, entry.ValueStartIndex);
            }).ToList();
        }

        private static List<ValidationEntry> BuildValidationEntries(Stream stream, Encoding encoding, PxFileSyntaxConf syntaxConf, string filename, int bufferSize, List<int> lineChangeIndexes)
        {
            bool isProcessingString = false;
            int character = 0;
            int entryStart = 0;

            using StreamReader reader = new(stream, encoding);
            List<ValidationEntry> entries = [];
            StringBuilder entryBuilder = new();
            char[] buffer = new char[bufferSize];

            while (reader.Read(buffer, 0, bufferSize) > 0)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (IsEndOfMetadataSection(buffer[i], syntaxConf, entryBuilder, isProcessingString))
                    {
                        break;
                    }
                    UpdateLineAndCharacter(buffer[i], syntaxConf, ref character, ref lineChangeIndexes, ref isProcessingString);
                    if (!isProcessingString && buffer[i] == syntaxConf.Symbols.EntrySeparator)
                    {
                        string stringEntry = entryBuilder.ToString();
                        entries.Add(new ValidationEntry(filename, stringEntry, entries.Count, entryStart));
                        entryStart = character + 1;
                        entryBuilder.Clear();
                    }
                    else
                    {
                        entryBuilder.Append(buffer[i]);
                    }
                }
            }
            return entries;
        }

        private static async Task<List<ValidationEntry>> BuildValidationEntriesAsync(Stream stream, Encoding encoding, PxFileSyntaxConf syntaxConf, string filename, int bufferSize, CancellationToken cancellationToken, List<int> lineChangeIndexes)
        {
            bool isProcessingString = false;
            int character = 0;
            int entryStart = 0;

            using StreamReader reader = new(stream, encoding);
            List<ValidationEntry> entries = [];
            StringBuilder entryBuilder = new();
            char[] buffer = new char[bufferSize];
            int read = 0;
            do
            {
                read = await reader.ReadAsync(buffer.AsMemory(), cancellationToken);
                for (int i = 0; i < read; i++)
                {
                    if (IsEndOfMetadataSection(buffer[i], syntaxConf, entryBuilder, isProcessingString))
                    {
                        break;
                    }
                    UpdateLineAndCharacter(buffer[i], syntaxConf, ref character, ref lineChangeIndexes, ref isProcessingString);
                    if (!isProcessingString && buffer[i] == syntaxConf.Symbols.EntrySeparator)
                    {
                        string stringEntry = entryBuilder.ToString();
                        entries.Add(new ValidationEntry(filename, stringEntry, entries.Count, entryStart));
                        entryStart = character + 1;
                        entryBuilder.Clear();
                    }
                    else
                    {
                        entryBuilder.Append(buffer[i]);
                    }
                }
            }
            while (read > 0);

            return entries;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEndOfMetadataSection(char currentCharacter, PxFileSyntaxConf syntaxConf, StringBuilder entryBuilder, bool isProcessingString)
        {
            if (!isProcessingString && currentCharacter == syntaxConf.Symbols.KeywordSeparator)
            {
                string stringEntry = entryBuilder.ToString();
                // When DATA keyword is reached, metadata parsing is complete
                return SyntaxValidationUtilityMethods.CleanString(stringEntry, syntaxConf).Equals(syntaxConf.Tokens.KeyWords.Data);
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateLineAndCharacter(char currentCharacter, PxFileSyntaxConf syntaxConf, ref int character, ref List<int> linebreakIndexes, ref bool isProcessingString)
        {
            if (currentCharacter == syntaxConf.Symbols.Linebreak)
            {
                linebreakIndexes.Add(character);
            }
            else
            {
                if (currentCharacter == syntaxConf.Symbols.Key.StringDelimeter)
                {
                    isProcessingString = !isProcessingString;
                }
            }
            character++;
        }

        private static ValidationStructuredEntryKey ParseStructuredValidationEntryKey(string input, PxFileSyntaxConf syntaxConf)
        {
            ExtractSectionResult languageResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(input, syntaxConf.Symbols.Key.LangParamStart, syntaxConf.Symbols.Key.StringDelimeter, syntaxConf.Symbols.Key.LangParamEnd);
            string? language = languageResult.Sections.Length > 0 ? SyntaxValidationUtilityMethods.CleanString(languageResult.Sections[0], syntaxConf) : null;
            ExtractSectionResult specifierResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(languageResult.Remainder, syntaxConf.Symbols.Key.SpecifierParamStart, syntaxConf.Symbols.Key.StringDelimeter, syntaxConf.Symbols.Key.SpecifierParamEnd);
            string[] specifiers = specifierResult.Sections.Length > 0
                ? SyntaxValidationUtilityMethods.ExtractSectionFromString(
                    specifierResult.Sections[0], 
                    syntaxConf.Symbols.Key.StringDelimeter, 
                    syntaxConf.Symbols.Key.StringDelimeter)
                .Sections 
                : [];
            string? firstSpecifier = specifiers.Length > 0 ? SyntaxValidationUtilityMethods.CleanString(specifiers[0], syntaxConf) : null;
            string? secondSpecifier = specifiers.Length > 1 ? SyntaxValidationUtilityMethods.CleanString(specifiers[1], syntaxConf) : null;
            string keyword = SyntaxValidationUtilityMethods.CleanString(specifierResult.Remainder, syntaxConf);

            return new(keyword, language, firstSpecifier, secondSpecifier);
        }
    }
}
