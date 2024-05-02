using PxUtils.Language;

namespace PxUtils.Models.Metadata.Dimensions
{
    public interface IDimension : IReadOnlyDimension
    {
        /// <summary>
        /// Multilanguage name of the dimension.
        /// </summary>
        new MultilanguageString Name { get; set; }

        /// <summary>
        /// Editable collection of properties of the dimension, excluding the required properties.
        /// </summary>
        new Dictionary<string, MetaProperty> AdditionalProperties { get; }

        /// <summary>
        /// Ordered list of editable dimension values that define the structure of the dimension.
        /// </summary>
        new IReadOnlyList<DimensionValue> Values { get; }
    }
}
