using System.Numerics;

namespace PxUtils.Models.Data.DataValue
{
    /// <summary>
    /// Structure representing one data point from a px file as a <see cref="decimal"/> value and its type.
    /// </summary>
    public readonly record struct DecimalDataValue : IEquatable<DecimalDataValue>, IEqualityOperators<DecimalDataValue, DecimalDataValue, Boolean>
    {
        /// <summary>
        /// Numeric value of the data point when the type is <see cref="DataValueType.Exists"/>.
        /// Otherwise the value is undefined and using it will cause undefined behavior.
        /// </summary>
        public readonly decimal UnsafeValue
        {
            get { return _unsafeValue; }
        }

        /// <summary>
        /// Defines if the data point has a numeric value or if it is a missing value.
        /// </summary>
        public readonly DataValueType Type
        {
            get { return _type; }
        }

        private readonly decimal _unsafeValue;
        private readonly DataValueType _type;

        public DecimalDataValue(decimal value, DataValueType type)
        {
            _unsafeValue = value;
            _type = type;
        }
    }
}
