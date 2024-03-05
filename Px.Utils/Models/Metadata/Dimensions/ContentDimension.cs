using PxUtils.Language;
using PxUtils.Models.Metadata.Enums;

namespace PxUtils.Models.Metadata.Dimensions
{
    public class ContentDimension(
        string code,
        MultilanguageString name,
        Dictionary<string, Property> additionalProperties,
        List<ContentDimensionValue> values,
        ContentDimensionValue? defaultValue = null
        ) : IDimension
    {
        public string Code { get; } = code;

        public DimensionType Type => DimensionType.Content;

        public MultilanguageString Name { get; } = name;

        public Dictionary<string, Property> AdditionalProperties { get; } = additionalProperties;

        public List<ContentDimensionValue> Values { get; } = values;

        public ContentDimensionValue? DefaultValue { get; } = defaultValue;

        #region Interface implementations

        DimensionValue? IDimension.DefaultValue => DefaultValue;

        MultilanguageString IReadOnlyDimension.Name => Name;

        IReadOnlyList<IReadOnlyDimensionValue> IReadOnlyDimension.Values => Values;

        IReadOnlyList<DimensionValue> IDimension.Values => Values;

        IReadOnlyDictionary<string, Property> IReadOnlyDimension.AdditionalProperties => AdditionalProperties;

        IReadOnlyDimensionValue? IReadOnlyDimension.DefaultValue => DefaultValue;

        #endregion
    }
}
