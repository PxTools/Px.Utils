using PxUtils.Language;
using PxUtils.Models.Metadata.Enums;

namespace PxUtils.Models.Metadata.Dimensions
{
    public class Dimension(
        string code,
        MultilanguageString name,
        Dictionary<string, Property> additionalProperties,
        List<DimensionValue> values,
        DimensionValue? defaultValue,
        DimensionType type)
        : IDimension
    {
        public string Code { get; } = code;

        public DimensionType Type { get; } = type;

        public MultilanguageString Name { get; } = name;

        public Dictionary<string, Property> AdditionalProperties { get; } = additionalProperties;

        public IReadOnlyList<DimensionValue> Values { get; } = values;

        public DimensionValue? DefaultValue { get; } = defaultValue;

        #region Interface implementations

        MultilanguageString IReadOnlyDimension.Name => Name;

        IReadOnlyDictionary<string, Property> IReadOnlyDimension.AdditionalProperties => AdditionalProperties;

        IReadOnlyList<IReadOnlyDimensionValue> IReadOnlyDimension.Values => Values;

        IReadOnlyDimensionValue? IReadOnlyDimension.DefaultValue => DefaultValue;

        #endregion

    }
}
