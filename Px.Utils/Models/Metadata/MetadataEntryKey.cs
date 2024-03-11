namespace PxUtils.Models.Metadata
{
    /// <summary>
    /// The MetadataEntryKey struct represents a unique identifier for a metadata entry in a Px-file.
    /// This struct is used to efficiently look up metadata entries in a collection or database.
    /// It encapsulates the key properties that uniquely identify a metadata entry.
    /// </summary>
    public readonly record struct MetadataEntryKey
    {
        /// <summary>
        /// Gets the keyword of the metadata entry, which is the primary identifier.
        /// </summary>
        public string KeyWord { get; }

        /// <summary>
        /// Gets the language of the metadata entry. This property can be null.
        /// </summary>
        public string? Language { get; }

        /// <summary>
        /// Gets the first identifier of the metadata entry. Typically a dimension name. This property can be null.
        /// </summary>
        public string? FirstIdentifier { get; }

        /// <summary>
        /// Gets the second identifier of the metadata entry. Typically a name of a dimension value. This property can be null.
        /// </summary>
        public string? SecondIdentifier { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="keyWord">The keyword of the metadata entry.</param>
        /// <param name="language">The language of the metadata entry. This parameter can be null.</param>
        /// <param name="firstIdentifier">The first identifier of the metadata entry. This parameter can be null.</param>
        /// <param name="secondIdentifier">The second identifier of the metadata entry. This parameter can be null.</param>
        public MetadataEntryKey(string keyWord, string? language = null, string? firstIdentifier = null, string? secondIdentifier = null)
        {
            KeyWord = keyWord;
            Language = language;
            FirstIdentifier = firstIdentifier;
            SecondIdentifier = secondIdentifier;
        }
    }
}
