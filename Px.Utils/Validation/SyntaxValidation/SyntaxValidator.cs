using Px.Utils.PxFile;
using Px.Utils.Validation.DatabaseValidation;
using System.Runtime.CompilerServices;
using System.Text;

namespace Px.Utils.Validation.SyntaxValidation
{
    /// <summary>
    /// Provides methods for validating the syntax of a PX file. Validation can be done using both synchronous and asynchronous methods.
    /// Additionally custom validation functions can be provided to be used during validation.
    /// <param name="conf">Object that stores syntax specific symbols and tokens for the PX file</param>
    /// <param name="customValidationFunctions">Object that contains any optional additional validation functions</param>
    /// his is required if multiple validations are executed for the same stream.</param>
    /// </summary>
    public class SyntaxValidator(
            PxFileConfiguration? conf = null,
            CustomSyntaxValidationFunctions? customValidationFunctions = null) 
        : IPxFileStreamValidator, IPxFileStreamValidatorAsync
    {
        private const int _bufferSize = 4096;
        private int _dataSectionStartRow = -1;
        private int _dataSectionStartStreamPosition = -1;

        ///<summary>
        /// Validates the syntax of a PX file's metadata.
        ///</summary>
        /// <param name="stream">Stream of the PX file to be validated</param>
        /// <param name="filename">Name of the PX file</param>
        /// <param name="encoding">Encoding of the PX file. If not provided, the validator attempts to find the encoding.</param>
        /// <param name="fileSystem">File system to use for file operations. If not provided, default file system is used.</param>
        /// <returns>A <see cref="SyntaxValidationResult"/> entry which contains a list of <see cref="ValidationStructuredEntry"/> entries 
        /// and a dictionary of type <see cref="ValidationFeedback"/> accumulated during the validation.</returns>
        public SyntaxValidationResult Validate(
            Stream stream,
            string filename,
            Encoding? encoding = null,
            IFileSystem? fileSystem = null)
        {
            fileSystem ??= new LocalFileSystem();
            encoding ??= fileSystem.GetEncoding(stream);

            SyntaxValidationFunctions validationFunctions = new();
            IEnumerable<EntryValidationFunction> stringValidationFunctions = validationFunctions.DefaultStringValidationFunctions;
            IEnumerable<KeyValuePairValidationFunction> keyValueValidationFunctions = validationFunctions.DefaultKeyValueValidationFunctions;
            IEnumerable<StructuredValidationFunction> structuredValidationFunctions = validationFunctions.DefaultStructuredValidationFunctions;

            if (customValidationFunctions is not null)
            {
                stringValidationFunctions = stringValidationFunctions.Concat(customValidationFunctions.CustomStringValidationFunctions);
                keyValueValidationFunctions = keyValueValidationFunctions.Concat(customValidationFunctions.CustomKeyValueValidationFunctions);
                structuredValidationFunctions = structuredValidationFunctions.Concat(customValidationFunctions.CustomStructuredValidationFunctions);
            }

            conf ??= PxFileConfiguration.Default;

            ValidationFeedback validationFeedbacks = [];
            List<ValidationEntry> stringEntries = BuildValidationEntries(stream, encoding, conf, filename, _bufferSize);
            validationFeedbacks.AddRange(ValidateEntries(stringEntries, stringValidationFunctions, conf));
            List<ValidationKeyValuePair> keyValuePairs = BuildKeyValuePairs(stringEntries, conf);
            validationFeedbacks.AddRange(ValidateKeyValuePairs(keyValuePairs, keyValueValidationFunctions, conf));
            List<ValidationStructuredEntry> structuredEntries = BuildValidationStructureEntries(keyValuePairs, conf);
            validationFeedbacks.AddRange(ValidateStructs(structuredEntries, structuredValidationFunctions, conf));

            return new SyntaxValidationResult(validationFeedbacks, structuredEntries, _dataSectionStartRow, _dataSectionStartStreamPosition);
        }

        /// <summary>
        /// Asynchronously validates the syntax of a PX file's metadata.
        /// </summary>
        /// <param name="stream">Stream of the PX file to be validated</param>
        /// <param name="filename">Name of the PX file</param>
        /// <param name="encoding">Encoding of the PX file. If not provided, the validator attempts to find the encoding.</param>
        /// <param name="fileSystem">File system to use for file operations. If not provided, default file system is used.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> parameter that can be used to cancel the operation.</param>
        /// <returns>A task that contains a <see cref="SyntaxValidationResult"/> entry which contains a list of <see cref="ValidationStructuredEntry"/> entries 
        /// and a dictionary of type <see cref="ValidationFeedback"/> accumulated during the validation.</returns>
        public async Task<SyntaxValidationResult> ValidateAsync(
            Stream stream,
            string filename,
            Encoding? encoding = null,
            IFileSystem? fileSystem = null,
            CancellationToken cancellationToken = default)
        {
            fileSystem ??= new LocalFileSystem();
            encoding ??= await fileSystem.GetEncodingAsync(stream, cancellationToken);

            SyntaxValidationFunctions validationFunctions = new();
            IEnumerable<EntryValidationFunction> stringValidationFunctions = validationFunctions.DefaultStringValidationFunctions;
            IEnumerable<KeyValuePairValidationFunction> keyValueValidationFunctions = validationFunctions.DefaultKeyValueValidationFunctions;
            IEnumerable<StructuredValidationFunction> structuredValidationFunctions = validationFunctions.DefaultStructuredValidationFunctions;

            if (customValidationFunctions is not null)
            {
                stringValidationFunctions = stringValidationFunctions.Concat(customValidationFunctions.CustomStringValidationFunctions);
                keyValueValidationFunctions = keyValueValidationFunctions.Concat(customValidationFunctions.CustomKeyValueValidationFunctions);
                structuredValidationFunctions = structuredValidationFunctions.Concat(customValidationFunctions.CustomStructuredValidationFunctions);
            }

            conf ??= PxFileConfiguration.Default;
            ValidationFeedback validationFeedbacks = [];
            List<ValidationEntry> entries = await BuildValidationEntriesAsync(stream, encoding, conf, filename, _bufferSize, cancellationToken);
            validationFeedbacks.AddRange(ValidateEntries(entries, stringValidationFunctions, conf));
            List<ValidationKeyValuePair> keyValuePairs = BuildKeyValuePairs(entries, conf);
            validationFeedbacks.AddRange(ValidateKeyValuePairs(keyValuePairs, keyValueValidationFunctions, conf));
            List<ValidationStructuredEntry> structuredEntries = BuildValidationStructureEntries(keyValuePairs, conf);
            validationFeedbacks.AddRange(ValidateStructs(structuredEntries, structuredValidationFunctions, conf));

            return new SyntaxValidationResult(validationFeedbacks, structuredEntries, _dataSectionStartRow, _dataSectionStartStreamPosition);
        }

        #region Interface implementation

        ValidationResult IPxFileStreamValidator.Validate(Stream stream, string filename, Encoding? encoding, IFileSystem? fileSystem)
            => Validate(stream, filename, encoding, fileSystem);

        Task<ValidationResult> IPxFileStreamValidatorAsync.ValidateAsync(
            Stream stream,
            string filename,
            Encoding? encoding,
            IFileSystem? fileSystem,
            CancellationToken cancellationToken)
            => ValidateAsync(stream, filename, encoding, fileSystem, cancellationToken).ContinueWith(task => (ValidationResult)task.Result);

        #endregion

        /// <summary>
        /// Checks whether the end of the metadata section of a px file stream has been reached.
        /// </summary>
        /// <param name="currentCharacter">Character currently being processed</param>
        /// <param name="syntaxConf"><see cref="PxFileConfiguration"/> object that contains symbols and tokens used for px file syntax</param>
        /// <param name="entryBuilder"><see cref="StringBuilder"/> object that contains the currently processed px file entry</param>
        /// <param name="isProcessingString">Whether the character currently processed is enclosed between string delimiters</param>
        /// <returns>True if the end of the metadata section has been reached, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEndOfMetadataSection(char currentCharacter, PxFileConfiguration syntaxConf, StringBuilder entryBuilder, bool isProcessingString)
        {
            if (!isProcessingString && currentCharacter == syntaxConf.Symbols.KeywordSeparator)
            {
                string stringEntry = entryBuilder.ToString();
                // When DATA keyword is reached, metadata parsing is complete
                return SyntaxValidationUtilityMethods.CleanString(stringEntry, syntaxConf).Equals(syntaxConf.Tokens.KeyWords.Data, StringComparison.Ordinal);
            }
            return false;
        }

        private static ValidationFeedback ValidateEntries(IEnumerable<ValidationEntry> entries, IEnumerable<EntryValidationFunction> validationFunctions, PxFileConfiguration syntaxConf)
        {
            ValidationFeedback validationFeedback = [];
            foreach (ValidationEntry entry in entries)
            {
                foreach (EntryValidationFunction function in validationFunctions)
                {
                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? feedback = function(entry, syntaxConf);
                    if (feedback is not null)
                    {
                        validationFeedback.Add((KeyValuePair <ValidationFeedbackKey, ValidationFeedbackValue>)feedback);
                    }
                }
            }
            return validationFeedback;
        }

        private static ValidationFeedback ValidateKeyValuePairs(
            IEnumerable<ValidationKeyValuePair> kvpObjects,
            IEnumerable<KeyValuePairValidationFunction> validationFunctions,
            PxFileConfiguration syntaxConf)
        {
            ValidationFeedback validationFeedback = [];
            foreach (ValidationKeyValuePair kvpObject in kvpObjects)
            {
                foreach (KeyValuePairValidationFunction function in validationFunctions)
                {
                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? feedback = function(kvpObject, syntaxConf);
                    if (feedback is not null)
                    {
                        validationFeedback.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)feedback);
                    }
                }
            }
            return validationFeedback;
        }

        private static ValidationFeedback ValidateStructs(
            IEnumerable<ValidationStructuredEntry> structuredEntries, 
            IEnumerable<StructuredValidationFunction> validationFunctions,
            PxFileConfiguration syntaxConf)
        {
            ValidationFeedback validationFeedback = [];
            foreach (ValidationStructuredEntry structuredEntry in structuredEntries)
            {
                foreach (StructuredValidationFunction function in validationFunctions)
                {
                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? feedback = function(structuredEntry, syntaxConf);
                    if (feedback is not null)
                    {
                        validationFeedback.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)feedback);
                    }
                }
            }
            return validationFeedback;
        }

        private static List<ValidationKeyValuePair> BuildKeyValuePairs(List<ValidationEntry> validationEntries, PxFileConfiguration syntaxConf)
        {
            return validationEntries.Select(entry =>
            {
                // Split the entry string into a key and a value from the first valid keyword separator
                int[] keywordSeparatorIndeces = SyntaxValidationUtilityMethods.FindKeywordSeparatorIndeces(entry.EntryString, syntaxConf);

                if (keywordSeparatorIndeces.Length == 0)
                {
                    return new ValidationKeyValuePair(entry.File, new KeyValuePair<string, string>(entry.EntryString, string.Empty), entry.KeyStartLineIndex, entry.LineChangeIndexes, -1);
                }

                // Key is the part of the entry string before the first keyword separator index
                string key = entry.EntryString[..keywordSeparatorIndeces[0]];

                // Value is the part of the entry string after the first keyword separator index
                int valueStart = keywordSeparatorIndeces[0] + 1;
                string value = entry.EntryString[valueStart..];

                return new ValidationKeyValuePair(entry.File, new KeyValuePair<string, string>(key, value), entry.KeyStartLineIndex, entry.LineChangeIndexes, valueStart);
            }).ToList();
        }

        private static List<ValidationStructuredEntry> BuildValidationStructureEntries(List<ValidationKeyValuePair> keyValuePairs, PxFileConfiguration syntaxConf)
        {
            return keyValuePairs.Select(entry =>
            {
                ValueType? valueType = SyntaxValidationUtilityMethods.GetValueTypeFromString(entry.KeyValuePair.Value, syntaxConf);
                ValidationStructuredEntryKey key = ParseStructuredValidationEntryKey(entry.KeyValuePair.Key, syntaxConf);
                return new ValidationStructuredEntry(entry.File, key, entry.KeyValuePair.Value, entry.KeyStartLineIndex, entry.LineChangeIndexes, entry.ValueStartIndex, valueType);
            }).ToList();
        }

        private List<ValidationEntry> BuildValidationEntries(Stream stream, Encoding encoding, PxFileConfiguration syntaxConf, string filename, int bufferSize)
        {
            bool isProcessingString = false;
            int characterIndex = 0;
            List<int> lineChangeIndexes = [];
            int entryStartIndex = 0;
            int entryStartLineIndex = 0;
            using StreamReader reader = new(stream, encoding, leaveOpen: true);
            List<ValidationEntry> entries = [];
            StringBuilder entryBuilder = new();
            char[] buffer = new char[bufferSize];
            while (reader.Read(buffer, 0, bufferSize) > 0)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (IsEndOfMetadataSection(buffer[i], syntaxConf, entryBuilder, isProcessingString))
                    {
                        _dataSectionStartRow = lineChangeIndexes.Count;
                        // This here should find the actual start of the data section, after line changes, spaces and whatnot.
                        _dataSectionStartStreamPosition = characterIndex + 1;
                        return entries;
                    }
                    UpdateLineAndCharacter(buffer[i], syntaxConf, ref characterIndex, ref lineChangeIndexes, ref isProcessingString);
                    if (!isProcessingString && buffer[i] == syntaxConf.Symbols.EntrySeparator)
                    {
                        string stringEntry = entryBuilder.ToString();
                        int[] entryLineChangeIndexes = GetStartCharacterAndLineChangeIndexes(entryStartIndex, lineChangeIndexes);
                        entries.Add(new ValidationEntry(filename, stringEntry, entries.Count, entryStartLineIndex + 1, entryLineChangeIndexes));
                        entryBuilder.Clear();
                        entryStartIndex = characterIndex;
                        entryStartLineIndex = lineChangeIndexes.Count;
                    }
                    else
                    {
                        entryBuilder.Append(buffer[i]);
                    }
                }
            }
            return entries;
        }

        private static int[] GetStartCharacterAndLineChangeIndexes(int entryStartIndex, List<int> lineChangeIndexes)
        {
            int[] entryLineChangeIndexes = lineChangeIndexes.Where(i => i >= entryStartIndex).ToArray();
            for (int i = 0; i < entryLineChangeIndexes.Length; i++)
            {
                entryLineChangeIndexes[i] -= entryStartIndex;
            }
            return entryLineChangeIndexes;
        }

        private async Task<List<ValidationEntry>> BuildValidationEntriesAsync(
            Stream stream, 
            Encoding encoding,
            PxFileConfiguration syntaxConf,
            string filename,
            int bufferSize,
            CancellationToken cancellationToken)
        {
            bool isProcessingString = false;
            int characterIndex = 0;
            List<int> lineChangeIndexes = [];
            int entryStartIndex = 0;
            int entryStartLineIndex = 0;
            
            using StreamReader reader = new(stream, encoding, leaveOpen: true);
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
                        _dataSectionStartRow = lineChangeIndexes.Count;
                        _dataSectionStartStreamPosition = characterIndex + 1;
                        return entries;
                    }
                    UpdateLineAndCharacter(buffer[i], syntaxConf, ref characterIndex, ref lineChangeIndexes, ref isProcessingString);
                    if (!isProcessingString && buffer[i] == syntaxConf.Symbols.EntrySeparator)
                    {
                        string stringEntry = entryBuilder.ToString();
                        int[] entryLineChangeIndexes = GetStartCharacterAndLineChangeIndexes(entryStartIndex, lineChangeIndexes);
                        entries.Add(new ValidationEntry(filename, stringEntry, entries.Count, entryStartLineIndex + 1, entryLineChangeIndexes));
                        entryBuilder.Clear();
                        entryStartIndex = characterIndex;
                        entryStartLineIndex = lineChangeIndexes.Count;
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
        private static void UpdateLineAndCharacter(char currentCharacter, PxFileConfiguration syntaxConf, ref int characterIndex, ref List<int> linebreakIndexes, ref bool isProcessingString)
        {
            if (currentCharacter == syntaxConf.Symbols.Linebreak)
            {
                linebreakIndexes.Add(characterIndex);
            }
            else if (currentCharacter == syntaxConf.Symbols.Key.StringDelimeter)
            {
                isProcessingString = !isProcessingString;
            }
            characterIndex++;
        }

        private static ValidationStructuredEntryKey ParseStructuredValidationEntryKey(string input, PxFileConfiguration conf)
        {
            ExtractSectionResult languageResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                input, 
                conf.Symbols.Key.LangParamStart, 
                conf.Symbols.Key.StringDelimeter,
                conf.Symbols.Key.LangParamEnd);
            string? language = languageResult.Sections.Length > 0 ? 
                SyntaxValidationUtilityMethods.CleanString(languageResult.Sections[0], conf).Trim(conf.Symbols.Key.StringDelimeter) :
                null;
            ExtractSectionResult specifierResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(languageResult.Remainder,
                conf.Symbols.Key.SpecifierParamStart, 
                conf.Symbols.Key.StringDelimeter, 
                conf.Symbols.Key.SpecifierParamEnd);
            string[] specifiers = specifierResult.Sections.Length > 0
                ? SyntaxValidationUtilityMethods.ExtractSectionFromString(
                    specifierResult.Sections[0], 
                    conf.Symbols.Key.StringDelimeter, 
                    conf.Symbols.Key.StringDelimeter)
                .Sections 
                : [];
            string? firstSpecifier = specifiers.Length > 0 ?
                SyntaxValidationUtilityMethods.CleanString(specifiers[0], conf).Trim(conf.Symbols.Key.StringDelimeter) : 
                null;
            string? secondSpecifier = specifiers.Length > 1 ?
                SyntaxValidationUtilityMethods.CleanString(specifiers[1], conf).Trim(conf.Symbols.Key.StringDelimeter) :
                null;
            string keyword = SyntaxValidationUtilityMethods.CleanString(specifierResult.Remainder, conf);

            return new(keyword, language, firstSpecifier, secondSpecifier);
        }
    }
}
