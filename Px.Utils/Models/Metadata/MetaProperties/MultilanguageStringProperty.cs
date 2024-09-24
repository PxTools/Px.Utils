using Px.Utils.Language;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Serializers.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.MetaProperties
{
    /// <summary>
    /// Class for px-file properties that are strings with translations in multiple languages.
    /// </summary>
    /// <param name="value">All translations.</param>
    [JsonConverter(typeof(MetaPropertyConverter))]
    public class MultilanguageStringProperty(MultilanguageString value) : MetaProperty()
    {
        public MultilanguageString Value { get; private set; } = value;

        public override MetaPropertyType Type => MetaPropertyType.MultilanguageText;
    }
}
