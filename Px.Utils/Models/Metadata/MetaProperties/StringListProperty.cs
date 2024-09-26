using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Serializers.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.MetaProperties
{
    /// <summary>
    /// Class representing a list of strings property of a px-file.
    /// The strings do not have any language information.
    /// </summary>
    /// <param name="values"></param>
    [JsonConverter(typeof(MetaPropertyConverter))]
    public class StringListProperty(List<string> values) : MetaProperty()
    {
        public IReadOnlyList<string> Value { get; private set; } = values;

        public override MetaPropertyType Type => MetaPropertyType.TextArray;
    }
}
