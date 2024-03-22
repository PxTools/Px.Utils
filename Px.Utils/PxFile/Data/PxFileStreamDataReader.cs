using PxUtils.Models.Data.DataValue;

namespace PxUtils.PxFile.Data
{
    public class PxFileStreamDataReader : IDisposable
    {
        private Stream _stream;
        private long _readerPosition;
        PxFileSyntaxConf _conf;

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
            _readerPosition = dataStart;
            _conf = conf ?? PxFileSyntaxConf.Default;
        }

        public long ReadUnsafeDoubles(double[] buffer, int offset, int count, IReadOnlyDictionary<string, double> missingDataEncodings)
        {
            SetReaderPositionIfZero();
            throw new NotImplementedException();
        }

        public long ReadDoubleDataValues(DoubleDataValue[] buffer, int offset, int count)
        {
            SetReaderPositionIfZero();
            throw new NotImplementedException();
        }
        
        public long ReadDecimalDataValues(DecimalDataValue[] buffer, int offset, int count)
        {
            SetReaderPositionIfZero();
            throw new NotImplementedException();
        }

        public async Task<long> ReadUnsafeDoublesAsync(double[] buffer, int offset, int count, IReadOnlyDictionary<string, double> missingDataEncodings)
        {
            SetReaderPositionIfZeroAsync();
            throw new NotImplementedException();
        }

        public async Task<long> ReadDoubleDataValuesAsync(DoubleDataValue[] buffer, int offset, int count)
        {
            SetReaderPositionIfZeroAsync();
            throw new NotImplementedException();
        }

        public async Task<long> ReadDecimalDataValuesAsync(DecimalDataValue[] buffer, int offset, int count)
        {
            SetReaderPositionIfZeroAsync();
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _stream.Dispose();
        }

        private void SetReaderPositionIfZero()
        {
            if (_readerPosition != 0) return;
            string dataKeyword = _conf.Tokens.KeyWords.Data;
            long start = StreamUtilities.FindKeywordPosition(_stream, dataKeyword, _conf);
            if (start == -1)
            {
                throw new ArgumentException($"Could not find data keyword '{dataKeyword}'");
            }
            _readerPosition = start + dataKeyword.Length + 1; // +1 to skip the '='
        }

        private async Task SetReaderPositionIfZeroAsync()
        {
            if (_readerPosition != 0) return;
            string dataKeyword = _conf.Tokens.KeyWords.Data;
            long start = await StreamUtilities.FindKeywordPositionAsync(_stream, dataKeyword, _conf);
            if (start == -1)
            {
                throw new ArgumentException($"Could not find data keyword '{dataKeyword}'");
            }
            _readerPosition = start + dataKeyword.Length + 1; // +1 to skip the '='
        }

        private void ReadItemsFromStream<T>(T[] buffer, int offset, int[] rows, int[] cols, Func<byte[], int, T> readItem)
        {
            _stream.Position = _readerPosition;

            int colsLength = cols.Length;
            int rowsLength = rows.Length;

            byte[] internalBuffer = new byte[INTERNAL_BUFFER_SIZE];
            byte[] valueBuffer = new byte[30]; // separate buffer is needed because a value can be split between two reads

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
                    byte c = internalBuffer[i];
                    if (c > 0x21) // Every allowed char > 0x21 belongs to a value
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
                                    return;
                                }
                            }
                        }
                        fileCol++;
                        valueIndex = 0;
                    }
                    else if(c == '\n')
                    {
                        fileRow++;
                        fileCol = 0;
                    }
                }
            }
            while (read > 0);
        }
    }
}
