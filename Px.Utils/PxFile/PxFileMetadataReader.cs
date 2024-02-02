
using System.Text;

namespace PxUtils.PxFile.Meta
{
    public class PxFileMetadataReader : IPxFileMetadataReader
    {
        public PxFileMetadataReader(IPxFileStream pxFileStream, PxFileMetaReaderConfig config)
        {
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
