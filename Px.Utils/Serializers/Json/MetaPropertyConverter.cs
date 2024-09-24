using Px.Utils.Language;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Models.Metadata.MetaProperties;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Serializers.Json
{
    public class MetaPropertyConverter : JsonConverter<MetaProperty>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsAssignableTo(typeof(MetaProperty));
        }

        private const string VALUE_PROPERTY_NAME = "Value";

        public override MetaProperty? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token");
            }

            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            JsonElement root = doc.RootElement;

            string typeName = options.PropertyNamingPolicy?.ConvertName(nameof(MetaProperty.Type)) ?? nameof(MetaProperty.Type);
            MetaPropertyType type = root.GetProperty(typeName).Deserialize<MetaPropertyType>(options);

            string valuePropertyName = options.PropertyNamingPolicy?.ConvertName(VALUE_PROPERTY_NAME) ?? VALUE_PROPERTY_NAME;

            switch (type)
            {
                case MetaPropertyType.Text:
                    string stringValue = root.GetProperty(valuePropertyName).GetString()
                        ?? throw new JsonException("Value property not found.");
                    return new StringProperty(stringValue);
                case MetaPropertyType.MultilanguageText:
                    MultilanguageString multilanguageString = root.GetProperty(valuePropertyName)
                        .Deserialize<MultilanguageString>(options) ?? throw new JsonException("Value property not found.");
                    return new MultilanguageStringProperty(multilanguageString);
                case MetaPropertyType.Numeric:
                    double numericValue = root.GetProperty(valuePropertyName).GetDouble();
                    return new NumericProperty(numericValue);
                case MetaPropertyType.Boolean:
                    bool booleanValue = root.GetProperty(valuePropertyName).GetBoolean();
                    return new BooleanProperty(booleanValue);
                case MetaPropertyType.TextArray:
                    List<string> strings = root.GetProperty(valuePropertyName)
                        .Deserialize<List<string>>(options) ?? throw new JsonException("Value property not found.");
                    return new StringListProperty(strings);
                case MetaPropertyType.MultilanguageTextArray:
                    List<MultilanguageString> multilanguageStrings = root.GetProperty(valuePropertyName)
                        .Deserialize<List<MultilanguageString>>(options) ?? throw new JsonException("Value property not found.");
                    return new MultilanguageStringListProperty(multilanguageStrings);
                default:
                    throw new NotSupportedException($"MetaProperty of type {type} is not supported.");
            }

        }

        public override void Write(Utf8JsonWriter writer, MetaProperty value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            string propertyName = options.PropertyNamingPolicy?.ConvertName(nameof(value.Type)) ?? nameof(value.Type);
            writer.WritePropertyName(propertyName);
            writer.WriteStringValue(value.Type.ToString());
            string valuePropertyName = options.PropertyNamingPolicy?.ConvertName(VALUE_PROPERTY_NAME) ?? VALUE_PROPERTY_NAME;
            writer.WritePropertyName(valuePropertyName);

            switch (value)
            {
                case StringProperty stringProperty:
                    JsonSerializer.Serialize(writer, stringProperty.Value, options);
                    break;
                case MultilanguageStringProperty mlsProperty:
                    JsonSerializer.Serialize(writer, mlsProperty.Value, options);
                    break;
                case StringListProperty stringListProperty:
                    JsonSerializer.Serialize(writer, stringListProperty.Value, options);
                    break;
                case MultilanguageStringListProperty mlsListProperty:
                    JsonSerializer.Serialize(writer, mlsListProperty.Value, options);
                    break;
                case NumericProperty numericProperty:
                    JsonSerializer.Serialize(writer, numericProperty.Value, options);
                    break;
                case BooleanProperty booleanProperty:
                    JsonSerializer.Serialize(writer, booleanProperty.Value, options);
                    break;
                default:
                    throw new NotSupportedException($"MetaProperty of type {value.Type} is not supported.");
            }

            writer.WriteEndObject();
        }
    }
}
