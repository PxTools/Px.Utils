using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Serializers.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.MetaProperties
{
    [JsonConverter(typeof(MetaPropertyConverter))]
    public class StringProperty(string value) : MetaProperty
    {
        public override MetaPropertyType Type => MetaPropertyType.Text;

        public string Value { get; } = value;
    }
}
