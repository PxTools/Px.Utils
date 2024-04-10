using PxUtils.Models.Data.DataValue;

namespace PxUtils.PxFile.Data
{
    /// <summary>
    /// Used to read the data section of a PX file from a stream.
    /// Stores the reader position to allow reading the data section in parts.
    /// </summary>
    public sealed class PxFileStreamDataReader : IPxFileStreamDataReader, IDisposable
    {
        private readonly Stream _stream;
        private readonly PxFileSyntaxConf _conf;
        private long _dataStart;

        private const int INTERNAL_BUFFER_SIZE = 4096;
        private const char VALUE_SEPARATOR = ' ';

        private int fileRow = 0;
        private int fileCol = 0;

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">Px file stream</param>
        /// <param name="conf">Px file syntax configuration</param>
        public PxFileStreamDataReader(Stream stream, PxFileSyntaxConf? conf = null)
        {
            _stream = stream;
            _conf = conf ?? PxFileSyntaxConf.Default;
        }

        /// <summary>
        /// Constructor that allows specifying the position of the data section in the file.
        /// </summary>
        /// <param name="stream">Px file stream</param>
        /// <param name="dataStart">Position of the first data point in the file</param>
        /// <param name="conf">Px file syntax configuration</param>
        public PxFileStreamDataReader(Stream stream, long dataStart, PxFileSyntaxConf? conf = null)
        {
            _stream = stream;
            _dataStart = dataStart;
            _conf = conf ?? PxFileSyntaxConf.Default;
        }

        #endregion

        #region Sync methods

        /// <summary>
        /// Reads a specified set of data values from the data section of a PX file stream into a buffer. 
        /// The method uses a fast, but potentially unsafe, parser to convert the data from the stream into <see cref="double"/> values.
        /// It is important that the file has been validated before using this method.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read from the data section.</param>
        /// <param name="cols">An array of column indices specifying which columns to read from the data section.</param>
        /// <param name="missingValueEncodings">
        /// An array of <see cref="double"/> values that represent missing data in the PX file in the following order:
        /// [0] "-"
        /// [1] "."
        /// [2] ".."
        /// [3] "..."
        /// [4] "...."
        /// [5] "....."
        /// [6] "......"
        /// </param>
        public void ReadUnsafeDoubles(double[] buffer, int offset, int[] rows, int[] cols, double[] missingValueEncodings)
        {
            SetDataStartIfZero();
            ReadItemsFromStream(buffer, offset, rows, cols, Parser);
            
            double Parser(char[] parseBuffer, int len)
            {
                return DataValueParsers.FastParseUnsafeDoubleDangerous(parseBuffer, len, missingValueEncodings);
            }
        }

        /// <summary>
        /// Reads a specified set of data values from the data section of a PX file stream into a buffer. 
        /// The method uses a fast, but potentially unsafe, parser to convert the data from the stream into <see cref="DoubleDataValue"/> instances.
        /// It is important that the file has been validated before using this method.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read from the data section.</param>
        /// <param name="cols">An array of column indices specifying which columns to read from the data section.</param>
        public void ReadDoubleDataValues(DoubleDataValue[] buffer, int offset, int[] rows, int[] cols)
        {
            SetDataStartIfZero();
            ReadItemsFromStream(buffer, offset, rows, cols, DataValueParsers.ParseDoubleDataValue);
        }

        /// <summary>
        /// Reads a specified set of data values from the data section of a PX file stream into a buffer.
        /// The method uses a fast, but potentially unsafe, parser to convert the data from the stream into <see cref="DecimalDataValue"/> instances.
        /// It is important that the file has been validated before using this method.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read from the data section.</param>
        /// <param name="cols">An array of column indices specifying which columns to read from the data section.</param>
        public void ReadDecimalDataValues(DecimalDataValue[] buffer, int offset, int[] rows, int[] cols)
        {
            SetDataStartIfZero();
            ReadItemsFromStream(buffer, offset, rows, cols, DataValueParsers.ParseDecimalDataValue);
        }

        #endregion

        #region Async methods

