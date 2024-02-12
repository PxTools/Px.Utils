
namespace PxUtils.Models.Metadata
{
    public class MatrixMetadata(string defaultLanguage, List<string> availableLanguages, List<Dimension> dimensions, List<Property> additionalProperties) : IReadOnlyMatrixMetadata
    {
        public string DefaultLanguage { get; } = defaultLanguage;

        public List<string> AvailableLanguages { get; } = availableLanguages;

        public List<Dimension> Dimensions { get; } = dimensions;

        public List<Property> AdditionalProperties { get; } = additionalProperties;

        IReadOnlyList<IReadOnlyDimension> IReadOnlyMatrixMetadata.Dimensions => Dimensions;
    }
}
