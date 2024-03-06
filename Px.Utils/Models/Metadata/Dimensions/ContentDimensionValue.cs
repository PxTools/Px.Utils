using PxUtils.Language;

namespace PxUtils.Models.Metadata.Dimensions
{
    public class ContentDimensionValue : DimensionValue
    {
        /// <summary>
        /// The unit for the data described by this dimension value.
        /// </summary>
        public MultilanguageString Unit { get; }

        /// <summary>
        /// The date and time when the data described by this dimension value was last updated.
        /// </summary>
        public DateTime LastUpdated { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="code">Unique code among the values of this dimension</param>
        /// <param name="name">Multilanguage name of the dimension value</param>
        /// <param name="unit">The unit for the data described by this dimension value</param>
        /// <param name="lastUpdated">The date and time when the data described by this dimension value was last updated</param>
        public ContentDimensionValue(string code, MultilanguageString name, MultilanguageString unit, DateTime lastUpdated) : base(code, name)
        {
            Unit = unit;
            LastUpdated = lastUpdated;
        }

        /// <summary>
        /// Constructor that takes a dimension value as input.
        /// </summary>
        /// <param name="dimensionValue">Used to set the code and name of the dimension value</param>
        /// <param name="unit">The unit for the data described by this dimension value</param>
        /// <param name="lastUpdated">The date and time when the data described by this dimension value was last updated</param>
        public ContentDimensionValue(DimensionValue dimensionValue, MultilanguageString unit, DateTime lastUpdated) : base(dimensionValue.Code, dimensionValue.Name)
        {
            Unit = unit;
            LastUpdated = lastUpdated;
        }
    }
}
