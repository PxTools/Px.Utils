using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MetaPropertyType
    {
        Text,
        MultilanguageText,
        Numeric,
        Boolean,
        TextArray,
        MultilanguageTextArray,
    }
}