        /// <summary>
        /// Asynchronously reads a specified set of data values from the data section of a PX file stream into a buffer. 
        /// The method uses a fast, but potentially unsafe, parser to convert the data from the stream into <see cref="double"/> values.
        /// It is important that the file has been validated before using this method.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read from the data section.</param>
        /// <param name="cols">An array of column indices specifying which columns to read from the data section.</param>
        /// <param name="missingValueEncodings">
        /// An array of <see cref="double"/> values that represent missing data in the PX file in the following order:
        /// [0] "-"
        /// [1] "."
        /// [2] ".."
        /// [3] "..."
        /// [4] "...."
        /// [5] "....."
        /// [6] "......"
        /// </param>
        public async Task ReadUnsafeDoublesAsync(double[] buffer, int offset, int[] rows, int[] cols, double[] missingValueEncodings)
        {
            await SetReaderPositionIfZeroAsync();
            await Task.Factory.StartNew(() =>
                ReadItemsFromStream(buffer, offset, rows, cols, Parser));

            double Parser(char[] parseBuffer, int len)
            {
                return DataValueParsers.FastParseUnsafeDoubleDangerous(parseBuffer, len, missingValueEncodings);
            }
        }

        /// <summary>
        /// Asynchronously reads a specified set of double values from the data section of a PX file stream into a buffer. 
        /// The method uses a fast, but potentially unsafe, parser to convert the data from the stream into <see cref="double"/> values.
        /// It is important that the file has been validated before using this method.
        /// This method supports cancellation through a <see cref="CancellationToken"/> .
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read from the data section.</param>
        /// <param name="cols">An array of column indices specifying which columns to read from the data section.</param>
        /// <param name="missingValueEncodings">
        /// An array of <see cref="double"/> values that represent missing data in the PX file in the following order:
        /// [0] "-"
        /// [1] "."
        /// [2] ".."
        /// [3] "..."
        /// [4] "...."
        /// [5] "....."
        /// [6] "......"
        /// </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        public async Task ReadUnsafeDoublesAsync(double[] buffer, int offset, int[] rows, int[] cols, double[] missingValueEncodings, CancellationToken cancellationToken)
        {
            await SetReaderPositionIfZeroAsync(cancellationToken);
            await Task.Factory.StartNew(() =>
                ReadItemsFromStream(buffer, offset, rows, cols, Parser, cancellationToken), cancellationToken);

            double Parser(char[] parseBuffer, int len)
            {
                return DataValueParsers.FastParseUnsafeDoubleDangerous(parseBuffer, len, missingValueEncodings);
            }
        }

        /// <summary>
        /// Asynchronously reads a specified set of double data values from the data section of a PX file stream into a buffer. 
        /// The method uses a parser to convert the data from the stream into <see cref="DoubleDataValue"/> instances.
        /// It is important that the file has been validated before using this method.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read from the data section.</param>
        /// <param name="cols">An array of column indices specifying which columns to read from the data section.</param>
        public async Task ReadDoubleDataValuesAsync(DoubleDataValue[] buffer, int offset, int[] rows, int[] cols)
        {
            await SetReaderPositionIfZeroAsync();
            await Task.Factory.StartNew(() =>
                ReadItemsFromStream(buffer, offset, rows, cols, DataValueParsers.ParseDoubleDataValue));
        }

