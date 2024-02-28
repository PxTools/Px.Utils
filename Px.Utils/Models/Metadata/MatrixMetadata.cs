using PxUtils.Models.Metadata.Dimensions;

namespace PxUtils.Models.Metadata
{
    public class MatrixMetadata(string defaultLanguage, IReadOnlyList<string> availableLanguages, List<IDimension> dimensions, List<Property> additionalProperties) : IReadOnlyMatrixMetadata
    {
        public string DefaultLanguage { get; } = defaultLanguage;

        public IReadOnlyList<string> AvailableLanguages { get; } = availableLanguages;

        public List<IDimension> Dimensions { get; } = dimensions;

        public List<Property> AdditionalProperties { get; } = additionalProperties;

        IReadOnlyList<IReadOnlyDimension> IReadOnlyMatrixMetadata.Dimensions => Dimensions;
    }
}
