using PxUtils.Models.Metadata.Enums;
using PxUtils.Language;

namespace PxUtils.Models.Metadata.Dimensions
{
    public class TimeDimension(
        string code,
        MultilanguageString name,
        Dictionary<string, Property> additionalProperties,
        List<DimensionValue> values,
        DimensionValue? defaultValue,
        TimeDimensionInterval interval)
        : IDimension
    {
        public string Code { get; } = code;

        public DimensionType Type => DimensionType.Time;

        public MultilanguageString Name {get; } = name;

        public Dictionary<string, Property> AdditionalProperties { get; } = additionalProperties;

        public IReadOnlyList<DimensionValue> Values { get; } = values;

        public DimensionValue? DefaultValue { get; } = defaultValue;

        public TimeDimensionInterval Interval { get; } = interval;

        #region Interface implementations

        MultilanguageString IReadOnlyDimension.Name => Name;

        IReadOnlyDictionary<string, Property> IReadOnlyDimension.AdditionalProperties => AdditionalProperties;

        IReadOnlyList<IReadOnlyDimensionValue> IReadOnlyDimension.Values => Values;

        IReadOnlyDimensionValue? IReadOnlyDimension.DefaultValue => DefaultValue;

        #endregion
    }
}
