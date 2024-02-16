namespace PxUtils.Models.Metadata
{
    public readonly record struct MetadataEntryKey
    {
        public string KeyWord { get; }
        public string? Language { get; }
        public string? FirstIdentifier { get; }
        public string? SecondIdentifier { get; }

        public MetadataEntryKey(string keyWord, string? language = null, string? firstIdentifier = null, string? secondIdentifier = null)
        {
            KeyWord = keyWord;
            Language = language;
            FirstIdentifier = firstIdentifier;
            SecondIdentifier = secondIdentifier;
        }
    }
}
