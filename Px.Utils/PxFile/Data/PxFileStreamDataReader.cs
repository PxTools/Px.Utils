namespace PxUtils.PxFile.Data
{
    public class PxFileStreamDataReader : IDisposable
    {
        private Stream _stream;
        private long _dataStart;

        public PxFileStreamDataReader(Stream stream)
        {
            _stream = stream;
        }

        public PxFileStreamDataReader(long dataStart, Stream stream)
        {
            _stream = stream;
            _dataStart = dataStart;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _stream.Dispose();
        }
    }
}
