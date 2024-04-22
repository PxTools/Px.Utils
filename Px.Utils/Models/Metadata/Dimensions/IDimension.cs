using Px.Utils.Models.Metadata;
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
        new Dictionary<string, Property> AdditionalProperties { get; }

        /// <summary>
        /// Ordered list of editable dimension values that define the structure of the dimension.
        /// </summary>
        new IReadOnlyList<DimensionValue> Values { get; }

        /// <summary>
        /// The default value of the dimension, this property is optional.
        /// </summary>
        new DimensionValue? DefaultValue { get; set; }

        /// <summary>
        /// Returns a new dimension object where the order of values is changed or some values are filtered out according to the map.
        /// </summary>
        /// <param name="map">Change the order of values or filter out some values</param>
        /// <returns>New dimension object with the transform applied</returns>
        new IDimension GetTransform(DimensionMap map);
    }
}
