using PxUtils.Models.Data.DataValue;

namespace PxUtils.PxFile.Data
{
    public sealed class PxFileStreamDataReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly PxFileSyntaxConf _conf;
        private long _dataStart;

        private const int INTERNAL_BUFFER_SIZE = 4096;
        private const char VALUE_SEPARATOR = ' ';

        public PxFileStreamDataReader(Stream stream, PxFileSyntaxConf? conf = null)
        {
            _stream = stream;
            _conf = conf ?? PxFileSyntaxConf.Default;
        }

        public PxFileStreamDataReader(Stream stream, long dataStart, PxFileSyntaxConf? conf = null)
        {
            _stream = stream;
            _dataStart = dataStart;
            _conf = conf ?? PxFileSyntaxConf.Default;
        }

        public void ReadUnsafeDoubles(double[] buffer, int offset, int[] rows, int[] cols, double[] missingValueEncodings)
        {
            SetDataStartIfZero();
            ReadItemsFromStream(buffer, offset, rows, cols, Parser);
            
            double Parser(char[] parseBuffer, int len)
            {
                return DataValueParsers.FastParseUnsafeDoubleDangerous(parseBuffer, len, missingValueEncodings);
            }
        }

        public void ReadDoubleDataValues(DoubleDataValue[] buffer, int offset, int[] rows, int[] cols)
        {
            SetDataStartIfZero();
            ReadItemsFromStream(buffer, offset, rows, cols, DataValueParsers.ParseDoubleDataValue);
        }
        
        public void ReadDecimalDataValues(DecimalDataValue[] buffer, int offset, int[] rows, int[] cols)
        {
            SetDataStartIfZero();
            ReadItemsFromStream(buffer, offset, rows, cols, DataValueParsers.ParseDecimalDataValue);
        }

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

        public async Task ReadDoubleDataValuesAsync(DoubleDataValue[] buffer, int offset, int[] rows, int[] cols)
        {
            await SetReaderPositionIfZeroAsync();
            await Task.Factory.StartNew(() =>
                ReadItemsFromStream(buffer, offset, rows, cols, DataValueParsers.ParseDoubleDataValue));
        }

        public async Task ReadDecimalDataValuesAsync(DecimalDataValue[] buffer, int offset, int[] rows, int[] cols)
        {
            await SetReaderPositionIfZeroAsync();
            await Task.Factory.StartNew(() =>
                ReadItemsFromStream(buffer, offset, rows, cols, DataValueParsers.ParseDecimalDataValue));
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

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

        private async Task SetReaderPositionIfZeroAsync()
        {
            if (_dataStart != 0) return;
            string dataKeyword = _conf.Tokens.KeyWords.Data;
            long start = await StreamUtilities.FindKeywordPositionAsync(_stream, dataKeyword, _conf);
            if (start == -1)
            {
                throw new ArgumentException($"Could not find data keyword '{dataKeyword}'");
            }
            _dataStart = start + dataKeyword.Length + 1; // +1 to skip the '='
        }

        private void ReadItemsFromStream<T>(T[] buffer, int offset, int[] rows, int[] cols, Func<char[], int, T> readItem)
        {
            _stream.Position = _dataStart;

            int colsLength = cols.Length;
            int rowsLength = rows.Length;

            byte[] internalBuffer = new byte[INTERNAL_BUFFER_SIZE];
            char[] valueBuffer = new char[30]; // separate buffer is needed because a value can be split between two reads

            int fileRow = 0;
            int fileCol = 0;

            int targetRowIndex = 0;
            int targetColIndex = 0;

            long read;
            int valueIndex = 0;

            do
            {
                read = _stream.Read(internalBuffer, 0, INTERNAL_BUFFER_SIZE);
                for(int i = 0; i < read; i++)
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
            if(valueIndex > 0)
            {
                buffer[offset] = readItem(valueBuffer, valueIndex - 1);
            }
        }
    }
}
