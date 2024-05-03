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

        private static int expectedRows;
        private static int expectedRowLength;
        private static int startRow;
        private static Encoding streamEncoding = Encoding.Default;
        private static PxFileSyntaxConf conf = PxFileSyntaxConf.Default;

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
            SetValidationParameters(streamEncoding, conf, numOfRows, rowLen, startRow);

            List<ValidationFeedback> feedbacks = ValidateDataStream(stream);

            ResetValidators();

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
            SetValidationParameters(streamEncoding, conf, numOfRows, rowLen, startRow);

            List<ValidationFeedback> feedbacks = await Task.Factory.StartNew(() => 
                ValidateDataStream(stream));

            ResetValidators();

            return feedbacks;
        }

        private static void SetValidationParameters(Encoding? _streamEncoding, PxFileSyntaxConf? _conf, int _numOfRows, int _rowLen, int _startRow)
        {
            streamEncoding = _streamEncoding ?? Encoding.Default;
            conf  = _conf ?? PxFileSyntaxConf.Default;
            expectedRows = _numOfRows;
            expectedRowLength = _rowLen;
            startRow = _startRow;

            commonValidators.Add(new DataStructureValidator());
            dataNumValidators.AddRange(commonValidators);
            dataNumValidators.Add(new DataNumberValidator());
            dataStringValidators.AddRange(commonValidators);
            dataStringValidators.Add(new DataStringValidator());
            dataSeparatorValidators.AddRange(commonValidators);
            dataSeparatorValidators.Add(new DataSeparatorValidator());
        }

        private static List<ValidationFeedback> ValidateDataStream(Stream stream)
        {
            byte endOfData = (byte)conf.Symbols.EntrySeparator;
            byte stringDelimeter = (byte)conf.Symbols.Value.StringDelimeter;
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
                        0x20 or 0x09 => EntryType.DataItemSeparator,
                        0x0A or 0x0D => EntryType.LineSeparator,
                        >= 0x22 and not 0x3B => EntryType.DataItem,
                        _ when currentByte == endOfData => EntryType.EndOfData,
                        _ => EntryType.Unknown
                    };
                    if (currentType != currentEntryType)
                    {
                        HandleEntryTypeChange(currentEntryType, currentEntry, lineNumber, charPosition, stringDelimeter, ref feedbacks);
                        if (currentType != EntryType.DataItemSeparator)
                        {
                            HandleNonSeparatorType(currentType, ref currentRowLength, ref lineNumber, ref charPosition, ref feedbacks);
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
                feedbacks.Add(new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidRowCount, lineNumber + startRow, charPosition));
            }

            return feedbacks;
        }

        private static void HandleEntryTypeChange(
            EntryType currentEntryType,
            List<byte> currentEntry,
            int lineNumber,
            int charPosition,
            byte stringDelimeter,
            ref List<ValidationFeedback> feedbacks)
        {
            if (currentEntryType == EntryType.Unknown && (lineNumber > 1 || charPosition > 0))
            {
                feedbacks.Add(new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidChar, lineNumber + startRow, charPosition));
            }
            else
            {
                List<IDataValidator> validators = currentEntryType switch
                {
                    EntryType.DataItemSeparator => dataSeparatorValidators,
                    EntryType.DataItem => currentEntry[0] == stringDelimeter ? dataStringValidators : dataNumValidators,
                    _ => commonValidators
                };

                foreach (IDataValidator validator in validators)
                {
                    validator.Validate(currentEntry, currentEntryType, streamEncoding, lineNumber + startRow, charPosition, ref feedbacks);
                }
            }
        }

        private static void HandleNonSeparatorType(
            EntryType currentType,
            ref long currentRowLength, 
            ref int lineNumber,
            ref int charPosition,
            ref List<ValidationFeedback> feedbacks)
        {
            if (currentType == EntryType.DataItem)
            {
                currentRowLength++;
            }
            else if (currentType == EntryType.LineSeparator)
            {
                if (currentRowLength != expectedRowLength)
                {
                    feedbacks.Add(new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidRowLength, lineNumber + startRow, charPosition));
                }
                lineNumber++;
                currentRowLength = 0;
                charPosition = 0;
            }
        }

        private static void ResetValidators()
        {
            commonValidators.Clear();
            dataNumValidators.Clear();
            dataStringValidators.Clear();
            dataSeparatorValidators.Clear();
        }
    }

    /// <summary>
    /// Represents the different types of entries encountered during data validation.
    /// </summary>
    public enum EntryType
    {
        DataItem,
        DataItemSeparator,
        LineSeparator,
        EndOfData,
        Unknown
    }

    internal interface IDataValidator
    {
        internal void Validate(List<byte> entry, EntryType entryType, Encoding encoding, int lineNumber, int charPos, ref List<ValidationFeedback> feedbacks);
    }
}