﻿using PxUtils.Language;
using PxUtils.Models.Metadata.Enums;

namespace PxUtils.Models.Metadata.Dimensions
{
    /// <summary>
    /// Readonly interface for a dimension object in a structured format.
    /// </summary>
    public interface IReadOnlyDimension
    {
        /// <summary>
        /// Unique code among the dimensions of the matrix.
        /// </summary>
        string Code { get; }

        /// <summary>
        /// The type of the dimension.
        /// </summary>
        DimensionType Type { get; }

        /// <summary>
        /// Multilanguage name of the dimension.
        /// </summary>
        MultilanguageString Name { get; }

        /// <summary>
        /// Properties of the dimension, excluding the required properties.
        /// </summary>
        IReadOnlyDictionary<string, Property> AdditionalProperties { get; }

        /// <summary>
        /// Ordered list of dimension values that define the structure of the dimension.
        /// </summary>
        IReadOnlyList<IReadOnlyDimensionValue> Values { get; }

        /// <summary>
        /// The default value of the dimension, this property is optional.
        /// </summary>
        IReadOnlyDimensionValue? DefaultValue { get; }
    }
}
