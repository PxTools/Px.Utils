using PxUtils.Models.Metadata.Enums;
using PxUtils.Language;

namespace PxUtils.Models.Metadata
{
    public class TimeDimension(string code, MultilanguageString name, List<Property> additionalProperties, TimeDimensionInterval interval)
        : Dimension(code, name, additionalProperties, DimensionType.Time)
    {
        public TimeDimensionInterval Interval { get; } = interval;
    }
}
