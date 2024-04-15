using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using PxUtils.PxFile;

namespace PxUtils.Validation.DataValidation
{
    /// <summary>
    /// The DataValidation class is used to validate the data section of a PX file.
    /// </summary>
    public static class DataValidation
    {
        private static readonly char[] ValidDataCharacters =
            ['1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-', '.', '"'];

        /// <summary>
        /// Validates the data in the stream according to the specified parameters and returns a collection of validation feedback items.
        /// Assumes that the stream is at the start of the data section (after 'DATA='-keyword).
        /// </summary>
        /// <param name="stream">The stream containing the data to be validated.</param>
        /// <param name="rowLen">The expected length of each row in the data.</param>
        /// <param name="numOfRows">The expected number of rows in the data.</param>
        /// <param name="startRow">The starting row number.</param>
        /// <param name="streamEncoding">The encoding of the stream. Set to null if the default encoding should be used.</param>
        /// <param name="conf">The syntax configuration for the PX file. Set to null to use the default configuration.</param>
        /// <returns>
        /// A collection of ValidationFeedbackItem objects representing the feedback for the data validation.
        /// </returns>
        public static IEnumerable<ValidationFeedback> Validate(Stream stream, int rowLen, int numOfRows,
            int startRow, Encoding? streamEncoding, PxFileSyntaxConf? conf = null)
        {
            conf ??= PxFileSyntaxConf.Default;

            List<ValidationFeedback> feedbacks = [];
            IEnumerable<Token> tokens = Tokenize(stream, conf, streamEncoding);

            List<IDataValidator> validators =
            [
                new DataSeparatorValidator(),
                new DataNumberValidator(),
                new DataStringValidator(),
                new DataRowLengthValidator(rowLen),
                new DataRowCountValidator(numOfRows),
                new DataStructureValidator()
            ];

            foreach (Token token in tokens)
            {
                if (token.Type == TokenType.InvalidDataChar)
                {
                    feedbacks.Add(new ValidationFeedback(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.DataValidationFeedbackInvalidChar, token.LineNumber, token.CharPosition,
                        $"{token.Value}"));
                    continue;
                }

                foreach (IDataValidator validator in validators)
                {
                    feedbacks.AddRange(validator.Validate(token));
                }
            }

            return feedbacks;
        }

        /// <summary>
        /// Validates the data in the specified stream asynchronously.
        /// </summary>
        /// <param name="stream">The stream containing the data to validate.</param>
        /// <param name="rowLen">The expected length of each row in the data.</param>
        /// <param name="numOfRows">The expected number of rows in the data.</param>
        /// <param name="startRow">The starting row number.</param>
        /// <param name="streamEncoding">The encoding of the stream. Can be null, in which case the default encoding is used.</param>
        /// <param name="conf">The configuration for validating the data file syntax. Can be null, in which case the default configuration is used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A collection of validation feedback for the data.</returns>
        public static async Task<IEnumerable<ValidationFeedback>> ValidateAsync(Stream stream, int rowLen,
            int numOfRows,
            int startRow, Encoding? streamEncoding, PxFileSyntaxConf? conf = null,
            CancellationToken cancellationToken = default)
        {
            conf ??= PxFileSyntaxConf.Default;

            List<ValidationFeedback> feedbacks = [];
            IAsyncEnumerable<Token> tokens = TokenizeAsync(stream, conf, streamEncoding, cancellationToken);

            List<IDataValidator> validators =
            [
                new DataSeparatorValidator(),
                new DataNumberValidator(),
                new DataStringValidator(),
                new DataRowLengthValidator(rowLen),
                new DataRowCountValidator(numOfRows),
                new DataStructureValidator()
            ];

            await foreach (Token token in tokens)
            {
                if (token.Type == TokenType.InvalidDataChar)
                {
                    feedbacks.Add(new ValidationFeedback(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.DataValidationFeedbackInvalidChar, token.LineNumber, token.CharPosition,
                        $"{token.Value}"));
                    continue;
                }

                foreach (IDataValidator validator in validators)
                {
                    feedbacks.AddRange(validator.Validate(token));
                }
            }

            return feedbacks;
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
            int charPosition = 0;
            int lineNumber = 1;
            using StreamReader reader = new(stream, streamEncoding, false, streamBufferSize, true);
            StringBuilder valueBuilder = new();

            while (!reader.EndOfStream)
            {
                char currentCharacter = (char)reader.Read();
                int nextCharacter = reader.EndOfStream ? conf.Symbols.EndOfStream : reader.Peek();

                Token? token = GetToken(currentCharacter, nextCharacter, ref lineNumber, ref charPosition, out bool skipNext,
                    conf, ref valueBuilder);
                if (skipNext)
                {
                    reader.Read();
                }

                if (token != null)
                {
                    yield return (Token)token;
                }
            }
            yield return new Token(TokenType.EndOfStream, valueBuilder.ToString(), lineNumber, charPosition);
        }


