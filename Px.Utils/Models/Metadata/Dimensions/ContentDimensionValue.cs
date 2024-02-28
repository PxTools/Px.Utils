using PxUtils.Language;

namespace PxUtils.Models.Metadata.Dimensions
{
    public class ContentDimensionValue : DimensionValue
    {
        public MultilanguageString Unit { get; }

        public DateTime LastUpdated { get; }

        public ContentDimensionValue(string code, MultilanguageString name, MultilanguageString unit, DateTime lastUpdated) : base(code, name)
        {
            Unit = unit;
            LastUpdated = lastUpdated;
        }

        public ContentDimensionValue(DimensionValue dimensionValue, MultilanguageString unit, DateTime lastUpdated) : base(dimensionValue.Code, dimensionValue.Name)
        {
            Unit = unit;
            LastUpdated = lastUpdated;
        }
    }
}
