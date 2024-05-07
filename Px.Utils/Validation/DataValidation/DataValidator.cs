using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using PxUtils.PxFile;

namespace PxUtils.Validation.DataValidation
{
    /// <summary>
    /// The DataValidator class is used to validate the data section of a PX file.
    /// </summary>
    public class DataValidator
    {
        private const int _streamBufferSize = 4096;

        private readonly List<IDataValidator> _commonValidators = [];
        private readonly List<IDataValidator> _dataNumValidators = [];
        private readonly List<IDataValidator> _dataStringValidators = [];
        private readonly List<IDataValidator> _dataSeparatorValidators = [];

        private int _expectedRows;
        private int _expectedRowLength;
        private int _startRow;
        private Encoding _streamEncoding = Encoding.Default;
        private PxFileSyntaxConf _conf = PxFileSyntaxConf.Default;

        private EntryType _currentEntryType = EntryType.Unknown;
        private byte _stringDelimeter;
        private List<byte> _currentEntry = [];
        private int _lineNumber = 1;
        private int _charPosition;
        private EntryType _currentCharacterType;
        private long _currentRowLength;

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
        public IEnumerable<ValidationFeedback> Validate(Stream stream, int rowLen, int numOfRows,
            int startRow, Encoding? streamEncoding, PxFileSyntaxConf? conf = null)
        {
            SetValidationParameters(streamEncoding, conf, numOfRows, rowLen, startRow);

            IEnumerable<ValidationFeedback> validationFeedbacks = ValidateDataStream(stream);

            ResetValidator();

            return validationFeedbacks;
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
        public async Task<IEnumerable<ValidationFeedback>> ValidateAsync(Stream stream, int rowLen,
            int numOfRows,
            int startRow, Encoding? streamEncoding, PxFileSyntaxConf? conf = null,
            CancellationToken cancellationToken = default)
        {
            SetValidationParameters(streamEncoding, conf, numOfRows, rowLen, startRow);

            IEnumerable<ValidationFeedback> validationFeedbacks =  await Task.Factory.StartNew(() => 
                ValidateDataStream(stream), cancellationToken);

            ResetValidator();

            return validationFeedbacks;
        }

        private void SetValidationParameters(Encoding? _streamEncoding, PxFileSyntaxConf? _conf, int _numOfRows, int _rowLen, int _startRow)
        {
            this._streamEncoding = _streamEncoding ?? Encoding.Default;
            this._conf  = _conf ?? PxFileSyntaxConf.Default;
            _expectedRows = _numOfRows;
            _expectedRowLength = _rowLen;
            this._startRow = _startRow;

            _commonValidators.Add(new DataStructureValidator());
            _dataNumValidators.AddRange(_commonValidators);
            _dataNumValidators.Add(new DataNumberValidator());
            _dataStringValidators.AddRange(_commonValidators);
            _dataStringValidators.Add(new DataStringValidator());
            _dataSeparatorValidators.AddRange(_commonValidators);
            _dataSeparatorValidators.Add(new DataSeparatorValidator());
        }

        private List<ValidationFeedback> ValidateDataStream(Stream stream)
        {
            List<ValidationFeedback> validationFeedbacks = [];
            byte endOfData = (byte)_conf.Symbols.EntrySeparator;
            _stringDelimeter = (byte)_conf.Symbols.Value.StringDelimeter;
            _currentEntry = new(_streamBufferSize);
            byte[] buffer = new byte[_streamBufferSize];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
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

            if (_expectedRows != _lineNumber - 1)
            {
                validationFeedbacks.Add(new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidRowCount, _lineNumber + _startRow, _charPosition));
            }

            return validationFeedbacks;
        }

        private void HandleEntryTypeChange(ref List<ValidationFeedback> validationFeedbacks)
        {
            if (_currentEntryType == EntryType.Unknown && (_lineNumber > 1 || _charPosition > 0))
            {
                validationFeedbacks.Add(new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidChar, _lineNumber + _startRow, _charPosition));
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
                    validator.Validate(_currentEntry, _currentEntryType, _streamEncoding, _lineNumber + _startRow, _charPosition, ref validationFeedbacks);
                }
            }
        }

        private void HandleNonSeparatorType(ref List<ValidationFeedback> validationFeedbacks)
        {
            if (_currentCharacterType == EntryType.DataItem)
            {
                _currentRowLength++;
            }
            else if (_currentCharacterType == EntryType.LineSeparator)
            {
                if (_currentRowLength != _expectedRowLength)
                {
                    validationFeedbacks.Add(new ValidationFeedback(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidRowLength, _lineNumber + _startRow, _charPosition));
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