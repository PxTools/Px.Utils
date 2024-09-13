using System.Text;
using Px.Utils.PxFile;
using Px.Utils.PxFile.Metadata;

namespace Px.Utils.Validation.DataValidation
{
    /// <summary>
    /// The DataValidator class is used to validate the data section of a Px file.
    /// <param name="rowLen">Length of one row of Px file data</param>
    /// <param name="numOfRows">Amount of rows of Px file data</param>
    /// <param name="startRow">The row number where the data section starts</param>
    /// <param name="conf">Syntax configuration for the Px file</param>
    /// </summary>
    public class DataValidator(int rowLen, int numOfRows, int startRow, PxFileSyntaxConf? conf = null) : IPxFileStreamValidator, IPxFileStreamValidatorAsync
    {
        private const int _streamBufferSize = 4096;

        private readonly PxFileSyntaxConf _conf = conf ?? PxFileSyntaxConf.Default;

        private readonly List<IDataValidator> _commonValidators = [];
        private readonly List<IDataValidator> _dataNumValidators = [];
        private readonly List<IDataValidator> _dataStringValidators = [];
        private readonly List<IDataValidator> _dataSeparatorValidators = [];

        private EntryType _currentEntryType = EntryType.Unknown;
        private byte _stringDelimeter;
        private List<byte> _currentEntry = [];
        private int _lineNumber = 1;
        private int _charPosition;
        private EntryType _currentCharacterType;
        private long _currentRowLength;
        private Encoding _encoding = Encoding.Default;
        private string _filename = string.Empty;

        /// <summary>
        /// Validates the data in the stream according to the specified parameters and returns a collection of validation feedback items.
        /// Assumes that the stream is at the start of the data section (after 'DATA='-keyword) at the first data item.
        /// </summary>
        /// <param name="stream">Px file stream to be validated</param>
        /// <param name="filename">Name of the file being validated</param>
        /// <param name="encoding">Encoding of the stream. If not provided, validator tries to find encoding.</param>
        /// <param name="leaveStreamOpen">Boolean value that determines whether the stream should be left open after validation.
        /// <returns>
        /// <see cref="ValidationResult"/> object that contains a collection of 
        /// validation feedback key value pairs representing the feedback for the data validation.
        /// </returns>
        public ValidationResult Validate(
            Stream stream,
            string filename,
            Encoding? encoding = null,
            bool leaveStreamOpen = false)
        {
            SetValidationParameters(encoding, filename, stream);

            ValidationFeedback validationFeedbacks = [];
            int dataStartIndex = GetStreamIndexOfFirstDataValue(stream, ref validationFeedbacks);
            if (dataStartIndex == -1)
            {
                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback =
                    new(new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.StartOfDataSectionNotFound),
                    new(filename, 0, 0));
                validationFeedbacks.Add(feedback);

                return new (validationFeedbacks);
            }
            stream.Position = dataStartIndex;
            ValidationFeedback dataStreamFeedbacks = ValidateDataStream(stream);
            validationFeedbacks.AddRange(dataStreamFeedbacks);

            ResetValidator();

            return new (validationFeedbacks);
        }

        /// <summary>
        /// Validates the data in the specified stream asynchronously.
        /// Assumes that the stream is at the start of the data section (after 'DATA='-keyword) at the first data item.
        /// <returns>
        /// <param name="stream">Px file stream to be validated</param>
        /// <param name="encoding">Encoding of the stream</param>
        /// <param name="filename">Name of the file being validated. If not provided, validator tries to find the encoding.</param>
        /// <param name="leaveStreamOpen">Boolean value that determines whether the stream should be left open after validation.
        /// <paramref name="cancellationToken"/>Cancellation token for cancelling the validation process</param>
        /// <see cref="ValidationResult"/> object that contains a collection of 
        /// validation feedback key value pairs representing the feedback for the data validation.
        /// </returns>
        public async Task<ValidationResult> ValidateAsync(
            Stream stream,
            string filename,
            Encoding? encoding = null,
            bool leaveStreamOpen = false,
            CancellationToken cancellationToken = default)
        {
            SetValidationParameters(encoding, filename, stream);

            ValidationFeedback validationFeedbacks = [];
            int dataStartIndex = GetStreamIndexOfFirstDataValue(stream, ref validationFeedbacks);
            if (dataStartIndex == -1)
            {
                KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback =
                    new(new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.StartOfDataSectionNotFound),
                    new(filename, 0, 0));
                validationFeedbacks.Add(feedback);

                return new (validationFeedbacks);
            }
            stream.Position = dataStartIndex;
            ValidationFeedback dataStreamFeedbacks =  await Task.Factory.StartNew(() => 
                ValidateDataStream(stream, cancellationToken), cancellationToken);
            validationFeedbacks.AddRange(dataStreamFeedbacks);

            ResetValidator();

