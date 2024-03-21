﻿using PxUtils.Exceptions;
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
        /// <summary>
        /// Represents the state of the line and character position in the file as it's being processed by the stream reader
        /// </summary>
        public class LineCharacterState
        {
            public int Line { get; set; } = 0;
            public int Character { get; set; } = 0;
            public bool IsProcessingString { get; set; } = false;
        }

        /// <summary>
        /// Collection of custom validation functions to be used during validation.
        /// </summary>
        public class CustomValidationFunctions(
            IEnumerable<ValidationFunctionDelegate> stringValidationFunctions, 
            IEnumerable<ValidationFunctionDelegate> keyValueValidationFunctions, IEnumerable<ValidationFunctionDelegate> structuredValidationFunctions)
        {
            public IEnumerable<ValidationFunctionDelegate> CustomStringValidationFunctions { get; } = stringValidationFunctions;
            public IEnumerable<ValidationFunctionDelegate> CustomKeyValueValidationFunctions { get; } = keyValueValidationFunctions;
            public IEnumerable<ValidationFunctionDelegate> CustomStructuredValidationFunctions { get; } = structuredValidationFunctions;
        }

        private const int DEFAULT_BUFFER_SIZE = 4096;

        ///<summary>
        /// Validates the syntax of a PX file's metadata.
        ///</summary>
        /// <param name="stream">The stream of the PX file to be validated.</param>
        /// <param name="filename">The name of the file to be validated.</param>
        /// <param name="syntaxConf">An optional <see cref="PxFileSyntaxConf"/> parameter that specifies the syntax configuration for the PX file. If not provided, the default syntax configuration is used.</param>
        /// <param name="bufferSize">An optional parameter that specifies the buffer size for reading the file. If not provided, a default buffer size of 4096 is used.</param>
        /// <param name="customValidationFunctions">An optional <see cref="CustomValidationFunctions"/> parameter that specifies custom validation functions to be used during validation. If not provided, the default validation functions are used.</param>
        /// <returns>A <see cref="SyntaxValidationResult"/> object which contains a <see cref="SyntaxValidationResult"/> and a list of <see cref="ValidationStruct"/> objects. The ValidationReport contains feedback items that provide information about any syntax errors or warnings found during validation. The list of ValidationStruct objects represents the structured objects in the PX file that were validated.</returns>
        public static SyntaxValidationResult ValidatePxFileMetadataSyntax(
            Stream stream,
            string filename,
            PxFileSyntaxConf? syntaxConf = null,
            int bufferSize = DEFAULT_BUFFER_SIZE,
            CustomValidationFunctions? customValidationFunctions = null)
        {
            SyntaxValidationFunctions validationFunctions = new();
            IEnumerable<ValidationFunctionDelegate> stringValidationFunctions = validationFunctions.DefaultStringValidationFunctions;
            IEnumerable<ValidationFunctionDelegate> keyValueValidationFunctions = validationFunctions.DefaultKeyValueValidationFunctions;
            IEnumerable<ValidationFunctionDelegate> structuredValidationFunctions = validationFunctions.DefaultStructuredValidationFunctions;

            if (customValidationFunctions is not null)
            {
                stringValidationFunctions = stringValidationFunctions.Concat(customValidationFunctions.CustomStringValidationFunctions);
                keyValueValidationFunctions = keyValueValidationFunctions.Concat(customValidationFunctions.CustomKeyValueValidationFunctions);
                structuredValidationFunctions = structuredValidationFunctions.Concat(customValidationFunctions.CustomStructuredValidationFunctions);
            }

            syntaxConf ??= PxFileSyntaxConf.Default;
            ValidationReport report = new();

            try
            {
                Encoding encoding = PxFileMetadataReader.GetEncoding(stream, syntaxConf);

                List<ValidationEntry> stringEntries = BuildValidationEntries(stream, encoding, syntaxConf, filename, bufferSize);
                ValidateObjects(stringEntries, stringValidationFunctions, report, syntaxConf);
                List<ValidationKeyValuePair> keyValuePairs = BuildKeyValuePairs(stringEntries, syntaxConf);
                ValidateObjects(keyValuePairs, keyValueValidationFunctions, report, syntaxConf);
                List<ValidationStruct> structuredEntries = BuildValidationStructs(keyValuePairs, syntaxConf);
                ValidateObjects(structuredEntries, structuredValidationFunctions, report, syntaxConf);

                return new(report, structuredEntries);
            }
            catch (InvalidPxFileMetadataException)
            {
                report.FeedbackItems.Add(new ValidationFeedbackItem(new ValidationEntry(0, 0, filename, string.Empty, 0), new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.NoEncoding)));
                return new(report, []);
            }
        }

        /// <summary>
        /// Asynchronously validates the syntax of a PX file's metadata.
        /// </summary>
        /// <param name="stream">The stream of the PX file to be validated.</param>
        /// <param name="filename">The name of the file to be validated.</param>
        /// <param name="syntaxConf">An optional <see cref="PxFileSyntaxConf"/> parameter that specifies the syntax configuration for the PX file. If not provided, the default syntax configuration is used.</param>
        /// <param name="bufferSize">An optional parameter that specifies the buffer size for reading the file. If not provided, a default buffer size of 4096 is used.</param>
        /// <param name="customValidationFunctions">An optional <see cref="CustomValidationFunctions"/> parameter that specifies custom validation functions to be used during validation. If not provided, the default validation functions are used.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> parameter that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task result contains a <see cref="SyntaxValidationResult"/> object which includes a <see cref="ValidationReport"/> and a list of <see cref="ValidationStruct"/> objects. The ValidationReport contains feedback items that provide information about any syntax errors or warnings found during validation. The list of ValidationStruct objects represents the structured objects in the PX file that were validated.</returns>
        public static async Task<SyntaxValidationResult> ValidatePxFileMetadataSyntaxAsync(
            Stream stream,
            string filename,
            PxFileSyntaxConf? syntaxConf = null,
            int bufferSize = DEFAULT_BUFFER_SIZE,
            CustomValidationFunctions? customValidationFunctions = null,
            CancellationToken cancellationToken = default)
        {
            SyntaxValidationFunctions validationFunctions = new();
            IEnumerable<ValidationFunctionDelegate> stringValidationFunctions = validationFunctions.DefaultStringValidationFunctions;
            IEnumerable<ValidationFunctionDelegate> keyValueValidationFunctions = validationFunctions.DefaultKeyValueValidationFunctions;
            IEnumerable<ValidationFunctionDelegate> structuredValidationFunctions = validationFunctions.DefaultStructuredValidationFunctions;

            if (customValidationFunctions is not null)
            {
                stringValidationFunctions = stringValidationFunctions.Concat(customValidationFunctions.CustomStringValidationFunctions);
                keyValueValidationFunctions = keyValueValidationFunctions.Concat(customValidationFunctions.CustomKeyValueValidationFunctions);
                structuredValidationFunctions = structuredValidationFunctions.Concat(customValidationFunctions.CustomStructuredValidationFunctions);
            }

            syntaxConf ??= PxFileSyntaxConf.Default;
            ValidationReport report = new();

            try
            {
                Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream, syntaxConf, cancellationToken);

                List<ValidationEntry> entries = await BuildValidationEntriesAsync(stream, encoding, syntaxConf, filename, bufferSize);
                ValidateObjects(entries, stringValidationFunctions, report, syntaxConf);
                List<ValidationKeyValuePair> keyValuePairs = BuildKeyValuePairs(entries, syntaxConf);
                ValidateObjects(keyValuePairs, keyValueValidationFunctions, report, syntaxConf);
                List<ValidationStruct> structs = BuildValidationStructs(keyValuePairs, syntaxConf);
                ValidateObjects(structs, structuredValidationFunctions, report, syntaxConf);

                return new(report, structs);
            }
            catch (InvalidPxFileMetadataException)
            {
                report.FeedbackItems.Add(new ValidationFeedbackItem(new ValidationEntry(0, 0, filename, string.Empty, 0), new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.NoEncoding)));
                return new(report, []);
            }
        }

        ///<summary>
        /// Validates a collection of objects using a collection of validation functions and adds any feedback to the provided report.
        ///</summary>
        /// <param name="objects">The collection of <see cref="ValidationObject"/> objects to be validated.</param>
        /// <param name="validationFunctions">The collection of <see cref="IValidationFunction"/> validation functions to be used for validation.</param>
        /// <param name="report">The <see cref="ValidationReport"/> report where validation feedback will be added.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        public static void ValidateObjects(IEnumerable<ValidationObject> objects, IEnumerable<ValidationFunctionDelegate> validationFunctions, ValidationReport report, PxFileSyntaxConf syntaxConf)
        {
            foreach (var @object in objects)
            {
                foreach (ValidationFunctionDelegate function in validationFunctions)
                {
                    ValidationFeedbackItem? feedback = function(@object, syntaxConf);
                    if (feedback is not null)
                    {
                        report.FeedbackItems.Add((ValidationFeedbackItem)feedback);
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
                string[] split = entry.EntryString.Split(entry.EntryString[keywordSeparatorIndeces[0]]);
                string value = split.Length > 1 ? split[1] : string.Empty;
                return new ValidationKeyValuePair(entry.Line, entry.Character, entry.File, new KeyValuePair<string, string>(split[0], value));
            }).ToList();
        }

        private static List<ValidationStruct> BuildValidationStructs(List<ValidationKeyValuePair> keyValuePairs, PxFileSyntaxConf syntaxConf)
        {
            return keyValuePairs.Select(entry =>
            {
                ValidationStructKey key = ParseValidationStructKey(entry.KeyValuePair.Key, syntaxConf);
                return new ValidationStruct(entry.Line, entry.Character, entry.File, key, entry.KeyValuePair.Value);
            }).ToList();
        }

        private static List<ValidationEntry> BuildValidationEntries(Stream stream, Encoding encoding, PxFileSyntaxConf syntaxConf, string filename, int bufferSize)
        {
            LineCharacterState state = new();
            stream.Seek(0, SeekOrigin.Begin);
            using StreamReader reader = new(stream, encoding);
            List<ValidationEntry> entries = [];
            StringBuilder entryBuilder = new();
            char[] buffer = new char[bufferSize];

            while (reader.Read(buffer, 0, bufferSize) > 0)
            {
                ProcessBuffer(buffer, syntaxConf, state, filename, entries, entryBuilder);
                Array.Clear(buffer, 0, buffer.Length);
            }
            return entries;
        }

        private static async Task<List<ValidationEntry>> BuildValidationEntriesAsync(Stream stream, Encoding encoding, PxFileSyntaxConf syntaxConf, string filename, int bufferSize)
        {
            LineCharacterState state = new() { Line = 0, Character = 0 };
            stream.Seek(0, SeekOrigin.Begin);
            using StreamReader reader = new(stream, encoding);
            List<ValidationEntry> entries = [];
            StringBuilder entryBuilder = new();
            char[] buffer = new char[bufferSize];

            while (await reader.ReadAsync(buffer, 0, bufferSize) > 0)
            {
                ProcessBuffer(buffer, syntaxConf, state, filename, entries, entryBuilder);
                Array.Clear(buffer, 0, buffer.Length);
            }
            return entries;
        }

        private static void ProcessBuffer(char[] buffer, PxFileSyntaxConf syntaxConf, LineCharacterState state, string filename, List<ValidationEntry> stringEntries, StringBuilder entryBuilder)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (IsEndOfMetadataSection(buffer[i], syntaxConf, entryBuilder, state))
                {
                    break;
                }
                UpdateLineAndCharacter(buffer[i], syntaxConf, state);
                CheckForEntryEnd(buffer[i], syntaxConf, state, filename, stringEntries, entryBuilder);
            }
        }

        private static bool IsEndOfMetadataSection(char currentCharacter, PxFileSyntaxConf syntaxConf, StringBuilder entryBuilder, LineCharacterState state)
        {
            if (!state.IsProcessingString && currentCharacter == syntaxConf.Symbols.KeywordSeparator)
            {
                string stringEntry = entryBuilder.ToString();
                // When DATA keyword is reached, metadata parsing is complete
                return SyntaxValidationUtilityMethods.CleanString(stringEntry, syntaxConf).Equals(syntaxConf.Tokens.KeyWords.Data);
            }
            return false;
        }

        private static void UpdateLineAndCharacter(char currentCharacter, PxFileSyntaxConf syntaxConf, LineCharacterState state)
        {
            if (currentCharacter == syntaxConf.Symbols.Linebreak)
            {
                state.Line++;
                state.Character = 0;
            }
            else
            {
                state.Character++;
            }
        }

        private static void CheckForEntryEnd(char currentCharacter, PxFileSyntaxConf syntaxConf, LineCharacterState state, string filename, List<ValidationEntry> stringEntries, StringBuilder entryBuilder)
        {
            if (!state.IsProcessingString && currentCharacter == syntaxConf.Symbols.EntrySeparator)
            {
                string stringEntry = entryBuilder.ToString();
                stringEntries.Add(new ValidationEntry(state.Line, state.Character, filename, stringEntry, stringEntries.Count));
                entryBuilder.Clear();
            }
            else
            {
                if (currentCharacter == syntaxConf.Symbols.Key.StringDelimeter)
                {
                    state.IsProcessingString = !state.IsProcessingString;
                }
                entryBuilder.Append(currentCharacter);
            }
        }

        private static ValidationStructKey ParseValidationStructKey(string input, PxFileSyntaxConf syntaxConf)
        {
            ExtractSectionResult languageResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(input, syntaxConf.Symbols.Key.LangParamStart, syntaxConf, syntaxConf.Symbols.Key.LangParamEnd);
            string? language = languageResult.Sections.Length > 0 ? SyntaxValidationUtilityMethods.CleanString(languageResult.Sections[0], syntaxConf) : null;
            ExtractSectionResult specifierResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(languageResult.Remainder, syntaxConf.Symbols.Key.SpecifierParamStart, syntaxConf, syntaxConf.Symbols.Key.SpecifierParamEnd);
            string[] specifiers = SyntaxValidationUtilityMethods.GetSpecifiersFromParameter(specifierResult.Sections.FirstOrDefault(), syntaxConf.Symbols.Key.StringDelimeter, syntaxConf);
            string? firstSpecifier = specifiers.Length > 0 ? SyntaxValidationUtilityMethods.CleanString(specifiers[0], syntaxConf) : null;
            string? secondSpecifier = specifiers.Length > 1 ? SyntaxValidationUtilityMethods.CleanString(specifiers[1], syntaxConf) : null;
            string keyword = SyntaxValidationUtilityMethods.CleanString(specifierResult.Remainder, syntaxConf);

            return new(keyword, language, firstSpecifier, secondSpecifier);
        }
    }
}
