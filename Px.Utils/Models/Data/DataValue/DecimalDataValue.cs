using Px.Utils.Models.Data;

namespace PxUtils.Models.Data.DataValue
{
    /// <summary>
    /// Structure representing one data point from a px file as a <see cref="decimal"/> value and its type.
    /// </summary>
    public readonly record struct DecimalDataValue : IDataValue<DecimalDataValue>
    {
        /// <summary>
        /// Numeric value of the data point when the type is <see cref="DataValueType.Exists"/>.
        /// Otherwise the value is undefined and using it will cause undefined behavior.
        /// </summary>
        public readonly decimal UnsafeValue { get; }

        /// <summary>
        /// Defines if the data point has a numeric value or if it is a missing value.
        /// </summary>
        public readonly DataValueType Type { get; }

        public DecimalDataValue(decimal value, DataValueType type)
        {
            UnsafeValue = value;
            Type = type;
        }

        public static DecimalDataValue AdditiveIdentity => new(0, DataValueType.Exists);

        public static DecimalDataValue MultiplicativeIdentity => new(1, DataValueType.Exists);

        public static DecimalDataValue operator +(DecimalDataValue value)
        {
            if (value.Type != DataValueType.Exists) return value;
            return new(+value.UnsafeValue, DataValueType.Exists);
        }

        public static DecimalDataValue operator +(DecimalDataValue left, DecimalDataValue right)
        {
            if (left.Type != DataValueType.Exists || right.Type != DataValueType.Exists)
            {
                if (left.Type == right.Type) return new(0, left.Type);
                return new(0, DataValueType.Missing);
            }
            return new(left.UnsafeValue + right.UnsafeValue, DataValueType.Exists);
        }

        public static DecimalDataValue operator -(DecimalDataValue value)
        {
            if (value.Type != DataValueType.Exists) return value;
            return new(-value.UnsafeValue, DataValueType.Exists);
        }

        public static DecimalDataValue operator -(DecimalDataValue left, DecimalDataValue right)
        {
            if (left.Type != DataValueType.Exists || right.Type != DataValueType.Exists)
            {
                if (left.Type == right.Type) return new(0, left.Type);
                return new(0, DataValueType.Missing);
            }
            return new(left.UnsafeValue - right.UnsafeValue, DataValueType.Exists);
        }

        public static DecimalDataValue operator *(DecimalDataValue left, DecimalDataValue right)
        {
            if (left.Type != DataValueType.Exists || right.Type != DataValueType.Exists)
            {
                if (left.Type == right.Type) return new(0, left.Type);
                return new(0, DataValueType.Missing);
            }
            return new(left.UnsafeValue * right.UnsafeValue, DataValueType.Exists);
        }

        public static DecimalDataValue operator /(DecimalDataValue left, DecimalDataValue right)
        {
            if (left.Type != DataValueType.Exists || right.Type != DataValueType.Exists)
            {
                if (left.Type == right.Type) return new(0, left.Type);
                return new(0, DataValueType.Missing);
            }
            return new(left.UnsafeValue / right.UnsafeValue, DataValueType.Exists);
        }
    }
}
