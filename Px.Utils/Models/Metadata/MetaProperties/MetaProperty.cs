using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Serializers.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.MetaProperties
{
    [JsonConverter(typeof(MetaPropertyConverter))]
    public abstract class MetaProperty()
    {
        public abstract MetaPropertyType Type { get; }
    }
}
