using PxUtils.PxFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation
{
    public static class SyntaxValidation
    {
        public static ValidationReport ValidatePxFileSyntax(Stream stream, string fileName, PxFileSyntaxConf? symbolsConf = null)
        {
            List<IStreamValidationFunction> StreamValidationFunctions = [
                    new MultipleEntriesOnLineValidationFunction()
                    // More tests...
                ];

            symbolsConf ??= PxFileSyntaxConf.Default;
            ValidationReport report = new ValidationReport();
            report.ReportType = ValidationTarget.Syntax;
            report.FeedbackItems = [];
            long line = 0;
            int character = 0;
            Encoding encoding = Meta.PxFileMetadataReader.GetEncoding(stream);
            // TODO: Add feedback if encoding is not supported
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new(stream, encoding);
            // Iterate each character of the reader
            List<TokenStream> entries = new();
            Stream currentEntry = new MemoryStream();
            while (!reader.EndOfStream)
            {
                char currentCharacter = (char)reader.Read();
                char nextCharacter = reader.EndOfStream ? '\0' : (char)reader.Peek();
                // Iterate through the stream level syntax validation functions
                foreach (IStreamValidationFunction function in StreamValidationFunctions
                    .Where(f => f.IsRelevant(currentCharacter, symbolsConf)))
                {
                    ValidationFeedbackItem feedback = function.Validate(nextCharacter, symbolsConf, line, character, fileName);
                    if (feedback != null)
                    {
                        report.FeedbackItems.Add(feedback);
                    }
                }
                if (currentCharacter == '\n')
                {
                    line++;
                    character = 0;
                }
                else
                {
                    character++;
                }
                if (currentCharacter == symbolsConf.Symbols.SectionSeparator)
                {
                    entries.Add(new TokenStream() { Line = line, Content = currentEntry });
                    currentEntry = new MemoryStream();
                }
                else
                {
                    currentEntry.WriteByte((byte)currentCharacter);
                }
            }
            // Process the entries, split them into key-value pairs, run entry level validation functions
            // Process the keys by running key level validation functions
            // Process the values by running value level validation functions
            // Return the validation report

            return report;
        }
    }
}
