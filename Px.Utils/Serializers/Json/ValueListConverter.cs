using Px.Utils.Models.Metadata.Dimensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Serializers.Json
{
    /// <summary>
    /// Custom converter for the <see cref="ValueList"/> class.
    /// Note that the conversion of <see cref="ContentDimensionValue"/> collection <see cref="ContentValueList"/> is handled by <see cref="ContentValueListConverter"/>.
    /// </summary>
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
