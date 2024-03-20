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
        /// <summary>
        /// Represents the state of the line and character position in the file as it's being processed by the stream reader
        /// </summary>
        public class LineCharacterState
        {
            public int Line { get; set; }
            public int Character { get; set; }
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
        /// <returns>A <see cref="SyntaxValidationResult"/> object which contains a <see cref="SyntaxValidationResult"/> and a list of <see cref="StructuredValidationEntry"/> objects. The ValidationReport contains feedback items that provide information about any syntax errors or warnings found during validation. The list of StructuredValidationEntry objects represents the structured entries in the PX file that were validated.</returns>
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

                List<StringValidationEntry> stringEntries = BuildStringEntries(stream, encoding, syntaxConf, filename, bufferSize);
                ValidateEntries(stringEntries, stringValidationFunctions, report, syntaxConf);
                List<KeyValuePairValidationEntry> keyValuePairs = BuildKeyValuePairs(stringEntries, syntaxConf);
                ValidateEntries(keyValuePairs, keyValueValidationFunctions, report, syntaxConf);
                List<StructuredValidationEntry> structuredEntries = BuildStructuredEntries(keyValuePairs, syntaxConf);
                ValidateEntries(structuredEntries, structuredValidationFunctions, report, syntaxConf);

                return new(report, structuredEntries);
            }
            catch (InvalidPxFileMetadataException)
            {
                report.FeedbackItems.Add(new ValidationFeedbackItem(new StringValidationEntry(0, 0, filename, string.Empty, 0), new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.NoEncoding)));
                return new(report, []);
            }
        }

        ///<summary>
        /// Validates a collection of entries using a collection of validation functions and adds any feedback to the provided report.
        ///</summary>
        /// <param name="entries">The collection of <see cref="ValidationEntry"/> entries to be validated.</param>
        /// <param name="validationFunctions">The collection of <see cref="IValidationFunction"/> validation functions to be used for validation.</param>
        /// <param name="report">The <see cref="ValidationReport"/> report where validation feedback will be added.</param>
        public static void ValidateEntries(IEnumerable<ValidationEntry> entries, IEnumerable<ValidationFunctionDelegate> validationFunctions, ValidationReport report, PxFileSyntaxConf syntaxConf)
        {
            foreach (var entry in entries)
            {
                foreach (ValidationFunctionDelegate function in validationFunctions)
                {
                    ValidationFeedbackItem? feedback = function(entry, syntaxConf);
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
                return new KeyValuePairValidationEntry(entry.Line, entry.Character, entry.File, new KeyValuePair<string, string>(split[0], value));
            }).ToList();
        }

        /// <summary>
        /// Builds a list of <see cref="StructuredValidationEntry"/> objects from a list of <see cref="KeyValuePairValidationEntry"/> objects.
        /// </summary>
        /// <param name="keyValuePairs">The list of <see cref="KeyValuePairValidationEntry"/> objects to be converted.</param>
        /// <returns>A list of <see cref="StructuredValidationEntry"/> objects built from the provided list of <see cref="KeyValuePairValidationEntry"/> objects.</returns>
        public static List<StructuredValidationEntry> BuildStructuredEntries(List<KeyValuePairValidationEntry> keyValuePairs, PxFileSyntaxConf syntaxConf)
        {
            return keyValuePairs.Select(entry =>
            {
                ValidationEntryKey key = ParseValidationEntryKey(entry.KeyValueEntry.Key, syntaxConf);
                return new StructuredValidationEntry(entry.Line, entry.Character, entry.File, key, entry.KeyValueEntry.Value);
            }).ToList();
        }

        /// <summary>
        /// Processes the buffer and builds a list of <see cref="StringValidationEntry"/> objects from the provided stream.
        /// </summary>
        /// <param name="buffer">The character buffer to process</param>
        /// <param name="syntaxConf"><see cref="PxFileSyntaxConf"/> configuration object</param>
        /// <param name="state"><see cref="LineCharacterState"/> object that stores the current indeces of line and character being processed</see></param>
        /// <param name="filename">Name of the file being processed</param>
        /// <param name="stringEntries">List of string entries to be populated by the method</param>
        /// <param name="entryBuilder">String builder that contains the current entry</param>
        public static void ProcessBuffer(char[] buffer, PxFileSyntaxConf syntaxConf, LineCharacterState state, string filename, List<StringValidationEntry> stringEntries, StringBuilder entryBuilder)
        {
            foreach (char currentCharacter in buffer)
            {
                if (IsEndOfMetadataSection(currentCharacter, syntaxConf, entryBuilder))
                {
                    break;
                }
                UpdateLineAndCharacter(currentCharacter, syntaxConf, state);
                CheckForEntryEnd(currentCharacter, syntaxConf, state, filename, stringEntries, entryBuilder);
            }
        }

        private static List<StringValidationEntry> BuildStringEntries(Stream stream, Encoding encoding, PxFileSyntaxConf syntaxConf, string filename, int bufferSize)
        {
            SyntaxValidation.LineCharacterState state = new() { Line = 0, Character = 0 };
            stream.Seek(0, SeekOrigin.Begin);
            using StreamReader reader = new(stream, encoding);
            List<StringValidationEntry> stringEntries = [];
            StringBuilder entryBuilder = new();
            char[] buffer = new char[bufferSize];

            while (reader.Read(buffer, 0, bufferSize) > 0)
            {
                ProcessBuffer(buffer, syntaxConf, state, filename, stringEntries, entryBuilder);
                Array.Clear(buffer, 0, buffer.Length);
            }
            return stringEntries;
        }

        private static bool IsEndOfMetadataSection(char currentCharacter, PxFileSyntaxConf syntaxConf, StringBuilder entryBuilder)
        {
            if (currentCharacter == syntaxConf.Symbols.KeywordSeparator)
            {
                string stringEntry = entryBuilder.ToString();
                // When DATA keyword is reached, metadata parsing is complete
                return SyntaxValidationUtilityMethods.CleanString(stringEntry).Equals(syntaxConf.Tokens.KeyWords.Data);
            }
            return false;
        }

        private static void UpdateLineAndCharacter(char currentCharacter, PxFileSyntaxConf syntaxConf, LineCharacterState state)
        {
            if (currentCharacter == syntaxConf.Symbols.LineSeparator)
            {
                state.Line++;
                state.Character = 0;
            }
            else
            {
                state.Character++;
            }
        }

        private static void CheckForEntryEnd(char currentCharacter, PxFileSyntaxConf syntaxConf, LineCharacterState state, string filename, List<StringValidationEntry> stringEntries, StringBuilder entryBuilder)
        {
            if (currentCharacter == syntaxConf.Symbols.SectionSeparator)
            {
                string stringEntry = entryBuilder.ToString();
                stringEntries.Add(new StringValidationEntry(state.Line, state.Character, filename, stringEntry, stringEntries.Count));
                entryBuilder.Clear();
            }
            else
            {
                entryBuilder.Append(currentCharacter);
            }
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
