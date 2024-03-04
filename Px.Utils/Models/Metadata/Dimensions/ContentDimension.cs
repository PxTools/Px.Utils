using PxUtils.Language;
using PxUtils.Models.Metadata.Enums;

namespace PxUtils.Models.Metadata.Dimensions
{
    public class ContentDimension(
        string code,
        MultilanguageString name,
        List<Property> additionalProperties,
        List<ContentDimensionValue> values,
        ContentDimensionValue? defaultValue = null
        ) : IDimension
    {
        public string Code { get; } = code;

        public DimensionType Type => DimensionType.Content;

        public MultilanguageString Name { get; } = name;

        public List<Property> AdditionalProperties { get; } = additionalProperties;

        public List<ContentDimensionValue> Values { get; } = values;

        public ContentDimensionValue? DefaultValue { get; } = defaultValue;

        #region Interface implementations

        MultilanguageString IReadOnlyDimension.Name => Name;

        IReadOnlyList<IReadOnlyDimensionValue> IReadOnlyDimension.Values => Values;

        IReadOnlyList<DimensionValue> IDimension.Values => Values;

        IReadOnlyList<Property> IReadOnlyDimension.AdditionalProperties => AdditionalProperties;

        IReadOnlyDimensionValue? IReadOnlyDimension.DefaultValue => DefaultValue;

        #endregion
    }
}
