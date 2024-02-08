using PxUtils.PxFile.MetadataUtility;
using System.Runtime.CompilerServices;
using System.Text;

namespace PxUtils.PxFile.Meta
{
    /// <summary>
    /// Provides methods to read metadata from a file. The metadata is returned as an IEnumerable or a dictionary of key-value pairs.
    /// This class uses a PxFileStreamFactory to open the file for reading when needed.
    /// </summary>
    public class PxFileMetadataReader(PxFileStreamFactory pxFileStreamFactory, int readBufferSize = 2048) : IPxFileMetadataReader
    {
        private Encoding? encodingCache;

        /// <summary>
        /// Reads the metadata from the file and returns it as an IEnumerable of key-value pairs.
        /// </summary>
        /// <returns>An IEnumerable of key-value pairs representing the metadata entries in the file.</returns>
        public IEnumerable<KeyValuePair<string, string>> ReadMetadata()
        {
            using Stream stream = pxFileStreamFactory.OpenStream();

            if (encodingCache is null)
            {
                encodingCache = MetadataParsing.GetEncoding(stream, readBufferSize);
                stream.Position = 0;
            }

            using StreamReader reader = new(stream, encodingCache);
            return MetadataParsing.GetMetadataEntries(reader, PxFileSymbolsConf.Default, readBufferSize);
        }

        /// <summary>
        /// Asynchronously reads the metadata from the file and returns it as an IEnumerable of key-value pairs.
        /// </summary>
        /// <returns>A Task yielding an IEnumerable of key-value pairs representing the metadata entries in the file.</returns>
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

        /// <summary>
        /// Reads the metadata from the file and returns it as a dictionary of key-value pairs.
        /// </summary>
        /// <returns>A dictionary containing the metadata entries in the file.</returns>
        public IReadOnlyDictionary<string, string> ReadMetadataToDictionary()
        {
            using Stream stream = pxFileStreamFactory.OpenStream();

            if (encodingCache is null)
            {
                encodingCache = MetadataParsing.GetEncoding(stream, readBufferSize);
                stream.Position = 0;
            }

            using StreamReader reader = new(stream, encodingCache);

            Dictionary<string, string> metaDict = [];

            IEnumerable<KeyValuePair<string, string>> metaEnumerable = MetadataParsing.GetMetadataEntries(reader, PxFileSymbolsConf.Default, readBufferSize);

            foreach (KeyValuePair<string, string> kvp in metaEnumerable) metaDict.Add(kvp.Key, kvp.Value);

            return metaDict;
        }

        /// <summary>
        /// Asynchronously reads the metadata from the file and returns it as a dictionary of key-value pairs.
        /// </summary>
        /// <returns>A Task yielding a dictionary containing the metadata entries in the file.</returns>
        public async Task<IReadOnlyDictionary<string, string>> ReadMetadataToDictionaryAsync(CancellationToken cancellationToken = default)
        {
            using Stream stream = pxFileStreamFactory.OpenStream();

            if (encodingCache is null)
            {
                encodingCache = await MetadataParsing.GetEncodingAsync(stream, readBufferSize);
                stream.Position = 0;
            }

            using StreamReader reader = new(stream, encodingCache);

            Dictionary<string, string> metaDict = [];

            ConfiguredCancelableAsyncEnumerable<KeyValuePair<string, string>> metaEnumerable = MetadataParsing.GetMetadataEntriesAsync(reader, PxFileSymbolsConf.Default, readBufferSize)
                .WithCancellation(cancellationToken);

            await foreach (KeyValuePair<string, string> kvp in metaEnumerable) metaDict.Add(kvp.Key, kvp.Value);

            return metaDict;
        }
    }
}
