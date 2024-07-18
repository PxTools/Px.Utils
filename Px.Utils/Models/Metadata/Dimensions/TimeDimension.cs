using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Language;

namespace Px.Utils.Models.Metadata.Dimensions
{
    /// <summary>
    /// Class representing a time dimension
    /// </summary>
    public class TimeDimension : Dimension
    {
        /// <summary>
        /// Time interval between the values of this dimension
        /// </summary>
        public TimeDimensionInterval Interval { get; }

        /// <param name="code">Unique code among all the dimensions of the metadata matrix</param>
        /// <param name="name">Multilanguage name of the dimension</param>
        /// <param name="additionalProperties">Properties of the dimension, excluding the required properties</param>
        /// <param name="values">Ordered list of dimension values that define the structure of the dimension</param>
        /// <param name="interval">Time interval between the values of this dimension</param>
        public TimeDimension(
            string code,
            MultilanguageString name,
            Dictionary<string, MetaProperty> additionalProperties,
            ValueList values,
            TimeDimensionInterval interval) : base(code, name, additionalProperties, values, DimensionType.Time)
        {
            Interval = interval;
        }

        /// <param name="code">Unique code among all the dimensions of the metadata matrix</param>
        /// <param name="name">Multilanguage name of the dimension</param>
        /// <param name="additionalProperties">Properties of the dimension, excluding the required properties</param>
        /// <param name="values">Ordered list of dimension values that define the structure of the dimension</param>
        /// <param name="interval">Time interval between the values of this dimension</param>
        public TimeDimension(
            string code,
            MultilanguageString name,
            Dictionary<string, MetaProperty> additionalProperties,
            IReadOnlyList<DimensionValue> values,
            TimeDimensionInterval interval) : base(code, name, additionalProperties, values, DimensionType.Time)
        {
            Interval = interval;
        }

        #region Interface implementations

        public override TimeDimension GetTransform(IDimensionMap map)
        {
            ValueList newValues = new (map.ValueCodes.Select(code =>
            {
                if (Values.Find(code) is DimensionValue value) return value;
                else throw new ArgumentException($"Value with code {code} not found in dimension");
            }));

            return new TimeDimension(Code, Name, AdditionalProperties, newValues, Interval);
        }

        #endregion
    }
}
