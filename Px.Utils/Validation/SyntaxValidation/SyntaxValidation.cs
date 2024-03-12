using Px.Utils.Validation.SyntaxValidation;
using PxUtils.PxFile;
using PxUtils.PxFile.Meta;
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
        public static ValidationReport ValidatePxFileSyntax(Stream stream, string filename, PxFileSyntaxConf? symbolsConf = null)
        {
            IEnumerable<IValidationFunction> streamValidationFunctions = [
                    new MultipleEntriesOnLineValidationFunction()
                ];

            symbolsConf ??= PxFileSyntaxConf.Default;
            ValidationReport report = new ValidationReport();
            report.FeedbackItems = [];
            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);
            // TODO: Add feedback if encoding is not supported

            List<StringValidationEntry> entries = ValidateStreamAndBuildEntries(stream, encoding, symbolsConf, streamValidationFunctions, filename, report);
            
            // report = ValidateStringEntries

            return report;
        }

        private static List<StringValidationEntry> ValidateStreamAndBuildEntries(Stream stream, Encoding encoding, PxFileSyntaxConf symbolsConf, IEnumerable<IValidationFunction> streamValidationFunctions, string filename, ValidationReport report)
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
                char nextCharacter = reader.EndOfStream ? symbolsConf.Symbols.EndOfStream : (char)reader.Peek();
                // Iterate through the stream level syntax validation functions
                StreamValidationEntry entry = new (line, character, filename, currentCharacter, nextCharacter, symbolsConf);
                foreach (IValidationFunction function in streamValidationFunctions
                    .Where(f => f.IsRelevant(entry)))
                {
                    ValidationFeedbackItem feedback = function.Validate(entry);
                    if (feedback != null)
                    {
                        report.FeedbackItems.Add(feedback);
                    }
                }
                if (currentCharacter == symbolsConf.Symbols.KeywordSeparator)
                {
                    string stringEntry = entryBuilder.ToString();
                    if (CleanString(stringEntry).Equals(symbolsConf.Tokens.KeyWords.Data))
                    {
                        break;
                    }
                }
                if (currentCharacter == symbolsConf.Symbols.LineSeparator)
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
                    string stringEntry = entryBuilder.ToString();
                    stringEntries.Add(new StringValidationEntry(line, character, filename, stringEntry));
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
    }
}
