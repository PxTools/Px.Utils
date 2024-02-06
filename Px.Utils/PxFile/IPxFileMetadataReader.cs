
namespace PxUtils.PxFile.Meta
{
    internal interface IPxFileMetadataReader
    {
        public IEnumerable<KeyValuePair<string, string>> ReadMetadata();
        public IAsyncEnumerable<KeyValuePair<string, string>> ReadMetadataAsync(CancellationToken cancellationToken = default);
        public IReadOnlyDictionary<string, string> ReadMetadataToDictionary();
        public Task<IReadOnlyDictionary<string, string>> ReadMetadataToDictionaryAsync();
    }
}
