using Px.Utils.Language;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Models.Metadata.MetaProperties;
using Px.Utils.Serializers.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.Dimensions
{
    /// <summary>
    /// Class representing a dimension without any fixed type.
    /// </summary>
    [JsonConverter(typeof(DimensionConverter))]
    public class Dimension : IReadOnlyDimension
    {
        /// <summary>
        /// Unique code among all the dimensions of the metadata matrix. Used for identifying this dimension.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// The type of the dimension.
        /// </summary>
        public DimensionType Type { get; }

        /// <summary>
        /// Multilanguage name of the dimension.
        /// </summary>
        public MultilanguageString Name { get; set; }

        /// <summary>
        /// Properties of the dimension, excluding the required properties.
        /// </summary>
        public Dictionary<string, MetaProperty> AdditionalProperties { get; }

        /// <summary>
        /// Ordered list of dimension values that define the structure of the dimension.
        /// </summary>
        public virtual ValueList Values { get; }

        #region Interface implementations

        MultilanguageString IReadOnlyDimension.Name => Name;

        IReadOnlyDictionary<string, MetaProperty> IReadOnlyDimension.AdditionalProperties => AdditionalProperties;

        IReadOnlyList<IReadOnlyDimensionValue> IReadOnlyDimension.Values => Values;

        IReadOnlyList<string> IDimensionMap.ValueCodes => Values.Codes;

        public virtual Dimension GetTransform(IDimensionMap map)
        {
            ValueList newValues = new(map.ValueCodes.Select(code =>
            {
                if (Values.Find(code) is DimensionValue value) return value;
                else throw new ArgumentException($"Value with code {code} not found in dimension");
            }));

            return new Dimension(Code, Name, AdditionalProperties, newValues, Type);
        }

        #endregion

        /// <param name="code">Unique code among all the dimensions of the metadata matrix</param>
        /// <param name="name">Multilanguage name of the dimension</param>
        /// <param name="additionalProperties">Properties of the dimension, excluding the required properties</param>
        /// <param name="values">Ordered list of dimension values that define the structure of the dimension</param>
        /// <param name="type">Type of the dimension. The dedicated classes must be used for time and content dimensions</param>
        [JsonConstructor]
        public Dimension(string code, MultilanguageString name, Dictionary<string, MetaProperty> additionalProperties,
            ValueList values, DimensionType type)
        {
            Code = code;
            Type = type;
            Name = name;
            AdditionalProperties = additionalProperties;
            Values = values;
        }

        /// <param name="code">Unique code among all the dimensions of the metadata matrix</param>
        /// <param name="name">Multilanguage name of the dimension</param>
        /// <param name="additionalProperties">Properties of the dimension, excluding the required properties</param>
        /// <param name="values">Ordered list of dimension values that define the structure of the dimension</param>
        /// <param name="type">Type of the dimension. The dedicated classes must be used for time and content dimensions</param>
        public Dimension(string code, MultilanguageString name, Dictionary<string, MetaProperty> additionalProperties,
            IReadOnlyList<DimensionValue> values, DimensionType type)
        {
            Code = code;
            Type = type;
            Name = name;
            AdditionalProperties = additionalProperties;
            Values = new(values);
        }
    }
}