            return new (validationFeedbacks);
        }

        private void SetValidationParameters(Encoding? encoding, string filename, Stream stream)
        {
            if (encoding is null)
            {
                PxFileMetadataReader reader = new();
                encoding = reader.GetEncoding(stream);
            }
            _commonValidators.Add(new DataStructureValidator());
            _dataNumValidators.AddRange(_commonValidators);
            _dataNumValidators.Add(new DataNumberValidator());
            _dataStringValidators.AddRange(_commonValidators);
            _dataStringValidators.Add(new DataStringValidator());
            _dataSeparatorValidators.AddRange(_commonValidators);
            _dataSeparatorValidators.Add(new DataSeparatorValidator());
            _encoding = encoding;
            _filename = filename;
        }

        private ValidationFeedback ValidateDataStream(Stream stream, CancellationToken? cancellationToken = null)
        {
            ValidationFeedback validationFeedbacks = [];
            byte endOfData = (byte)_conf.Symbols.EntrySeparator;
            _stringDelimeter = (byte)_conf.Symbols.Value.StringDelimeter;
            _currentEntry = new(_streamBufferSize);
            byte[] buffer = new byte[_streamBufferSize];
            int bytesRead = 0;

            do
            {
                cancellationToken?.ThrowIfCancellationRequested();
                for (int i = 0; i < bytesRead; i++)
                {
                    byte currentByte = buffer[i];
                    _currentCharacterType = currentByte switch
                    {
                        CharacterConstants.SPACE or CharacterConstants.HORIZONTALTAB => EntryType.DataItemSeparator,
                        CharacterConstants.LINEFEED or CharacterConstants.CARRIAGERETURN => EntryType.LineSeparator,
                        >= CharacterConstants.QUOTATIONMARK and not CharacterConstants.SEMICOLON => EntryType.DataItem,
                        _ when currentByte == endOfData => EntryType.EndOfData,
                        _ => EntryType.Unknown
                    };
                    if (_currentCharacterType != _currentEntryType)
                    {
                        HandleEntryTypeChange(ref validationFeedbacks);
                        if (_currentCharacterType != EntryType.DataItemSeparator)
                        {
                            HandleNonSeparatorType(ref validationFeedbacks);
                        }
                        _currentEntryType = _currentCharacterType;
                        _currentEntry.Clear();
                    }

                    _currentEntry.Add(currentByte);
                    _charPosition++;
                }
            }
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0);

            if (numOfRows != _lineNumber - 1)
            {
                validationFeedbacks.Add(new(
                    new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.DataValidationFeedbackInvalidRowCount),
                    new(_filename, _lineNumber + startRow, _charPosition, $" Expected {numOfRows} rows, got {_lineNumber - 1} rows."))
                );
            }

            return validationFeedbacks;
        }

        private void HandleEntryTypeChange(ref ValidationFeedback validationFeedbacks)
        {
            if (_currentEntryType == EntryType.Unknown && (_lineNumber > 1 || _charPosition > 0))
            {
                validationFeedbacks.Add(new(
                    new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidChar),
                    new(_filename, _lineNumber + startRow, _charPosition))
                );
            }
            else
            {
                List<IDataValidator> validators = _currentEntryType switch
                {
                    EntryType.DataItemSeparator => _dataSeparatorValidators,
                    EntryType.DataItem => _currentEntry[0] == _stringDelimeter ? _dataStringValidators : _dataNumValidators,
                    _ => _commonValidators
                };

                foreach (IDataValidator validator in validators)
                {
                    KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? feedback = validator.Validate(
                        _currentEntry,
                        _currentEntryType,
                        _encoding, 
                        _lineNumber + startRow,
                        _charPosition,
                        _filename);
                    if (feedback is not null)
                    {
                        validationFeedbacks.Add((KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>)feedback);
                    }
                }
            }
        }

        private void HandleNonSeparatorType(ref ValidationFeedback validationFeedbacks)
        {
            if (_currentCharacterType == EntryType.DataItem)
            {
                _currentRowLength++;
            }
            else if (_currentCharacterType == EntryType.LineSeparator)
            {
                if (_currentRowLength != rowLen)
                {
                    validationFeedbacks.Add(new(
                        new (ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.DataValidationFeedbackInvalidRowLength),
                        new(_filename, _lineNumber + startRow, _charPosition,
                        $"Expected {rowLen}, got row length of {_currentRowLength}."))
                    );
                }
                _lineNumber++;
                _currentRowLength = 0;
                _charPosition = 0;
            }
        }

        private void ResetValidator()
        {
            _commonValidators.Clear();
            _dataNumValidators.Clear();
            _dataStringValidators.Clear();
            _dataSeparatorValidators.Clear();
            _currentEntryType = EntryType.Unknown;
            _currentEntry.Clear();
            _lineNumber = 1;
            _charPosition = 0;
            _currentRowLength = 0;
        }

        private int GetStreamIndexOfFirstDataValue(Stream stream, ref ValidationFeedback feedbacks)
        {
            byte[] buffer = new byte[_streamBufferSize];
            int bytesRead;
            do
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                for (int i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] >= CharacterConstants.Zero && buffer[i] <= CharacterConstants.Nine)
                    {
                        return (int)stream.Position - bytesRead + i;
                    }
                    else if (!CharacterConstants.WhitespaceCharacters.Contains((char)buffer[i]))
                    {
                        feedbacks.Add(new(
                            new(ValidationFeedbackLevel.Error,
                            ValidationFeedbackRule.DataValidationFeedbackInvalidChar),
                            new(_filename, _lineNumber + startRow, _charPosition))
                        );
                    }
                }
            } while (bytesRead > 0);

            return -1;
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
        internal KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? Validate(List<byte> entry, EntryType entryType, Encoding encoding, int lineNumber, int charPos, string filename);
    }
}