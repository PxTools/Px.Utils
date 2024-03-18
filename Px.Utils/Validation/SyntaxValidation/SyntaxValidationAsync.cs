using PxUtils.PxFile;
using PxUtils.PxFile.Meta;
using System.Text;

namespace PxUtils.Validation.SyntaxValidation
{
    public static class SyntaxValidationAsync
    {
        private const int DEFAULT_BUFFER_SIZE = 4096;

        public static async Task<SyntaxValidationResult> ValidatePxFileSyntaxAsync(
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
            StreamReader reader = new(stream, encoding);
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
            }
            return stringEntries;
        }
    }

}
    