        /// <summary>
        /// Asynchronously reads a specified set of double data values from the data section of a PX file stream into a buffer. 
        /// The method uses a parser to convert the data from the stream into <see cref="DoubleDataValue"/> instances.
        /// It is important that the file has been validated before using this method.
        /// This method supports cancellation through a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read from the data section.</param>
        /// <param name="cols">An array of column indices specifying which columns to read from the data section.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        public async Task ReadDoubleDataValuesAsync(DoubleDataValue[] buffer, int offset, int[] rows, int[] cols, CancellationToken cancellationToken)
        {
            await SetReaderPositionIfZeroAsync(cancellationToken);
            await Task.Factory.StartNew(() =>
                ReadItemsFromStream(buffer, offset, rows, cols, DataValueParsers.ParseDoubleDataValue, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads a specified set of decimal data values from the data section of a PX file stream into a buffer. 
        /// The method uses a parser to convert the data from the stream into <see cref="DecimalDataValue"/> instances.
        /// It is important that the file has been validated before using this method.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read from the data section.</param>
        /// <param name="cols">An array of column indices specifying which columns to read from the data section.</param>
        public async Task ReadDecimalDataValuesAsync(DecimalDataValue[] buffer, int offset, int[] rows, int[] cols)
        {
            await SetReaderPositionIfZeroAsync();
            await Task.Factory.StartNew(() =>
                ReadItemsFromStream(buffer, offset, rows, cols, DataValueParsers.ParseDecimalDataValue));
        }

        /// <summary>
        /// Asynchronously reads a specified set of decimal data values from the data section of a PX file stream into a buffer. 
        /// The method uses a parser to convert the data from the stream into <see cref="DecimalDataValue"/> instances.
        /// It is important that the file has been validated before using this method.
        /// This method supports cancellation through a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read from the data section.</param>
        /// <param name="cols">An array of column indices specifying which columns to read from the data section.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        public async Task ReadDecimalDataValuesAsync(DecimalDataValue[] buffer, int offset, int[] rows, int[] cols, CancellationToken cancellationToken)
        {
            await SetReaderPositionIfZeroAsync(cancellationToken);
            await Task.Factory.StartNew(() =>
                ReadItemsFromStream(buffer, offset, rows, cols, DataValueParsers.ParseDecimalDataValue, cancellationToken), cancellationToken);
        }

        #endregion

        /// <summary>
        /// Disposes the stream reader.
        /// </summary>
        public void Dispose()
        {
            _stream.Dispose();
        }

        #region Private methods

        private void SetDataStartIfZero()
        {
            if (_dataStart != 0) return;
            string dataKeyword = _conf.Tokens.KeyWords.Data;
            long start = StreamUtilities.FindKeywordPosition(_stream, dataKeyword, _conf);
            if (start == -1)
            {
                throw new ArgumentException($"Could not find data keyword '{dataKeyword}'");
            }
            _dataStart = start + dataKeyword.Length + 1; // +1 to skip the '='
        }

        private async Task SetReaderPositionIfZeroAsync(CancellationToken? cancellationToken = null)
        {
            if (_dataStart != 0) return;
            string dataKeyword = _conf.Tokens.KeyWords.Data;
            long start = await StreamUtilities.FindKeywordPositionAsync(_stream, dataKeyword, _conf, cancellationToken);
            if (start == -1)
            {
                throw new ArgumentException($"Could not find data keyword '{dataKeyword}'");
            }
            _dataStart = start + dataKeyword.Length + 1; // +1 to skip the '='
        }

        private void ReadItemsFromStream<T>(T[] buffer, int offset, int[] rows, int[] cols, Func<char[], int, T> readItem, CancellationToken? token = null)
        {
            _stream.Position = _dataStart;
            int startingIndex = offset;

            int colsLength = cols.Length;
            int rowsLength = rows.Length;

            byte[] internalBuffer = new byte[INTERNAL_BUFFER_SIZE];
            char[] valueBuffer = new char[30]; // separate buffer is needed because a value can be split between two reads

            int targetRowIndex = 0;
            int targetColIndex = 0;

            long read;
            int valueIndex = 0;

            do
            {
                token?.ThrowIfCancellationRequested();
                read = _stream.Read(internalBuffer, 0, INTERNAL_BUFFER_SIZE);
                for(int i = 0; i < read; i++) // Hot loop, mind the performance!
                {
                    char c = (char)internalBuffer[i];
                    if (c > 0x21)
                    {
                        valueBuffer[valueIndex] = c;
                        valueIndex++;
                    }
                    else if(c == VALUE_SEPARATOR)
                    {
                        if(fileCol == cols[targetColIndex] && fileRow == rows[targetRowIndex])
                        {
                            buffer[offset] = readItem(valueBuffer, valueIndex);
                            offset++;
                            targetColIndex++;
                            if(targetColIndex == colsLength)
                            {
                                targetColIndex = 0;
                                targetRowIndex++;
                                if(targetRowIndex == rowsLength)
                                {
                                    // All requested values have been read
                                    _dataStart = _stream.Position - read + i;
                                    return;
                                }
                            }
                        }
                        fileCol++;
                        valueIndex = 0;
                    }
                    else if(c == '\n' && fileCol > 0) // Ignore empty lines
                    {
                        fileRow++;
                        fileCol = 0;
                    }
                }
            }
            while (read > 0);

            // Read the last value that contains section separator
            if(valueIndex > 0) buffer[offset++] = readItem(valueBuffer, valueIndex - 1);

            if (offset - startingIndex < rowsLength * colsLength)
            {
                throw new ArgumentException("Some of the requested values were outside the data section.");
            }
        }

        #endregion
    }
}
