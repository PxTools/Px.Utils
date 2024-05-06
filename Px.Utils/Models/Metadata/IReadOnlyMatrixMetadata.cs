using Px.Utils.Models.Metadata;
using PxUtils.Models.Metadata.Dimensions;

namespace PxUtils.Models.Metadata
{
    /// <summary>
    /// Readonly metadata interface for a matrix object in a strucured format.
    /// </summary>
    public interface IReadOnlyMatrixMetadata : IMatrixMap
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
        public IReadOnlyDictionary<string, MetaProperty> AdditionalProperties { get; }

        /// <summary>
        /// Returns a new metadata object with the specified transform map applied.
        /// Map can be used to change the order of dimensions and their values or to filter out some values.
        /// </summary>
        /// <param name="map">The resulting new metadata object will this structure</param>
        /// <returns>New metadata object with the transform applied</returns>
        public MatrixMetadata GetTransform(IMatrixMap map);
    }
}
