using Px.Utils.Models.Metadata;
using PxUtils.Language;
using PxUtils.Models.Metadata.Enums;

namespace PxUtils.Models.Metadata.Dimensions
{
    /// <summary>
    /// Class representing a content dimension.
    /// </summary>
    /// <param name="code">Unique code among the dimensions of the metadata matrix</param>
    /// <param name="name">Multilanguage name of the dimension</param>
    /// <param name="additionalProperties">Properties of the dimension, excluding the required properties</param>
    /// <param name="values">Ordered list of dimension values that define the structure of the dimension</param>
    /// <param name="defaultValue">Default value of the dimension, this property is optional</param>
    public class ContentDimension(
        string code,
        MultilanguageString name,
        Dictionary<string, Property> additionalProperties,
        List<ContentDimensionValue> values,
        ContentDimensionValue? defaultValue = null
        ) : IDimension
    {
        /// <summary>
        /// Unique code among the dimensions of the metadata matrix. Used for identifying this dimension.
        /// </summary>
        public string Code { get; } = code;

        /// <summary>
        /// The type of the dimension. Always DimensionType.Content for this class.
        /// </summary>
        public DimensionType Type => DimensionType.Content;

        /// <summary>
        /// Multilanguage name of the dimension.
        /// </summary>
        public MultilanguageString Name { get; set; } = name;

        /// <summary>
        /// Editable collection of properties of the dimension, excluding the required properties.
        /// </summary>
        public Dictionary<string, Property> AdditionalProperties { get; } = additionalProperties;

        /// <summary>
        /// List of editable dimension values that define the structure of the dimension.
        /// </summary>
        public IReadOnlyList<ContentDimensionValue> Values { get; } = values;

        /// <summary>
        /// The default value of the dimension, this property is optional.
        /// </summary>
        public ContentDimensionValue? DefaultValue { get; set; } = defaultValue;

        #region Interface implementations

        DimensionValue? IDimension.DefaultValue
        {
            get => DefaultValue;
            set => DefaultValue = value as ContentDimensionValue;
        }

        MultilanguageString IReadOnlyDimension.Name => Name;

        IReadOnlyList<IReadOnlyDimensionValue> IReadOnlyDimension.Values => Values;

        IReadOnlyList<DimensionValue> IDimension.Values => Values;

        IReadOnlyDictionary<string, Property> IReadOnlyDimension.AdditionalProperties => AdditionalProperties;

        IReadOnlyDimensionValue? IReadOnlyDimension.DefaultValue => DefaultValue;

        IReadOnlyList<string> IDimensionMap.ValueCodes => _valueCodes;

        public IDimension GetTransform(IDimensionMap map)
        {
            List<ContentDimensionValue> newValues = map.ValueCodes.Select(code =>
            {
                if (Values.First(value => value.Code == code) is ContentDimensionValue value) return value;
                else throw new ArgumentException($"Value with code {code} not found in dimension");
            }).ToList();

            return new ContentDimension(Code, Name, AdditionalProperties, newValues, DefaultValue);
        }

        #endregion

        private readonly IReadOnlyList<string> _valueCodes = values.Select(value => value.Code).ToList();
    }
}
