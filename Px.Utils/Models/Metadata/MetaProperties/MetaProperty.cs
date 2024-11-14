using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Serializers.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.MetaProperties
{
    /// <summary>
    /// Base class for all px file properties that are not part of the <see cref="MatrixMetadata"/> structure.
    /// </summary>
    [JsonConverter(typeof(MetaPropertyConverter))]
    public abstract class MetaProperty()
    {
        /// <summary>
        /// Enum representing the type of the property. Required for deserialization.
        /// </summary>
        public abstract MetaPropertyType Type { get; }
    }
}
