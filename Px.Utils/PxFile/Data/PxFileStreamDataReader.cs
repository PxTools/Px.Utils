using PxUtils.Models.Data.DataValue;

namespace PxUtils.PxFile.Data
{
    public class PxFileStreamDataReader : IDisposable
    {
        private Stream _stream;
        private long? _dataStart;
        PxFileSyntaxConf _conf;

        public PxFileStreamDataReader(Stream stream, PxFileSyntaxConf? conf = null)
        {
            _stream = stream;
            _conf = conf ?? PxFileSyntaxConf.Default;
        }

        public PxFileStreamDataReader(long dataStart, Stream stream, PxFileSyntaxConf? conf = null)
        {
            _stream = stream;
            _dataStart = dataStart;
            _conf = conf ?? PxFileSyntaxConf.Default;
        }

        public long ReadUnsafeDoubles(double[] buffer, int offset, int count, IReadOnlyDictionary<string, double> missingDataEncodings)
        {
            SetDataStartIfNotSet();
            throw new NotImplementedException();
        }

        public long ReadDoubleDataValues(DoubleDataValue[] buffer, int offset, int count)
        {
            SetDataStartIfNotSet();
            throw new NotImplementedException();
        }
        
        public long ReadDecimalDataValues(DecimalDataValue[] buffer, int offset, int count)
        {
            SetDataStartIfNotSet();
            throw new NotImplementedException();
        }

        public async Task<long> ReadUnsafeDoublesAsync(double[] buffer, int offset, int count, IReadOnlyDictionary<string, double> missingDataEncodings)
        {
            SetDataStartIfNotSet();
            throw new NotImplementedException();
        }

        public async Task<long> ReadDoubleDataValuesAsync(DoubleDataValue[] buffer, int offset, int count)
        {
            SetDataStartIfNotSet();
            throw new NotImplementedException();
        }

        public async Task<long> ReadDecimalDataValuesAsync(DecimalDataValue[] buffer, int offset, int count)
        {
            SetDataStartIfNotSet();
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _stream.Dispose();
        }

        private void SetDataStartIfNotSet()
        {
            if (_dataStart == null)
            {
                string dataKeyword = _conf.Tokens.KeyWords.Data;
                long dataKeyStart = StreamUtilities.FindKeywordPosition(_stream, dataKeyword, _conf);
                if (dataKeyStart == -1) throw new ArgumentException("Data keyword not found");

                _dataStart = dataKeyStart + dataKeyword.Length + 1; // +1 to skip the '='
            }
        }
    }
}
