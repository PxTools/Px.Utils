using Px.Utils.Models.Metadata.Dimensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Serializers.Json
{
    public class ValueListConverter : JsonConverter<ValueList>
    {
        public override ValueList? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<DimensionValue>? values = JsonSerializer.Deserialize<List<DimensionValue>>(ref reader, options);
            return values is not null ? new ValueList(values) : null;
        }

        public override void Write(Utf8JsonWriter writer, ValueList value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value as IReadOnlyList<IReadOnlyDimensionValue>, options);
    }
}
