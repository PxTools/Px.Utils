
using PxUtils.PxFile.MetadataUtility;
using System.Runtime.CompilerServices;
using System.Text;

namespace PxUtils.PxFile.Meta
{
    public class PxFileMetadataReader(PxFileStreamFactory pxFileStreamFactory, int readBufferSize = 4096) : IPxFileMetadataReader
    {
        private Encoding? encodingCache;

        public IEnumerable<KeyValuePair<string, string>> ReadMetadata()
        {
            throw new NotImplementedException();
        }

        public async IAsyncEnumerable<KeyValuePair<string, string>> ReadMetadataAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using Stream stream = pxFileStreamFactory.OpenStream();

            if(encodingCache is null)
            {
                encodingCache = await MetadataParsing.GetEncodingAsync(stream, readBufferSize);
                stream.Position = 0;
            }

            using StreamReader reader = new(stream, encodingCache);

            ConfiguredCancelableAsyncEnumerable<KeyValuePair<string, string>> metaEnumerable = MetadataParsing.GetMetadataEntriesAsync(reader, PxFileSymbolsConf.Default, readBufferSize)
                .WithCancellation(cancellationToken);
            await foreach (KeyValuePair<string, string> kvp in metaEnumerable) yield return kvp;
        }

        public IReadOnlyDictionary<string, string> ReadMetadataToDictionary()
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyDictionary<string, string>> ReadMetadataToDictionaryAsync()
        {
            throw new NotImplementedException();
        }
    }
}
