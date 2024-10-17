using Px.Utils.Language;
using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Models.Metadata.MetaProperties;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Serializers.Json
{
    /// <summary>
    /// Custom converter for all objects that implement the <see cref="IReadOnlyDimension"/> interface.
    /// Deserialization produces <see cref="Dimension"/> objects that will have the correct subtype."/>
    /// </summary>
    public class DimensionConverter : JsonConverter<Dimension>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsAssignableTo(typeof(IReadOnlyDimension));
        }

        public override Dimension? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token");
            }

            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            JsonElement root = doc.RootElement;

            string typeName = FN(nameof(IReadOnlyDimension.Type), options);
            DimensionType type = root.GetProperty(typeName, options).Deserialize<DimensionType>(options);

            string codeName = FN(nameof(IReadOnlyDimension.Code), options);
            string code = root.GetProperty(codeName, options).GetString()
                ?? throw new JsonException("Code property not found.");

            string nameName = FN(nameof(IReadOnlyDimension.Name), options);
            MultilanguageString name = root.GetProperty(nameName, options).Deserialize<MultilanguageString>(options)
                ?? throw new JsonException("Name property not found.");

            string additionalPropertiesName = FN(nameof(IReadOnlyDimension.AdditionalProperties), options);
            Dictionary<string, MetaProperty> additionalProperties = root.GetProperty(additionalPropertiesName, options)
                .Deserialize<Dictionary<string, MetaProperty>>(options)
                ?? throw new JsonException("AdditionalProperties property not found.");

            string valuesName = FN(nameof(IReadOnlyDimension.Values), options);
            if (type == DimensionType.Content)
            {
                ContentValueList contentValues = root.GetProperty(valuesName, options).Deserialize<ContentValueList>(options)
                    ?? throw new JsonException("Values property not found.");

                return new ContentDimension(code, name, additionalProperties, contentValues);
            }

            ValueList values = root.GetProperty(valuesName, options).Deserialize<ValueList>(options)
                ?? throw new JsonException("Values property not found.");

            if (type == DimensionType.Time)
            {
                string intervalName = FN(nameof(TimeDimension.Interval), options);
                TimeDimensionInterval interval = root.GetProperty(intervalName, options).Deserialize<TimeDimensionInterval>(options);
                return new TimeDimension(code, name, additionalProperties, values, interval);
            }

            return new Dimension(code, name, additionalProperties, values, type);
        }

        public override void Write(Utf8JsonWriter writer, Dimension value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            string typeName = options.PropertyNamingPolicy?.ConvertName(nameof(value.Type)) ?? nameof(value.Type);
            writer.WritePropertyName(typeName);
            JsonSerializer.Serialize(writer, value.Type, options);

            string codeName = options.PropertyNamingPolicy?.ConvertName(nameof(value.Code)) ??
                nameof(value.Code);
            writer.WritePropertyName(codeName);
            JsonSerializer.Serialize(writer, value.Code, options);

            string nameName = options.PropertyNamingPolicy?.ConvertName(nameof(value.Name)) ??
                nameof(value.Name);
            writer.WritePropertyName(nameName);
            JsonSerializer.Serialize(writer, value.Name, options);

            if (value is TimeDimension timeDim)
            {
                string intervalName = options.PropertyNamingPolicy?.ConvertName(nameof(timeDim.Interval)) ??
                    nameof(timeDim.Interval);
                writer.WritePropertyName(intervalName);
                JsonSerializer.Serialize(writer, timeDim.Interval, options);
            }

            if(value is ContentDimension contentDim)
            {
                string valuesName = options.PropertyNamingPolicy?.ConvertName(nameof(contentDim.Values)) ??
                    nameof(contentDim.Values);
                writer.WritePropertyName(valuesName);
                JsonSerializer.Serialize(writer, contentDim.Values, options);
            }
            else
            {
                string valuesName = options.PropertyNamingPolicy?.ConvertName(nameof(value.Values)) ??
                    nameof(value.Values);
                writer.WritePropertyName(valuesName);
                JsonSerializer.Serialize(writer, value.Values, options);
            }

            string additionalPropertiesName = options.PropertyNamingPolicy?.ConvertName(nameof(value.AdditionalProperties)) ??
                nameof(value.AdditionalProperties);
            writer.WritePropertyName(additionalPropertiesName);
            JsonSerializer.Serialize(writer, value.AdditionalProperties, options);

            writer.WriteEndObject();
        }

        private static string FN(string input, JsonSerializerOptions options)
            => options.PropertyNamingPolicy?.ConvertName(input) ?? input;
    }
}
