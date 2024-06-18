using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using System.Numerics;

namespace Px.Utils.Operations
{
    public class MultiplicationMatrixFunction<TData> : MatrixFunction<TData> where TData : IMultiplyOperators<TData, TData, TData>, IMultiplicativeIdentity<TData, TData>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="newValue">Dimension value that will represent the product of the values</param>
        /// <param name="productMap">Defines which values will be multiplied</param>
        public MultiplicationMatrixFunction(DimensionValue newValue, IDimensionMap productMap) : base(newValue, productMap, Multiply, TData.MultiplicativeIdentity) { }

        /// <summary>
        /// Constructor that allows to specify the index where the new value will be inserted in the dimension
        /// </summary>
        /// <param name="newValue">Dimension value that will represent the product of the values</param>
        /// <param name="productMap">Defines which values will be multiplied</param>
        /// <param name="insertIndex">Zero based index where the new value will be inserted in the dimension</param>
        public MultiplicationMatrixFunction(DimensionValue newValue, IDimensionMap productMap, int insertIndex) : base(newValue, productMap, Multiply, TData.MultiplicativeIdentity, insertIndex) { }

        /// <summary>
        /// Constructor for multiplying all values defined by the target map with a given value.
        /// </summary>
        /// <param name="targetMap">Defines which values will be multiplied</param>
        /// <param name="multiplier">Multiply by this constant</param>
        public MultiplicationMatrixFunction(IMatrixMap targetMap, TData multiplier): base(targetMap, (input) => input * multiplier) { }

        private static TData Multiply(TData a, TData b) => a * b;
    }
}
