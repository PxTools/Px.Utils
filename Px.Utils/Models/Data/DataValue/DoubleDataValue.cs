using Px.Utils.Models.Data;

namespace PxUtils.Models.Data.DataValue
{
    /// <summary>
    /// Structure representing one data point from a px file as a <see cref="double"/> value and its type.
    /// </summary>
    public readonly record struct DoubleDataValue : IDataValue<DoubleDataValue>
    {
        /// <summary>
        /// Numeric value of the data point when the type is <see cref="DataValueType.Exists"/>.
        /// Otherwise the value is undefined and using it will cause undefined behavior.
        /// </summary>
        public readonly double UnsafeValue { get; }

        /// <summary>
        /// Defines if the data point has a numeric value or if it is a missing value.
        /// </summary>
        public readonly DataValueType Type { get; }

        public DoubleDataValue(double value, DataValueType type)
        {
            UnsafeValue = value;
            Type = type;
        }

        public static DoubleDataValue AdditiveIdentity => new(0, DataValueType.Exists);

        public static DoubleDataValue MultiplicativeIdentity => new(1, DataValueType.Exists);

        public static DoubleDataValue operator +(DoubleDataValue value)
        {
            if (value.Type != DataValueType.Exists) return value;
            return new(+value.UnsafeValue, DataValueType.Exists);
        }

        public static DoubleDataValue operator +(DoubleDataValue left, DoubleDataValue right)
        {
            if (left.Type != DataValueType.Exists || right.Type != DataValueType.Exists)
            {
                if (left.Type == right.Type) return new(0, left.Type);
                return new(0, DataValueType.Missing);
            }
            return new(left.UnsafeValue + right.UnsafeValue, DataValueType.Exists);
        }

        public static DoubleDataValue operator -(DoubleDataValue value)
        {
            if (value.Type != DataValueType.Exists) return value;
            return new(-value.UnsafeValue, DataValueType.Exists);
        }

        public static DoubleDataValue operator -(DoubleDataValue left, DoubleDataValue right)
        {
            if (left.Type != DataValueType.Exists || right.Type != DataValueType.Exists)
            {
                if (left.Type == right.Type) return new(0, left.Type);
                return new(0, DataValueType.Missing);
            }
            return new(left.UnsafeValue - right.UnsafeValue, DataValueType.Exists);
        }

        public static DoubleDataValue operator *(DoubleDataValue left, DoubleDataValue right)
        {
            if (left.Type != DataValueType.Exists || right.Type != DataValueType.Exists)
            {
                if (left.Type == right.Type) return new(0, left.Type);
                return new(0, DataValueType.Missing);
            }
            return new(left.UnsafeValue * right.UnsafeValue, DataValueType.Exists);
        }

        public static DoubleDataValue operator /(DoubleDataValue left, DoubleDataValue right)
        {
            if (left.Type != DataValueType.Exists || right.Type != DataValueType.Exists)
            {
                if (left.Type == right.Type) return new(0, left.Type);
                return new(0, DataValueType.Missing);
            }
            if(right.UnsafeValue == 0) throw new DivideByZeroException();
            return new(left.UnsafeValue / right.UnsafeValue, DataValueType.Exists);
        }
    }
}
