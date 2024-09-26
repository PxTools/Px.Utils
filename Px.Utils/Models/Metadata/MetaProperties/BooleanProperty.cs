using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Serializers.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.MetaProperties
{
    /// <summary>
    /// Class for representing a boolean property of a px-file.
    /// </summary>
    /// <param name="value"></param>
    [JsonConverter(typeof(MetaPropertyConverter))]
    public class BooleanProperty(bool value) : MetaProperty()
    {
        public bool Value { get; private set; } = value;

        public override MetaPropertyType Type => MetaPropertyType.Boolean;
    }
}
