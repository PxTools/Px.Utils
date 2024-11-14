using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Models.Metadata.MetaProperties;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata
{
    /// <summary>
    /// Metadata for a matrix object in a structured format.
    /// </summary>
    /// <param name="defaultLanguage">The default language of the matrix</param>
    /// <param name="availableLanguages">Every available language for the matrix</param>
    /// <param name="dimensions">Ordered list of dimension objects that define the structure of the matrix</param>
    /// <param name="additionalProperties">Properties of the matrix object, excluding dimension metadata or the languages</param>
    public class MatrixMetadata(
        string defaultLanguage,
        IReadOnlyList<string> availableLanguages,
        List<Dimension> dimensions,
        Dictionary<string, MetaProperty> additionalProperties)
        : IReadOnlyMatrixMetadata
    {
        /// <summary>
        /// The default language of the matrix
        /// </summary>
        public string DefaultLanguage { get; } = defaultLanguage;

        /// <summary>
        /// The available languages of the matrix, including the default language
        /// </summary>
        public IReadOnlyList<string> AvailableLanguages { get; } = availableLanguages;

        /// <summary>
        /// Ordered list of dimension objects that define the structure of the matrix
        /// </summary>
        public List<Dimension> Dimensions { get; } = dimensions;

        /// <summary>
        /// Additional properties of the matrix object, does not include properties of the dimensions or their values.
        /// </summary>
        public Dictionary<string, MetaProperty> AdditionalProperties { get; } = additionalProperties;

        #region Interface implementation

        IReadOnlyList<IReadOnlyDimension> IReadOnlyMatrixMetadata.Dimensions => Dimensions;

        IReadOnlyDictionary<string, MetaProperty> IReadOnlyMatrixMetadata.AdditionalProperties => AdditionalProperties;

        [JsonIgnore]
        IReadOnlyList<IDimensionMap> IMatrixMap.DimensionMaps => Dimensions;

        public MatrixMetadata GetTransform(IMatrixMap map)
        {
            List<Dimension> newDimensions = map.DimensionMaps.Select(map =>
            {
                if(Dimensions.Find(dimension => dimension.Code == map.Code) is Dimension dimension) return dimension.GetTransform(map);
                else throw new ArgumentException($"Dimension with code {map.Code} not found in metadata");

            }).ToList();
            return new MatrixMetadata(DefaultLanguage, AvailableLanguages, newDimensions, AdditionalProperties);
        }

        #endregion
    }
}
