using PxUtils.Language;

namespace PxUtils.Models.Metadata.Dimensions
{
    /// <summary>
    /// Readonly interface for a dimension value object in a structured format.
    /// </summary>
    public interface IReadOnlyDimensionValue
    {
        /// <summary>
        /// Unique code among the values of this dimension.
        /// </summary>
        string Code { get; }

        /// <summary>
        /// Multilanguage name of the dimension value.
        /// </summary>
        MultilanguageString Name { get; }

        /// <summary>
        /// Additional properties of the dimension value.
        /// </summary>
        IReadOnlyDictionary<string, MetaProperty> AdditionalProperties { get; }

        /// <summary>
        /// True if the value is not from the original data but was created by computing or aggregating other values.
        /// </summary>
        public bool Virtual { get; }
    }
}
