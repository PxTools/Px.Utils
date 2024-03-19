using System.Numerics;

namespace PxUtils.Models.Data.DataValue
{
    public readonly record struct DoubleDataValue : IEquatable<DoubleDataValue>, IEqualityOperators<DoubleDataValue, DoubleDataValue, Boolean>
    {
        public readonly double UnsafeValue;
        public readonly DataValueType Type;

        public DoubleDataValue(double value, DataValueType type)
        {
            UnsafeValue = value;
            Type = type;
        }
    }
}
