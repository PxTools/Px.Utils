using Px.Utils.PxFile.Data;
using Px.Utils.Models.Data.DataValue;

namespace Px.Utils.PxFile.Data
{
    /// <summary>
    /// Used to read the data section of a PX file from a stream.
    /// Stores the reader position to allow reading the data section in parts.
    /// </summary>
    public sealed class PxFileStreamDataReader : IPxFileStreamDataReader, IDisposable
    {
        private readonly Stream _stream;
        private readonly PxFileSyntaxConf _conf;
        private long readIndex;
        private readonly int _readBufferSize;
        private readonly char _valueSeparator = ' ';
        private const int validCharactersMinIndex = 0x21;

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">Px file stream</param>
        /// <param name="conf">Px file syntax configuration</param>
        public PxFileStreamDataReader(Stream stream, PxFileSyntaxConf? conf = null, int readBufferSize = 4096)
        {
            _stream = stream;
            _readBufferSize = readBufferSize;
            _conf = conf ?? PxFileSyntaxConf.Default;
        }

        /// <summary>
        /// Constructor that allows specifying the position of the data section in the file.
        /// </summary>
        /// <param name="stream">Px file stream</param>
        /// <param name="dataStart">Position of the first data point in the file</param>
        /// <param name="conf">Px file syntax configuration</param>
        public PxFileStreamDataReader(Stream stream, long dataStart, PxFileSyntaxConf? conf = null, int readBufferSize = 4096)
        {
            _stream = stream;
            _stream.Position = dataStart;
            _readBufferSize = readBufferSize;
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
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
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
        public void ReadUnsafeDoubles(double[] buffer, int offset, DataIndexer indexer, double[] missingValueEncodings)
        {
            SetReaderPositionIfZero();
            ReadItemsFromStreamByCoordinate(buffer, offset, indexer, Parser);
            
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
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
        public void ReadDoubleDataValues(DoubleDataValue[] buffer, int offset, DataIndexer indexer)
        {
            SetReaderPositionIfZero();
            ReadItemsFromStreamByCoordinate(buffer, offset, indexer, DataValueParsers.FastParseDoubleDataValueDangerous);
        }

        /// <summary>
        /// Reads a specified set of data values from the data section of a PX file stream into a buffer.
        /// The method uses a fast, but potentially unsafe, parser to convert the data from the stream into <see cref="DecimalDataValue"/> instances.
        /// It is important that the file has been validated before using this method.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
        public void ReadDecimalDataValues(DecimalDataValue[] buffer, int offset, DataIndexer indexer)
        {
            SetReaderPositionIfZero();
            ReadItemsFromStreamByCoordinate(buffer, offset, indexer, DataValueParsers.FastParseDecimalDataValueDangerous);
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
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
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
        public async Task ReadUnsafeDoublesAsync(double[] buffer, int offset, DataIndexer indexer, double[] missingValueEncodings)
        {
            await SetReaderPositionIfZeroAsync();
            await Task.Factory.StartNew(() =>
                ReadItemsFromStreamByCoordinate(buffer, offset, indexer, Parser));

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
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
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
        public async Task ReadUnsafeDoublesAsync(double[] buffer, int offset, DataIndexer indexer, double[] missingValueEncodings, CancellationToken cancellationToken)
        {
            await SetReaderPositionIfZeroAsync(cancellationToken);
            await Task.Factory.StartNew(() =>
                ReadItemsFromStreamByCoordinate(buffer, offset, indexer, Parser, cancellationToken), cancellationToken);

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
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
        public async Task ReadDoubleDataValuesAsync(DoubleDataValue[] buffer, int offset, DataIndexer indexer)
        {
            await SetReaderPositionIfZeroAsync();
            await Task.Factory.StartNew(() =>
                ReadItemsFromStreamByCoordinate(buffer, offset, indexer, DataValueParsers.FastParseDoubleDataValueDangerous));
        }

        /// <summary>
        /// Asynchronously reads a specified set of double data values from the data section of a PX file stream into a buffer. 
        /// The method uses a parser to convert the data from the stream into <see cref="DoubleDataValue"/> instances.
        /// It is important that the file has been validated before using this method.
        /// This method supports cancellation through a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        public async Task ReadDoubleDataValuesAsync(DoubleDataValue[] buffer, int offset, DataIndexer indexer, CancellationToken cancellationToken)
        {
            await SetReaderPositionIfZeroAsync(cancellationToken);
            await Task.Factory.StartNew(() =>
                ReadItemsFromStreamByCoordinate(buffer, offset, indexer, DataValueParsers.FastParseDoubleDataValueDangerous, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads a specified set of decimal data values from the data section of a PX file stream into a buffer. 
        /// The method uses a parser to convert the data from the stream into <see cref="DecimalDataValue"/> instances.
        /// It is important that the file has been validated before using this method.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
        public async Task ReadDecimalDataValuesAsync(DecimalDataValue[] buffer, int offset, DataIndexer indexer)
        {
            await SetReaderPositionIfZeroAsync();
            await Task.Factory.StartNew(() =>
                ReadItemsFromStreamByCoordinate(buffer, offset, indexer, DataValueParsers.FastParseDecimalDataValueDangerous));
        }

        /// <summary>
        /// Asynchronously reads a specified set of decimal data values from the data section of a PX file stream into a buffer. 
        /// The method uses a parser to convert the data from the stream into <see cref="DecimalDataValue"/> instances.
        /// It is important that the file has been validated before using this method.
        /// This method supports cancellation through a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        public async Task ReadDecimalDataValuesAsync(DecimalDataValue[] buffer, int offset, DataIndexer indexer, CancellationToken cancellationToken)
        {
            await SetReaderPositionIfZeroAsync(cancellationToken);
            await Task.Factory.StartNew(() =>
                ReadItemsFromStreamByCoordinate(buffer, offset, indexer, DataValueParsers.FastParseDecimalDataValueDangerous, cancellationToken), cancellationToken);
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

        private void SetReaderPositionIfZero()
        {
            if (_stream.Position != 0) return;
            string dataKeyword = _conf.Tokens.KeyWords.Data;
            long start = StreamUtilities.FindKeywordPosition(_stream, dataKeyword, _conf);
            if (start == -1)
            {
                throw new ArgumentException($"Could not find data keyword '{dataKeyword}'");
            }
            _stream.Position = start + dataKeyword.Length + 1; // +1 to skip the '='
        }

        private async Task SetReaderPositionIfZeroAsync(CancellationToken? cancellationToken = null)
        {
            if (_stream.Position != 0) return;
            string dataKeyword = _conf.Tokens.KeyWords.Data;
            long start = await StreamUtilities.FindKeywordPositionAsync(_stream, dataKeyword, _conf, cancellationToken);
            if (start == -1)
            {
                throw new ArgumentException($"Could not find data keyword '{dataKeyword}'");
            }
            _stream.Position = start + dataKeyword.Length + 1; // +1 to skip the '='
        }

        private void ReadItemsFromStreamByCoordinate<T>(T[] buffer, int offset, DataIndexer indexer, Func<char[], int, T> readItem, CancellationToken? token = null)
        {
            int startingIndex = offset;

            byte[] internalBuffer = new byte[_readBufferSize];
            char[] valueBuffer = new char[30]; // separate buffer is needed because a value can be split between two reads
            int valueBufferIndex = 0;

            long read;

            do
            {
                token?.ThrowIfCancellationRequested();
                read = _stream.Read(internalBuffer, 0, _readBufferSize);
                for (int i = 0; i < read; i++) // Hot loop, mind the performance!
                {
                    char c = (char)internalBuffer[i];
                    if (c > validCharactersMinIndex)
                    {
                        valueBuffer[valueBufferIndex++] = c;
                    }
                    else if (c == _valueSeparator)
                    {
                        if(readIndex++ == indexer.CurrentIndex)
                        {
                            if(!indexer.Next())
                            {
                                _stream.Position = _stream.Position - read + i + 1;
                                read = 0; // kills the do-while loop
                                break;
                            }
                            buffer[offset++] = readItem(valueBuffer, valueBufferIndex);
                        }
                        valueBufferIndex = 0;
                    }
                }
            }
            while (read > 0);

            // Read the last value that may contain a section separator
            if (valueBuffer[valueBufferIndex - 1] == _conf.Symbols.EntrySeparator) valueBufferIndex--; // Remove the section separator
            buffer[offset++] = readItem(valueBuffer, valueBufferIndex);

            if (offset < startingIndex + indexer.DataLength - 1)
            {
                throw new ArgumentException("Some of the requested values were outside the data section.");
            }
        }

        #endregion
    }
}
