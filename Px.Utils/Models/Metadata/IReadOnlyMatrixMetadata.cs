using PxUtils.Models.Metadata.Dimensions;

namespace PxUtils.Models.Metadata
{
    /// <summary>
    /// Readonly metadata interface for a matrix object in a strucured format.
    /// </summary>
    public interface IReadOnlyMatrixMetadata
    {
        /// <summary>
        /// The default language of the matrix
        /// </summary>
        public string DefaultLanguage { get; }

        /// <summary>
        /// The available languages of the matrix, including the default language
        /// </summary>
        public IReadOnlyList<string> AvailableLanguages { get; }

        /// <summary>
        /// Ordered list of dimension objects that define the structure of the matrix
        /// </summary>
        IReadOnlyList<IReadOnlyDimension> Dimensions { get; }

        /// <summary>
        /// Additional properties of the matrix object, does not include properties of the dimensions or their values.
        /// </summary>
        public IReadOnlyDictionary<string, Property> AdditionalProperties { get; }
    }
}
