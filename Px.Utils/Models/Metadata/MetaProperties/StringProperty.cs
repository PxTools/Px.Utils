using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Serializers.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.MetaProperties
{
    /// <summary>
    /// Class representing a string property of a px-file. The string does not have any language information.
    /// </summary>
    /// <param name="value"></param>
    [JsonConverter(typeof(MetaPropertyConverter))]
    public class StringProperty(string value) : MetaProperty
    {
        public override MetaPropertyType Type => MetaPropertyType.Text;

        public string Value { get; } = value;
    }
}
