using Px.Utils.Language;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Serializers.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.MetaProperties
{
    [JsonConverter(typeof(MetaPropertyConverter))]
    public class MultilanguageStringListProperty(List<MultilanguageString> values) : MetaProperty()
    {
        public IReadOnlyList<MultilanguageString> Value { get; private set; } = values;
        public override MetaPropertyType Type => MetaPropertyType.MultilanguageTextArray;
    }
}