        /// <summary>
        /// Tokenizes a stream asynchronously.
        /// </summary>
        /// <param name="stream">The stream to tokenize.</param>
        /// <param name="conf">The configuration for tokenization.</param>
        /// <param name="streamEncoding">The encoding of the stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous enumerable of tokens.</returns>
        public static async IAsyncEnumerable<Token> TokenizeAsync(Stream stream, PxFileSyntaxConf conf,
            Encoding? streamEncoding, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            const int streamBufferSize = 1024;
            int charPosition = 0;
            int lineNumber = 1;
            using StreamReader reader = new(stream, streamEncoding, false, streamBufferSize, true);
            StringBuilder valueBuilder = new();
            char[] buffer = new char[streamBufferSize];
            int readStartIndex = 0;

            int charsRead = 0;
            bool ignoreNext = false;
            Token? token;
            do
            {
                charsRead = await reader.ReadAsync(buffer.AsMemory(readStartIndex), cancellationToken);

                for (int currentPos = 0; currentPos < charsRead - 1 + readStartIndex; currentPos++)
                {
                    if (ignoreNext)
                    {
                        ignoreNext = false;
                        continue;
                    }
                    char currentCharacter = buffer[currentPos];
                    char nextCharacter = buffer[currentPos + 1];

                    token = GetToken(currentCharacter, nextCharacter, ref lineNumber, ref charPosition,
                        out bool skipNext,
                        conf, ref valueBuilder);
                    ignoreNext = skipNext;
                    if (token != null)
                    {
                        yield return (Token)token;
                    }
                }
                buffer[0] = buffer[charsRead - 1 + readStartIndex];
                readStartIndex = 1;

            } while (charsRead > 0);

            if (!ignoreNext)
            {
                token = GetToken(buffer[0], 0, ref lineNumber, ref charPosition, out _, conf, ref valueBuilder);

                if (token != null)
                {
                    yield return (Token)token;
                }
            }

            yield return new Token(TokenType.EndOfStream, valueBuilder.ToString(), lineNumber, charPosition);
        }


        private static Token? GetToken(char currentCharacter, int nextCharacter, ref int lineNumber,
            ref int charPosition, out bool skipNext, PxFileSyntaxConf conf, ref StringBuilder valueBuilder)
        {
            skipNext = false;
            charPosition++;
            if (currentCharacter is ' ' or '\t')
            {
                return new Token(TokenType.DataItemSeparator, currentCharacter.ToString(), lineNumber,
                    charPosition);
            } 
            if (currentCharacter is '\n' or '\r')
            {
                string separator = currentCharacter.ToString();
                if (nextCharacter is '\n' or '\r')
                {
                    separator += (char)nextCharacter;
                    skipNext = true;
                }

                Token token = new(TokenType.LineSeparator, separator, lineNumber++, charPosition);
                charPosition = 0;
                return token;
            }
            if (currentCharacter == conf.Symbols.EntrySeparator)
            {
                return new Token(TokenType.EndOfData, currentCharacter.ToString(), lineNumber, charPosition);
            }
            if (ValidDataCharacters.Contains(currentCharacter))
            {
                valueBuilder.Append(currentCharacter);
                if (nextCharacter is ' ' or '\t' || nextCharacter == conf.Symbols.EntrySeparator)
                {
                    TokenType tokenType = TokenType.NumDataItem;
                    if (valueBuilder[0] == '\"')
                    {
                        tokenType = TokenType.StringDataItem;
                    }

                    Token token = new(tokenType, valueBuilder.ToString(), lineNumber,
                        charPosition - valueBuilder.Length + 1);
                    valueBuilder.Clear();
                    return token;
                }
            }
            else
            {
                Token token = new(TokenType.InvalidDataChar, currentCharacter.ToString(), lineNumber,
                    charPosition);
                valueBuilder.Clear();
                return token;
            }
            
            return null;
        }
    }

    /// <summary>
    /// Represents the different types of tokens used in data validation.
    /// </summary>
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

    /// <summary>
    /// Represents a token of a specific type in data validation.
    /// </summary>
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

}