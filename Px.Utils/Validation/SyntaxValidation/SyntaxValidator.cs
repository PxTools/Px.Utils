using Px.Utils.PxFile;
using System.Runtime.CompilerServices;
using System.Text;

namespace Px.Utils.Validation.SyntaxValidation
{
    /// <summary>
    /// Provides methods for validating the syntax of a PX file. Validation can be done using both synchronous and asynchronous methods.
    /// Additionally custom validation functions can be provided to be used during validation.
    /// <param name="stream">Stream of the PX file to be validated</param>
    /// <param name="encoding">Encoding of the PX file</param>
    /// <param name="filename">Name of the PX file</param>
    /// <param name="syntaxConf">Object that stores syntax specific symbols and tokens for the PX file</param>
    /// <param name="customValidationFunctions">Object that contains any optional additional validation functions</param>
    /// <param name="leaveStreamOpen">Boolean value that determines whether the stream should be left open after validation.
    /// his is required if multiple validations are executed for the same stream.</param>
    /// </summary>
    public class SyntaxValidator(
            Stream stream,
            Encoding encoding,
            string filename,
            PxFileSyntaxConf? syntaxConf = null,
            CustomSyntaxValidationFunctions? customValidationFunctions = null,
            bool leaveStreamOpen = false) : IPxFileValidator, IPxFileValidatorAsync
    {
        private const int _bufferSize = 4096;
        private int _dataSectionStartRow = -1;
        private int _dataSectionStartStreamPosition = -1;

        ///<summary>
        /// Validates the syntax of a PX file's metadata.
        ///</summary>
        /// <returns>A <see cref="SyntaxValidationResult"/> entry which contains a list of <see cref="ValidationStructuredEntry"/> entries 
        /// and a list of <see cref="ValidationFeedbackItem"/> entries accumulated during the validation.</returns>
        public SyntaxValidationResult Validate()
        {
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

            syntaxConf ??= PxFileSyntaxConf.Default;

            List<ValidationFeedbackItem> validationFeedback = [];
            List<ValidationEntry> stringEntries = BuildValidationEntries(stream, encoding, syntaxConf, filename, _bufferSize);
            validationFeedback.AddRange(ValidateEntries(stringEntries, stringValidationFunctions, syntaxConf));
            List<ValidationKeyValuePair> keyValuePairs = BuildKeyValuePairs(stringEntries, syntaxConf);
            validationFeedback.AddRange(ValidateKeyValuePairs(keyValuePairs, keyValueValidationFunctions, syntaxConf));
            List<ValidationStructuredEntry> structuredEntries = BuildValidationStructureEntries(keyValuePairs, syntaxConf);
            validationFeedback.AddRange(ValidateStructs(structuredEntries, structuredValidationFunctions, syntaxConf));

            return new SyntaxValidationResult([.. validationFeedback], structuredEntries, _dataSectionStartRow, _dataSectionStartStreamPosition);
        }

        /// <summary>
        /// Asynchronously validates the syntax of a PX file's metadata.
        /// </summary>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> parameter that can be used to cancel the operation.</param>
        /// <returns>A task that contains a <see cref="SyntaxValidationResult"/> entry, which contains the structured validation entries 
        /// and a list of <see cref="ValidationStructuredEntry"/> entries accumulated during the validation.</returns>
        public async Task<SyntaxValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
        {
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

            syntaxConf ??= PxFileSyntaxConf.Default;
            List<ValidationFeedbackItem> validationFeedback = [];
            List<ValidationEntry> entries = await BuildValidationEntriesAsync(stream, encoding, syntaxConf, filename, _bufferSize, cancellationToken);
            validationFeedback.AddRange(ValidateEntries(entries, stringValidationFunctions, syntaxConf));
            List<ValidationKeyValuePair> keyValuePairs = BuildKeyValuePairs(entries, syntaxConf);
            validationFeedback.AddRange(ValidateKeyValuePairs(keyValuePairs, keyValueValidationFunctions, syntaxConf));
            List<ValidationStructuredEntry> structuredEntries = BuildValidationStructureEntries(keyValuePairs, syntaxConf);
            validationFeedback.AddRange(ValidateStructs(structuredEntries, structuredValidationFunctions, syntaxConf));

            return new SyntaxValidationResult([.. validationFeedback], structuredEntries, _dataSectionStartRow, _dataSectionStartStreamPosition);
        }

        #region Interface implementation

        ValidationResult IPxFileValidator.Validate()
            => Validate();

        async Task<ValidationResult> IPxFileValidatorAsync.ValidateAsync(CancellationToken cancellationToken) 
            => await ValidateAsync(cancellationToken);

        #endregion

        private static List<ValidationFeedbackItem> ValidateEntries(IEnumerable<ValidationEntry> entries, IEnumerable<EntryValidationFunction> validationFunctions, PxFileSyntaxConf syntaxConf)
        {
            List<ValidationFeedbackItem> validationFeedback = [];
            foreach (ValidationEntry entry in entries)
            {
                foreach (EntryValidationFunction function in validationFunctions)
                {
                    ValidationFeedbackItem? feedback = function(entry, syntaxConf);
                    if (feedback is not null)
                    {
                        validationFeedback.Add((ValidationFeedbackItem)feedback);
                    }
                }
            }
            return validationFeedback;
        }

        private static List<ValidationFeedbackItem> ValidateKeyValuePairs(
            IEnumerable<ValidationKeyValuePair> kvpObjects,
            IEnumerable<KeyValuePairValidationFunction> validationFunctions,
            PxFileSyntaxConf syntaxConf)
        {
            List<ValidationFeedbackItem> validationFeedback = [];
            foreach (ValidationKeyValuePair kvpObject in kvpObjects)
            {
                foreach (KeyValuePairValidationFunction function in validationFunctions)
                {
                    ValidationFeedbackItem? feedback = function(kvpObject, syntaxConf);
                    if (feedback is not null)
                    {
                        validationFeedback.Add((ValidationFeedbackItem)feedback);
                    }
                }
            }
            return validationFeedback;
        }

        private static List<ValidationFeedbackItem> ValidateStructs(
            IEnumerable<ValidationStructuredEntry> structuredEntries, 
            IEnumerable<StructuredValidationFunction> validationFunctions,
            PxFileSyntaxConf syntaxConf)
        {
            List<ValidationFeedbackItem> validationFeedback = [];
            foreach (ValidationStructuredEntry structuredEntry in structuredEntries)
            {
                foreach (StructuredValidationFunction function in validationFunctions)
                {
                    ValidationFeedbackItem? feedback = function(structuredEntry, syntaxConf);
                    if (feedback is not null)
                    {
                        validationFeedback.Add((ValidationFeedbackItem)feedback);
                    }
                }
            }
            return validationFeedback;
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

                return new ValidationKeyValuePair(entry.File, new KeyValuePair<string, string>(key, value), entry.KeyStartLineIndex, entry.LineChangeIndexes, valueStart);
            }).ToList();
        }

        private static List<ValidationStructuredEntry> BuildValidationStructureEntries(List<ValidationKeyValuePair> keyValuePairs, PxFileSyntaxConf syntaxConf)
        {
            return keyValuePairs.Select(entry =>
            {
                ValueType? valueType = SyntaxValidationUtilityMethods.GetValueTypeFromString(entry.KeyValuePair.Value, syntaxConf);
                ValidationStructuredEntryKey key = ParseStructuredValidationEntryKey(entry.KeyValuePair.Key, syntaxConf);
                string value = SyntaxValidationUtilityMethods.CleanValue(entry.KeyValuePair.Value, syntaxConf, valueType);
                return new ValidationStructuredEntry(entry.File, key, value, entry.KeyStartLineIndex, entry.LineChangeIndexes, entry.ValueStartIndex, valueType);
            }).ToList();
        }

        private List<ValidationEntry> BuildValidationEntries(Stream stream, Encoding encoding, PxFileSyntaxConf syntaxConf, string filename, int bufferSize)
        {
            bool isProcessingString = false;
            int characterIndex = 0;
            List<int> lineChangeIndexes = [];
            int entryStartIndex = 0;
            int entryStartLineIndex = 0;

            using StreamReader reader = new(stream, encoding, leaveOpen: leaveStreamOpen);
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
            PxFileSyntaxConf syntaxConf,
            string filename,
            int bufferSize,
            CancellationToken cancellationToken)
        {
            bool isProcessingString = false;
            int characterIndex = 0;
            List<int> lineChangeIndexes = [];
            int entryStartIndex = 0;
            int entryStartLineIndex = 0;
            
            using StreamReader reader = new(stream, encoding, leaveOpen: leaveStreamOpen);
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
        private static bool IsEndOfMetadataSection(char currentCharacter, PxFileSyntaxConf syntaxConf, StringBuilder entryBuilder, bool isProcessingString)
        {
            if (!isProcessingString && currentCharacter == syntaxConf.Symbols.KeywordSeparator)
            {
                string stringEntry = entryBuilder.ToString();
                // When DATA keyword is reached, metadata parsing is complete
                return SyntaxValidationUtilityMethods.CleanString(stringEntry, syntaxConf).Equals(syntaxConf.Tokens.KeyWords.Data, StringComparison.Ordinal);
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateLineAndCharacter(char currentCharacter, PxFileSyntaxConf syntaxConf, ref int characterIndex, ref List<int> linebreakIndexes, ref bool isProcessingString)
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

        private static ValidationStructuredEntryKey ParseStructuredValidationEntryKey(string input, PxFileSyntaxConf syntaxConf)
        {
            ExtractSectionResult languageResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(
                input, 
                syntaxConf.Symbols.Key.LangParamStart, 
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.Key.LangParamEnd);
            string? language = languageResult.Sections.Length > 0 ? 
                SyntaxValidationUtilityMethods.CleanString(languageResult.Sections[0], syntaxConf).Trim(syntaxConf.Symbols.Key.StringDelimeter) :
                null;
            ExtractSectionResult specifierResult = SyntaxValidationUtilityMethods.ExtractSectionFromString(languageResult.Remainder,
                syntaxConf.Symbols.Key.SpecifierParamStart, 
                syntaxConf.Symbols.Key.StringDelimeter, 
                syntaxConf.Symbols.Key.SpecifierParamEnd);
            string[] specifiers = specifierResult.Sections.Length > 0
                ? SyntaxValidationUtilityMethods.ExtractSectionFromString(
                    specifierResult.Sections[0], 
                    syntaxConf.Symbols.Key.StringDelimeter, 
                    syntaxConf.Symbols.Key.StringDelimeter)
                .Sections 
                : [];
            string? firstSpecifier = specifiers.Length > 0 ?
                SyntaxValidationUtilityMethods.CleanString(specifiers[0], syntaxConf).Trim(syntaxConf.Symbols.Key.StringDelimeter) : 
                null;
            string? secondSpecifier = specifiers.Length > 1 ?
                SyntaxValidationUtilityMethods.CleanString(specifiers[1], syntaxConf).Trim(syntaxConf.Symbols.Key.StringDelimeter) :
                null;
            string keyword = SyntaxValidationUtilityMethods.CleanString(specifierResult.Remainder, syntaxConf);

            return new(keyword, language, firstSpecifier, secondSpecifier);
        }
    }
}
