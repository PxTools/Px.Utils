
using System.Diagnostics.CodeAnalysis;
using System.Text;
using PxUtils.PxFile;
using PxUtils.Validation;

namespace PxUtils.Validation.DataValidation
{
    /// <summary>
    /// The DataValidation class is used to validate the data section of a PX file.
    /// </summary>
    public static class DataValidation
    {
        private static readonly char[] ValidDataCharacters = ['1', '2', '3', '4', '5','6','7','8','9','0','-','.','"'];

        /// <summary>
        /// Validates the data in the stream according to the specified parameters and returns a collection of validation feedback items.
        /// Assumes that the stream is at the start of the data section (after 'DATA='-keyword).
        /// </summary>
        /// <param name="stream">The stream containing the data to be validated.</param>
        /// <param name="fileName">Name of the file of the stream (only used for reporting)</param>
        /// <param name="rowLen">The expected length of each row in the data.</param>
        /// <param name="numOfRows">The expected number of rows in the data.</param>
        /// <param name="startRow">The starting row number.</param>
        /// <param name="streamEncoding">The encoding of the stream. Set to null if the default encoding should be used.</param>
        /// <param name="conf">The syntax configuration for the PX file. Set to null to use the default configuration.</param>
        /// <returns>
        /// A collection of ValidationFeedbackItem objects representing the feedback for the data validation.
        /// </returns>
        public static IEnumerable<ValidationFeedbackItem> Validate(Stream stream, string fileName, int rowLen, int numOfRows,
            int startRow, Encoding? streamEncoding, PxFileSyntaxConf? conf = null)
        {
            conf ??= PxFileSyntaxConf.Default;
            
            var feedbackItems = new List<ValidationFeedbackItem>();
            var tokens = Tokenize(stream, conf, streamEncoding);

            var validators = new List<IDataValidator>
            {
                new DataSeparatorValidator(),
                new DataNumberDataValidator(),
                new DataStringValidator(),
                new DataRowLengthValidator(rowLen),
                new DataRowCountValidator(numOfRows),
                new DataStructureValidator()
            };
            
            foreach (var token in tokens)
            {
                if (token.Type == TokenType.InvalidDataChar)
                {
                    var validationEntry = new DataValidationEntry(fileName, startRow + token.LineNumber, token.CharPosition);
                    feedbackItems.Add(new ValidationFeedbackItem(validationEntry, new ValidationFeedback(ValidationFeedbackLevel.Error, 
                        ValidationFeedbackRule.DataValidationFeedbackInvalidChar, $"{token.Value}")));
                    continue;
                }
                var feedbacks = new List<ValidationFeedback>();
                foreach (var validator in validators)
                {
                    feedbacks.AddRange(validator.Validate(token));                    
                }
                
                if (feedbacks.Count > 0)
                {
                    var validationEntry = new DataValidationEntry(fileName, startRow + token.LineNumber, token.CharPosition);
                    feedbackItems.AddRange(feedbacks.Select(feedback => new ValidationFeedbackItem(validationEntry, feedback)));
                }
            }

            return feedbackItems;
        }


        /// <summary>
        /// Tokenizes the given stream according to the specified configuration and encoding.
        /// </summary>
        /// <param name="stream">The stream to tokenize.</param>
        /// <param name="conf">The configuration for tokenizing the stream.</param>
        /// <param name="streamEncoding">The encoding of the stream. It can be null for the default encoding.</param>
        /// <returns>An enumerable collection of tokens.</returns>
        public static IEnumerable<Token> Tokenize(Stream stream, PxFileSyntaxConf conf, Encoding? streamEncoding)
        {
            const int streamBufferSize = 1024;
            var charPosition = 0;
            var lineNumber = 1;
            using StreamReader reader = new(stream, streamEncoding, false, streamBufferSize, true);
            var valueBuilder = new StringBuilder();

            while (!reader.EndOfStream)
            {
                var currentCharacter = (char)reader.Read();
                charPosition++;
                var nextCharacter = reader.EndOfStream ? conf.Symbols.EndOfStream : reader.Peek();

                if (currentCharacter is ' ' or '\t')
                {
                    yield return new Token(TokenType.DataItemSeparator, currentCharacter.ToString(), lineNumber, charPosition);
                }
                else if (currentCharacter is '\n' or '\r')
                {
                    var separator = currentCharacter.ToString();
                    if(nextCharacter is '\n' or '\r')
                    {
                        separator += (char)nextCharacter;
                        reader.Read();
                    }
                    yield return new Token(TokenType.LineSeparator, separator, lineNumber, charPosition);
                    lineNumber++;
                    charPosition = 0;
                }
                else if (currentCharacter == conf.Symbols.EntrySeparator)
                {
                    yield return new Token(TokenType.EndOfData, currentCharacter.ToString(), lineNumber, charPosition);
                }
                else if (ValidDataCharacters.Contains(currentCharacter))
                {
                    valueBuilder.Append(currentCharacter);                     
                    if (nextCharacter is ' ' or '\t' || nextCharacter == conf.Symbols.EntrySeparator)
                    {                    
                        var tokentype = TokenType.NumDataItem;
                        if (valueBuilder[0] == '\"')
                        {
                            tokentype = TokenType.StringDataItem;
                        }
                        yield return new Token(tokentype, valueBuilder.ToString(), lineNumber, charPosition - valueBuilder.Length + 1);
                        valueBuilder.Clear();
                    }
                }
                else
                {
                    yield return new Token(TokenType.InvalidDataChar, currentCharacter.ToString(), lineNumber, charPosition);
                    valueBuilder.Clear();
                }
            }
            yield return new Token(TokenType.EndOfStream, valueBuilder.ToString(), lineNumber, charPosition);
        }
    }

    public enum TokenType
    {
        EmptyToken,
        InvalidDataChar,
        NumDataItem,
        StringDataItem,
        DataItemSeparator,
        LineSeparator,
        EndOfData,
        EndOfStream
    }
    public readonly struct Token(TokenType type, string value, int lineNumber, int charPosition)
    {
        public TokenType Type { get; } = type;
        public string Value { get; } = value;
        public int LineNumber { get; } = lineNumber;
        public int CharPosition { get; } = charPosition;

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"token: {Type}, value: {Value}, line: {LineNumber}, pos: {CharPosition}";
        }
    }

    internal interface IDataValidator
    {
        internal IEnumerable<ValidationFeedback> Validate(Token token);
    }

    public class DataValidationEntry(string file, int line, int character) : ValidationObject(line, character, file);
}




