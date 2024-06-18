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
        /// <param name="newValue">Dimension value that will represent the sum of the values</param>
        /// <param name="sumMap">Defines which values will be summed</param>
        public MultiplicationMatrixFunction(DimensionValue newValue, IDimensionMap sumMap) : base(newValue, sumMap, Multiply, TData.MultiplicativeIdentity) { }

        /// <summary>
        /// Constructor that allows to specify the index where the new value will be inserted in the dimension
        /// </summary>
        /// <param name="newValue">Dimension value that will represent the sum of the values</param>
        /// <param name="sumMap">Defines which values will be summed</param>
        /// <param name="insertIndex">Zero based index where the new value will be inserted in the dimension</param>
        public MultiplicationMatrixFunction(DimensionValue newValue, IDimensionMap sumMap, int insertIndex) : base(newValue, sumMap, Multiply, TData.MultiplicativeIdentity, insertIndex) { }

        private static TData Multiply(TData a, TData b) => a * b;
    }
}
