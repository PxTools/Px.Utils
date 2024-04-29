using PxUtils.Models.Metadata.Enums;
using PxUtils.Language;
using Px.Utils.Models.Metadata;

namespace PxUtils.Models.Metadata.Dimensions
{
    /// <summary>
    /// Class representing a time dimension
    /// </summary>
    /// <param name="code">Unique code among all the dimensions of the metadata matrix</param>
    /// <param name="name">Multilanguage name of the dimension</param>
    /// <param name="additionalProperties">Properties of the dimension, excluding the required properties</param>
    /// <param name="values">Ordered list of dimension values that define the structure of the dimension</param>
    /// <param name="defaultValue">The default value of the dimension, this property is optional</param>
    /// <param name="interval">Time interval between the values of this dimension</param>
    public class TimeDimension(
        string code,
        MultilanguageString name,
        Dictionary<string, Property> additionalProperties,
        List<DimensionValue> values,
        DimensionValue? defaultValue,
        TimeDimensionInterval interval)
        : IDimension
    {
        /// <summary>
        /// Unique code among all the dimensions of the metadata matrix. Used for identifying this dimension.
        /// </summary>
        public string Code { get; } = code;

        /// <summary>
        /// The type of the dimension, always DimensionType.Time for this class
        /// </summary>
        public DimensionType Type => DimensionType.Time;

        /// <summary>
        /// Multilanguage name of the dimension
        /// </summary>
        public MultilanguageString Name { get; set; } = name;

        /// <summary>
        /// Properties of the dimension, excluding the required properties
        /// </summary>
        public Dictionary<string, Property> AdditionalProperties { get; } = additionalProperties;

        /// <summary>
        /// Ordered list of dimension values that define the structure of the dimension
        /// </summary>
        public IReadOnlyList<DimensionValue> Values { get; } = values;

        /// <summary>
        /// The default value of the dimension, this property is optional
        /// </summary>
        public DimensionValue? DefaultValue { get; set; } = defaultValue;

        /// <summary>
        /// Time interval between the values of this dimension
        /// </summary>
        public TimeDimensionInterval Interval { get; } = interval;

        #region Interface implementations

        MultilanguageString IReadOnlyDimension.Name => Name;

        IReadOnlyDictionary<string, Property> IReadOnlyDimension.AdditionalProperties => AdditionalProperties;

        IReadOnlyList<IReadOnlyDimensionValue> IReadOnlyDimension.Values => Values;

        IReadOnlyList<string> IDimensionMap.ValueCodes => _valueCodes;

        IReadOnlyDimensionValue? IReadOnlyDimension.DefaultValue => DefaultValue;

        public IDimension GetTransform(IDimensionMap map)
        {
            List<DimensionValue> newValues = map.ValueCodes.Select(code =>
            {
                if (Values.First(value => value.Code == code) is DimensionValue value) return value;
                else throw new ArgumentException($"Value with code {code} not found in dimension");
            }).ToList();

            return new TimeDimension(Code, Name, AdditionalProperties, newValues, DefaultValue, Interval);
        }

        #endregion

        private readonly List<string> _valueCodes = values.Select(value => value.Code).ToList();
    }
}
