using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Serializers.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.MetaProperties
{
    [JsonConverter(typeof(MetaPropertyConverter))]
    public class StringListProperty(List<string> values) : MetaProperty()
    {
        public IReadOnlyList<string> Value { get; private set; } = values;

        public override MetaPropertyType Type => MetaPropertyType.TextArray;
    }
}
