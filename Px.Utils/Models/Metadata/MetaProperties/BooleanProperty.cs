using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Serializers.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.MetaProperties
{
    [JsonConverter(typeof(MetaPropertyConverter))]
    public class BooleanProperty(bool value) : MetaProperty()
    {
        public bool Value { get; private set; } = value;

        public override MetaPropertyType Type => MetaPropertyType.Boolean;
    }
}
