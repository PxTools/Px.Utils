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

        private readonly static List<IDataValidator> commonValidators = [];
        private readonly static List<IDataValidator> dataNumValidators = [];
        private readonly static List<IDataValidator> dataStringValidators = [];
        private readonly static List<IDataValidator> dataSeparatorValidators = [];

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
            if (streamEncoding is null)
            {
                return [new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.NoEncoding, 0, 0) ];
            }

            conf ??= PxFileSyntaxConf.Default;
            commonValidators.Add(new DataStructureValidator());
            dataNumValidators.AddRange(commonValidators);
            dataNumValidators.Add(new DataNumberValidator());
            dataStringValidators.AddRange(commonValidators);
            dataStringValidators.Add(new DataStringValidator());
            dataSeparatorValidators.AddRange(commonValidators);
            dataSeparatorValidators.Add(new DataSeparatorValidator());

            // TODO: Remove console logs
            Console.WriteLine($"Starting with parameters: {rowLen} {numOfRows} {startRow} {streamEncoding} {conf}");
            List<ValidationFeedback> feedbacks = ValidateDataStream(stream, conf, streamEncoding, numOfRows, rowLen);

            // TODO: Remove console logs
            Console.WriteLine($"Feedbacks: {feedbacks.Count}");
            foreach (ValidationFeedback feedback in feedbacks)
            {
                Console.WriteLine($"{feedback.Rule} - {feedback.AdditionalInfo} - {feedback.Line}/{feedback.Character}");
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
        public static async Task TokenizeAsync(Stream stream, PxFileSyntaxConf conf,
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
            do
            {
                charsRead = await reader.ReadAsync(buffer.AsMemory(readStartIndex), cancellationToken);
                int currentBufferSize = charsRead + readStartIndex;

                for (int currentPos = 0; currentPos < currentBufferSize - 1; )
                {
                    /*token = GetToken(ref buffer, ref currentPos, ref tokenStartIndex, 
                        currentBufferSize -1 , ref lineNumber, ref charPosition,
                        out leftOver, conf);
                    if (token != null)
                    {
                        yield return (Token)token;
                    }*/
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

        }


        /// <summary>
        /// Tokenizes the data in the stream and returns a collection of tokens.
        /// Assumes that the stream is at the start of the data section (after 'DATA='-keyword) at the first data item.
        /// </summary>
        /// <param name="stream">The stream containing the data to be tokenized.</param>
        /// <param name="conf">The syntax configuration for the PX file.</param>
        /// <param name="streamEncoding">The encoding of the stream. Set to null if the default encoding should be used.</param>
        /// <returns>A collection of Token objects representing the tokens in the data stream.</returns>
        public static List<ValidationFeedback> ValidateDataStream(Stream stream, PxFileSyntaxConf conf, Encoding streamEncoding, int expectedRows, int expectedRowLength)
        {
            EntryType currentEntryType = EntryType.Unknown;
            List<byte> currentEntry = new(StreamBufferSize);
            List<ValidationFeedback> feedbacks = new(StreamBufferSize);
            int charPosition = 0;
            int lineNumber = 1;
            byte[] buffer = new byte[StreamBufferSize];
            int bytesRead;
            long currentRowLength = 0;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    byte currentByte = buffer[i];
                    EntryType currentType = currentByte switch
                    {
                        >= 0x22 => EntryType.DataItem,
                        0x20 or 0x09 => EntryType.DataItemSeparator,
                        0x0A or 0x0D => EntryType.LineSeparator,
                        _ => EntryType.Unknown
                    };
                    if (currentType != currentEntryType)
                    {
                        if (currentEntryType == EntryType.Unknown && 
                            (lineNumber > 1 || charPosition > 0))
                        {
                            feedbacks.Add(
                                new(ValidationFeedbackLevel.Error,
                                ValidationFeedbackRule.DataValidationFeedbackInvalidChar,
                                lineNumber,
                                charPosition));
                        }
                        else
                        {
                            List<IDataValidator> validators = currentEntryType switch
                            {
                                EntryType.DataItemSeparator => dataSeparatorValidators,
                                EntryType.DataItem => currentEntry[0] is 0x22 ? dataStringValidators : dataNumValidators,
                                _ => commonValidators
                            };

                            foreach (IDataValidator validator in validators)
                            {
                                feedbacks.AddRange(validator.Validate(currentEntry, currentEntryType, streamEncoding, lineNumber, charPosition));
                            }

                            if (currentType != EntryType.DataItemSeparator)
                            {
                                if (currentType == EntryType.DataItem)
                                {
                                    currentRowLength++;
                                }
                                else if (currentType == EntryType.LineSeparator)
                                {
                                    if (currentRowLength != expectedRowLength)
                                    {
                                        feedbacks.Add(new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidRowLength, lineNumber, charPosition));
                                    }
                                    lineNumber++;
                                    currentRowLength = 0;
                                    charPosition = 0;
                                }
                            }
                        }
                        
                        currentEntryType = currentType;
                        currentEntry.Clear();
                    }

                    currentEntry.Add(currentByte);
                    charPosition++;
                }
            }

            if (expectedRows != lineNumber - 1)
            {
                feedbacks.Add(new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidRowCount, lineNumber, charPosition));
            }

            return feedbacks;
        }
    }

    /// <summary>
    /// Represents the different types of tokens used in data validation.
    /// </summary>
    public enum EntryType
    {
        DataItem,
        DataItemSeparator,
        LineSeparator,
        Unknown
    }

    internal interface IDataValidator
    {
        internal IEnumerable<ValidationFeedback> Validate(List<byte> entry, EntryType entryType, Encoding encoding, int lineNumber, int charPos);
    }
}