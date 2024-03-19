using PxUtils.PxFile;
using PxUtils.PxFile.Meta;
using System.Text;

namespace PxUtils.Validation.SyntaxValidation
{
    public static class SyntaxValidationAsync
    {
        private const int DEFAULT_BUFFER_SIZE = 4096;

        /// <summary>
        /// Asynchronously validates the syntax of a PX file's metadata.
        /// </summary>
        /// <param name="stream">The stream of the PX file to be validated.</param>
        /// <param name="filename">The name of the file to be validated.</param>
        /// <param name="syntaxConf">An optional <see cref="PxFileSyntaxConf"/> parameter that specifies the syntax configuration for the PX file. If not provided, the default syntax configuration is used.</param>
        /// <param name="bufferSize">An optional parameter that specifies the buffer size for reading the file. If not provided, a default buffer size of 4096 is used.</param>
        /// <param name="customStringValidationFunctions">An optional parameter that specifies custom <see cref="IValidationFunction"/> validation functions for string level entries. If not provided, only default validation functions are used.</param>
        /// <param name="customKeyValueValidationFunctions">An optional parameter that specifies custom key-value pair level <see cref="IValidationFunction"/>  validation functions. If not provided, only default key-value pair validation functions are used.</param>
        /// <param name="customStructuredValidationFunctions">An optional parameter that specifies custom <see cref="IValidationFunction"/> validation functions for structured key-value pairs. If not provided, only default structured validation functions are used.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> parameter that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task result contains a <see cref="SyntaxValidationResult"/> object which includes a <see cref="ValidationReport"/> and a list of <see cref="StructuredValidationEntry"/> objects. The ValidationReport contains feedback items that provide information about any syntax errors or warnings found during validation. The list of StructuredValidationEntry objects represents the structured entries in the PX file that were validated.</returns>
        public static async Task<SyntaxValidationResult> ValidatePxFileMetadataSyntaxAsync(
            Stream stream,
            string filename,
            PxFileSyntaxConf? syntaxConf = null,
            int bufferSize = DEFAULT_BUFFER_SIZE,
            IEnumerable<IValidationFunction>? customStringValidationFunctions = null,
            IEnumerable<IValidationFunction>? customKeyValueValidationFunctions = null,
            IEnumerable<IValidationFunction>? customStructuredValidationFunctions = null,
            CancellationToken cancellationToken = default)
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

            (bool encodingFound, Encoding? encoding) = await PxFileMetadataReader.TryGetEncodingAsync(stream, syntaxConf, cancellationToken);
            if (!encodingFound || encoding is null)
            {
                report.FeedbackItems.Add(new ValidationFeedbackItem(new StringValidationEntry(0, 0, filename, string.Empty, syntaxConf, 0), new SyntaxValidationFeedbackNoEncoding()));
                return new(report, []);
            }

            List<StringValidationEntry> stringEntries = await BuildStringEntriesAsync(stream, encoding, syntaxConf, filename, bufferSize);
            SyntaxValidation.ValidateEntries(stringEntries, stringValidationFunctions, report);
            List<KeyValuePairValidationEntry> keyValuePairs = SyntaxValidation.BuildKeyValuePairs(stringEntries, syntaxConf);
            SyntaxValidation.ValidateEntries(keyValuePairs, keyValueValidationFunctions, report);
            List<StructuredValidationEntry> structuredEntries = SyntaxValidation.BuildStructuredEntries(keyValuePairs);
            SyntaxValidation.ValidateEntries(structuredEntries, structuredValidationFunctions, report);

            return new(report, structuredEntries);
        }

        private static async Task<List<StringValidationEntry>> BuildStringEntriesAsync(Stream stream, Encoding encoding, PxFileSyntaxConf syntaxConf, string filename, int bufferSize)
        {
            int line = 0;
            int character = 0;
            stream.Seek(0, SeekOrigin.Begin);
            using StreamReader reader = new(stream, encoding);
            List<StringValidationEntry> stringEntries = [];
            StringBuilder entryBuilder = new();
            char[] buffer = new char[bufferSize];

            while (await reader.ReadAsync(buffer, 0, bufferSize) > 0)
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
                Array.Clear(buffer, 0, buffer.Length);
            }
            return stringEntries;
        }
    }

}
    