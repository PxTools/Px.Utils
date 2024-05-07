using PxUtils.Language;

namespace PxUtils.Models.Metadata.Dimensions
{
    /// <summary>
    /// Class representing a generic dimension value.
    /// </summary>
    /// <param name="code">Unique code among the values of this dimension</param>
    /// <param name="name">Multilanguage name of the dimension value</param>
    public class DimensionValue(string code, MultilanguageString name) : IReadOnlyDimensionValue
    {
        /// <summary>
        /// Unique code among the values of this dimension.
        /// </summary>
        public string Code { get; } = code;

        /// <summary>
        /// Multilanguage name of the dimension value.
        /// </summary>
        public MultilanguageString Name { get; set; } = name;

        /// <summary>
        /// Additional properties of the dimension value, excluding the required properties.
        /// </summary>
        public Dictionary<string, MetaProperty> AdditionalProperties { get; } = [];

        #region Interface implementations

        MultilanguageString IReadOnlyDimensionValue.Name => Name;

        IReadOnlyDictionary<string, MetaProperty> IReadOnlyDimensionValue.AdditionalProperties => AdditionalProperties;

        #endregion
    }
}
