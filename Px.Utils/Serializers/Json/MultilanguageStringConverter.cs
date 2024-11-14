using Px.Utils.Language;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Serializers.Json
{
    /// <summary>
    /// Custom converter for <see cref="MultilanguageString"/> objects.
    /// </summary>
    public class MultilanguageStringConverter : JsonConverter<MultilanguageString>
    {
        public override MultilanguageString? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Dictionary<string, string>? translations = JsonSerializer.Deserialize<Dictionary<string, string>?>(ref reader, options);
            return translations is not null ? new MultilanguageString(translations) : null;
        }

        public override void Write(Utf8JsonWriter writer, MultilanguageString value, JsonSerializerOptions options)
        {
            Dictionary<string, string> translations = value.Languages.ToDictionary(l => l, l => value[l]);
            JsonSerializer.Serialize(writer, translations, options);
        }
    }
}
