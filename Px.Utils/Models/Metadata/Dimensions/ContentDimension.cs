﻿using Px.Utils.Language;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Models.Metadata.MetaProperties;
using Px.Utils.Serializers.Json;
using System.Text.Json.Serialization;

namespace Px.Utils.Models.Metadata.Dimensions
{
    /// <summary>
    /// Class representing a content dimension.
    /// </summary>
    [JsonConverter(typeof(DimensionConverter))]
    public class ContentDimension : Dimension
    {
        /// <summary>
        /// List of editable dimension values that define the structure of the dimension.
        /// </summary>
        public override ContentValueList Values { get; }

        /// <param name="code">Unique code among the dimensions of the metadata matrix</param>
        /// <param name="name">Multilanguage name of the dimension</param>
        /// <param name="additionalProperties">Properties of the dimension, excluding the required properties</param>
        /// <param name="values">Ordered list of dimension values that define the structure of the dimension</param>
        [JsonConstructor]
        public ContentDimension(
            string code,
            MultilanguageString name,
            Dictionary<string, MetaProperty> additionalProperties,
            ContentValueList values
        ) : base(code, name, additionalProperties, values, DimensionType.Content)
        {
            Values = values;
        }

        /// <param name="code">Unique code among the dimensions of the metadata matrix</param>
        /// <param name="name">Multilanguage name of the dimension</param>
        /// <param name="additionalProperties">Properties of the dimension, excluding the required properties</param>
        /// <param name="values">Ordered list of dimension values that define the structure of the dimension</param>
        public ContentDimension(
            string code,
            MultilanguageString name,
            Dictionary<string, MetaProperty> additionalProperties,
            IReadOnlyList<ContentDimensionValue> values
        ) : base(code, name, additionalProperties, values, DimensionType.Content)
        {
            Values = new(values);
        }

        public override ContentDimension GetTransform(IDimensionMap map)
        {
            ContentValueList newValues = new(map.ValueCodes.Select(code =>
            {
                if (Values.OfType<ContentDimensionValue>().First(value => value.Code == code) is ContentDimensionValue value) return value;
                else throw new ArgumentException($"Value with code {code} not found in dimension");
            }));

            return new ContentDimension(Code, Name, AdditionalProperties, newValues);
        }
    }
}
