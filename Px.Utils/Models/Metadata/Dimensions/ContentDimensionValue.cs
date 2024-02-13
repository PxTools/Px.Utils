using PxUtils.Language;

namespace PxUtils.Models.Metadata.Dimensions
{
    public class ContentDimensionValue(string code, MultilanguageString name, MultilanguageString unit, DateTime lastUpdated)
        : DimensionValue(code, name)
    {
        public MultilanguageString Unit { get; } = unit;

        public DateTime LastUpdated { get; } = lastUpdated;
    }
}
