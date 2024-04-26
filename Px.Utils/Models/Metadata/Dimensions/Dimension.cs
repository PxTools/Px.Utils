using Px.Utils.Models.Metadata;
using PxUtils.Language;
using PxUtils.Models.Metadata.Enums;

namespace PxUtils.Models.Metadata.Dimensions
{
    /// <summary>
    /// Class representing a dimension without any fixed type.
    /// </summary>
    /// <param name="code">Unique code among all the dimensions of the metadata matrix</param>
    /// <param name="name">Multilanguage name of the dimension</param>
    /// <param name="additionalProperties">Properties of the dimension, excluding the required properties</param>
    /// <param name="values">Ordered list of dimension values that define the structure of the dimension</param>
    /// <param name="defaultValue">Default value of the dimension, this property is optional</param>
    /// <param name="type">Type of the dimension. The dedicated classes must be used for time and content dimensions</param>
    public class Dimension(
        string code,
        MultilanguageString name,
        Dictionary<string, Property> additionalProperties,
        List<DimensionValue> values,
        DimensionValue? defaultValue,
        DimensionType type)
        : IDimension
    {
        /// <summary>
        /// Unique code among all the dimensions of the metadata matrix. Used for identifying this dimension.
        /// </summary>
        public string Code { get; } = code;

        /// <summary>
        /// The type of the dimension.
        /// </summary>
        public DimensionType Type { get; } = type;

        /// <summary>
        /// Multilanguage name of the dimension.
        /// </summary>
        public MultilanguageString Name { get; set; } = name;

        /// <summary>
        /// Properties of the dimension, excluding the required properties.
        /// </summary>
        public Dictionary<string, Property> AdditionalProperties { get; } = additionalProperties;

        /// <summary>
        /// Ordered list of dimension values that define the structure of the dimension.
        /// </summary>
        public IReadOnlyList<DimensionValue> Values { get; } = values;

        /// <summary>
        /// Default value of the dimension, this property is optional.
        /// </summary>
        public DimensionValue? DefaultValue { get; set; } = defaultValue;

        #region Interface implementations

        MultilanguageString IReadOnlyDimension.Name => Name;

        IReadOnlyDictionary<string, Property> IReadOnlyDimension.AdditionalProperties => AdditionalProperties;

        IReadOnlyList<IReadOnlyDimensionValue> IReadOnlyDimension.Values => Values;

        IReadOnlyDimensionValue? IReadOnlyDimension.DefaultValue => DefaultValue;

        IReadOnlyList<string> IDimensionMap.ValueCodes => _valueCodes;

        public IDimension GetTransform(DimensionMap map)
        {
            List<DimensionValue> newValues = map.ValueCodes.Select(code =>
            {
                if (Values.First(value => value.Code == code) is DimensionValue value) return value;
                else throw new ArgumentException($"Value with code {code} not found in dimension");
            }).ToList();

            return new Dimension(Code, Name, AdditionalProperties, newValues, DefaultValue, Type);
        }

        IReadOnlyDimension IReadOnlyDimension.GetTransform(DimensionMap map) => GetTransform(map);

        #endregion

        private readonly List<string> _valueCodes = values.Select(value => value.Code).ToList();

    }
}
