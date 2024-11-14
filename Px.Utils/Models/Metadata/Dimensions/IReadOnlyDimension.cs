using Px.Utils.Language;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Models.Metadata.MetaProperties;
using Px.Utils.Serializers.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.Dimensions
{
    /// <summary>
    /// Readonly interface for a dimension object in a structured format.
    /// </summary>
    [JsonConverter(typeof(DimensionConverter))]
    public interface IReadOnlyDimension : IDimensionMap
    {
        /// <summary>
        /// The type of the dimension.
        /// </summary>
        DimensionType Type { get; }

        /// <summary>
        /// Multilanguage name of the dimension.
        /// </summary>
        MultilanguageString Name { get; }

        /// <summary>
        /// Properties of the dimension, excluding the required properties.
        /// </summary>
        IReadOnlyDictionary<string, MetaProperty> AdditionalProperties { get; }

        /// <summary>
        /// Ordered list of dimension values that define the structure of the dimension.
        /// </summary>
        IReadOnlyList<IReadOnlyDimensionValue> Values { get; }

        /// <summary>
        /// Returns a new dimension object where the order of values is changed or some values are filtered out according to the map.
        /// </summary>
        /// <param name="map">Change the order of values or filter out some values</param>
        /// <returns>New dimension object with the transform applied</returns>
        Dimension GetTransform(IDimensionMap map);
    }
}
