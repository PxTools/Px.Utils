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
        IReadOnlyDictionary<string, Property> AdditionalProperties { get; }
    }
}
