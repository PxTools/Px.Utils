using PxUtils.Language;
using PxUtils.Models.Metadata.Enums;

namespace PxUtils.Models.Metadata.Dimensions
{
    public class ContentDimension(
        string code,
        MultilanguageString name,
        List<Property> additionalProperties,
        List<ContentDimensionValue> values
        ) : IReadOnlyDimension
    {
        public string Code { get; } = code;

        public DimensionType Type => DimensionType.Content;

        public MultilanguageString Name { get; } = name;

        public List<Property> AdditionalProperties { get; } = additionalProperties;

        public List<ContentDimensionValue> Values { get; } = values;

        #region Interface implementations

        IReadOnlyMultilanguageString IReadOnlyDimension.Name => Name;

        IReadOnlyList<IReadOnlyDimensionValue> IReadOnlyDimension.Values => Values;

        IReadOnlyList<Property> IReadOnlyDimension.AdditionalProperties => AdditionalProperties;

        #endregion
    }
}
