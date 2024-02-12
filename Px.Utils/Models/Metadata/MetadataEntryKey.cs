namespace PxUtils.Models.Metadata
{
    public readonly record struct MetadataEntryKey
    {
        public string KeyWord { get; }
        public string? Language { get; }
        public string? DimensionIdentifier { get; }
        public string? ValueIdentifier { get; }

        public MetadataEntryKey(string keyWord)
        {
            KeyWord = keyWord;
        }

        public MetadataEntryKey(string keyWord, string language, string? dimensionIdentifier = null, string? valueIdentifier = null)
        {
            KeyWord = keyWord;
            Language = language;
            DimensionIdentifier = dimensionIdentifier;
            ValueIdentifier = valueIdentifier;
        }
    }
}
