using PxUtils.Models.Metadata.Enums;
using PxUtils.Language;

namespace PxUtils.Models.Metadata.Dimensions
{
    public class TimeDimension(string code, MultilanguageString name, List<Property> additionalProperties, List<DimensionValue> values, TimeDimensionInterval interval)
        : IReadOnlyDimension
    {
        public string Code { get; } = code;

        public DimensionType Type => DimensionType.Time;

        public MultilanguageString Name {get; } = name;

        public List<Property> AdditionalProperties { get; } = additionalProperties;

        public IReadOnlyList<DimensionValue> Values { get; } = values;

        public TimeDimensionInterval Interval { get; } = interval;

        IReadOnlyMultilanguageString IReadOnlyDimension.Name => Name;

        IReadOnlyList<Property> IReadOnlyDimension.AdditionalProperties => AdditionalProperties;

        IReadOnlyList<IReadOnlyDimensionValue> IReadOnlyDimension.Values => Values;
    }
}
