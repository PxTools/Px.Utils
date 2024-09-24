using Px.Utils.Language;
using Px.Utils.Models.Metadata.MetaProperties;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.Dimensions
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
        /// The number of decimal places to which the data described by this dimension value is rounded.
        /// </summary>
        public int Precision { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="code">Unique code among the values of this dimension</param>
        /// <param name="name">Multilanguage name of the dimension value</param>
        /// <param name="unit">The unit for the data described by this dimension value</param>
        /// <param name="lastUpdated">The date and time when the data described by this dimension value was last updated</param>
        /// <param name="precision">The number of decimal places to which the data described by this dimension value is rounded</param>
        public ContentDimensionValue(
            string code,
            MultilanguageString name,
            MultilanguageString unit,
            DateTime lastUpdated,
            int precision)
            : base(code, name)
        {
            Unit = unit;
            LastUpdated = lastUpdated;
            Precision = precision;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="code">Unique code among the values of this dimension</param>
        /// <param name="name">Multilanguage name of the dimension value</param>
        /// <param name="unit">The unit for the data described by this dimension value</param>
        /// <param name="lastUpdated">The date and time when the data described by this dimension value was last updated</param>
        /// <param name="precision">The number of decimal places to which the data described by this dimension value is rounded</param>
        /// <param name="isVirtual">True if the value is not from the original data but was created by computing or aggregating other values</param>
        public ContentDimensionValue(
            string code,
            MultilanguageString name,
            MultilanguageString unit,
            DateTime lastUpdated,
            int precision,
            bool isVirtual)
            : base(code, name, isVirtual)
        {
            Unit = unit;
            LastUpdated = lastUpdated;
            Precision = precision;
        }


        /// <summary>
        /// Constructor mainly for deserialization purposes. 
        /// </summary>
        /// <param name="code">Unique code among the values of this dimension</param>
        /// <param name="name">Multilanguage name of the dimension value</param>
        /// <param name="isVirtual">Indicates whether the value is in the original data or was created by computing or aggregating other values</param>
        /// <param name="additionalProperties">Additional properties of the dimension value</param>
        [JsonConstructor]
        public ContentDimensionValue(
            string code,
            MultilanguageString name,
            MultilanguageString unit,
            DateTime lastUpdated,
            int precision,
            bool isVirtual,
            Dictionary<string, MetaProperty> additionalProperties)
            : base(code, name, isVirtual, additionalProperties)
        {
            Unit = unit;
            LastUpdated = lastUpdated;
            Precision = precision;
        }

        /// <summary>
        /// Constructor that takes a dimension value as input.
        /// </summary>
        /// <param name="dimensionValue">Used to set the code and name of the dimension value</param>
        /// <param name="unit">The unit for the data described by this dimension value</param>
        /// <param name="lastUpdated">The date and time when the data described by this dimension value was last updated</param>
        /// <param name="precision">The number of decimal places to which the data described by this dimension value is rounded</param>
        public ContentDimensionValue(
            DimensionValue dimensionValue,
            MultilanguageString unit,
            DateTime lastUpdated,
            int precision)
            : base(dimensionValue.Code, dimensionValue.Name, dimensionValue.IsVirtual)
        {
            Unit = unit;
            LastUpdated = lastUpdated;
            Precision = precision;
        }
    }
}
