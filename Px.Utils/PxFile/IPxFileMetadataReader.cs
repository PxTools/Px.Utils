namespace PxUtils.PxFile
{
    internal interface IPxFileMetadataReader
    {
        public IEnumerable<KeyValuePair<string, string>> ReadMetadata();
        public IAsyncEnumerable<KeyValuePair<string, string>> ReadMetadataAsync();
        public IReadOnlyDictionary<string, string> ReadMetadataToDictionary();
        public Task<IReadOnlyDictionary<string, string>> ReadMetadataToDictionaryAsync();
    }
}
