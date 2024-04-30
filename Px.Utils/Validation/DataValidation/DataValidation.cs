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
        private const int StreamBufferSize = 4096;

        private static readonly string[] delimiters = [" ", "t"];
        /// <summary>
        /// Validates the data in the stream according to the specified parameters and returns a collection of validation feedback items.
        /// Assumes that the stream is at the start of the data section (after 'DATA='-keyword) at the first data item.
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
        /// Assumes that the stream is at the start of the data section (after 'DATA='-keyword) at the first data item.
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
        /// Asynchronously tokenizes the data in the stream according to the specified parameters and yields the tokens.
        /// Assumes that the stream is at the start of the data section (after 'DATA='-keyword) at the first data item.
        /// </summary>
        /// <param name="stream">The stream containing the data to be tokenized.</param>
        /// <param name="conf">The syntax configuration for the PX file. Set to null to use the default configuration.</param>
        /// <param name="streamEncoding">The encoding of the stream. Set to null if the default encoding should be used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel tokenization.</param>
        /// <returns>
        /// An <see cref="IAsyncEnumerable{T}"/> of <see cref="Token"/> representing the tokens in the data.
        /// </returns>
        public static async IAsyncEnumerable<Token> TokenizeAsync(Stream stream, PxFileSyntaxConf conf,
            Encoding? streamEncoding, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            int charPosition = 0;
            int lineNumber = 1;
            int tokenStartIndex = 0;
            using StreamReader reader = new(stream, streamEncoding, false, StreamBufferSize, true);
            char[] buffer = new char[StreamBufferSize];
            int readStartIndex = 0;
            int leftOver = 0;
            int charsRead = 0;
            Token? token;
            do
            {
                charsRead = await reader.ReadAsync(buffer.AsMemory(readStartIndex), cancellationToken);
                int currentBufferSize = charsRead + readStartIndex;

                for (int currentPos = 0; currentPos < currentBufferSize - 1; )
                {
                    token = GetToken(ref buffer, ref currentPos, ref tokenStartIndex, 
                        currentBufferSize -1 , ref lineNumber, ref charPosition,
                        out leftOver, conf);
                    if (token != null)
                    {
                        yield return (Token)token;
                    }
                }

                if (leftOver != 0)
                {
                    readStartIndex = 0;
                    for (int i = tokenStartIndex; i < currentBufferSize; i++)
                    {
                        buffer[i - tokenStartIndex] = buffer[i];
                        readStartIndex++;
                    }
                }
                else
                {
                    buffer[0] = buffer[currentBufferSize - 1];
                    readStartIndex = 1;
                }
            } while (charsRead > 0);

            yield return new Token(TokenType.EndOfStream, "", lineNumber, charPosition);
        }


        /// <summary>
        /// Tokenizes the data in the stream and returns a collection of tokens.
        /// Assumes that the stream is at the start of the data section (after 'DATA='-keyword) at the first data item.
        /// </summary>
        /// <param name="stream">The stream containing the data to be tokenized.</param>
        /// <param name="conf">The syntax configuration for the PX file.</param>
        /// <param name="streamEncoding">The encoding of the stream. Set to null if the default encoding should be used.</param>
        /// <returns>A collection of Token objects representing the tokens in the data stream.</returns>
        public static IEnumerable<Token> Tokenize(Stream stream, PxFileSyntaxConf conf, Encoding? streamEncoding)
        {
            int charPosition = 0;
            int lineNumber = 1;
            int tokenStartIndex = 0;
            using StreamReader reader = new(stream, streamEncoding, false, StreamBufferSize, true);
            char[] buffer = new char[StreamBufferSize];
            int readStartIndex = 0;
            int leftOver = 0;
            int charsRead = 0;
            Token? token;
            do
            {
                charsRead = reader.Read(buffer.AsSpan(readStartIndex));
                
                int currentBufferSize = charsRead + readStartIndex;

                for (int currentPos = 0; currentPos < currentBufferSize - 1; )
                {
                    token = GetToken(ref buffer, ref currentPos, ref tokenStartIndex, 
                        currentBufferSize -1 , ref lineNumber, ref charPosition,
                        out leftOver, conf);
                    if (token != null)
                    {
                        yield return (Token)token;
                    }
                }

                if (leftOver != 0)
                {
                    readStartIndex = 0;
                    for (int i = tokenStartIndex; i < currentBufferSize; i++)
                    {
                        buffer[i - tokenStartIndex] = buffer[i];
                        readStartIndex++;
                    }
                }
                else
                {
                    buffer[0] = buffer[currentBufferSize - 1];
                    readStartIndex = 1;
                }
            } while (charsRead > 0);

            yield return new Token(TokenType.EndOfStream, "", lineNumber, charPosition);
        }


        private static Token? GetToken(ref char[] buffer, ref int currentPos, ref int tokenStartIndex, int bufLen, 
            ref int lineNumber, ref int charPosition, out int leftOver, PxFileSyntaxConf conf)
        {
            leftOver = 0;
            charPosition++;
            tokenStartIndex = currentPos;
            char currentCharacter = buffer[currentPos];
            if (currentCharacter is ' ')
            {
                currentPos++;
                return new Token(TokenType.DataItemSeparator, delimiters[0], lineNumber,
                    charPosition);
            } 
            if (currentCharacter is '\t')
            {
                currentPos++;
                return new Token(TokenType.DataItemSeparator, delimiters[1], lineNumber,
                    charPosition);
            } 
            if (currentCharacter is '\n' or '\r')
            {
                int nextCharacter = buffer[currentPos + 1];

                string separator = currentCharacter.ToString();
                if (nextCharacter is '\n' or '\r')
                {
                    separator += (char)nextCharacter;
                    currentPos++;
                    leftOver = currentPos;
                    tokenStartIndex = currentPos +1 ;
                }

                currentPos++;
                Token token = new(TokenType.LineSeparator, separator, lineNumber++, charPosition);
                charPosition = 0;
                return token;
            }
            if (currentCharacter == conf.Symbols.EntrySeparator)
            {
                currentPos++;
                return new Token(TokenType.EndOfData, currentCharacter.ToString(), lineNumber, charPosition);
            }
            if (IsValidDataValueCharacter(currentCharacter))
            {
                tokenStartIndex = currentPos;

                while (currentPos < bufLen  && IsValidDataValueCharacter(buffer[currentPos + 1]))
                {
                    currentPos++; 
                }

                if (currentPos == bufLen  )
                {
                    leftOver = tokenStartIndex;
                    charPosition--;
                    return null;
                }
                char nextCharacter = buffer[currentPos + 1];               
                if (nextCharacter is ' ' or '\t' || nextCharacter == conf.Symbols.EntrySeparator)
                {
                    TokenType tokenType = TokenType.NumDataItem;
                    if (buffer[tokenStartIndex] == '\"')
                    {
                        tokenType = TokenType.StringDataItem;
                    }

                    Token token = new(tokenType, StringFrom(buffer, tokenStartIndex, currentPos), lineNumber,
                        charPosition);
                    charPosition += (currentPos - tokenStartIndex );
                    currentPos++;
                    tokenStartIndex = currentPos;
                    return token;
                }
                else
                {
                    Token token = new(TokenType.InvalidDataChar, currentCharacter.ToString(), lineNumber,
                        charPosition);
                    tokenStartIndex = currentPos;
                    currentPos++;
                    return token;
                    
                }
            }
            else
            {
                Token token = new(TokenType.InvalidDataChar, currentCharacter.ToString(), lineNumber,
                    charPosition);
                tokenStartIndex = currentPos;
                currentPos++;
                return token;
            }
            
        }

        private static string StringFrom(char[] buffer, int startIndex, int currentPosition)
        {
            int length = currentPosition - startIndex + 1;
            var context = new { Buf=buffer, Start = startIndex, Length=length};
            return string.Create(length, context, (chars, context) =>
            {
                Span<char> sp = context.Buf.AsSpan(context.Start, context.Length);
                sp.CopyTo(chars);
            });
        }

        private static bool IsValidDataValueCharacter(char currentCharacter)
        {
            //  allowed characters '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-', '.', '"'
            return currentCharacter is >= '0' and <= '9' or '-' or '.' or '"';
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