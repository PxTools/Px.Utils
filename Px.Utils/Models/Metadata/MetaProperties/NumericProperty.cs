using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Serializers.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.MetaProperties
{
    /// <summary>
    /// Class representing a numeric property of a px-file.
    /// </summary>
    /// <param name="value"></param>
    [JsonConverter(typeof(MetaPropertyConverter))]
    public class NumericProperty(double value) : MetaProperty()
    {
        public double Value { get; private set; } = value;

        public override MetaPropertyType Type => MetaPropertyType.Numeric;
    }
}
