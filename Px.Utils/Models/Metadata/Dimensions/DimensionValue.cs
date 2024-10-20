﻿using Px.Utils.Language;
using Px.Utils.Models.Metadata.MetaProperties;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.Dimensions
{
    /// <summary>
    /// Class representing a generic dimension value.
    /// </summary>
    public class DimensionValue : IReadOnlyDimensionValue
    {
        /// <summary>
        /// Unique code among the values of this dimension.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Multilanguage name of the dimension value.
        /// </summary>
        public MultilanguageString Name { get; set; }

        /// <summary>
        /// Additional properties of the dimension value, excluding the required properties.
        /// </summary>
        public Dictionary<string, MetaProperty> AdditionalProperties { get; }

        /// <summary>
        /// True if the value is not from the original data but was created by computing or aggregating other values.
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// Default constructor for a non-virtual dimension value.
        /// </summary>
        /// <param name="code">Unique code among the values of this dimension</param>
        /// <param name="name">Multilanguage name of the dimension value</param>
        public DimensionValue(string code, MultilanguageString name)
        {
            Code = code;
            Name = name;
            IsVirtual = false;
            AdditionalProperties = [];
        }

        /// <summary>
        /// Constructor for dimension values where the virtuality can be set.
        /// </summary>
        /// <param name="code">Unique code among the values of this dimension</param>
        /// <param name="name">Multilanguage name of the dimension value</param>
        /// <param name="isVirtual">Indicates whether the value is in the original data or was created by computing or aggregating other values</param>
        public DimensionValue(string code, MultilanguageString name, bool isVirtual)
        {
            Code = code;
            Name = name;
            IsVirtual = isVirtual;
            AdditionalProperties = [];
        }

        /// <summary>
        /// Constructor mainly for deserialization purposes. 
        /// </summary>
        /// <param name="code">Unique code among the values of this dimension</param>
        /// <param name="name">Multilanguage name of the dimension value</param>
        /// <param name="isVirtual">Indicates whether the value is in the original data or was created by computing or aggregating other values</param>
        /// <param name="additionalProperties">Additional properties of the dimension value</param>
        [JsonConstructor]
        public DimensionValue(string code, MultilanguageString name, bool isVirtual, Dictionary<string, MetaProperty> additionalProperties)
        {
            Code = code;
            Name = name;
            IsVirtual = isVirtual;
            AdditionalProperties = additionalProperties;
        }

        #region Interface implementations

        MultilanguageString IReadOnlyDimensionValue.Name => Name;

        IReadOnlyDictionary<string, MetaProperty> IReadOnlyDimensionValue.AdditionalProperties => AdditionalProperties;

        #endregion
    }
}
