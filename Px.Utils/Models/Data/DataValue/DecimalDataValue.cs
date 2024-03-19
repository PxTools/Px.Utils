using System.Numerics;

namespace PxUtils.Models.Data.DataValue
{
    public readonly record struct DecimalDataValue : IEquatable<DecimalDataValue>, IEqualityOperators<DecimalDataValue, DecimalDataValue, Boolean>
    {
        public readonly decimal UnsafeValue;
        public readonly DataValueType Type;

        public DecimalDataValue(decimal value, DataValueType type)
        {
            UnsafeValue = value;
            Type = type;
        }
    }
}
