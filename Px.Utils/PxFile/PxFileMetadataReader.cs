
namespace PxUtils.PxFile
{
    public class PxFileMetadataReader : IPxFileMetadataReader
    {
        private readonly IPxFileStream _pxFileStream;

        public PxFileMetadataReader(IPxFileStream pxFileStream)
        {
            _pxFileStream = pxFileStream;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadMetadata()
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<KeyValuePair<string, string>> ReadMetadataAsync()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<string, string> ReadMetadataToDictionary()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<string, string>> ReadMetadataToDictionaryAsync()
        {
            throw new NotImplementedException();
        }
    }
}
