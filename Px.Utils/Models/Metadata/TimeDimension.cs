using PxUtils.Models.Metadata.Enums;
using PxUtils.Language;

namespace PxUtils.Models.Metadata
{
    public class TimeDimension(string code, MultilanguageString name, TimeDimensionInterval interval) : Dimension(code, name, DimensionType.Time)
    {
        public TimeDimensionInterval Interval { get; } = interval;
    }
}
