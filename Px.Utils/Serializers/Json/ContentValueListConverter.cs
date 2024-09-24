using Px.Utils.Models.Metadata.Dimensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Serializers.Json
{
    public class ContentValueListConverter : JsonConverter<ContentValueList>
    {
        public override ContentValueList? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<ContentDimensionValue>? dimensions = JsonSerializer.Deserialize<List<ContentDimensionValue>>(ref reader, options);
            return dimensions is null ? null : new ContentValueList(dimensions);
        }

        public override void Write(Utf8JsonWriter writer, ContentValueList value, JsonSerializerOptions options)
        {
            List<ContentDimensionValue> dimensions = value.Map(d => d).ToList();
            JsonSerializer.Serialize(writer, dimensions, options);
        }
    }
}
